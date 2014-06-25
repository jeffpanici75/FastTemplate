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
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Test
{
    [TestFixture]
    public class LoopTest
    {
        [Test]
        public void ComplexLoop()
        {
            var cartItems = new List<CartItem>
            {
                new CartItem
                {
                    Sku = "PS-APP-01",
                    Description = "PSapp Server License",
                    Price = 59999,
                    Quantity = 12
                },
                new CartItem
                {
                    Sku = "PS-DB-01",
                    Description = "PSapp Server License",
                    Price = 59999,
                    Quantity = 12
                },
                new CartItem
                {
                    Sku = "PS-CACHE-01",
                    Description = "PSapp Server License",
                    Price = 499,
                    Quantity = 12
                },
                new CartItem
                {
                    Sku = "PS-QUEUE-01",
                    Description = "PSapp Server License",
                    Price = 499,
                    Quantity = 12
                },
                new CartItem
                {
                    Sku = "PS-MESH-01",
                    Description = "PSapp Server License",
                    Price = 499,
                    Quantity = 12
                }
            };

            Func<object, string> f = obj => obj.GetType().Name;

            var state = new TemplateDictionary
            {
                {
                    "foo", new
                    {
                        bar = (Int64) 0,
                        One = "Value of One",
                        two = "value of two",
                        three = new
                        {
                            one = "Two levels down one",
                            two = "Two levels down two",
                        }
                    }
                },
                {"getType", f},
                {"bar", "key point at by bar"},
                {"bing", "key point at by bing"},
                {"baz", "key point at by baz followed by ${bing}"},
                {"shoppingCart", cartItems},
                {"moneyDollars", (decimal) 123.456},
                {"showCartTotal", true},
                {"formatCurrency", (Func<object, string>) (o => Convert.ToDecimal(o).ToString("C"))},
                {"printFoo", (Action) (() => Console.Out.WriteLine("Foo"))}
            };
            var source = File.ReadAllText("complex-loop.source");
            var target = File.ReadAllText("complex-loop.target");

            var result = Template.CompileAndRun("test", source, state);
            Assert.IsFalse(result.Errors.ContainsError());
            Assert.IsFalse(result.Errors.ContainsWarning());
            Assert.AreEqual(target, result.Output);
        }

        [Test]
        public void ComplexLoopWithTemplateWriterOutput()
        {
            var cartItems = new List<CartItem>
            {
                new CartItem
                {
                    Sku = "PS-APP-01",
                    Description = "PSapp Server License",
                    Price = 59999,
                    Quantity = 12
                },
                new CartItem
                {
                    Sku = "PS-DB-01",
                    Description = "PSapp Server License",
                    Price = 59999,
                    Quantity = 12
                },
                new CartItem
                {
                    Sku = "PS-CACHE-01",
                    Description = "PSapp Server License",
                    Price = 499,
                    Quantity = 12
                },
                new CartItem
                {
                    Sku = "PS-QUEUE-01",
                    Description = "PSapp Server License",
                    Price = 499,
                    Quantity = 12
                },
                new CartItem
                {
                    Sku = "PS-MESH-01",
                    Description = "PSapp Server License",
                    Price = 499,
                    Quantity = 12
                }
            };

            var state = new TemplateDictionary
            {
                {"shoppingCart", cartItems}
            };

            var result = Template.CompileAndRun(
                "test",
                "#foreach ($each in $shoppingCart) #each $each #end",
                state);
            Assert.IsFalse(result.Errors.ContainsError());
            Assert.IsFalse(result.Errors.ContainsWarning());
            Assert.AreEqual(
                @"  <td>PS-APP-01</td><td>PSapp Server License</td><td>12</td><td>59999</td><td>$719,988.00</td>   <td>PS-DB-01</td><td>PSapp Server License</td><td>12</td><td>59999</td><td>$719,988.00</td>   <td>PS-CACHE-01</td><td>PSapp Server License</td><td>12</td><td>499</td><td>$5,988.00</td>   <td>PS-QUEUE-01</td><td>PSapp Server License</td><td>12</td><td>499</td><td>$5,988.00</td>   <td>PS-MESH-01</td><td>PSapp Server License</td><td>12</td><td>499</td><td>$5,988.00</td> ",
                result.Output);
        }

        [Test]
        public void LoopsAreInclusive()
        {
            const string text = "#loop(1 to 10 as $i)a#end";
            
            var d = new TemplateDictionary();

            var result1 = Template.CompileAndRun("test", text, d);
            Assert.IsFalse(result1.Errors.ContainsError());
            Assert.IsFalse(result1.Errors.ContainsWarning());
            Assert.AreEqual(10, result1.Output.Length);

            var result2 = Template.ImmediateApply(d, text);
            Assert.IsFalse(result2.Errors.ContainsError());
            Assert.IsFalse(result2.Errors.ContainsWarning());
            Assert.AreEqual(10, result2.Output.Length);
        }

        [Test]
        public void SimpleLoop()
        {
            const string text = "#loop( 123124124 to 123124124 + 9 as $i step 1 )a#end";

            var d = new TemplateDictionary();

            var result1 = Template.CompileAndRun("test", text, d);
            Assert.IsFalse(result1.Errors.ContainsError());
            Assert.IsFalse(result1.Errors.ContainsWarning());
            Assert.AreEqual("aaaaaaaaaa", result1.Output);

            var result2 = Template.ImmediateApply(d, text);
            Assert.IsFalse(result2.Errors.ContainsError());
            Assert.IsFalse(result2.Errors.ContainsWarning());
            Assert.AreEqual("aaaaaaaaaa", result2.Output);
        }

        [Test]
        public void SimpleStatementStepLoop()
        {
            const string text = "#set( $step = 2 )#loop( 123124124 to 123124124 + 9 as $i step $step )a#end";

            var d = new TemplateDictionary();

            var result1 = Template.CompileAndRun("test", text, d);
            Assert.IsFalse(result1.Errors.ContainsError());
            Assert.IsFalse(result1.Errors.ContainsWarning());
            Assert.AreEqual("aaaaa", result1.Output);

            var result2 = Template.ImmediateApply(d, text);
            Assert.IsFalse(result2.Errors.ContainsError());
            Assert.IsFalse(result2.Errors.ContainsWarning());
            Assert.AreEqual("aaaaa", result2.Output);
        }

        [Test]
        public void SimpleStepOf2Loop()
        {
            const string text = "#loop( 123124124 to 123124124 + 9 as $i step 2 )a#end";

            var d = new TemplateDictionary();

            var result1 = Template.CompileAndRun("test", text, d);
            Assert.IsFalse(result1.Errors.ContainsError());
            Assert.IsFalse(result1.Errors.ContainsWarning());
            Assert.AreEqual("aaaaa", result1.Output);

            var result2 = Template.ImmediateApply(d, text);
            Assert.IsFalse(result2.Errors.ContainsError());
            Assert.IsFalse(result2.Errors.ContainsWarning());
            Assert.AreEqual("aaaaa", result2.Output);
        }

        [Test]
        public void SomeDirectivesStatementStepLoop()
        {
            const string text = "#set( $step = 2 )#loop( 123124124 to 123124124 + 999 as $i step $step )\n" +
                "a\n" +
                "#each\n" +
                "This is the each text.\n" +
                "#end";

            const string section = "This is the each text.\na\n";
            var expected = string.Empty;
            for (var i = 0; i < 1000; i += 2)
            {
                expected += section;
            }

            var d = new TemplateDictionary();

            var result1 = Template.CompileAndRun("test", text, d);
            Assert.IsFalse(result1.Errors.ContainsError());
            Assert.IsFalse(result1.Errors.ContainsWarning());
            Assert.AreEqual(expected, result1.Output);

            var result2 = Template.ImmediateApply(d, text);
            Assert.IsFalse(result2.Errors.ContainsError());
            Assert.IsFalse(result2.Errors.ContainsWarning());
            Assert.AreEqual(expected, result2.Output);
        }
    }
}