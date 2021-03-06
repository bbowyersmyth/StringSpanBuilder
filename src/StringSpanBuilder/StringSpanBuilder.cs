﻿// Copyright (c) Bruce Bowyer-Smyth. All rights reserved.
// Licensed under the MIT License. See the LICENSE.txt file in the project root for more information.

using System;
using System.Diagnostics;

namespace Spans.Text.StringSpanBuilder
{
    /// <summary>
    /// Represents a list of string spans appearing as a single whole.
    /// </summary>
    public sealed class StringSpanBuilder
    {
        private const int DefaultCapacity = 8;
        private const int MaxChunkSize = 4000;
        private const int MaxLength = int.MaxValue;

        internal CharSpan[] _chunkSpans;
        private StringSpanBuilder _chunkPrevious;
        private int _totalLength;
        private int _spanIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Spans.Text.StringSpanBuilder" /> class.
        /// </summary>
        public StringSpanBuilder()
            : this(DefaultCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Spans.Text.StringSpanBuilder" /> class using the specified capacity.
        /// </summary>
        /// <param name="capacity">The suggested starting size of this instance.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="capacity" /> is less than zero. </exception>
        public StringSpanBuilder(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), string.Format(Resources.Strings.ArgumentOutOfRange_MustBePositive, nameof(capacity)));
            }

