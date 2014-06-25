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
using Antlr.Runtime.Tree;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Interpreter
{
    internal class Constant : Evaluator
    {
        public Constant(ITree t)
            : base(t)
        {
        }

        protected override void OnEval(out ITemplateType o)
        {
            if (T.Type == 0)
            {
                Errors.ErrorParse((CommonErrorNode) T);
                o = null;
                return;
            }

            var child = (CommonTree) T.Children[0];
            switch (child.Type)
            {
                case 0:
                {
                    Errors.ErrorParse((CommonErrorNode) child);
                    o = null;
                    break;
                }
                case TemplateLexer.Integer:
                {
                    o = new IntType {Value = int.Parse(child.Text)};
                    break;
                }
                case TemplateLexer.SignedLong:
                {
                    o = new LongType {Value = Int64.Parse(child.Text)};
                    break;
                }
                case TemplateLexer.UnsignedInteger:
                {
                    o = new UIntType {Value = UInt64.Parse(child.Text)};
                    break;
                }
                case TemplateLexer.Double:
                {
                    o = new DoubleType {Value = double.Parse(child.Text)};
                    break;
                }
                case TemplateLexer.Decimal:
                {
                    o = new DecimalType {Value = decimal.Parse(child.Text)};
                    break;
                }
                case TemplateLexer.Hex:
                {
                    o = new LongType
                    {Value = Convert.ToInt64(child.Text, 16)};
                    break;
                }
                case TemplateLexer.StringLiteral:
                {
                    o = new StringType {Value = child.Text};
                    break;
                }
                case TemplateLexer.True:
                {
                    o = BooleanType.True();
                    break;
                }
                case TemplateLexer.False:
                {
                    o = BooleanType.False();
                    break;
                }
                case TemplateLexer.Null:
                {
                    o = new NullType();
                    break;
                }
                case TemplateParser.ConstDict:
                {
                    var dict = new Dictionary<string, object>();
                    if (child.Children[0].Type
                        != TemplateParser.Empty)
                    {
                        for (var i = 0; i < child.Children.Count; i += 2)
                        {
                            var key = child.Children[i].Text;
                            var value = (CommonTree) child.Children[i + 1];
                            var exp = new Expression(value);
                            ITemplateType v;
                            Errors.AddRange(exp.Eval(Ctx, Interpreter, out v));
                            if (Errors.ContainsError())
                                continue;
                            dict[key] = v;
                        }
                    }
                    o = new DictType {Value = dict};
                    break;
                }
                case TemplateParser.ConstList:
                {
                    var list = new List<object>();
                    if (child.Children[0].Type
                        != TemplateParser.Empty)
                    {
                        foreach (var element in child.Children)
                        {
                            ITemplateType item;
                            var c = new Constant(element);
                            var e = c.Eval(Ctx, Interpreter, out item);
                            Errors.AddRange(e);
                            if (e.ContainsError())
                                break;
                            list.Add(item.RawValue);
                        }
                    }
                    o = new ListType {Value = list};
                    break;
                }
                default:
                {
                    o = null;
                    Errors.ErrorUnknownKeyword(child.Text);
                    break;
                }
            }
        }
    }
}