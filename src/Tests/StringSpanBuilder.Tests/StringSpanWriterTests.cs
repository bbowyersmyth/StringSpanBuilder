// Copyright (c) Bruce Bowyer-Smyth. All rights reserved.
// Licensed under the MIT License. See the LICENSE.txt file in the project root for more information.

using Spans.Text.StringSpanBuilder;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Spans.IO.StringSpanWriter.Tests
{
    public static class StringSpanWriterTests
    {
        static int[] iArrInvalidValues = new int[] { -1, -2, -100, -1000, -10000, -100000, -1000000, -10000000, -100000000, -1000000000, int.MinValue, short.MinValue };
        static int[] iArrLargeValues = new int[] { int.MaxValue, int.MaxValue - 1, int.MaxValue / 2, int.MaxValue / 10, int.MaxValue / 100 };
        static int[] iArrValidValues = new int[] { 10000, 100000, int.MaxValue / 2000, int.MaxValue / 5000, short.MaxValue };

        private static char[] GetCharArray()
        {
            return new char[]{
            char.MinValue
            ,char.MaxValue
            ,'\t'
            ,' '
            ,'$'
            ,'@'
            ,'#'
            ,'\0'
            ,'\v'
            ,'\''
            ,'\u3190'
            ,'\uC3A0'
            ,'A'
            ,'5'
            ,'\uFE70'
            ,'-'
            ,';'
            ,'\u00E6'
        };
        }

        private static StringSpanBuilder GetSb()
        {
            var chArr = GetCharArray();
            var sb = new StringSpanBuilder(40);
            for (int i = 0; i < chArr.Length; i++)
                sb.Append(chArr[i].ToString());

            return sb;
        }

        [Fact]
        public static void Ctor()
        {
            StringSpanWriter sw = new StringSpanWriter();
            Assert.NotNull(sw);
        }

        [Fact]
        public static void CtorWithStringSpanBuilder()
        {
            var sb = GetSb();
            StringSpanWriter sw = new StringSpanWriter(GetSb());
            Assert.NotNull(sw);
            Assert.Equal(sb.Length, sw.GetStringSpanBuilder().Length);
        }

        [Fact]
        public static void CtorWithCultureInfo()
        {
            StringSpanWriter sw = new StringSpanWriter(new CultureInfo("en-gb"));
            Assert.NotNull(sw);

            Assert.Equal(new CultureInfo("en-gb"), sw.FormatProvider);
        }

        [Fact]
        public static void SimpleWriter()
        {
            var sw = new StringSpanWriter();
            sw.Write(4);
            var sb = sw.GetStringSpanBuilder();
            Assert.Equal("4", sb.ToString());
        }

        [Fact]
        public static void WriteArray()
        {
            var chArr = GetCharArray();
            StringSpanBuilder sb = GetSb();
            StringSpanWriter sw = new StringSpanWriter(sb);

            var sr = new StringReader(sw.GetStringSpanBuilder().ToString());

            for (int i = 0; i < chArr.Length; i++)
            {
                int tmp = sr.Read();
                Assert.Equal((int)chArr[i], tmp);
            }
        }

        [Fact]
        public static void CantWriteNullArray()
        {
            var sw = new StringSpanWriter();
            Assert.Throws<ArgumentNullException>(() => sw.Write(null, 0, 0));
        }

        [Fact]
        public static void CantWriteNegativeOffset()
        {
            var sw = new StringSpanWriter();
            Assert.Throws<ArgumentOutOfRangeException>(() => sw.Write(new char[0], -1, 0));
        }

        [Fact]
        public static void CantWriteNegativeCount()
        {
            var sw = new StringSpanWriter();
            Assert.Throws<ArgumentOutOfRangeException>(() => sw.Write(new char[0], 0, -1));
        }

        [Fact]
        public static void CantWriteIndexLargeValues()
        {
            var chArr = GetCharArray();
            for (int i = 0; i < iArrLargeValues.Length; i++)
            {
                StringSpanWriter sw = new StringSpanWriter();
                Assert.Throws<ArgumentException>(() => sw.Write(chArr, iArrLargeValues[i], chArr.Length));
            }
        }

        [Fact]
        public static void CantWriteCountLargeValues()
        {
            var chArr = GetCharArray();
            for (int i = 0; i < iArrLargeValues.Length; i++)
            {
                StringSpanWriter sw = new StringSpanWriter();
                Assert.Throws<ArgumentException>(() => sw.Write(chArr, 0, iArrLargeValues[i]));
            }
        }

        [Fact]
        public static void WriteWithOffset()
        {
            StringSpanWriter sw = new StringSpanWriter();
            StringReader sr;

            var chArr = GetCharArray();

            sw.Write(chArr, 2, 5);

            sr = new StringReader(sw.ToString());
            for (int i = 2; i < 7; i++)
            {
                int tmp = sr.Read();
                Assert.Equal((int)chArr[i], tmp);
            }
        }

        [Fact]
        public static void WriteWithLargeIndex()
        {
            for (int i = 0; i < iArrValidValues.Length; i++)
            {
                StringSpanBuilder sb = new StringSpanBuilder(int.MaxValue / 2000);
                StringSpanWriter sw = new StringSpanWriter(sb);

                var chArr = new char[int.MaxValue / 2000];
                for (int j = 0; j < chArr.Length; j++)
                    chArr[j] = (char)(j % 256);
                sw.Write(chArr, iArrValidValues[i] - 1, 1);

                string strTemp = sw.GetStringSpanBuilder().ToString();
                Assert.Equal(1, strTemp.Length);
            }
        }

        [Fact]
        public static void WriteWithLargeCount()
        {
            for (int i = 0; i < iArrValidValues.Length; i++)
            {
                StringSpanBuilder sb = new StringSpanBuilder(int.MaxValue / 2000);
                StringSpanWriter sw = new StringSpanWriter(sb);

                var chArr = new char[int.MaxValue / 2000];
                for (int j = 0; j < chArr.Length; j++)
                    chArr[j] = (char)(j % 256);

                sw.Write(chArr, 0, iArrValidValues[i]);

                string strTemp = sw.GetStringSpanBuilder().ToString();
                Assert.Equal(iArrValidValues[i], strTemp.Length);
            }
        }

        [Fact]
        public static void NewStringSpanWriterIsEmpty()
        {
            var sw = new StringSpanWriter();
            Assert.Equal(string.Empty, sw.ToString());
        }

        [Fact]
        public static void NewStringSpanWriterHasEmptyStringSpanBuilder()
        {
            var sw = new StringSpanWriter();
            Assert.Equal(string.Empty, sw.GetStringSpanBuilder().ToString());
        }

        [Fact]
        public static void ToStringReturnsWrittenData()
        {
            StringSpanBuilder sb = GetSb();
            StringSpanWriter sw = new StringSpanWriter(sb);

            sw.Write(sb.ToString());

            Assert.Equal(sb.ToString(), sw.ToString());
        }

        [Fact]
        public static void StringSpanBuilderHasCorrectData()
        {
            StringSpanBuilder sb = GetSb();
            StringSpanWriter sw = new StringSpanWriter(sb);

            sw.Write(sb.ToString());

            Assert.Equal(sb.ToString(), sw.GetStringSpanBuilder().ToString());
        }

        [Fact]
        public static void Disposed()
        {
            StringSpanWriter sw = new StringSpanWriter();
            sw.Dispose();
        }

        [Fact]
        public static async Task FlushAsyncWorks()
        {
            StringSpanBuilder sb = GetSb();
            StringSpanWriter sw = new StringSpanWriter(sb);

            sw.Write(sb.ToString());

            await sw.FlushAsync(); // I think this is a noop in this case

            Assert.Equal(sb.ToString(), sw.GetStringSpanBuilder().ToString());
        }

        [Fact]
        public static void MiscWrites()
        {
            var sw = new StringSpanWriter();
            sw.Write('H');
            sw.Write("ello World!");

            Assert.Equal("Hello World!", sw.ToString());
        }

        [Fact]
        public static async Task MiscWritesAsync()
        {
            var sw = new StringSpanWriter();
            await sw.WriteAsync('H');
            await sw.WriteAsync(new char[] { 'e', 'l', 'l', 'o', ' ' });
            await sw.WriteAsync("World!");

            Assert.Equal("Hello World!", sw.ToString());
        }

        [Fact]
        public static async Task MiscWriteLineAsync()
        {
            var sw = new StringSpanWriter();
            await sw.WriteLineAsync('H');
            await sw.WriteLineAsync(new char[] { 'e', 'l', 'l', 'o' });
            await sw.WriteLineAsync("World!");

            Assert.Equal(
                string.Format("H{0}ello{0}World!{0}", Environment.NewLine),
                sw.ToString());
        }

        [Fact]
        public static void GetEncoding()
        {
            var sw = new StringSpanWriter();
            Assert.Equal(Encoding.Unicode.WebName, sw.Encoding.WebName);
        }

        [Fact]
        public static void TestWriteMisc()
        {
            CultureInfo old = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo("en-US"); // floating-point formatting comparison depends on culture
            try
            {
                var sw = new StringSpanWriter();

                sw.Write(true);
                sw.Write((char)'a');
                sw.Write(new decimal(1234.01));
                sw.Write((double)3452342.01);
                sw.Write((int)23456);
                sw.Write((long)long.MinValue);
                sw.Write((float)1234.50f);
                sw.Write((uint)uint.MaxValue);
                sw.Write((ulong)ulong.MaxValue);

                Assert.Equal("Truea1234.013452342.0123456-92233720368547758081234.5429496729518446744073709551615", sw.ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = old;
            }
        }

        [Fact]
        public static void TestWriteObject()
        {
            var sw = new StringSpanWriter();
            sw.Write(new Object());
            Assert.Equal("System.Object", sw.ToString());
        }

        [Fact]
        public static void TestWriteLineMisc()
        {
            CultureInfo old = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo("en-US"); // floating-point formatting comparison depends on culture
            try
            {
                var sw = new StringSpanWriter();
                sw.WriteLine((bool)false);
                sw.WriteLine((char)'B');
                sw.WriteLine((int)987);
                sw.WriteLine((long)875634);
                sw.WriteLine((float)1.23457f);
                sw.WriteLine((uint)45634563);
                sw.WriteLine((ulong.MaxValue));

                Assert.Equal(
                    string.Format("False{0}B{0}987{0}875634{0}1.23457{0}45634563{0}18446744073709551615{0}", Environment.NewLine),
                    sw.ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = old;
            }
        }

        [Fact]
        public static void TestWriteLineObject()
        {
            var sw = new StringSpanWriter();
            sw.WriteLine(new Object());
            Assert.Equal("System.Object" + Environment.NewLine, sw.ToString());
        }

        [Fact]
        public static void TestWriteLineAsyncCharArray()
        {
            StringSpanWriter sw = new StringSpanWriter();
            sw.WriteLineAsync(new char[] { 'H', 'e', 'l', 'l', 'o' });

            Assert.Equal("Hello" + Environment.NewLine, sw.ToString());
        }
    }
}
