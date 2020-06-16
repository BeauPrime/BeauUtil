using System;
using System.Text;
using NUnit.Framework;
using UnityEngine;

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
    }
}