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
    public class MacroTests
    {
        [Test]
        public void ExpandNonExistentMacroResultsInError()
        {
            var s = new TemplateDictionary
            {
                {"bar", "bing"},
                {"foo", "$bar"}
            };
            const string text = "jeff.panici@mac.com";

            var result1 = Template.ImmediateApply(s, text);
            Assert.IsFalse(result1.Errors.ContainsError());
            Assert.IsTrue(result1.Errors.ContainsWarning());

            var result2 = Template.CompileAndRun("test", text, s);
            Assert.IsFalse(result2.Errors.ContainsError());
            Assert.IsTrue(result2.Errors.ContainsWarning());
        }

        [Test]
        public void SimpleMacroExpands()
        {
            var s = new TemplateDictionary
            {
                {"bar", "bing"},
                {"foo", "$bar"}
            };
            const string text = "@foo";

            var result1 = Template.ImmediateApply(s, text);
            Assert.IsFalse(result1.Errors.ContainsError());
            Assert.IsFalse(result1.Errors.ContainsWarning());
            Assert.AreEqual("bing", result1.Output);

            var result2 = Template.CompileAndRun("test", text, s);
            Assert.IsFalse(result2.Errors.ContainsError());
            Assert.IsFalse(result2.Errors.ContainsWarning());
            Assert.AreEqual("bing", result2.Output);
        }
    }
}