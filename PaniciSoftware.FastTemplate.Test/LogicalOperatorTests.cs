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
    public class LogicalOperatorTests
    {
        [Test]
        public void AndOperatorYieldsProperBooleanResult()
        {
            var dict = new TemplateDictionary();
            const string template1 = "#if ($foo == null && $bar != null) true #else false #end";

            var result1 = Template.ImmediateApply(dict, template1);
            Assert.False(result1.Errors.ContainsError());
            Assert.AreEqual(" false ", result1.Output);

            var result2 = Template.CompileAndRun("test", template1, dict);
            Assert.False(result2.Errors.ContainsError());
            Assert.AreEqual(" false ", result2.Output);

            dict["foo"] = "not null";
            dict["bar"] = null;
            const string template2 = "#if ($foo == null && $bar != null) true #else false #end";
            var result3 = Template.ImmediateApply(dict, template2);
            Assert.False(result3.Errors.ContainsError());
            Assert.AreEqual(" false ", result3.Output);

            var result4 = Template.CompileAndRun("test", template2, dict);
            Assert.False(result4.Errors.ContainsError());
            Assert.AreEqual(" false ", result4.Output);

            dict["foo"] = null;
            dict["bar"] = "not null";
            const string template3 = "#if ($foo == null && $bar != null) true #else false #end";
            var result5 = Template.ImmediateApply(dict, template3);
            Assert.False(result5.Errors.ContainsError());
            Assert.AreEqual(" true ", result5.Output);

            var result6 = Template.CompileAndRun("test", template3, dict);
            Assert.False(result6.Errors.ContainsError());
            Assert.AreEqual(" true ", result6.Output);

            dict["foo"] = false;
            dict["bar"] = true;
            const string template4 = "#if (!$foo && $bar) true #else false #end";
            var result7 = Template.ImmediateApply(dict, template4);
            Assert.False(result7.Errors.ContainsError());
            Assert.AreEqual(" true ", result7.Output);

            var result8 = Template.CompileAndRun("test", template4, dict);
            Assert.False(result8.Errors.ContainsError());
            Assert.AreEqual(" true ", result8.Output);
        }

        [Test]
        public void LogicalAndWithLogicalOrYieldsProperBooleanResult()
        {
            var dict = new TemplateDictionary();
            const string template1 = "#if (($foo == null || $bar == null) || ($baz != 'bing!')) true #else false #end";

            var result1 = Template.ImmediateApply(dict, template1);
            Assert.False(result1.Errors.ContainsError());
            Assert.AreEqual(" true ", result1.Output);

            var result2 = Template.CompileAndRun("test", template1, dict);
            Assert.False(result2.Errors.ContainsError());
            Assert.AreEqual(" true ", result2.Output);
        }

        [Test]
        public void OrOperatorYieldsProperBooleanResult()
        {
            var dict = new TemplateDictionary();
            const string template1 = "#if ($foo == null || $bar != null) true #else false #end";

            var result1 = Template.ImmediateApply(dict, template1);
            Assert.False(result1.Errors.ContainsError());
            Assert.AreEqual(" true ", result1.Output);

            var result2 = Template.CompileAndRun("test", template1, dict);
            Assert.False(result2.Errors.ContainsError());
            Assert.AreEqual(" true ", result2.Output);

            dict["foo"] = "not null";
            dict["bar"] = null;
            const string template2 = "#if ($foo == null || $bar != null) true #else false #end";
            var result3 = Template.ImmediateApply(dict, template2);
            Assert.False(result3.Errors.ContainsError());
            Assert.AreEqual(" false ", result3.Output);

            var result4 = Template.CompileAndRun("test", template2, dict);
            Assert.False(result4.Errors.ContainsError());
            Assert.AreEqual(" false ", result4.Output);

            dict["foo"] = null;
            dict["bar"] = "not null";
            const string template3 = "#if ($foo == null || $bar != null) true #else false #end";
            var result5 = Template.ImmediateApply(dict, template3);
            Assert.False(result5.Errors.ContainsError());
            Assert.AreEqual(" true ", result5.Output);

            var result6 = Template.CompileAndRun("test", template3, dict);
            Assert.False(result6.Errors.ContainsError());
            Assert.AreEqual(" true ", result6.Output);
        }
    }
}