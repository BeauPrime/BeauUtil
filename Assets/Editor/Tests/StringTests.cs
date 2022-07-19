using System;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using BeauUtil.Variants;
using System.IO;

namespace BeauUtil.UnitTests
{
    static public class StringTests
    {
        public const string UnescapedString = "This is a \"string\" that requires some escaped characters" +
            "\nIt also has some newlines";
        public const string EscapedString = "This is a \\\"string\\\" that requires some escaped characters" +
            "\\nIt also has some newlines";
        public const string CSVEscapedString = "This is a \"\"string\"\" that requires some escaped characters" +
            "\\nIt also has some newlines";

        public const string CSVString = "Test,\"\",\"Test, with Comma\",\"Quoted \"\"Test\"\", with Comma\"";
        public const string MultiLineString = "This\nHas\n\r\nMultiple Lines";

        private static readonly string[] IntParseStrings = new string[] { "0", "255", "333", "-1", "-130", "-64", "", "not", "0+", "10000000", "--555", "-66666", "+6234", "5555555555555555555555", "83987192837192837192837912873918273918273", "0x000442F", "0x2371", "0x00000000", "0x4E423ab" };
        private static readonly string[] FloatParseStrings = new string[] { "0.5", ".2", "1.", "NaN", "naN", "Infinity", "infinity", "-Infinity", "-infinity", "5.23123123", "66.2317075189723", "-25555.30239817239", "4.25e+64" };

        [Test]
        static public void CanEscapeString()
        {
            string firstString = UnescapedString;
            string escaped = StringUtils.Escape(firstString);

            Assert.AreEqual(escaped, EscapedString);
        }

        [Test]
        static public void CanUnescapeString()
        {
            string firstString = EscapedString;
            string unescaped = StringUtils.Unescape(firstString);

            Assert.AreEqual(UnescapedString, unescaped);
        }

        [Test]
        static public void CanEscapeCSV()
        {
            string firstString = UnescapedString;
            string escaped = StringUtils.Escape(firstString, StringUtils.CSV.Escaper.Instance);

            Assert.AreEqual(CSVEscapedString, escaped);
        }

        [Test]
        static public void CanUnescapeCSV()
        {
            string firstString = CSVEscapedString;
            string unescaped = StringUtils.Unescape(firstString, StringUtils.CSV.Escaper.Instance);

            Assert.AreEqual(UnescapedString, unescaped);
        }

