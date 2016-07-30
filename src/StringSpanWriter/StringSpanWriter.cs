// Copyright (c) Bruce Bowyer-Smyth. All rights reserved.
// Licensed under the MIT License. See the LICENSE.txt file in the project root for more information.

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Spans.Text.StringSpanBuilder;

namespace Spans.IO.StringSpanWriter
{
    /// <summary>
    /// Implements a <see cref="T:System.IO.TextWriter" /> for writing information to a string. The information is stored in an underlying <see cref="T:Spans.Text.StringSpanBuilder" />.
    /// </summary>
    public class StringSpanWriter : TextWriter
    {
        private static volatile UnicodeEncoding _encoding = null;
        private StringSpanBuilder _sb;
        private bool _isOpen;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:IO.StringSpanWriter" /> class.
        /// </summary>
        public StringSpanWriter()
            : this(new StringSpanBuilder(), CultureInfo.CurrentCulture)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:IO.StringSpanWriter" /> class with the specified format control.
        /// </summary>
        /// <param name="formatProvider">An <see cref="T:System.IFormatProvider" /> object that controls formatting. </param>
        public StringSpanWriter(IFormatProvider formatProvider)
            : this(new StringSpanBuilder(), formatProvider)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:IO.StringSpanWriter" /> class that writes to the specified <see cref="T:Spans.Text.StringSpanBuilder" />.
        /// </summary>
        /// <param name="sb">The <see cref="T:Spans.Text.StringSpanBuilder" /> object to write to. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="sb" /> is null. </exception>
        public StringSpanWriter(StringSpanBuilder sb)
            : this(sb, CultureInfo.CurrentCulture)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:IO.StringSpanWriter" /> class that writes to the specified <see cref="T:Spans.Text.StringSpanBuilder" /> 
        /// and has the specified format provider.
        /// </summary>
        /// <param name="sb">The <see cref="T:Spans.Text.StringSpanBuilder" /> object to write to. </param>
        /// <param name="formatProvider">An <see cref="T:System.IFormatProvider" /> object that controls formatting. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="sb" /> is null. </exception>
        public StringSpanWriter(StringSpanBuilder sb, IFormatProvider formatProvider)
            : base(formatProvider)
        {
            if (sb == null)
            {
                throw new ArgumentNullException(nameof(sb), Resources.Strings.ArgumentNull_Buffer);
            }

            _sb = sb;
            _isOpen = true;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:IO.StringSpanWriter" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
        protected override void Dispose(bool disposing)
        {
            // Do not destroy _sb, so that we can extract this after we are
            // done writing (similar to MemoryStream's GetBuffer & ToArray methods)
            _isOpen = false;
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets the <see cref="T:System.Text.Encoding" /> in which the output is written.
        /// </summary>
        /// <returns>The Encoding in which the output is written.</returns>
        public override Encoding Encoding
        {
            get
            {
                if (_encoding == null)
                {
                    _encoding = new UnicodeEncoding(false, false);
                }
                return _encoding;
            }
        }

        /// <summary>
        /// Returns the underlying <see cref="T:Spans.Text.StringSpanBuilder" />.
        /// </summary>
        /// <returns>The underlying StringSpanBuilder.</returns>
        public virtual StringSpanBuilder GetStringSpanBuilder()
        {
            return _sb;
        }

        /// <summary>
        /// Writes a character.
        /// </summary>
        /// <param name="value">The character to write. </param>
        /// <exception cref="T:System.ObjectDisposedException">The writer is closed. </exception>
        public override void Write(char value)
        {
            if (!_isOpen)
            {
                throw new ObjectDisposedException(null, Resources.Strings.ObjectDisposed_WriterClosed);
            }

            switch (value)
            {
                case '\n':
                    _sb.Append("\n");
                    break;

                case '\r':
                    _sb.Append("\r");
                    break;

                case ' ':
                    _sb.Append(" ");
                    break;

                case '"':
                    _sb.Append("\"");
                    break;

                case '\'':
                    _sb.Append("'");
                    break;

                case '(':
                    _sb.Append("(");
                    break;

                case ')':
                    _sb.Append(")");
                    break;

                case ',':
                    _sb.Append(",");
                    break;

                case '/':
                    _sb.Append("/");
                    break;

                case '0':
                    _sb.Append("0");
                    break;

                case '1':
                    _sb.Append("1");
                    break;

                case '2':
                    _sb.Append("2");
                    break;

                case '3':
                    _sb.Append("3");
                    break;

                case '4':
                    _sb.Append("4");
                    break;

                case '5':
                    _sb.Append("5");
                    break;

                case '6':
                    _sb.Append("6");
                    break;

                case '7':
                    _sb.Append("7");
                    break;

                case '8':
                    _sb.Append("8");
                    break;

                case '9':
                    _sb.Append("9");
                    break;

                case ':':
                    _sb.Append(":");
                    break;

                case ';':
                    _sb.Append(";");
                    break;

                case '<':
                    _sb.Append("<");
                    break;

                case '=':
                    _sb.Append("=");
                    break;

                case '>':
                    _sb.Append(">");
                    break;

                case '[':
                    _sb.Append("[");
                    break;

                case ']':
                    _sb.Append("]");
                    break;

                case '{':
                    _sb.Append("{");
                    break;

                case '}':
                    _sb.Append("}");
                    break;

                default:
                    _sb.Append(value.ToString());
                    break;
            }
        }

        /// <summary>
        /// WARNING: Writes a subarray of characters. A new string will be created pre call to this method.
        /// Recommend using Write(new string(buffer, index, count) to make this obvious when reading.
        /// </summary>
        /// <param name="buffer">The character array to write data from. </param>
        /// <param name="index">The position in the buffer at which to start reading data.</param>
        /// <param name="count">The maximum number of characters to write. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" /> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> or <paramref name="count" /> is negative. </exception>
        /// <exception cref="T:System.ArgumentException">(<paramref name="index" /> + <paramref name="count" />)&gt; <paramref name="buffer" />. Length. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The writer is closed. </exception>
        public override void Write(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer), Resources.Strings.ArgumentNull_Buffer);
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), Resources.Strings.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), Resources.Strings.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (buffer.Length - index < count)
            {
                throw new ArgumentException(Resources.Strings.Argument_InvalidOffLen);
            }

