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

using System.IO;
using NUnit.Framework;
using PaniciSoftware.FastTemplate.Common;
using PaniciSoftware.FastTemplate.Compiler;

namespace PaniciSoftware.FastTemplate.Test
{
    [TestFixture]
    public class IncludeTests
    {
        [Test]
        public void ParseRecurisvelyBuildsOutput()
        {
            var mainView = File.ReadAllText("main.view");
            var dict = new TemplateDictionary();

            var result1 = Template.ImmediateApply(dict, mainView);
            Assert.IsEmpty(result1.Errors);
            Assert.AreEqual(
                "We're going to test multiple levels of #parse and #include.\r\n\r\nInside of one.partial: $foo = 10\r\n\r\nWe're now going another level deep:\r\n\r\nInside of two.partial: $foo = 5\r\n\r\nWe've done this twice!\r\nInside of three.partial: $foo = 20\r\n\r\nInside of one.partial: $foo = 20\r\n\r\nWe're now going another level deep:\r\n\r\nInside of two.partial: $foo = 5\r\n\r\nWe've done this twice!Inside of two.partial: $foo = 5\r\n\r\nWe've done this twice!",
                result1.Output);

            var compilerResult = Template.Compile(
                "main.view",
                mainView,
                OptimizeLevel.All);
            Assert.IsEmpty(compilerResult.Errors);

            var result2 = compilerResult.Template.Execute(dict);
            Assert.IsEmpty(result2.Errors);
            Assert.AreEqual(
                "We're going to test multiple levels of #parse and #include.\r\n\r\nInside of one.partial: $foo = 10\r\n\r\nWe're now going another level deep:\r\n\r\nInside of two.partial: $foo = 5\r\n\r\nWe've done this twice!\r\nInside of three.partial: $foo = 20\r\n\r\nInside of one.partial: $foo = 20\r\n\r\nWe're now going another level deep:\r\n\r\nInside of two.partial: $foo = 5\r\n\r\nWe've done this twice!Inside of two.partial: $foo = 5\r\n\r\nWe've done this twice!",
                result2.Output);
        }

        [Test]
        public void IncludeRecurisvelyBuildsOutput()
        {
            var mainView = File.ReadAllText("main2.view");
            var dict = new TemplateDictionary();

            var result1 = Template.ImmediateApply(dict, mainView);
            Assert.IsEmpty(result1.Errors);
            Assert.AreEqual(
                "We're going to test multiple levels of #include.\r\n\r\nInside of one.partial: \\$foo = $foo\r\n\r\nWe're now going another level deep:\r\n\r\n#set ($foo = 5)\r\n#parse ('two.partial')\r\nInside of three.partial: \\$foo = $foo\r\n\r\n#parse ('one.partial')\r\n#parse ('two.partial')",
                result1.Output);

            var compilerResult = Template.Compile(
                "main2.view",
                mainView,
                OptimizeLevel.All);
            Assert.IsEmpty(compilerResult.Errors);

            var result2 = compilerResult.Template.Execute(dict);
            Assert.IsEmpty(result2.Errors);
            Assert.AreEqual(
                "We're going to test multiple levels of #include.\r\n\r\nInside of one.partial: \\$foo = $foo\r\n\r\nWe're now going another level deep:\r\n\r\n#set ($foo = 5)\r\n#parse ('two.partial')\r\nInside of three.partial: \\$foo = $foo\r\n\r\n#parse ('one.partial')\r\n#parse ('two.partial')",
                result2.Output);
        }
    }
}