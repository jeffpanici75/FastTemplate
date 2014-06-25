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
    public class SetBlockTest
    {
        [Test]
        public void Complex()
        {
            const string block =
                "#set( ${ bag .   GetThis   ( 1231231L, 'asdfasdfa' )   [   121231L   ,  'asdfasdfasdf'  ]  }    =   'Action man' )$bag[   123123L   ,   'asdfasdf'   ]";
            Func<object, string> f = obj => obj.GetType().Name;
            var bag = new ClassWithSetters();
            var s = new TemplateDictionary
            {
                {"getType", f},
                {"bag", bag}
            };

            var result1 = Template.CompileAndRun("test", block, s);
            Assert.IsFalse(result1.Errors.ContainsError());
            Assert.IsFalse(result1.Errors.ContainsWarning());
            Assert.AreEqual("Action man", result1.Output.Trim());
            Assert.AreEqual(bag.HiddenIndex, result1.Output);

            var result2 = Template.ImmediateApply(s, block);
            Assert.IsFalse(result2.Errors.ContainsError());
            Assert.IsFalse(result2.Errors.ContainsWarning());
            Assert.AreEqual("Action man", result2.Output.Trim());
            Assert.AreEqual(bag.HiddenIndex, result2.Output);
        }

        [Test]
        public void InvokeNotValidLValue()
        {
            const string block = "#set( ${ bag .   GetThis   ( 1231231, 'asdfasdfa' )  .GetThis( 231231, 'aasdfasdf12123' ) }    =   'Action man' )$bag[   123123   ,   'asdfasdf'   ]";

            Func<object, string> f = obj => obj.GetType().Name;
            var bag = new ClassWithSetters();
            var s = new TemplateDictionary
            {
                {"getType", f},
                {"bag", bag}
            };

            var result1 = Template.ImmediateApply(s, block);
            Assert.IsTrue(result1.Errors.ContainsError());
            Assert.IsFalse(result1.Errors.ContainsWarning());

            var result2 = Template.CompileAndRun("test", block, s);
            Assert.IsTrue(result2.Errors.ContainsError());
            Assert.IsFalse(result2.Errors.ContainsWarning());
        }

        [Test]
        public void SimpleSet()
        {
            const string block = "#set( $bag.F1 = 'Action man' )$bag.F1";

            Func<object, string> f = obj => obj.GetType().Name;

            var bag = new ClassWithSetters();
            var s = new TemplateDictionary
            {
                {"getType", f},
                {"bag", bag}
            };

            var result1 = Template.ImmediateApply(s, block);
            Assert.IsFalse(result1.Errors.ContainsError());
            Assert.IsFalse(result1.Errors.ContainsWarning());
            Assert.AreEqual("Action man", result1.Output.Trim());
            Assert.AreEqual(bag.F1, result1.Output);

            var result2 = Template.CompileAndRun("test", block, s);
            Assert.IsFalse(result2.Errors.ContainsError());
            Assert.IsFalse(result2.Errors.ContainsWarning());
            Assert.AreEqual("Action man", result2.Output.Trim());
            Assert.AreEqual(bag.F1, result2.Output);
        }
    }
}