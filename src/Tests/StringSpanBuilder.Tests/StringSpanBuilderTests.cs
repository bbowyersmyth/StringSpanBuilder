// Copyright (c) Bruce Bowyer-Smyth. All rights reserved.
// Licensed under the MIT License. See the LICENSE.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using Xunit;

namespace Spans.Text.StringSpanBuilder.Tests
{
    public static class StringSpanBuilderTests
    {
        private static StringSpanBuilder StringSpanBuilderWithMultipleChunks() => new StringSpanBuilder(2).Append("ABC").Append("123").Append("XYZ");

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
            Assert.Equal("ABC123XYZ", builder.ToString());
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
            Assert.Throws<IndexOutOfRangeException>(() => builder.ToString());

            builder._chunkSpans[0].Length = int.MaxValue;
            Assert.Throws<IndexOutOfRangeException>(() => builder.ToString());

            builder._chunkSpans[0].StartPosition = 4;
            builder._chunkSpans[0].Length = 3;
            Assert.Throws<IndexOutOfRangeException>(() => builder.ToString());

            builder._chunkSpans[0].StartPosition = -1;
            builder._chunkSpans[0].Length = origLength - 1;
            Assert.Throws<IndexOutOfRangeException>(() => builder.ToString());

            builder._chunkSpans[0].StartPosition = 6;
            builder._chunkSpans[0].Length = origLength;
            Assert.Throws<IndexOutOfRangeException>(() => builder.ToString());


            // Total length corruption
            builder._chunkSpans[0].Value = "Longer";
            builder._chunkSpans[0].StartPosition = 0;
            builder._chunkSpans[0].Length = 6;
            Assert.Throws<IndexOutOfRangeException>(() => builder.ToString());
        }
    }
}