        [Test]
        static public void CanSplitCSV()
        {
            string firstString = CSVString;
            StringSlice[] split = StringSlice.Split(firstString, new StringUtils.CSV.Splitter(true), StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(3, split.Length);
            Assert.AreEqual("Test", split[0].ToString());
            Assert.AreEqual("Test, with Comma", split[1].ToString());
            Assert.AreEqual("Quoted \"Test\", with Comma", split[2].ToString());
        }

        [Test]
        static public void CanSplitOnLineBreak()
        {
            string firstString = MultiLineString;
            StringSlice[] split = StringSlice.Split(firstString, new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(3, split.Length);

            split = StringSlice.Split(firstString, new char[] { '\n', '\r' }, StringSplitOptions.None);

            Assert.AreEqual(5, split.Length);
        }

        [Test]
        static public void CanTrimStart()
        {
            StringSlice firstString = " \nTrim Me";
            StringSlice trimmed = firstString.TrimStart();

            Assert.AreEqual("Trim Me", trimmed.ToString());
        }

        [Test]
        static public void CanTrimEnd()
        {
            StringSlice firstString = "Trim Me \r";
            StringSlice trimmed = firstString.TrimEnd();

            Assert.AreEqual("Trim Me", trimmed.ToString());
        }

        [Test]
        static public void CanTrimBoth()
        {
            StringSlice firstString = "\n \tTrim Me\t\n";
            StringSlice trimmed = firstString.Trim();

            Assert.AreEqual("Trim Me", trimmed.ToString());
        }

        [Test]
        static public void CanMatchWildcardStrings()
        {
            string firstString = "Some things are better left unsaid.";

            Assert.True(StringUtils.WildcardMatch(firstString, "*"), "Single wildcard match does not work");
            Assert.True(StringUtils.WildcardMatch(firstString, "**"), "All wildcard match does not work");
            Assert.False(StringUtils.WildcardMatch(firstString, "unsaid"), "Non-wildcard match does not work");
            Assert.True(StringUtils.WildcardMatch(firstString, "*unsaid."), "Cannot test leading wildcard");
            Assert.True(StringUtils.WildcardMatch(firstString, "Some*"), "Cannot test trailing wildcard");
            Assert.True(StringUtils.WildcardMatch(firstString, "* things*"), "Cannot test leading and trailing wildcard combination");
            Assert.False(StringUtils.WildcardMatch(firstString, "* things w*"), "Cannot test leading and trailing wildcard combination");
        }

        [Test]
        static public void CanFlushStringBuilder()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("abcd").Append("efg");
            
            string flushed = builder.Flush();
            Assert.AreEqual("abcdefg", flushed);
            Assert.LessOrEqual(builder.Length, 0);
        }

        [Test]
        static public void CanSplitArgsWithVaradic()
        {
            StringSlice args = "Some args, \"this is a cool thing\", player == true, whatever == 0, this >= 2";
            StringUtils.ArgsList.Splitter argsSplitter = new StringUtils.ArgsList.Splitter(',', false, false);
            StringSlice[] split = args.Split(argsSplitter, new StringSliceOptions(StringSplitOptions.None, 3));
            Assert.True(split.Length == 3, "Bounded string split doesn't work");
        }

        [Test]
        static public void TestStringHash32()
        {
            StringHashing.ClearReverseLookup();

            int collisionCount = 0;
            StringHashing.SetOnCollision((a, b, s, h) => collisionCount++);

            for(int i = -1000; i <= 1000; ++i)
            {
                new StringHash32(i.ToString());
            }

            string allText = File.ReadAllText("Assets/Editor/words.txt");
            foreach(var word in StringSlice.EnumeratedSplit(allText, new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                word.Hash32();
            }

            if (collisionCount > 0)
                Debug.LogErrorFormat("[StringTests] {0} collisions with hash size 32", collisionCount);

            StringHashing.SetOnCollision(null);
        }

        [Test]
        static public void TestStringHash64()
        {
            StringHashing.ClearReverseLookup();

            int collisionCount = 0;
            StringHashing.SetOnCollision((a, b, s, h) => collisionCount++);

            for(int i = -1000; i <= 1000; ++i)
            {
                new StringHash64(i.ToString());
            }

            string allText = File.ReadAllText("Assets/Editor/words.txt");
            foreach(var word in StringSlice.EnumeratedSplit(allText, new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                word.Hash64();
            }

            if (collisionCount > 0)
                Debug.LogErrorFormat("[StringTests] {0} collisions with hash size 16", collisionCount);

            StringHashing.SetOnCollision(null);
        }

        [Test]
        static public void CanParseBools()
        {
            TestParser<bool>(bool.TryParse, StringParser.TryParseBool,
                "true", "false", "", "True", "False", "TrUe", "fAlSe");
        }

        [Test]
        static public void CanParseBytes()
        {
            TestParser<byte>(byte.TryParse, StringParser.TryParseByte, IntParseStrings);
            TestParser<sbyte>(sbyte.TryParse, StringParser.TryParseSByte, IntParseStrings);
        }

        [Test]
        static public void CanParseShorts()
        {
            TestParser<short>(short.TryParse, StringParser.TryParseShort, IntParseStrings);
            TestParser<ushort>(ushort.TryParse, StringParser.TryParseUShort, IntParseStrings);
        }

        [Test]
        static public void CanParseInts()
        {
            TestParser<int>(int.TryParse, StringParser.TryParseInt, IntParseStrings);
            TestParser<uint>(uint.TryParse, StringParser.TryParseUInt, IntParseStrings);
        }

        [Test]
        static public void CanParseLongs()
        {
            TestParser<long>(long.TryParse, StringParser.TryParseLong, IntParseStrings);
            TestParser<ulong>(ulong.TryParse, StringParser.TryParseULong, IntParseStrings);
        }

        [Test]
        static public void CanParseVariants()
        {
            TestParser<Variant>(Variant.TryParse, IntParseStrings);
        }

        [Test]
        static public void CanParseFloatsAsIntegers()
        {
            TestParserFloat(float.TryParse, StringParser.TryParseFloat, IntParseStrings);
            TestParserDouble(double.TryParse, StringParser.TryParseDouble, IntParseStrings);
        }

        [Test]
        static public void CanParseFloats()
        {
            TestParserFloat(float.TryParse, StringParser.TryParseFloat, FloatParseStrings);
            TestParserDouble(double.TryParse, StringParser.TryParseDouble, FloatParseStrings);
        }

        static private void TestParser<T>(StringParseDelegate<T> inStringParse, StringSliceParseDelegate<T> inSliceParse, params string[] inStrings)
        {
            foreach(var str in inStrings)
            {
                T valFromString, valFromSlice;
                bool bFromString = inStringParse(str, out valFromString);
                bool bFromSlice = inSliceParse(str, out valFromSlice);

                Debug.LogFormat("parsed '{0}' to {1}, got {2}:{3} and {4}:{5}", str, typeof(T).Name, bFromString, valFromString, bFromSlice, valFromSlice);

                Assert.AreEqual(bFromString, bFromSlice, "Success<{0}> from slice {1} is different from system {2} for '{3}'", typeof(T).Name, bFromSlice, bFromString, str);
                Assert.AreEqual(valFromString, valFromSlice, "Result<{0}> from slice {1} is different from system {2} for '{3}'", typeof(T).Name, valFromSlice, valFromString, str);
            }
        }

        static private void TestParser<T>(StringSliceParseDelegate<T> inSliceParse, params string[] inStrings)
        {
            foreach(var str in inStrings)
            {
                T valFromSlice;
                bool bFromSlice = inSliceParse(str, out valFromSlice);

                Debug.LogFormat("parsed '{0}' to {1}, got {2}:{3}", str, typeof(T).Name, bFromSlice, valFromSlice);
            }
        }

        static private void TestParserFloat(StringParseDelegate<float> inStringParse, StringSliceParseDelegate<float> inSliceParse, params string[] inStrings)
        {
            foreach(var str in inStrings)
            {
                float valFromString, valFromSlice;
                bool bFromString = inStringParse(str, out valFromString);
                bool bFromSlice = inSliceParse(str, out valFromSlice);

                Debug.LogFormat("parsed '{0}' to {1}, got {2}:{3} and {4}:{5}", str, typeof(float).Name, bFromString, valFromString, bFromSlice, valFromSlice);

                Assert.AreEqual(bFromString, bFromSlice, "Success<{0}> from slice {1} is different from system {2} for '{3}'", typeof(float).Name, bFromSlice, bFromString, str);
                Assert.AreEqual(valFromString, valFromSlice, "Result<{0}> from slice {1} is different from system {2} for '{3}'", typeof(float).Name, valFromSlice, valFromString, str);
            }
        }

        static private void TestParserDouble(StringParseDelegate<double> inStringParse, StringSliceParseDelegate<double> inSliceParse, params string[] inStrings)
        {
            foreach(var str in inStrings)
            {
                double valFromString, valFromSlice;
                bool bFromString = inStringParse(str, out valFromString);
                bool bFromSlice = inSliceParse(str, out valFromSlice);

                Debug.LogFormat("parsed '{0}' to {1}, got {2}:{3} and {4}:{5}", str, typeof(double).Name, bFromString, valFromString, bFromSlice, valFromSlice);

                Assert.AreEqual(bFromString, bFromSlice, "Success<{0}> from slice {1} is different from system {2} for '{3}'", typeof(double).Name, bFromSlice, bFromString, str);
                Assert.AreEqual(valFromString, valFromSlice, "Result<{0}> from slice {1} is different from system {2} for '{3}'", typeof(double).Name, valFromSlice, valFromString, str);
            }
        }

        private delegate bool StringParseDelegate<T>(string arg0, out T out0);
        private delegate bool StringSliceParseDelegate<T>(StringSlice arg0, out T out0);
    }
}