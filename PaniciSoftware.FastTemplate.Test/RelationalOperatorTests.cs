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
    public class RelationalOperatorTests
    {
        [Test]
        public void EqualsOperator()
        {
            var dict = new TemplateDictionary();
            dict["foo"] = true.ToString();
            dict["bar"] = false.ToString();

            const string template1 = "#if ($foo == $bar) true #else false #end";
            var result1 = Template.ImmediateApply(dict, template1);
            Assert.False(result1.Errors.ContainsError());
            Assert.AreEqual(" false ", result1.Output);

            dict["foo"] = null;
            dict["bar"] = string.Empty;
            const string template2 = "#if ($foo == $bar) true #else false #end";
            var result2 = Template.ImmediateApply(dict, template2);
            Assert.False(result2.Errors.ContainsError());
            Assert.AreEqual(" false ", result2.Output);
        }

        [Test]
        public void GreaterThanOpeator()
        {
            var dict = new TemplateDictionary();
            dict["foo"] = 3;
            dict["bar"] = 4;

            const string template1 = "#if ($foo > $bar) true #else false #end";
            var result1 = Template.ImmediateApply(dict, template1);
            Assert.False(result1.Errors.ContainsError());
            Assert.AreEqual(" false ", result1.Output);

            dict["foo"] = 13;
            var result2 = Template.ImmediateApply(dict, template1);
            Assert.False(result2.Errors.ContainsError());
            Assert.AreEqual(" true ", result2.Output);

            dict["foo"] = 4.5;
            dict["bar"] = 1.6;
            var result3 = Template.ImmediateApply(dict, template1);
            Assert.False(result3.Errors.ContainsError());
            Assert.AreEqual(" true ", result3.Output);
        }

        [Test]
        public void LessThanOperator()
        {
            var dict = new TemplateDictionary();
            dict["foo"] = 3;
            dict["bar"] = 4;

            const string template1 = "#if ($foo < $bar) true #else false #end";
            var result1 = Template.ImmediateApply(dict, template1);
            Assert.False(result1.Errors.ContainsError());
            Assert.AreEqual(" true ", result1.Output);

            dict["foo"] = 13;
            var result2 = Template.ImmediateApply(dict, template1);
            Assert.False(result2.Errors.ContainsError());
            Assert.AreEqual(" false ", result2.Output);
        }
    }
}