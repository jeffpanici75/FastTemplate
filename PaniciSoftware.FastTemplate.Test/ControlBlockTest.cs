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
    public class ControlBlockTest
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
        public void ComplexExpression()
        {
            const string exp = "$(30+-10)";
            var s = State();

            var result1 = Template.ImmediateApply(s, exp);
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual("20", result1.Output);

            var result2 = Template.CompileAndRun("test", exp, s);
            Assert.False(result2.Errors.ContainsError());
            Assert.False(result2.Errors.ContainsWarning());
            Assert.AreEqual("20", result2.Output);
        }

        [Test]
        public void FlatSimpleIfAndElseBlock()
        {
            const string block = "#if( 1 + 1 < 1 )Text that appears#{else}This is the text in the else block$#end";
            var s = State();

            var result1 = Template.ImmediateApply(s, block);
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual("This is the text in the else block$", result1.Output);

            var result2 = Template.CompileAndRun("test", block, s);
            Assert.False(result2.Errors.ContainsError());
            Assert.False(result2.Errors.ContainsWarning());
            Assert.AreEqual("This is the text in the else block$", result2.Output);
        }

        [Test]
        public void FlatSimpleIfAndElseIfBlock()
        {
            const string block = "#if( 1 + 1 < 1 )Text that appears#elseif( true )Else if text here#{else}This is the text in the else block$#end";
            var s = State();

            var result1 = Template.ImmediateApply(s, block);
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual("Else if text here", result1.Output);

            var result2 = Template.CompileAndRun("test", block, s);
            Assert.False(result2.Errors.ContainsError());
            Assert.False(result2.Errors.ContainsWarning());
            Assert.AreEqual("Else if text here", result2.Output);
        }

        [Test]
        public void FlatSimpleIfBlock()
        {
            const string block = "#if( 1 + 1 > 1 )Text that appears#end";
            var s = State();

            var result1 = Template.ImmediateApply(s, block);
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual("Text that appears", result1.Output);

            var result2 = Template.CompileAndRun("test", block, s);
            Assert.False(result2.Errors.ContainsError());
            Assert.False(result2.Errors.ContainsWarning());
            Assert.AreEqual("Text that appears", result2.Output);
        }

        [Test]
        public void NotNullCompareInIfBlock()
        {
            const string block = "#if( $str != null )STOPPPPP#{else}GOOO#{end}";
            var s = State();

            s["str"] = new NullType();
            var result1 = Template.ImmediateApply(s, block);
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual("GOOO", result1.Output);

            var result2 = Template.CompileAndRun("test", block, s);
            Assert.False(result2.Errors.ContainsError());
            Assert.False(result2.Errors.ContainsWarning());
            Assert.AreEqual("GOOO", result2.Output);
        }

        [Test]
        public void NullCompareInIfBlock()
        {
            const string block = "#if( $str == null )GOOO#{else}STOPPPPP#{end}";
            var s = State();

            s["str"] = new NullType();
            var result1 = Template.ImmediateApply(s, block);
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual("GOOO", result1.Output);

            var result2 = Template.CompileAndRun("test", block, s);
            Assert.False(result2.Errors.ContainsError());
            Assert.False(result2.Errors.ContainsWarning());
            Assert.AreEqual("GOOO", result2.Output);
        }

        [Test]
        public void PragmaTrimNoTrim()
        {
            const string block = "#pragma(trim)\n\t      some text\nmore text    \n#pragma(notrim)a  string  with  extra  spaces  in  it";
            var s = State();

            var result1 = Template.ImmediateApply(s, block);
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual(" some text\nmore text\na  string  with  extra  spaces  in  it", result1.Output);

            var result2 = Template.CompileAndRun("test", block, s);
            Assert.False(result2.Errors.ContainsError());
            Assert.False(result2.Errors.ContainsWarning());
            Assert.AreEqual(" some text\nmore text\na  string  with  extra  spaces  in  it", result2.Output);
        }

        [Test]
        public void SimpleIfAndElseBlock()
        {
            const string block = "#if( 1 + 1 < 1 )\nText that appears\n#{else}\nThis is the text in the else block#\n#end\n";
            var s = State();

            var result1 = Template.ImmediateApply(s, block);
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual("This is the text in the else block#\n", result1.Output);

            var result2 = Template.CompileAndRun("test", block, s);
            Assert.False(result2.Errors.ContainsError());
            Assert.False(result2.Errors.ContainsWarning());
            Assert.AreEqual("This is the text in the else block#\n", result2.Output);
        }

        [Test]
        public void SimpleIfAndElseIfBlock()
        {
            const string block = "#if( 1 + 1 < 1 )\nText that appears\n#elseif( true )\nElse if text here\n#{else}\nThis is the text in the else block#\n#end\n";
            var s = State();

            var result1 = Template.ImmediateApply(s, block);
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual("Else if text here\n", result1.Output);

            var result2 = Template.CompileAndRun("test", block, s);
            Assert.False(result2.Errors.ContainsError());
            Assert.False(result2.Errors.ContainsWarning());
            Assert.AreEqual("Else if text here\n", result2.Output);
        }

        [Test]
        public void SimpleIfBlock()
        {
            const string block = "#if( 1 + 1 > 1 )\nText that appears\n#end\n";
            var s = State();

            var result1 = Template.ImmediateApply(s, block);
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual("Text that appears\n", result1.Output);
        }

        [Test]
        public void UnParsedBlock()
        {
            const string exp = "Leading text#[$(30+-10)#if( #elseif #else #end ]#";
            var s = State();

            var result1 = Template.ImmediateApply(s, exp);
            Assert.False(result1.Errors.ContainsError());
            Assert.False(result1.Errors.ContainsWarning());
            Assert.AreEqual("Leading text$(30+-10)#if( #elseif #else #end ", result1.Output);

            var result2 = Template.CompileAndRun("test", exp, s);
            Assert.False(result2.Errors.ContainsError());
            Assert.False(result2.Errors.ContainsWarning());
            Assert.AreEqual("Leading text$(30+-10)#if( #elseif #else #end ", result2.Output);
        }
    }
}