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

using NUnit.Framework;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Test
{
    [TestFixture]
    public class ComplexTest
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

        [Test]
        public void DynamicString()
        {
            const string t = "$($($($(\"Foo\\\"\\\"\\\"\\\" Bar\\\"\\\"\\\"\\\"\\\"\\\"\\\"\\\" Bing Baz $bar$bing\"))))";

            var s = State();
            var result1 = Template.ImmediateApply(s, t);
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual(
                string.Format("Foo\"\"\"\" Bar\"\"\"\"\"\"\"\" Bing Baz {0}{1}", s["bar"], s["bing"]),
                result1.Output);

            var result2 = Template.CompileAndRun("test", t, s);
            Assert.False(result2.Errors.ContainsError());
            Assert.False(result2.Errors.ContainsWarning());
            Assert.AreEqual(
                string.Format("Foo\"\"\"\" Bar\"\"\"\"\"\"\"\" Bing Baz {0}{1}", s["bar"], s["bing"]),
                result1.Output);
        }

        [Test]
        public void DynamicString2()
        {
            const string t = "#if( $str == \"agents\" ) Yes. #else No. #end";
            var s = State();
            s["str"] = (StringType) "agents";

            var result1 = Template.ImmediateApply(s, t);
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual(string.Format(" Yes. "), result1.Output);

            var result2 = Template.CompileAndRun("test", t, s);
            Assert.False(result2.Errors.ContainsError());
            Assert.False(result2.Errors.ContainsWarning());
            Assert.AreEqual(string.Format(" Yes. "), result2.Output);
        }

        [Test]
        public void Passthrough()
        {
            const string t = "$($($($($foo))))";
            var s = State();

            var result1 = Template.ImmediateApply(s, t);
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual(s["foo"], result1.Output);

            var result2 = Template.CompileAndRun("test", t, s);
            Assert.False(result2.Errors.ContainsError());
            Assert.False(result2.Errors.ContainsWarning());
            Assert.AreEqual(s["foo"], result2.Output);
        }

        [Test]
        public void UnexpectedEofInside()
        {
            const string t = "$($($($($foo)";
            var s = State();

            var result1 = Template.ImmediateApply(s, t);
            Assert.IsTrue(result1.Errors.ContainsError());

            var result2 = Template.CompileAndRun("test", t, s);
            Assert.IsTrue(result2.Errors.ContainsError());
        }

        [Test]
        public void UnexpectedEofOutside()
        {
            const string t = "#if(true) some text and then EOF.";
            var s = State();

            var result1 = Template.ImmediateApply(s, t);
            Assert.IsTrue(result1.Errors.ContainsError());

            var result2 = Template.CompileAndRun("test", t, s);
            Assert.IsTrue(result2.Errors.ContainsError());
        }
    }
}