            _chunkSpans = new CharSpan[capacity];
            _totalLength = 0;
            _spanIndex = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Spans.Text.StringSpanBuilder" /> class using the specified string.
        /// </summary>
        /// <param name="value">The string used to initialize the value of the instance.</param>
        public StringSpanBuilder(string value)
            : this(DefaultCapacity)
        {
            Append(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Spans.Text.StringSpanBuilder" /> class using the specified string and capacity.
        /// </summary>
        /// <param name="value">The string used to initialize the value of the instance.</param>
        /// <param name="capacity">The suggested starting size of the <see cref="T:Spans.Text.StringSpanBuilder" />. </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="capacity" /> is less than zero. </exception>
        public StringSpanBuilder(string value, int capacity)
            : this(capacity)
        {
            Append(value);
        }

        private StringSpanBuilder(StringSpanBuilder from)
        {
            _spanIndex = from._spanIndex;
            _chunkSpans = from._chunkSpans;
            _chunkPrevious = from._chunkPrevious;
        }

        /// <summary>
        /// Removes all string spans from the current <see cref="T:Spans.Text.StringSpanBuilder" /> instance.
        /// </summary>
        /// <returns>An object whose <see cref="P:Spans.Text.StringSpanBuilder.Length" /> is 0 (zero).</returns>
        public StringSpanBuilder Clear()
        {
            Length = 0;
            return this;
        }

        /// <summary>
        /// Gets or sets the length of the current <see cref="T:Spans.Text.StringSpanBuilder" /> object.
        /// </summary>
        /// <returns>The length of this instance.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The value specified for a set operation is less than zero or greater 
        /// than the current length. Length can only be reduced in size.</exception>
        public int Length
        {
            get
            {
                return _totalLength;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), Resources.Strings.ArgumentOutOfRange_NegativeLength);
                }

                if (value == 0 && _chunkPrevious == null)
                {
                    _totalLength = 0;
                    ClearSpans(_chunkSpans, 0, _spanIndex);
                    _spanIndex = -1;
                    return;
                }

                int delta = value - Length;

                if (delta == 0)
                {
                    return;
                }
                else if (delta > 0)
                {
                    // StringBuilder supports increasing the length which will pad the new space with null chars.
                    // There is not much reason to do that and there is even less reason with a span builder.
                    throw new ArgumentOutOfRangeException(nameof(value), Resources.Strings.ArgumentOutOfRange_GreaterLength);
                }
                else
                {
                    int spanIndex;
                    int charLength;
                    StringSpanBuilder chunk = FindChunkForCharIndex(value, out spanIndex, out charLength);
                    if (chunk != this)
                    {
                        _chunkSpans = chunk._chunkSpans;
                        _chunkPrevious = chunk._chunkPrevious;
                        _spanIndex = chunk._spanIndex;
                    }
                    _totalLength += delta;  // Delta is negative
                    _chunkSpans[spanIndex].Length = charLength;
                    ClearSpans(_chunkSpans, spanIndex + 1, _spanIndex);
                    _spanIndex = spanIndex;
                }
            }
        }

        /// <summary>
        /// Appends a copy of the specified string to this instance.
        /// </summary>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <param name="value">The string to append.</param>
        /// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed the length allowed for a string.</exception>
        public StringSpanBuilder Append(string value)
        {
            if (value?.Length > 0)
            {
                AppendHelper(value, 0, value.Length);
            }
            return this;
        }

        /// <summary>
        /// Appends the specified string portion to this instance.
        /// </summary>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <param name="value">The string that contains the portion to append. </param>
        /// <param name="startIndex">The starting position of the portion within <paramref name="value" />. </param>
        /// <param name="length">The number of characters in <paramref name="value" /> to append. </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="value" /> is null, and <paramref name="startIndex" /> and <paramref name="length" /> are not zero. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="length" /> less than zero.-or- <paramref name="startIndex" /> less than zero.-or- 
        /// <paramref name="startIndex" /> + <paramref name="length" /> is greater than the length of <paramref name="value" />.</exception>
        /// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed the length allowed for a string.</exception>
        public StringSpanBuilder Append(string value, int startIndex, int length)
        {
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), Resources.Strings.ArgumentOutOfRange_Index);
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), Resources.Strings.ArgumentOutOfRange_GenericPositive);
            }

            // If the value being added is null, eat the null and return.
            if (value == null)
            {
                if (startIndex == 0 && length == 0)
                {
                    return this;
                }
                throw new ArgumentNullException(nameof(value));
            }

            if (length == 0)
            {
                return this;
            }

            if (startIndex > value.Length - length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), Resources.Strings.ArgumentOutOfRange_Index);
            }

            AppendHelper(value, startIndex, length);

            return this;
        }

        /// <summary>
        /// Appends the default line terminator to the end of the current <see cref="T:Spans.Text.StringSpanBuilder" /> object.
        /// </summary>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed the length allowed for a string.</exception>
        public StringSpanBuilder AppendLine()
        {
            return Append(Environment.NewLine);
        }

        /// <summary>
        /// Appends the specified string followed by the default line terminator to the end of the current <see cref="T:Spans.Text.StringSpanBuilder" /> object.
        /// </summary>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <param name="value">The string to append. </param>
        /// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed the length allowed for a string.</exception>
        public StringSpanBuilder AppendLine(string value)
        {
            Append(value);
            return Append(Environment.NewLine);
        }

        /// <summary>
        /// Prepends a copy of the specified string to this instance.
        /// </summary>
        /// <returns>A reference to this instance after the prepend operation has completed.</returns>
        /// <param name="value">The string to prepend.</param>
        /// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed the length allowed for a string.</exception>
        public StringSpanBuilder Prepend(string value)
        {
            if (value != null && value.Length > 0)
            {
                PrependHelper(value, 0, value.Length);
            }
            return this;
        }

        /// <summary>
        /// Prepends the specified string portion to this instance.
        /// </summary>
        /// <returns>A reference to this instance after the prepend operation has completed.</returns>
        /// <param name="value">The string that contains the portion to prepend. </param>
        /// <param name="startIndex">The starting position of the portion within <paramref name="value" />. </param>
        /// <param name="length">The number of characters in <paramref name="value" /> to prepend. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="value" /> is null, and <paramref name="startIndex" /> and <paramref name="length" /> are not zero. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="length" /> less than zero.-or- <paramref name="startIndex" /> less than zero.-or- 
        /// <paramref name="startIndex" /> + <paramref name="length" /> is greater than the length of <paramref name="value" />.</exception>
        /// <exception cref="T:System.OutOfMemoryException">Enlarging the value of this instance would exceed the length allowed for a string.</exception>
        public StringSpanBuilder Prepend(string value, int startIndex, int length)
        {
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), Resources.Strings.ArgumentOutOfRange_Index);
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), Resources.Strings.ArgumentOutOfRange_GenericPositive);
            }

            // If the value being added is null, eat the null and return.
            if (value == null)
            {
                if (startIndex == 0 && length == 0)
                {
                    return this;
                }
                throw new ArgumentNullException(nameof(value));
            }

            if (length == 0)
            {
                return this;
            }

            if (startIndex > value.Length - length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), Resources.Strings.ArgumentOutOfRange_Index);
            }

            PrependHelper(value, startIndex, length);

            return this;
        }

        //public char this[int index] { get; }
        //public StringSpanBuilder Insert(int index, string value, int count)
        //public StringSpanBuilder Insert(int index, string value)
        //public StringSpanBuilder Remove(int startIndex, int length)
        //public StringSpanBuilder Replace(string oldValue, string newValue)
        //public StringSpanBuilder Replace(string oldValue, string newValue, int startIndex, int count)
        //public StringSpanBuilder Replace(char oldChar, char newChar)
        //public StringSpanBuilder Replace(char oldChar, char newChar, int startIndex, int count)

        /// <summary>
        /// Removes all leading white-space characters from the current <see cref="T:Spans.Text.StringSpanBuilder" /> object.
        /// </summary>
        /// <returns>A reference to this instance after the trim operation has completed.</returns>
        public StringSpanBuilder TrimStart()
        {
            StringSpanBuilder secondToLastChunk;
            StringSpanBuilder chunk;

            do
            {
                secondToLastChunk = null;
                chunk = this;

                // Work our way to the first chunk
                while (chunk._chunkPrevious != null)
                {
                    secondToLastChunk = chunk;
                    chunk = chunk._chunkPrevious;
                }


                int currentIndex = 0;

                while (currentIndex <= chunk._spanIndex)
                {
                    var startingLength = chunk._chunkSpans[currentIndex].Length;
                    chunk._chunkSpans[currentIndex].TrimStart();
                    var newLength = chunk._chunkSpans[currentIndex].Length;

                    if (newLength > 0)
                    {
                        // Found a span that is not a whitespace. Recalculate length.
                        _totalLength -= (startingLength - newLength);
                        return this;
                    }

                    _totalLength -= startingLength;
                    currentIndex++;
                }

                if (secondToLastChunk != null)
                {
                    // Cut off the head chunk, thereby making the second to last now the last.
                    // The outer loop then begins again. Searching through this chunk.
                    secondToLastChunk._chunkPrevious = null;
                }
                else
                {
                    // We reach here when chunk == this. So there is no previous chunk to disconnect.
                    // We have trimmed away the entire string.
                    chunk._spanIndex = -1;
                }

            } while (chunk != this);

            return this;
        }

        /// <summary>
        /// Removes all trailing white-space characters from the current <see cref="T:Spans.Text.StringSpanBuilder" /> object.
        /// </summary>
        /// <returns>A reference to this instance after the trim operation has completed.</returns>
        public StringSpanBuilder TrimEnd()
        {
            StringSpanBuilder chunk = this;

            do
            {
                int currentIndex = chunk._spanIndex;

                while (currentIndex > -1)
                {
                    var startingLength = chunk._chunkSpans[currentIndex].Length;
                    chunk._chunkSpans[currentIndex].TrimEnd();
                    var newLength = chunk._chunkSpans[currentIndex].Length;

                    if (newLength > 0)
                    {
                        // Found a span that is not a whitespace. Recalculate length.
                        _totalLength -= (startingLength - newLength);
                        return this;
                    }

                    _totalLength -= startingLength;
                    currentIndex--;
                    chunk._spanIndex = currentIndex;
                }

                chunk = chunk._chunkPrevious;
            } while (chunk != null);

            return this;
        }

        /// <summary>
        /// Removes all leading and trailing white-space characters from the current <see cref="T:Spans.Text.StringSpanBuilder" /> object.
        /// </summary>
        /// <returns>A reference to this instance after the trim operation has completed.</returns>
        public StringSpanBuilder Trim()
        {
            TrimStart();
            TrimEnd();

            return this;
        }

        /// <summary>
        /// Copies the characters from a specified segment of this instance to a specified segment of a destination <see cref="T:System.Char" /> array.
        /// </summary>
        /// <param name="sourceIndex">The starting position in this instance where characters will be copied from. The index is zero-based.</param>
        /// <param name="destination">The array where characters will be copied.</param>
        /// <param name="destinationIndex">The starting position in <paramref name="destination" /> where characters will be copied. The index is zero-based.</param>
        /// <param name="count">The number of characters to be copied.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="destination" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="sourceIndex" />, <paramref name="destinationIndex" />, or <paramref name="count" />, is less than zero.-or-<paramref name="sourceIndex" /> is greater than the length of this instance.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="sourceIndex" /> + <paramref name="count" /> is greater than the length of this instance.-or-<paramref name="destinationIndex" /> + <paramref name="count" /> is greater than the length of <paramref name="destination" />.</exception>
        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), Resources.Strings.Arg_NegativeArgCount);
            }

            if (destinationIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex),
                    string.Format(Resources.Strings.ArgumentOutOfRange_MustBeNonNegNum, nameof(destinationIndex)));
            }

            if (destinationIndex > destination.Length - count)
            {
                throw new ArgumentException(Resources.Strings.ArgumentOutOfRange_OffsetOut);
            }

            if ((uint)sourceIndex > (uint)Length)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex), Resources.Strings.ArgumentOutOfRange_Index);
            }

            if (sourceIndex > Length - count)
            {
                throw new ArgumentException(Resources.Strings.Arg_LongerThanSrcString);
            }

            if (count == 0)
            {
                return;
            }

            StringSpanBuilder lastProcessed = null;
            StringSpanBuilder chunk;
            int sourceEndIndex = sourceIndex + count;
            int readOffset = 0;

            unsafe
            {
                fixed (char* destinationPtr = destination)
                {
                    char* destinationWritePtr = destinationPtr + destinationIndex;
                    char* destinationEndPtr = destinationWritePtr + count;

                    do
                    {
                        chunk = this;
                        while (chunk._chunkPrevious != lastProcessed)
                        {
                            chunk = chunk._chunkPrevious;
                        }

                        int currentIndex = 0;

                        while (currentIndex <= chunk._spanIndex)
                        {
                            var currentSpan = chunk._chunkSpans[currentIndex];

                            if (currentSpan.Length > 0 && readOffset + currentSpan.Length > sourceIndex)
                            {
                                int spanReadPos = currentSpan.StartPosition;
                                int spanReadLength = currentSpan.Length;

                                if (readOffset < sourceIndex)
                                {
                                    int startDelta = sourceIndex - readOffset;
                                    spanReadPos += startDelta;
                                    spanReadLength -= startDelta;
                                }

                                if (readOffset + spanReadLength > sourceEndIndex)
                                {
                                    spanReadLength = (sourceEndIndex - readOffset);
                                }

                                if (spanReadPos < 0 ||
                                    spanReadPos + spanReadLength > currentSpan.Value.Length)
                                {
                                    // There has been an error calculating lengths at some point.
                                    // Bail to avoid a buffer overrun read.
                                    throw new ArgumentOutOfRangeException("writeOffset");
                                }

                                fixed (char* sourcePtr = currentSpan.Value)
                                {
#if NOMEMORYCOPY
                                    BufferCompat.MemoryCopy(
                                        sourcePtr + spanReadPos,
                                        destinationWritePtr,
                                        (byte*)destinationEndPtr - (byte*)destinationWritePtr,
                                        (long)spanReadLength * sizeof(char));
#else
                                    Buffer.MemoryCopy(
                                        sourcePtr + spanReadPos,
                                        destinationWritePtr,
                                        (byte*)destinationEndPtr - (byte*)destinationWritePtr,
                                        (long)spanReadLength * sizeof(char));
#endif //NOMEMORYCOPY
                                }

                                destinationWritePtr += spanReadLength;

                                if (destinationWritePtr >= destinationEndPtr)
                                {
                                    return;
                                }
                            }

                            readOffset += currentSpan.Length;
                            currentIndex++;
                        }

                        lastProcessed = chunk;
                    } while (chunk != this);
                }
            }
        }

        /// <summary>
        /// Converts the value of this instance to a <see cref="T:System.String" />.
        /// </summary>
        /// <returns>A string whose value is the same as this instance.</returns>
        public override string ToString()
        {
            int lengthLocal = Length;

            if (lengthLocal == 0)
            {
                return string.Empty;
            }

            string ret = new string('\0', lengthLocal);
            StringSpanBuilder lastProcessed = null;
            StringSpanBuilder chunk;

            unsafe
            {
                fixed (char* destinationPtr = ret)
                {
                    char* destinationWritePtr = destinationPtr;
                    char* destinationEndPtr = destinationPtr + lengthLocal;

                    do
                    {
                        chunk = this;
                        while (chunk._chunkPrevious != lastProcessed)
                        {
                            chunk = chunk._chunkPrevious;
                        }

                        int currentIndex = 0;

                        while (currentIndex <= chunk._spanIndex)
                        {
                            var currentSpan = chunk._chunkSpans[currentIndex];

                            if (currentSpan.Length == 1)
                            {
                                // Perf: Delimiters are often added as single char strings. Avoid the memory copy and
                                //       just assign directly.

                                if (destinationWritePtr >= destinationEndPtr)
                                {
                                    // There has been an error calculating lengths at some point.
                                    // Bail to avoid a buffer overrun write.
                                    throw new ArgumentOutOfRangeException("writeOffset");
                                }

                                *destinationWritePtr = currentSpan.Value[currentSpan.StartPosition];
                                destinationWritePtr++;
                            }
                            else if (currentSpan.Length > 0)
                            {
                                if (currentSpan.StartPosition < 0 ||
                                    currentSpan.StartPosition + currentSpan.Length > currentSpan.Value.Length)
                                {
                                    // There has been an error calculating lengths at some point.
                                    // Bail to avoid a buffer overrun read.
                                    throw new ArgumentOutOfRangeException("writeOffset");
                                }

                                fixed (char* sourcePtr = currentSpan.Value)
                                {
#if NOMEMORYCOPY
                                    BufferCompat.MemoryCopy(
                                        sourcePtr + currentSpan.StartPosition,
                                        destinationWritePtr,
                                        (byte*)destinationEndPtr - (byte*)destinationWritePtr,
                                        (long)currentSpan.Length * sizeof(char));
#else
                                    Buffer.MemoryCopy(
                                        sourcePtr + currentSpan.StartPosition,
                                        destinationWritePtr,
                                        (byte*)destinationEndPtr - (byte*)destinationWritePtr,
                                        (long)currentSpan.Length * sizeof(char));
#endif //NOMEMORYCOPY
                                }

                                destinationWritePtr += currentSpan.Length;
                            }

                            currentIndex++;
                        }

                        lastProcessed = chunk;
                    } while (chunk != this);

                    return ret;
                }
            }
        }

        /// <summary>
        /// Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a single argument.
        /// </summary>
        /// <returns>A reference to this instance with <paramref name="format" /> appended. Each format item in <paramref name="format" /> is replaced by the string representation of <paramref name="arg0" />.</returns>
        /// <param name="format">A composite format string (see Remarks). </param>
        /// <param name="arg0">An object to format. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="format" /> is null. </exception>
        /// <exception cref="T:System.FormatException"><paramref name="format" /> is invalid. -or-The index of a format item is less than 0 (zero), or greater than or equal to 1.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed the length allowed for a string. </exception>
        public StringSpanBuilder AppendFormat(string format, object arg0)
        {
            return AppendFormatHelper(null, format, new ParamsArray(arg0));
        }

        /// <summary>
        /// Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of either of two arguments.
        /// </summary>
        /// <returns>A reference to this instance with <paramref name="format" /> appended. Each format item in <paramref name="format" /> is replaced by the string representation of the corresponding object argument.</returns>
        /// <param name="format">A composite format string (see Remarks). </param>
        /// <param name="arg0">The first object to format. </param>
        /// <param name="arg1">The second object to format. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="format" /> is null. </exception>
        /// <exception cref="T:System.FormatException"><paramref name="format" /> is invalid.-or-The index of a format item is less than 0 (zero), or greater than or equal to 2. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed the length allowed for a string. </exception>
        public StringSpanBuilder AppendFormat(string format, object arg0, object arg1)
        {
            return AppendFormatHelper(null, format, new ParamsArray(arg0, arg1));
        }

        /// <summary>
        /// Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of either of three arguments.
        /// </summary>
        /// <returns>A reference to this instance with <paramref name="format" /> appended. Each format item in <paramref name="format" /> is replaced by the string representation of the corresponding object argument.</returns>
        /// <param name="format">A composite format string (see Remarks). </param>
        /// <param name="arg0">The first object to format. </param>
        /// <param name="arg1">The second object to format. </param>
        /// <param name="arg2">The third object to format. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="format" /> is null. </exception>
        /// <exception cref="T:System.FormatException"><paramref name="format" /> is invalid.-or-The index of a format item is less than 0 (zero), or greater than or equal to 3.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed the length allowed for a string. </exception>
        public StringSpanBuilder AppendFormat(string format, object arg0, object arg1, object arg2)
        {
            return AppendFormatHelper(null, format, new ParamsArray(arg0, arg1, arg2));
        }

        /// <summary>
        /// Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter array.
        /// </summary>
        /// <returns>A reference to this instance with <paramref name="format" /> appended. Each format item in <paramref name="format" /> is replaced by the string representation of the corresponding object argument.</returns>
        /// <param name="format">A composite format string (see Remarks). </param>
        /// <param name="args">An array of objects to format. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="format" /> or <paramref name="args" /> is null. </exception>
        /// <exception cref="T:System.FormatException"><paramref name="format" /> is invalid. -or-The index of a format item is less than 0 (zero), or greater than or equal to the length of the <paramref name="args" /> array.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed the length allowed for a string. </exception>
        public StringSpanBuilder AppendFormat(string format, params object[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException((format == null) ? nameof(format) : nameof(args));
            }

            return AppendFormatHelper(null, format, new ParamsArray(args));
        }

        /// <summary>
        /// Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a single argument using a specified format provider. 
        /// </summary>
        /// <returns>A reference to this instance after the append operation has completed. After the append operation, this instance contains any data that existed before the operation, suffixed by a copy of <paramref name="format" /> in which any format specification is replaced by the string representation of <paramref name="arg0" />. </returns>
        /// <param name="provider">An object that supplies culture-specific formatting information. </param>
        /// <param name="format">A composite format string (see Remarks). </param>
        /// <param name="arg0">The object to format. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="format" /> is null. </exception>
        /// <exception cref="T:System.FormatException"><paramref name="format" /> is invalid. -or-The index of a format item is less than 0 (zero), or greater than or equal to one (1). </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed the length allowed for a string. </exception>
        public StringSpanBuilder AppendFormat(IFormatProvider provider, string format, object arg0)
        {
            return AppendFormatHelper(provider, format, new ParamsArray(arg0));
        }

        /// <summary>
        /// Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of either of two arguments using a specified format provider.
        /// </summary>
        /// <returns>A reference to this instance after the append operation has completed. After the append operation, this instance contains any data that existed before the operation, suffixed by a copy of <paramref name="format" /> where any format specification is replaced by the string representation of the corresponding object argument. </returns>
        /// <param name="provider">An object that supplies culture-specific formatting information. </param>
        /// <param name="format">A composite format string (see Remarks). </param>
        /// <param name="arg0">The first object to format. </param>
        /// <param name="arg1">The second object to format. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="format" /> is null. </exception>
        /// <exception cref="T:System.FormatException"><paramref name="format" /> is invalid. -or-The index of a format item is less than 0 (zero), or greater than or equal to 2 (two). </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed the length allowed for a string. </exception>
        public StringSpanBuilder AppendFormat(IFormatProvider provider, string format, object arg0, object arg1)
        {
            return AppendFormatHelper(provider, format, new ParamsArray(arg0, arg1));
        }

        /// <summary>
        /// Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of either of three arguments using a specified format provider.
        /// </summary>
        /// <returns>A reference to this instance after the append operation has completed. After the append operation, this instance contains any data that existed before the operation, suffixed by a copy of <paramref name="format" /> where any format specification is replaced by the string representation of the corresponding object argument. </returns>
        /// <param name="provider">An object that supplies culture-specific formatting information. </param>
        /// <param name="format">A composite format string (see Remarks). </param>
        /// <param name="arg0">The first object to format. </param>
        /// <param name="arg1">The second object to format. </param>
        /// <param name="arg2">The third object to format. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="format" /> is null. </exception>
        /// <exception cref="T:System.FormatException"><paramref name="format" /> is invalid. -or-The index of a format item is less than 0 (zero), or greater than or equal to 3 (three). </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed the length allowed for a string. </exception>
        public StringSpanBuilder AppendFormat(IFormatProvider provider, string format, object arg0, object arg1, object arg2)
        {
            return AppendFormatHelper(provider, format, new ParamsArray(arg0, arg1, arg2));
        }

        /// <summary>
        /// Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter array using a specified format provider.
        /// </summary>
        /// <returns>A reference to this instance after the append operation has completed. After the append operation, this instance contains any data that existed before the operation, suffixed by a copy of <paramref name="format" /> where any format specification is replaced by the string representation of the corresponding object argument. </returns>
        /// <param name="provider">An object that supplies culture-specific formatting information. </param>
        /// <param name="format">A composite format string (see Remarks). </param>
        /// <param name="args">An array of objects to format.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="format" /> is null. </exception>
        /// <exception cref="T:System.FormatException"><paramref name="format" /> is invalid. -or-The index of a format item is less than 0 (zero), or greater than or equal to the length of the <paramref name="args" /> array.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The length of the expanded string would exceed the length allowed for a string. </exception>
        public StringSpanBuilder AppendFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException((format == null) ? nameof(format) : nameof(args));
            }

            return AppendFormatHelper(provider, format, new ParamsArray(args));
        }

        /// <summary>
        /// Determines whether the beginning of this string matches the specified character.
        /// </summary>
        /// <returns>true if <paramref name="value" /> matches the beginning of the string; otherwise, false.</returns>
        /// <param name="value">The character to compare. </param>
        public bool StartsWith(char value)
        {
            if (Length == 0)
            {
                return false;
            }

            int spanIndex;
            int charLength;

            var chunk = FindChunkForCharIndex(0, out spanIndex, out charLength);

            return chunk._chunkSpans[spanIndex].StartsWith(value);
        }

        /// <summary>
        /// Determines whether the end of this string instance matches the specified character.
        /// </summary>
        /// <returns>true if <paramref name="value" /> matches the end of this string; otherwise, false.</returns>
        /// <param name="value">The character to compare. </param>
        public bool EndsWith(char value)
        {
            if (Length == 0)
            {
                return false;
            }

            int spanIndex;
            int charLength;

            var chunk = FindChunkForCharIndex(Length - 1, out spanIndex, out charLength);

            return chunk._chunkSpans[spanIndex].EndsWith(value);
        }

        private static void FormatError()
        {
            throw new FormatException(Resources.Strings.Format_InvalidString);
        }

        private StringSpanBuilder AppendFormatHelper(IFormatProvider provider, string format, ParamsArray args)
        {
            if (format == null)
            {
                throw new ArgumentNullException(nameof(format));
            }

            int startPos = 0;
            int pos = 0;
            int len = format.Length;
            char ch = '\x0';

            ICustomFormatter cf = null;
            if (provider != null)
            {
                cf = (ICustomFormatter)provider.GetFormat(typeof(ICustomFormatter));
            }

            while (true)
            {
                startPos = pos;

                while (pos < len)
                {
                    ch = format[pos];
                    pos++;

                    if (ch == '}')
                    {
                        if (pos < len && format[pos] == '}') // Treat as escape character for }}
                        {
                            Append(format, startPos, pos - startPos);
                            pos++;
                            startPos = pos;
                        }
                        else
                        {
                            FormatError();
                        }
                    }

                    if (ch == '{')
                    {
                        if (pos < len && format[pos] == '{') // Treat as escape character for {{
                        {
                            Append(format, startPos, pos - startPos);
                            pos++;
                            startPos = pos;
                        }
                        else
                        {
                            pos--;
                            if (startPos != pos)
                            {
                                Append(format, startPos, pos - startPos);
                            }
                            break;
                        }
                    }
                }

                if (pos == len)
                {
                    if (startPos != pos - 1)
                    {
                        Append(format, startPos, pos - startPos);
                    }

                    break;
                }

                pos++;
                if (pos == len || (ch = format[pos]) < '0' || ch > '9')
                {
                    FormatError();
                }

                int index = 0;
                do
                {
                    index = index * 10 + ch - '0';
                    pos++;
                    if (pos == len)
                    {
                        FormatError();
                    }
                    ch = format[pos];
                } while (ch >= '0' && ch <= '9' && index < 1000000);

                if (index >= args.Length)
                {
                    throw new FormatException(Resources.Strings.Format_IndexOutOfRange);
                }

                while (pos < len && (ch = format[pos]) == ' ')
                {
                    pos++;
                }

                bool leftJustify = false;
                int width = 0;

                if (ch == ',')
                {
                    pos++;
                    while (pos < len && format[pos] == ' ')
                    {
                        pos++;
                    }

                    if (pos == len)
                    {
                        FormatError();
                    }

                    ch = format[pos];
                    if (ch == '-')
                    {
                        leftJustify = true;
                        pos++;
                        if (pos == len)
                        {
                            FormatError();
                        }
                        ch = format[pos];
                    }

                    if (ch < '0' || ch > '9')
                    {
                        FormatError();
                    }

                    do
                    {
                        width = width * 10 + ch - '0';
                        pos++;
                        if (pos == len)
                        {
                            FormatError();
                        }
                        ch = format[pos];
                    } while (ch >= '0' && ch <= '9' && width < 1000000);
                }

                while (pos < len && (ch = format[pos]) == ' ')
                {
                    pos++;
                }

                object arg = args[index];
                string itemFormat = null;

                if (ch == ':')
                {
                    pos++;
                    int itemFormatStartPos = pos;
                    bool isEscaped = false;

                    while (true)
                    {
                        if (pos == len)
                        {
                            FormatError();
                        }

                        ch = format[pos];
                        pos++;

                        if (ch == '}' || ch == '{')
                        {
                            if (ch == '{')
                            {
                                if (pos < len && format[pos] == '{')  // Treat as escape character for {{
                                {
                                    isEscaped = true;
                                    pos++;
                                }
                                else
                                {
                                    FormatError();
                                }
                            }
                            else
                            {
                                if (pos < len && format[pos] == '}')  // Treat as escape character for }}
                                {
                                    isEscaped = true;
                                    pos++;
                                }
                                else
                                {
                                    pos--;
                                    break;
                                }
                            }
                        }
                    }

                    if (itemFormatStartPos != pos)
                    {
                        itemFormat = format.Substring(itemFormatStartPos, pos - itemFormatStartPos);

                        if (isEscaped)
                        {
                            itemFormat = itemFormat.Replace("{{", "{").Replace("}}", "}");
                        }
                    }
                }

                if (ch != '}')
                {
                    FormatError();
                }

                pos++;
                string s = null;

                if (cf != null)
                {
                    s = cf.Format(itemFormat, arg, provider);
                }

                if (s == null)
                {
                    IFormattable formattableArg = arg as IFormattable;

                    if (formattableArg != null)
                    {
                        s = formattableArg.ToString(itemFormat, provider);
                    }
                    else if (arg != null)
                    {
                        s = arg.ToString();
                    }
                }

                if (s == null)
                {
                    s = String.Empty;
                }

                int pad = width - s.Length;
                if (!leftJustify && pad > 0)
                {
                    Append(new string(' ', pad));
                }
                Append(s);
                if (leftJustify && pad > 0)
                {
                    Append(new string(' ', pad));
                }
            }
            return this;
        }

        private void AppendHelper(string value, int startIndex, int length)
        {
            _spanIndex++;

            if (_spanIndex == _chunkSpans.Length)
            {
                _spanIndex--;
                ExpandByABlock();
            }

            _chunkSpans[_spanIndex] = new CharSpan(value, startIndex, length);

            // Check for integer overflow
            if (_totalLength + length < _totalLength)
            {
                _chunkSpans = null;
                throw new OutOfMemoryException();
            }

            _totalLength += length;
        }

        private void PrependHelper(string value, int startIndex, int length)
        {
            StringSpanBuilder chunk = this;

            while (chunk._chunkPrevious != null)
            {
                chunk = chunk._chunkPrevious;
            }

            // Could get lucky if a trim or similar action has taken place
            if (chunk._chunkSpans.Length > 0 && chunk._chunkSpans[0].Length == 0)
            {
                chunk._chunkSpans[0] = new CharSpan(value, startIndex, length);

                if (chunk._spanIndex == -1)
                {
                    chunk._spanIndex = 0;
                }
            }
            else if (chunk._spanIndex + 1 < chunk._chunkSpans.Length)
            {
                // Still room in this chunk to shuffle everything down
                Array.Copy(chunk._chunkSpans, 0, chunk._chunkSpans, 1, chunk._spanIndex + 1);
                chunk._spanIndex++;
                chunk._chunkSpans[0] = new CharSpan(value, startIndex, length);
            }
            else
            {
                // No room left in the first chunk
                StringSpanBuilder newChunk = new StringSpanBuilder();
                chunk._chunkPrevious = newChunk;
                newChunk._chunkSpans[0] = new CharSpan(value, startIndex, length);
                newChunk._spanIndex = 0;
            }

            // Check for integer overflow
            if (_totalLength + length < _totalLength)
            {
                _chunkSpans = null;
                throw new OutOfMemoryException();
            }

            _totalLength += length;
        }

        private void ExpandByABlock()
        {
            int newBlockLength = Math.Max(Math.Min(_chunkSpans.Length * 2, MaxChunkSize), DefaultCapacity);

            // Copy the current block to the new block
            _chunkPrevious = new StringSpanBuilder(this);
            _spanIndex = 0;
            _chunkSpans = new CharSpan[newBlockLength];
        }

        private StringSpanBuilder FindChunkForCharIndex(int value, out int spanIndex, out int charLength)
        {
            StringSpanBuilder chunk = this;
            int remainingLength = _totalLength - value;
            spanIndex = -1;
            charLength = -1;

            Debug.Assert(remainingLength >= 0, "Index not found in builder");

            while (chunk != null)
            {
                // Work our way backwards through the lengths until we find the length
                // that the index is within
                for (int i = chunk._spanIndex; i > -1; i--)
                {
                    remainingLength -= chunk._chunkSpans[i].Length;

                    if (remainingLength <= 0)
                    {
                        spanIndex = i;
                        charLength = -remainingLength;
                        return chunk;
                    }
                }

                chunk = chunk._chunkPrevious;
            }

            Debug.Assert(spanIndex >= 0, "Index not found in builder");
            Debug.Assert(charLength >= 0, "Index not found in builder");

            return chunk;
        }

        private void ClearSpans(CharSpan[] chunkSpans, int startIndex, int endIndex)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (chunkSpans[i].Length > 0)
                {
                    chunkSpans[i].Length = 0;
                    chunkSpans[i].Value = null;
                }
            }
        }

        internal struct CharSpan
        {
            public string Value { get; set; }
            public int StartPosition { get; set; }
            public int Length { get; set; }

            public CharSpan(string value, int startPosition, int length)
            {
                Value = value;
                StartPosition = startPosition;
                Length = length;
            }

            public void TrimStart()
            {
                if (Length == 0)
                {
                    return;
                }

                var currentStartPosition = StartPosition;

                while (currentStartPosition < StartPosition + Length)
                {
                    if (!char.IsWhiteSpace(Value[currentStartPosition]))
                    {
                        // Recalculate length based on the new position.
                        Length -= (currentStartPosition - StartPosition);
                        StartPosition = currentStartPosition;
                        return;
                    }

                    currentStartPosition++;
                }

                Length = 0;
                Value = null;
            }

            public void TrimEnd()
            {
                if (Length == 0)
                {
                    return;
                }

                var currentLength = Length;

                while (currentLength > 0)
                {
                    if (!char.IsWhiteSpace(Value[StartPosition + currentLength - 1]))
                    {
                        // We have reduced the length up to the point in the span that is not a whitespace.
                        Length = currentLength;
                        return;
                    }

                    currentLength--;
                }

                Length = 0;
                Value = null;
            }

            public bool StartsWith(char c)
            {
                if (Length == 0)
                {
                    return false;
                }

                return Value[StartPosition] == c;
            }

            public bool EndsWith(char c)
            {
                if (Length == 0)
                {
                    return false;
                }

                return Value[StartPosition + Length - 1] == c;
            }
        }

        internal struct ParamsArray
        {
            // Sentinel fixed-length arrays eliminate the need for a "count" field keeping this
            // struct down to just 4 fields. These are only used for their "Length" property,
            // that is, their elements are never set or referenced.
            private static readonly object[] oneArgArray = new object[1];
            private static readonly object[] twoArgArray = new object[2];
            private static readonly object[] threeArgArray = new object[3];

            private readonly object arg0;
            private readonly object arg1;
            private readonly object arg2;

            // After construction, the first three elements of this array will never be accessed
            // because the indexer will retrieve those values from arg0, arg1, and arg2.
            private readonly object[] args;

            public ParamsArray(object arg0)
            {
                this.arg0 = arg0;
                this.arg1 = null;
                this.arg2 = null;

                // Always assign this.args to make use of its "Length" property
                this.args = oneArgArray;
            }

            public ParamsArray(object arg0, object arg1)
            {
                this.arg0 = arg0;
                this.arg1 = arg1;
                this.arg2 = null;

                // Always assign this.args to make use of its "Length" property
                this.args = twoArgArray;
            }

            public ParamsArray(object arg0, object arg1, object arg2)
            {
                this.arg0 = arg0;
                this.arg1 = arg1;
                this.arg2 = arg2;

                // Always assign this.args to make use of its "Length" property
                this.args = threeArgArray;
            }

            public ParamsArray(object[] args)
            {
                int len = args.Length;
                this.arg0 = len > 0 ? args[0] : null;
                this.arg1 = len > 1 ? args[1] : null;
                this.arg2 = len > 2 ? args[2] : null;
                this.args = args;
            }

            public int Length
            {
                get { return this.args.Length; }
            }

            public object this[int index]
            {
                get { return index == 0 ? this.arg0 : GetAtSlow(index); }
            }

            private object GetAtSlow(int index)
            {
                if (index == 1)
                    return this.arg1;
                if (index == 2)
                    return this.arg2;
                return this.args[index];
            }
        }
    }
}
