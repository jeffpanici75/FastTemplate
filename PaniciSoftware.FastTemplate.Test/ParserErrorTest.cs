﻿//
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
    public class ParserErrorTest
    {
        [Test]
        public void BadLoop()
        {
            var dict = new TemplateDictionary
            {
                {"decimal", (decimal) 1.2},
                {"double", 1.2}
            };

            const string exp = "#loop Yalp";

            var result1 = Template.ImmediateApply(dict, exp);
            Assert.IsTrue(result1.Errors.ContainsError());
            Assert.AreEqual("R999", result1.Errors[0].Code);
            Assert.IsFalse(result1.Errors.ContainsWarning());
            Assert.IsTrue(string.IsNullOrEmpty(result1.Output));

            var result2 = Template.CompileAndRun("test", exp, dict);
            Assert.IsTrue(result2.Errors.ContainsError());
            Assert.AreEqual("R999", result2.Errors[0].Code);
            Assert.IsFalse(result2.Errors.ContainsWarning());
            Assert.IsTrue(string.IsNullOrEmpty(result2.Output));
        }

        [Test]
        public void Badkeyword()
        {
            var dict = new TemplateDictionary
            {
                {"decimal", (decimal) 1.2},
                {"double", 1.2}
            };

            const string exp = "$($(decimal) + $(double))";

            var result1 = Template.ImmediateApply(dict, exp);
            Assert.IsTrue(result1.Errors.ContainsError());
            Assert.AreEqual(2, result1.Errors.Count);
            Assert.IsFalse(result1.Errors.ContainsWarning());
            Assert.IsTrue(string.IsNullOrEmpty(result1.Output));

            var result2 = Template.CompileAndRun("test", exp, dict);
            Assert.IsTrue(result2.Errors.ContainsError());
            Assert.AreEqual(2, result2.Errors.Count);
            Assert.IsFalse(result2.Errors.ContainsWarning());
            Assert.IsTrue(string.IsNullOrEmpty(result2.Output));
        }

        [Test]
        public void SeeminglyBadPound()
        {
            var dict = new TemplateDictionary
            {
                {"decimal", (decimal) 1.2},
                {"double", 1.2}
            };

            const string exp = "#foo";

            var result1 = Template.ImmediateApply(dict, exp);
            Assert.IsFalse(result1.Errors.ContainsError());
            Assert.IsFalse(result1.Errors.ContainsWarning());
            Assert.AreEqual("#foo", result1.Output);

            var result2 = Template.CompileAndRun("test", exp, dict);
            Assert.IsFalse(result2.Errors.ContainsError());
            Assert.IsFalse(result2.Errors.ContainsWarning());
            Assert.AreEqual("#foo", result2.Output);
        }

        [Test]
        public void SeemsBadButIsOkParens()
        {
            var dict = new TemplateDictionary
            {
                {"decimal", (decimal) 1.2},
                {"double", 1.2}
            };

            const string exp = "$($decimal) + $double)";

            var result1 = Template.ImmediateApply(dict, exp);
            Assert.IsFalse(result1.Errors.ContainsError());
            Assert.IsFalse(result1.Errors.ContainsWarning());
            Assert.AreEqual("1.2 + 1.2)", result1.Output);

            var result2 = Template.CompileAndRun("test", exp, dict);
            Assert.IsFalse(result2.Errors.ContainsError());
            Assert.IsFalse(result2.Errors.ContainsWarning());
            Assert.AreEqual("1.2 + 1.2)", result2.Output);
        }
    }
}