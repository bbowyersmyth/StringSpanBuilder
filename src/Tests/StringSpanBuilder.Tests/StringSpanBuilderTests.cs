// Copyright (c) Bruce Bowyer-Smyth. All rights reserved.
// Licensed under the MIT License. See the LICENSE.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace Spans.Text.StringSpanBuilder.Tests
{
    public static class StringSpanBuilderTests
    {
        private static readonly string s_chunkSplitSource = "ABC123XYZ";
        private static StringSpanBuilder StringSpanBuilderWithMultipleChunks() => new StringSpanBuilder(2).Append("ABC").Append("*123*", 1, 3).Append("XYZ");

        [Fact]
        public static void Ctor_Empty()
        {
            var builder = new StringSpanBuilder();
            Assert.Same(string.Empty, builder.ToString());
            Assert.Equal(0, builder.Length);
        }

        [Fact]
        public static void Ctor_Int()
        {
            var builder = new StringSpanBuilder(42);
            Assert.Same(string.Empty, builder.ToString());
            Assert.Equal(0, builder.Length);
        }

        [Fact]
        public static void Ctor_Int_NegativeCapacity_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>("capacity", () => new StringSpanBuilder(-1)); // Capacity < 0
        }

        [Theory]
        [InlineData("Hello")]
        [InlineData("")]
        [InlineData(null)]
        public static void Ctor_String(string value)
        {
            var builder = new StringSpanBuilder(value);

            string expected = value ?? "";
            Assert.Equal(expected, builder.ToString());
            Assert.Equal(expected.Length, builder.Length);
        }

        [Theory]
        [InlineData("Hello")]
        [InlineData("")]
        [InlineData(null)]
        public static void Ctor_String_Int(string value)
        {
            var builder = new StringSpanBuilder(value, 42);

            string expected = value ?? "";
            Assert.Equal(expected, builder.ToString());
            Assert.Equal(expected.Length, builder.Length);
        }

        [Fact]
        public static void Ctor_String_Int_NegativeCapacity_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>("capacity", () => new StringSpanBuilder("", -1)); // Capacity < 0
        }

        [Fact]
        public static void Length_Get_Set()
        {
            var builder = new StringSpanBuilder("Hello");

            builder.Length = 2;
            Assert.Equal(2, builder.Length);
            Assert.Equal("He", builder.ToString());
        }

        [Fact]
        public static void Length_Set_InvalidValue_ThrowsArgumentOutOfRangeException()
        {
            var builder = new StringSpanBuilder(10);
            builder.Append("Hello");

            Assert.Throws<ArgumentOutOfRangeException>("value", () => builder.Length = -1); // Value < 0
            Assert.Throws<ArgumentOutOfRangeException>("value", () => builder.Length = 6); // Value > existing value
        }

        [Theory]
        [InlineData("Hello", "abc", 0, 3, "Helloabc")]
        [InlineData("Hello", "def", 1, 2, "Helloef")]
        [InlineData("Hello", "def", 2, 1, "Hellof")]
        [InlineData("", "g", 0, 1, "g")]
        [InlineData("Hello", "g", 1, 0, "Hello")]
        [InlineData("Hello", "g", 0, 0, "Hello")]
        [InlineData("Hello", "", 0, 0, "Hello")]
        [InlineData("Hello", null, 0, 0, "Hello")]
        public static void Append_String(string original, string value, int startIndex, int count, string expected)
        {
            StringSpanBuilder builder;
            if (startIndex == 0 && count == (value?.Length ?? 0))
            {
                // Use Append(string)
                builder = new StringSpanBuilder(original);
                builder.Append(value);
                Assert.Equal(expected, builder.ToString());
            }
            // Use Append(string, int, int)
            builder = new StringSpanBuilder(original);
            builder.Append(value, startIndex, count);
            Assert.Equal(expected, builder.ToString());
        }

        [Fact]
        public static void Append_String_Invalid()
        {
            var builder = new StringSpanBuilder(5);
            builder.Append("Hello");

            Assert.Throws<ArgumentNullException>("value", () => builder.Append((string)null, 1, 1)); // Value is null, startIndex > 0 and count > 0

            Assert.Throws<ArgumentOutOfRangeException>("startIndex", () => builder.Append("", -1, 0)); // Start index < 0
            Assert.Throws<ArgumentOutOfRangeException>("length", () => builder.Append("", 0, -1)); // Count < 0

            Assert.Throws<ArgumentOutOfRangeException>("startIndex", () => builder.Append("hello", 5, 1)); // Start index + count > value.Length
            Assert.Throws<ArgumentOutOfRangeException>("startIndex", () => builder.Append("hello", 4, 2)); // Start index + count > value.Length
        }

        [Theory]
        [InlineData("Hello", "abc", 0, 3, "abcHello")]
        [InlineData("Hello", "def", 1, 2, "efHello")]
        [InlineData("Hello", "def", 2, 1, "fHello")]
        [InlineData("", "g", 0, 1, "g")]
        [InlineData("Hello", "g", 1, 0, "Hello")]
        [InlineData("Hello", "g", 0, 0, "Hello")]
        [InlineData("Hello", "", 0, 0, "Hello")]
        [InlineData("Hello", null, 0, 0, "Hello")]
        public static void Prepend_String(string original, string value, int startIndex, int count, string expected)
        {
            StringSpanBuilder builder;
            if (startIndex == 0 && count == (value?.Length ?? 0))
            {
                // Use Prepend(string)
                builder = new StringSpanBuilder(original);
                builder.Prepend(value);
                Assert.Equal(expected, builder.ToString());
            }
            // Use Prepend(string, int, int)
            builder = new StringSpanBuilder(original);
            builder.Prepend(value, startIndex, count);
            Assert.Equal(expected, builder.ToString());
        }

        [Fact]
        public static void Prepend_String_Invalid()
        {
            var builder = new StringSpanBuilder(5);
            builder.Append("Hello");

            Assert.Throws<ArgumentNullException>("value", () => builder.Prepend((string)null, 1, 1)); // Value is null, startIndex > 0 and count > 0

            Assert.Throws<ArgumentOutOfRangeException>("startIndex", () => builder.Prepend("", -1, 0)); // Start index < 0
            Assert.Throws<ArgumentOutOfRangeException>("length", () => builder.Prepend("", 0, -1)); // Count < 0

            Assert.Throws<ArgumentOutOfRangeException>("startIndex", () => builder.Prepend("hello", 5, 1)); // Start index + count > value.Length
            Assert.Throws<ArgumentOutOfRangeException>("startIndex", () => builder.Prepend("hello", 4, 2)); // Start index + count > value.Length
        }

        [Fact]
        public static void Prepend_String_FirstBlank()
        {
            var builder = new StringSpanBuilder();
            builder.Append("  ");
            builder.TrimStart();
            builder.Prepend("Hello");

            Assert.Equal("Hello", builder.ToString());
        }

        [Fact]
        public static void Prepend_String_WithExpand()
        {
            var builder = new StringSpanBuilder(1);
            builder.Append("Hello");
            builder.Prepend("ABC");
            builder.Prepend("123");

            Assert.Equal("123ABCHello", builder.ToString());
        }

        public static IEnumerable<object[]> AppendFormat_TestData()
        {
            yield return new object[] { "", null, "", new object[0], "" };
            yield return new object[] { "", null, ", ", new object[0], ", " };

            yield return new object[] { "Hello", null, ", Foo {0  }", new object[] { "Bar" }, "Hello, Foo Bar" }; // Ignores whitespace

            yield return new object[] { "Hello", null, ", Foo {0}", new object[] { "Bar" }, "Hello, Foo Bar" };
            yield return new object[] { "Hello", null, ", Foo {0} Baz {1}", new object[] { "Bar", "Foo" }, "Hello, Foo Bar Baz Foo" };
            yield return new object[] { "Hello", null, ", Foo {0} Baz {1} Bar {2}", new object[] { "Bar", "Foo", "Baz" }, "Hello, Foo Bar Baz Foo Bar Baz" };
            yield return new object[] { "Hello", null, ", Foo {0} Baz {1} Bar {2} Foo {3}", new object[] { "Bar", "Foo", "Baz", "Bar" }, "Hello, Foo Bar Baz Foo Bar Baz Foo Bar" };

            // Length is positive
            yield return new object[] { "Hello", null, ", Foo {0,2}", new object[] { "Bar" }, "Hello, Foo Bar" }; // MiValue's length > minimum length (so don't prepend whitespace)
            yield return new object[] { "Hello", null, ", Foo {0,3}", new object[] { "B" }, "Hello, Foo   B" }; // Value's length < minimum length (so prepend whitespace)            
            yield return new object[] { "Hello", null, ", Foo {0,     3}", new object[] { "B" }, "Hello, Foo   B" }; // Same as above, but verify AppendFormat ignores whitespace
            yield return new object[] { "Hello", null, ", Foo {0,0}", new object[] { "Bar" }, "Hello, Foo Bar" }; // Minimum length is 0

            // Length is negative
            yield return new object[] { "Hello", null, ", Foo {0,-2}", new object[] { "Bar" }, "Hello, Foo Bar" }; // Value's length > |minimum length| (so don't prepend whitespace)
            yield return new object[] { "Hello", null, ", Foo {0,-3}", new object[] { "B" }, "Hello, Foo B  " }; // Value's length < |minimum length| (so append whitespace)
            yield return new object[] { "Hello", null, ", Foo {0,     -3}", new object[] { "B" }, "Hello, Foo B  " }; // Same as above, but verify AppendFormat ignores whitespace
            yield return new object[] { "Hello", null, ", Foo {0,0}", new object[] { "Bar" }, "Hello, Foo Bar" }; // Minimum length is 0

            yield return new object[] { "Hello", null, ", Foo {0:D6}", new object[] { 1 }, "Hello, Foo 000001" }; // Custom format
            yield return new object[] { "Hello", null, ", Foo {0     :D6}", new object[] { 1 }, "Hello, Foo 000001" }; // Custom format with ignored whitespace
            yield return new object[] { "Hello", null, ", Foo {0:}", new object[] { 1 }, "Hello, Foo 1" }; // Missing custom format

            yield return new object[] { "Hello", null, ", Foo {0,9:D6}", new object[] { 1 }, "Hello, Foo    000001" }; // Positive minimum length and custom format
            yield return new object[] { "Hello", null, ", Foo {0,-9:D6}", new object[] { 1 }, "Hello, Foo 000001   " }; // Negative length and custom format

            yield return new object[] { "Hello", null, ", Foo {0:{{X}}Y{{Z}}} {0:X{{Y}}Z}", new object[] { 1 }, "Hello, Foo {X}Y{Z} X{Y}Z" }; // Custom format (with escaped curly braces)
            yield return new object[] { "Hello", null, ", Foo {{{0}", new object[] { 1 }, "Hello, Foo {1" }; // Escaped open curly braces
            yield return new object[] { "Hello", null, ", Foo }}{0}", new object[] { 1 }, "Hello, Foo }1" }; // Escaped closed curly braces
            yield return new object[] { "Hello", null, ", Foo {0} {{0}}", new object[] { 1 }, "Hello, Foo 1 {0}" }; // Escaped placeholder


            yield return new object[] { "Hello", null, ", Foo {0}", new object[] { null }, "Hello, Foo " }; // Values has null only
            yield return new object[] { "Hello", null, ", Foo {0} {1} {2}", new object[] { "Bar", null, "Baz" }, "Hello, Foo Bar  Baz" }; // Values has null

            yield return new object[] { "Hello", CultureInfo.InvariantCulture, ", Foo {0,9:D6}", new object[] { 1 }, "Hello, Foo    000001" }; // Positive minimum length, custom format and custom format provider

            yield return new object[] { "", new CustomFormatter(), "{0}", new object[] { 1.2 }, "abc" }; // Custom format provider
            yield return new object[] { "", new CustomFormatter(), "{0:0}", new object[] { 1.2 }, "abc" }; // Custom format provider
        }

        [Theory]
        [MemberData(nameof(AppendFormat_TestData))]
        public static void AppendFormat(string original, IFormatProvider provider, string format, object[] values, string expected)
        {
            StringSpanBuilder builder;
            if (values != null)
            {
                if (values.Length == 1)
                {
                    // Use AppendFormat(string, object) or AppendFormat(IFormatProvider, string, object)
                    if (provider == null)
                    {
                        // Use AppendFormat(string, object)
                        builder = new StringSpanBuilder(original);
                        builder.AppendFormat(format, values[0]);
                        Assert.Equal(expected, builder.ToString());
                    }
                    // Use AppendFormat(IFormatProvider, string, object)
                    builder = new StringSpanBuilder(original);
                    builder.AppendFormat(provider, format, values[0]);
                    Assert.Equal(expected, builder.ToString());
                }
                else if (values.Length == 2)
                {
                    // Use AppendFormat(string, object, object) or AppendFormat(IFormatProvider, string, object, object)
                    if (provider == null)
                    {
                        // Use AppendFormat(string, object, object)
                        builder = new StringSpanBuilder(original);
                        builder.AppendFormat(format, values[0], values[1]);
                        Assert.Equal(expected, builder.ToString());
                    }
                    // Use AppendFormat(IFormatProvider, string, object, object)
                    builder = new StringSpanBuilder(original);
                    builder.AppendFormat(provider, format, values[0], values[1]);
                    Assert.Equal(expected, builder.ToString());
                }
                else if (values.Length == 3)
                {
                    // Use AppendFormat(string, object, object, object) or AppendFormat(IFormatProvider, string, object, object, object)
                    if (provider == null)
                    {
                        // Use AppendFormat(string, object, object, object)
                        builder = new StringSpanBuilder(original);
                        builder.AppendFormat(format, values[0], values[1], values[2]);
                        Assert.Equal(expected, builder.ToString());
                    }
                    // Use AppendFormat(IFormatProvider, string, object, object, object)
                    builder = new StringSpanBuilder(original);
                    builder.AppendFormat(provider, format, values[0], values[1], values[2]);
                    Assert.Equal(expected, builder.ToString());
                }
            }
            // Use AppendFormat(string, object[]) or AppendFormat(IFormatProvider, string, object[])
            if (provider == null)
            {
                // Use AppendFormat(string, object[])
                builder = new StringSpanBuilder(original);
                builder.AppendFormat(format, values);
                Assert.Equal(expected, builder.ToString());
            }
            // Use AppendFormat(IFormatProvider, string, object[])
            builder = new StringSpanBuilder(original);
            builder.AppendFormat(provider, format, values);
            Assert.Equal(expected, builder.ToString());
        }

        [Fact]
        public static void AppendFormat_Invalid()
        {
            var builder = new StringSpanBuilder(0);
            builder.Append("Hello");

            IFormatProvider formatter = null;
            var obj1 = new object();
            var obj2 = new object();
            var obj3 = new object();
            var obj4 = new object();

            Assert.Throws<ArgumentNullException>("format", () => builder.AppendFormat(null, obj1)); // Format is null
            Assert.Throws<ArgumentNullException>("format", () => builder.AppendFormat(null, obj1, obj2, obj3)); // Format is null
            Assert.Throws<ArgumentNullException>("format", () => builder.AppendFormat(null, obj1, obj2, obj3)); // Format is null
            Assert.Throws<ArgumentNullException>("format", () => builder.AppendFormat(null, obj1, obj2, obj3, obj4)); // Format is null
            Assert.Throws<ArgumentNullException>("args", () => builder.AppendFormat("", null)); // Args is null
            Assert.Throws<ArgumentNullException>("format", () => builder.AppendFormat(null, (object[])null)); // Both format and args are null
            Assert.Throws<ArgumentNullException>("format", () => builder.AppendFormat(formatter, null, obj1)); // Format is null
            Assert.Throws<ArgumentNullException>("format", () => builder.AppendFormat(formatter, null, obj1, obj2)); // Format is null
            Assert.Throws<ArgumentNullException>("format", () => builder.AppendFormat(formatter, null, obj1, obj2, obj3)); // Format is null
            Assert.Throws<ArgumentNullException>("format", () => builder.AppendFormat(formatter, null, obj1, obj2, obj3, obj4)); // Format is null
            Assert.Throws<ArgumentNullException>("args", () => builder.AppendFormat(formatter, "", null)); // Args is null
            Assert.Throws<ArgumentNullException>("format", () => builder.AppendFormat(formatter, null, null)); // Both format and args are null

            Assert.Throws<FormatException>(() => builder.AppendFormat("{-1}", obj1)); // Format has value < 0
            Assert.Throws<FormatException>(() => builder.AppendFormat("{-1}", obj1, obj2)); // Format has value < 0
            Assert.Throws<FormatException>(() => builder.AppendFormat("{-1}", obj1, obj2, obj3)); // Format has value < 0
            Assert.Throws<FormatException>(() => builder.AppendFormat("{-1}", obj1, obj2, obj3, obj4)); // Format has value < 0
            Assert.Throws<FormatException>(() => builder.AppendFormat(formatter, "{-1}", obj1)); // Format has value < 0
            Assert.Throws<FormatException>(() => builder.AppendFormat(formatter, "{-1}", obj1, obj2)); // Format has value < 0
            Assert.Throws<FormatException>(() => builder.AppendFormat(formatter, "{-1}", obj1, obj2, obj3)); // Format has value < 0
            Assert.Throws<FormatException>(() => builder.AppendFormat(formatter, "{-1}", obj1, obj2, obj3, obj4)); // Format has value < 0

            Assert.Throws<FormatException>(() => builder.AppendFormat("{1}", obj1)); // Format has value >= 1
            Assert.Throws<FormatException>(() => builder.AppendFormat("{2}", obj1, obj2)); // Format has value >= 2
            Assert.Throws<FormatException>(() => builder.AppendFormat("{3}", obj1, obj2, obj3)); // Format has value >= 3
            Assert.Throws<FormatException>(() => builder.AppendFormat("{4}", obj1, obj2, obj3, obj4)); // Format has value >= 4
            Assert.Throws<FormatException>(() => builder.AppendFormat(formatter, "{1}", obj1)); // Format has value >= 1
            Assert.Throws<FormatException>(() => builder.AppendFormat(formatter, "{2}", obj1, obj2)); // Format has value >= 2
            Assert.Throws<FormatException>(() => builder.AppendFormat(formatter, "{3}", obj1, obj2, obj3)); // Format has value >= 3
            Assert.Throws<FormatException>(() => builder.AppendFormat(formatter, "{4}", obj1, obj2, obj3, obj4)); // Format has value >= 4

            Assert.Throws<FormatException>(() => builder.AppendFormat("{", "")); // Format has unescaped {
            Assert.Throws<FormatException>(() => builder.AppendFormat("{a", "")); // Format has unescaped {

            Assert.Throws<FormatException>(() => builder.AppendFormat("}", "")); // Format has unescaped }
            Assert.Throws<FormatException>(() => builder.AppendFormat("}a", "")); // Format has unescaped }

            Assert.Throws<FormatException>(() => builder.AppendFormat("{\0", "")); // Format has invalid character after {
            Assert.Throws<FormatException>(() => builder.AppendFormat("{a", "")); // Format has invalid character after {

            Assert.Throws<FormatException>(() => builder.AppendFormat("{0     ", "")); // Format with index and spaces is not closed

            Assert.Throws<FormatException>(() => builder.AppendFormat("{1000000", new string[10])); // Format index is too long
            Assert.Throws<FormatException>(() => builder.AppendFormat("{10000000}", new string[10])); // Format index is too long

            Assert.Throws<FormatException>(() => builder.AppendFormat("{0,", "")); // Format with comma is not closed
            Assert.Throws<FormatException>(() => builder.AppendFormat("{0,   ", "")); // Format with comma and spaces is not closed
            Assert.Throws<FormatException>(() => builder.AppendFormat("{0,-", "")); // Format with comma and minus sign is not closed

            Assert.Throws<FormatException>(() => builder.AppendFormat("{0,-\0", "")); // Format has invalid character after minus sign
            Assert.Throws<FormatException>(() => builder.AppendFormat("{0,-a", "")); // Format has invalid character after minus sign

            Assert.Throws<FormatException>(() => builder.AppendFormat("{0,1000000", new string[10])); // Format length is too long
            Assert.Throws<FormatException>(() => builder.AppendFormat("{0,10000000}", new string[10])); // Format length is too long

            Assert.Throws<FormatException>(() => builder.AppendFormat("{0:", new string[10])); // Format with colon is not closed
            Assert.Throws<FormatException>(() => builder.AppendFormat("{0:    ", new string[10])); // Format with colon and spaces is not closed

            Assert.Throws<FormatException>(() => builder.AppendFormat("{0:{", new string[10])); // Format with custom format contains unescaped {
        }

        public static IEnumerable<object[]> AppendLine_TestData()
        {
            yield return new object[] { "Hello", "abc", "Helloabc" + Environment.NewLine };
            yield return new object[] { "Hello", "", "Hello" + Environment.NewLine };
            yield return new object[] { "Hello", null, "Hello" + Environment.NewLine };
            yield return new object[] { "Hello", "!", "Hello!" + Environment.NewLine };
        }

        [Theory]
        [MemberData(nameof(AppendLine_TestData))]
        public static void AppendLine(string original, string value, string expected)
        {
            StringSpanBuilder builder;
            if (string.IsNullOrEmpty(value))
            {
                // Use AppendLine()
                builder = new StringSpanBuilder(original);
                builder.AppendLine();
                Assert.Equal(expected, builder.ToString());
            }
            // Use AppendLine(string)
            builder = new StringSpanBuilder(original);
            builder.AppendLine(value);
            Assert.Equal(expected, builder.ToString());
        }

        [Fact]
        public static void Clear()
        {
            var builder = new StringSpanBuilder("Hello");
            builder.Clear();
            Assert.Equal(0, builder.Length);
            Assert.Same(string.Empty, builder.ToString());
        }

        [Theory]
        [InlineData("Hello", "", "", "", "")]
        [InlineData("  Hello  ", "", "", "", "")]
        [InlineData("        Hello          ", "", "", "", "")]
        [InlineData("123", "  ", "Hello", "  ", "abc")]
        [InlineData("  ", "Hello", "  ", "", "")]
        [InlineData("      \t      ", "", "", "", "")]
        [InlineData("Hello", "  ", "", "", "")]
        [InlineData("", "", "", "  ", "Hello")]
        public static void Trim(string s1, string s2, string s3, string s4, string s5)
        {
            StringSpanBuilder ssb;
            System.Text.StringBuilder sb = new System.Text.StringBuilder(2);
            sb.Append(s1).Append(s2).Append(s3).Append(s4).Append(s5);
            string expected = sb.ToString();

            ssb = (new StringSpanBuilder(2)).Append(s1).Append(s2).Append(s3).Append(s4).Append(s5);
            Assert.Equal(expected.Trim(), ssb.Trim().ToString());

            ssb = (new StringSpanBuilder(2)).Append(s1).Append(s2).Append(s3).Append(s4).Append(s5);
            Assert.Equal(expected.TrimEnd(), ssb.TrimEnd().ToString());

            ssb = (new StringSpanBuilder(2)).Append(s1).Append(s2).Append(s3).Append(s4).Append(s5);
            Assert.Equal(expected.TrimStart(), ssb.TrimStart().ToString());
        }

        [Theory]
        [InlineData("Hello", 2, 3)]
        [InlineData("  Hello  ", 2, 5)]
        [InlineData("        Hello          ", 2, 12)]
        [InlineData("Hello  ", 0, 5)]
        public static void TrimWithSubtrings(string s, int startIndex, int length)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(2);
            sb.Append(s, startIndex, length);
            string expected = sb.ToString();

            StringSpanBuilder ssb = (new StringSpanBuilder(2)).Append(s, startIndex, length);
            Assert.Equal(expected.Trim(), ssb.Trim().ToString());

            ssb = (new StringSpanBuilder(2)).Append(s, startIndex, length);
            Assert.Equal(expected.TrimEnd(), ssb.TrimEnd().ToString());

            ssb = (new StringSpanBuilder(2)).Append(s, startIndex, length);
            Assert.Equal(expected.TrimStart(), ssb.TrimStart().ToString());
        }

        [Theory]
        [InlineData("Hello", 0, 5, "Hello")]
        [InlineData("Hello", 2, 3, "llo")]
        [InlineData("Hello", 2, 2, "ll")]
        [InlineData("Hello", 5, 0, "")]
        [InlineData("Hello", 4, 0, "")]
        [InlineData("Hello", 0, 0, "")]
        [InlineData("", 0, 0, "")]
        public static void ToString(string value, int startIndex, int length, string expected)
        {
            var builder = new StringSpanBuilder(value);
            if (startIndex == 0 && length == value.Length)
            {
                Assert.Equal(expected, builder.ToString());
            }
        }

        [Fact]
        public static void ToString_StringSpanBuilderWithMultipleChunks()
        {
            StringSpanBuilder builder = StringSpanBuilderWithMultipleChunks();
            Assert.Equal(s_chunkSplitSource, builder.ToString());
        }

        [Fact]
        public static void ToString_Corrupt()
        {
            var builder = new StringSpanBuilder(5);
            builder.Append("Hello");

            string origValue = builder._chunkSpans[0].Value;
            int origStartIndex = builder._chunkSpans[0].StartPosition;
            int origLength = builder._chunkSpans[0].Length;


            // Length or start index corruption
            builder._chunkSpans[0].Length = 6;
            Assert.Throws<ArgumentOutOfRangeException>(() => builder.ToString());

            builder._chunkSpans[0].Length = int.MaxValue;
            Assert.Throws<ArgumentOutOfRangeException>(() => builder.ToString());

            builder._chunkSpans[0].StartPosition = 4;
            builder._chunkSpans[0].Length = 3;
            Assert.Throws<ArgumentOutOfRangeException>(() => builder.ToString());

            builder._chunkSpans[0].StartPosition = -1;
            builder._chunkSpans[0].Length = origLength - 1;
            Assert.Throws<ArgumentOutOfRangeException>(() => builder.ToString());

            builder._chunkSpans[0].StartPosition = 6;
            builder._chunkSpans[0].Length = origLength;
            Assert.Throws<ArgumentOutOfRangeException>(() => builder.ToString());


            // Total length corruption
            builder._chunkSpans[0].Value = "Longer";
            builder._chunkSpans[0].StartPosition = 0;
            builder._chunkSpans[0].Length = 6;
            Assert.Throws<ArgumentOutOfRangeException>(() => builder.ToString());
        }

        [Theory]
        [InlineData("Hello", 0, new char[] { '\0', '\0', '\0', '\0', '\0' }, 0, 5, new char[] { 'H', 'e', 'l', 'l', 'o' })]
        [InlineData("Hello", 0, new char[] { '\0', '\0', '\0', '\0', '\0', '\0' }, 1, 5, new char[] { '\0', 'H', 'e', 'l', 'l', 'o' })]
        [InlineData("Hello", 0, new char[] { '\0', '\0', '\0', '\0' }, 0, 4, new char[] { 'H', 'e', 'l', 'l' })]
        [InlineData("Hello", 1, new char[] { '\0', '\0', '\0', '\0', '\0', '\0', '\0' }, 2, 4, new char[] { '\0', '\0', 'e', 'l', 'l', 'o', '\0' })]
        public static void CopyTo(string value, int sourceIndex, char[] destination, int destinationIndex, int count, char[] expected)
        {
            var builder = new StringSpanBuilder(value);
            builder.CopyTo(sourceIndex, destination, destinationIndex, count);
        }

        [Fact]
        public static void CopyTo_StringSpanBuilderWithMultipleChunks()
        {
            StringSpanBuilder builder = StringSpanBuilderWithMultipleChunks();

            char[] destination = new char[builder.Length];
            builder.CopyTo(0, destination, 0, destination.Length);
            Assert.Equal(s_chunkSplitSource.ToCharArray(), destination);

            destination = new char[builder.Length - 2];
            builder.CopyTo(1, destination, 0, destination.Length);
            Assert.Equal(s_chunkSplitSource.Substring(1, builder.Length - 2).ToCharArray(), destination);

            destination = new char[builder.Length - 4];
            builder.CopyTo(0, destination, 0, destination.Length);
            Assert.Equal(s_chunkSplitSource.Substring(0, builder.Length - 4).ToCharArray(), destination);

            destination = new char[1];
            builder.CopyTo(3, destination, 0, destination.Length);
            Assert.Equal(s_chunkSplitSource.Substring(3, 1).ToCharArray(), destination);
        }

        [Fact]
        public static void CopyTo_Invalid()
        {
            var builder = new StringSpanBuilder("Hello");
            Assert.Throws<ArgumentNullException>("destination", () => builder.CopyTo(0, null, 0, 0)); // Destination is null

            Assert.Throws<ArgumentOutOfRangeException>("sourceIndex", () => builder.CopyTo(-1, new char[10], 0, 0)); // Source index < 0
            Assert.Throws<ArgumentOutOfRangeException>("sourceIndex", () => builder.CopyTo(6, new char[10], 0, 0)); // Source index > builder.Length

            Assert.Throws<ArgumentOutOfRangeException>("destinationIndex", () => builder.CopyTo(0, new char[10], -1, 0)); // Destination index < 0
            Assert.Throws<ArgumentOutOfRangeException>("count", () => builder.CopyTo(0, new char[10], 0, -1)); // Count < 0

            Assert.Throws<ArgumentException>(null, () => builder.CopyTo(5, new char[10], 0, 1)); // Source index + count > builder.Length
            Assert.Throws<ArgumentException>(null, () => builder.CopyTo(4, new char[10], 0, 2)); // Source index + count > builder.Length

            Assert.Throws<ArgumentException>(null, () => builder.CopyTo(0, new char[10], 10, 1)); // Destination index + count > destinationArray.Length
            Assert.Throws<ArgumentException>(null, () => builder.CopyTo(0, new char[10], 9, 2)); // Destination index + count > destinationArray.Length
        }

        [Fact]
        public static void CopyTo_Corrupt()
        {
            var builder = new StringSpanBuilder(5);
            int count = 7;
            char[] destination = new char[count];
            builder.Append("Hello").Append("Padding");

            string origValue = builder._chunkSpans[0].Value;
            int origStartIndex = builder._chunkSpans[0].StartPosition;
            int origLength = builder._chunkSpans[0].Length;


            // Length or start index corruption
            builder._chunkSpans[0].Length = 6;
            Assert.Throws<ArgumentOutOfRangeException>(() => builder.CopyTo(0, destination, 0, count));

            builder._chunkSpans[0].Length = int.MaxValue;
            Assert.Throws<ArgumentOutOfRangeException>(() => builder.CopyTo(0, destination, 0, count));

            builder._chunkSpans[0].StartPosition = 4;
            builder._chunkSpans[0].Length = 3;
            Assert.Throws<ArgumentOutOfRangeException>(() => builder.CopyTo(0, destination, 0, count));

            builder._chunkSpans[0].StartPosition = -1;
            builder._chunkSpans[0].Length = origLength - 1;
            Assert.Throws<ArgumentOutOfRangeException>(() => builder.CopyTo(0, destination, 0, count));

            builder._chunkSpans[0].StartPosition = 6;
            builder._chunkSpans[0].Length = origLength;
            Assert.Throws<ArgumentOutOfRangeException>(() => builder.CopyTo(0, destination, 0, count));
        }

        [Theory]
        [InlineData("Hello", 0, 5, 'H', true)]
        [InlineData("Hello", 1, 3, 'H', false)]
        [InlineData("Hello", 1, 3, 'e', true)]
        [InlineData("Hello", 0, 5, "h", false)]
        [InlineData("", 0, 0, "h", false)]
        public static void StartsWith(string s, int startPosition, int length, char value, bool expected)
        {
            var builder = new StringSpanBuilder();
            builder.Append(s, startPosition, length);

            Assert.Equal(expected, builder.StartsWith(value));
        }

        [Theory]
        [InlineData("Hello", 0, 5, 'o', true)]
        [InlineData("Hello", 1, 3, 'o', false)]
        [InlineData("Hello", 1, 3, 'l', true)]
        [InlineData("Hello", 0, 5, "O", false)]
        [InlineData("", 0, 0, "o", false)]
        public static void EndssWith(string s, int startPosition, int length, char value, bool expected)
        {
            var builder = new StringSpanBuilder();
            builder.Append(s, startPosition, length);

            Assert.Equal(expected, builder.EndsWith(value));
        }

        public class CustomFormatter : ICustomFormatter, IFormatProvider
        {
            public string Format(string format, object arg, IFormatProvider formatProvider) => "abc";
            public object GetFormat(Type formatType) => this;
        }
    }
}
