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
    public class StatementTest
    {
        [Test]
        public void SimpleFormalStatement()
        {
            var ab = new ActionBucket
            {
                bar = "~`!1@2#3$4%5^6&7*8(9)0_-+={[}]:;\"\"<,<.?//*-+.a string pointed to by bar"
            };

            Func<int, int, int, int, ActionBucket> f = (a, b, c, d) => ab;
            var s = new TemplateDictionary
            {
                {"foo", f}
            };

            const string text = "${ foo    (1,2,3,4)  .  func   ( 'a string', 0L ) [ 1L , 2L ]   [ 1L , 2L ] .    func    ( 'adfasdfad', 92321312L ) [123123L,123123L].bar}";

            var result1 = Template.ImmediateApply(s, text);
            Assert.IsFalse(result1.Errors.ContainsError());
            Assert.IsFalse(result1.Errors.ContainsWarning());
            Assert.AreEqual(ab.bar, result1.Output);

            var result2 = Template.CompileAndRun("test", text, s);
            Assert.IsFalse(result2.Errors.ContainsError());
            Assert.IsFalse(result2.Errors.ContainsWarning());
            Assert.AreEqual(ab.bar, result2.Output);
        }

        [Test]
        public void SimpleStatement()
        {
            var ab = new ActionBucket
            {
                bar = "~`!1@2#3$4%5^6&7*8(9)0_-+={[}]:;\"\"<,<.?//*-+.a string pointed to by bar"
            };

            Func<Int64, Int64, Int64, Int64, ActionBucket> f = (a, b, c, d) => ab;
            var s = new TemplateDictionary
            {
                {"foo", f}
            };

            const string text = "$foo(1L,2L,3L,4L).func( 'a string', 0L )[1L,2L][1L,2L].func( 'adfasdfad', 92321312L )[123123L,123123L].bar";

            var result1 = Template.ImmediateApply(s, text);
            Assert.IsFalse(result1.Errors.ContainsError());
            Assert.IsFalse(result1.Errors.ContainsWarning());
            Assert.AreEqual(ab.bar, result1.Output);

            var result2 = Template.CompileAndRun("test", text, s);
            Assert.IsFalse(result2.Errors.ContainsError());
            Assert.IsFalse(result2.Errors.ContainsWarning());
            Assert.AreEqual(ab.bar, result2.Output);
        }
    }
}