            if (!_isOpen)
            {
                throw new ObjectDisposedException(null, Resources.Strings.ObjectDisposed_WriterClosed);
            }

            _sb.Append(new string(buffer, index, count));
        }

        /// <summary>
        /// Writes a string.
        /// </summary>
        /// <param name="value">The string to write. </param>
        /// <exception cref="T:System.ObjectDisposedException">The writer is closed. </exception>
        public override void Write(string value)
        {
            if (!_isOpen)
            {
                throw new ObjectDisposedException(null, Resources.Strings.ObjectDisposed_WriterClosed);
            }

            if (value != null)
            {
                _sb.Append(value);
            }
        }

        /// <summary>
        /// Writes a line terminator.
        /// </summary>
        /// <exception cref="T:System.ObjectDisposedException">The writer is closed.</exception>
        public override void WriteLine()
        {
            if (CoreNewLine.Length == 2)
            {
                if (CoreNewLine[0] == '\r' && CoreNewLine[1] == '\n')
                {
                    Write("\r\n");
                    return;
                }
            }
            else if (CoreNewLine.Length == 1)
            {
                if (CoreNewLine[0] == '\n')
                {
                    Write("\n");
                    return;
                }

                if (CoreNewLine[0] == '\r')
                {
                    Write("\r");
                    return;
                }
            }

            Write(CoreNewLine);
        }

        /// <summary>
        /// Writes a character asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <param name="value">The character to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The writer is closed.</exception>
        public override Task WriteAsync(char value)
        {
            Write(value);
            return Task.CompletedTask;
        }

        /// <summary>Writes a string asynchronously.</summary>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <param name="value">The string to write. If <paramref name="value" /> is null, nothing is written to the text stream.</param>
        /// <exception cref="T:System.ObjectDisposedException">The writer is closed.</exception>
        public override Task WriteAsync(string value)
        {
            Write(value);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Writes a subarray of characters asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <param name="buffer">The character array to write data from.</param>
        /// <param name="index">The position in the buffer at which to start reading data.</param>
        /// <param name="count">The maximum number of characters to write.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">The <paramref name="index" /> plus <paramref name="count" /> is greater than the buffer length.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> or <paramref name="count" /> is negative.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The writer is closed.</exception>
        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            Write(buffer, index, count);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Writes a character followed by a line terminator asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <param name="value">The character to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The writer is closed.</exception>
        public override Task WriteLineAsync(char value)
        {
            WriteLine(value);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Writes a string followed by a line terminator asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <param name="value">The string to write. If the value is null, only a line terminator is written.</param>
        /// <exception cref="T:System.ObjectDisposedException">The writer is closed.</exception>
        public override Task WriteLineAsync(String value)
        {
            WriteLine(value);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Writes a subarray of characters followed by a line terminator asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <param name="buffer">The character array to write data from.</param>
        /// <param name="index">The position in the buffer at which to start reading data.</param>
        /// <param name="count">The maximum number of characters to write. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">The <paramref name="index" /> plus <paramref name="count" /> is greater than the buffer length.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> or <paramref name="count" /> is negative.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The writer is closed.</exception>
        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            WriteLine(buffer, index, count);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Asynchronously clears all buffers for the current writer and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        public override Task FlushAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a string containing the characters written to the current StringSpanWriter so far.
        /// </summary>
        /// <returns>The string containing the characters written to the current StringSpanWriter.</returns>
        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}
