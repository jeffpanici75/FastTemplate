//
// FastTemplate
//
// The MIT License (MIT)
//
// Copyright (c) 2014 Jeff Panici

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using NUnit.Framework;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Test
{
    [TestFixture]
    public class SimpleTest
    {
        public static TemplateDictionary State()
        {
            return new TemplateDictionary
            {
                {
                    "foo",
                    "~`!1@2#3$4%5^6&7*8(9)0_-+={[}]:;\"\"<,<.?//*-+.a string pointed to by foo"
                },
                {
                    "bar",
                    "~`!1@2#3$4%5^6&7*8(9)0_-+={[}]:;\"\"<,<.?//*-+.a string pointed to by bar"
                },
                {
                    "bing",
                    "~`!1@2#3$4%5^6&7*8(9)0_-+={[}]:;\"\"<,<.?//*-+.a string pointed to by bar"
                },
            };
        }

        public static string ComplexPadding()
        {
            return "\\$\\#\\@\"~`!1\\$\\#\\@\\@2\\#3\\$4%5^6&7*8(9)0_-+={[}]:;\"\"<,<.?//*-+.\"\\$\\#\\@";
        }

        public static string ComplexResult()
        {
            return "$#@\"~`!1$#@@2#3$4%5^6&7*8(9)0_-+={[}]:;\"\"<,<.?//*-+.\"$#@";
        }

        [Test]
        public void DecimalTypeCheckTest()
        {
            Func<object, string> f = obj => obj.GetType().Name;

            const string t = "#set( $d = 1.0M) $getType($d)";

            var s = new TemplateDictionary
            {
                {"getType", f}
            };

            var result1 = Template.ImmediateApply(s, t);
            Assert.IsFalse(result1.Errors.ContainsError());
            Assert.IsFalse(result1.Errors.ContainsWarning());
            Assert.AreEqual("Decimal", result1.Output.Trim());

            var result2 = Template.CompileAndRun("test", t, s);
            Assert.IsFalse(result2.Errors.ContainsError());
            Assert.IsFalse(result2.Errors.ContainsWarning());
            Assert.AreEqual("Decimal", result2.Output.Trim());
        }

        [Test]
        public void DoubleTypeCheckTest()
        {
            Func<object, string> f = obj => obj.GetType().Name;

            const string t = "#set( $d = 1D) $getType($d)";

            var s = new TemplateDictionary
            {
                {"getType", f}
            };

            var result1 = Template.ImmediateApply(s, t);
            Assert.IsFalse(result1.Errors.ContainsError());
            Assert.IsFalse(result1.Errors.ContainsWarning());
            Assert.AreEqual("Double", result1.Output.Trim());

            var result2 = Template.CompileAndRun("test", t, s);
            Assert.IsFalse(result2.Errors.ContainsError());
            Assert.IsFalse(result2.Errors.ContainsWarning());
            Assert.AreEqual("Double", result2.Output.Trim());
        }

        [Test]
        public void IntegerTypeCheckTest()
        {
            Func<object, string> f = obj => obj.GetType().Name;

            const string t = "#set( $d = 1L) $getType($d)";

            var s = new TemplateDictionary
            {
                {"getType", f}
            };

            var result1 = Template.ImmediateApply(s, t);
            Assert.IsFalse(result1.Errors.ContainsError());
            Assert.IsFalse(result1.Errors.ContainsWarning());
            Assert.AreEqual("Int64", result1.Output.Trim());

            var result2 = Template.CompileAndRun("test", t, s);
            Assert.IsFalse(result2.Errors.ContainsError());
            Assert.IsFalse(result2.Errors.ContainsWarning());
            Assert.AreEqual("Int64", result2.Output.Trim());
        }

        [Test]
        public void LiteralThenExpansion()
        {
            const string t = "This is some literal text${foo}";
            var s = State();

            var result1 = Template.ImmediateApply(s, t);
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual(
                string.Format("This is some literal text{0}", s["foo"]),
                result1.Output);

            var result2 = Template.CompileAndRun("test", t, s);
            Assert.False(result2.Errors.ContainsError());
            Assert.False(result2.Errors.ContainsWarning());
            Assert.AreEqual(
                string.Format("This is some literal text{0}", s["foo"]),
                result2.Output);
        }

        [Test]
        public void MacroPassthrough()
        {
            var t = ComplexPadding() + "This is some literal text@('$bing')";
            var s = State();

            var result1 = Template.ImmediateApply(s, t);
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual(
                ComplexResult() + string.Format("This is some literal text{0}", s["bing"]),
                result1.Output);

            var result2 = Template.CompileAndRun("test", t, s);
            Assert.False(result2.Errors.ContainsError());
            Assert.False(result2.Errors.ContainsWarning());
            Assert.AreEqual(
                ComplexResult()+ string.Format("This is some literal text{0}", s["bing"]),
                result2.Output);
        }

        [Test]
        public void Passthrough()
        {
            var t = ComplexPadding() + "This is some literal text$(1+1)";
            var s = State();

            var result1 = Template.ImmediateApply(s, t);
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual(ComplexResult() + "This is some literal text2", result1.Output);

            var result2 = Template.CompileAndRun("test", t, s);
            Assert.False(result2.Errors.ContainsError());
            Assert.False(result2.Errors.ContainsWarning());
            Assert.AreEqual(ComplexResult() + "This is some literal text2", result2.Output);
        }

        [Test]
        public void SingleExpansion()
        {
            var s = State();

            var result1 = Template.ImmediateApply(s, "$foo");
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual(s["foo"], result1.Output);

            var result2 = Template.CompileAndRun("test", "$foo", s);
            Assert.False(result2.Errors.ContainsError());
            Assert.False(result2.Errors.ContainsWarning());
            Assert.AreEqual(s["foo"], result2.Output);
        }

        [Test]
        public void TwoExpansions()
        {
            var s = State();

            var result1 = Template.ImmediateApply(s, "${foo}${bar}");
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual(
                string.Format("{0}{1}", s["foo"], s["bar"]),
                result1.Output);

            var result2 = Template.CompileAndRun("test", "${foo}${bar}", s);
            Assert.False(result2.Errors.ContainsError());
            Assert.False(result2.Errors.ContainsWarning());
            Assert.AreEqual(
                string.Format("{0}{1}", s["foo"], s["bar"]),
                result2.Output);
        }

        [Test]
        public void UnsignedIntegerTypeCheckTest()
        {
            Func<object, string> f = obj => obj.GetType().Name;

            const string t = "#set( $d = 1U) $getType($d)";

            var s = new TemplateDictionary
            {
                {"getType", f}
            };

            var result1 = Template.ImmediateApply(s, t);
            Assert.IsFalse(result1.Errors.ContainsError());
            Assert.IsFalse(result1.Errors.ContainsWarning());
            Assert.AreEqual("UInt64", result1.Output.Trim());

            var result2 = Template.CompileAndRun("test", t, s);
            Assert.IsFalse(result2.Errors.ContainsError());
            Assert.IsFalse(result2.Errors.ContainsWarning());
            Assert.AreEqual("UInt64", result2.Output.Trim());
        }
    }
}