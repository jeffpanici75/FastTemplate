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
using System.Reflection;
using System.Reflection.Emit;
using Antlr.Runtime.Tree;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Compiler
{
    internal class Constant : Emitter
    {
        public Constant(ITree tree)
            : base(tree)
        {
        }

        protected override void OnGenerate()
        {
            if (T.Type == 0)
            {
                Ctx.EmitNullRef();
                Errors.ErrorParse((CommonErrorNode) T);
                return;
            }
            try
            {
                var child = (CommonTree) T.Children[0];
                switch (child.Type)
                {
                    case 0:
                    {
                        Ctx.EmitNullRef();
                        Errors.ErrorParse((CommonErrorNode) child);
                        break;
                    }
                    case TemplateLexer.Integer:
                    {
                        var result = int.Parse(child.Text);
                        Ctx.Sink.Emit(OpCodes.Ldc_I4, result);
                        var ctor =
                            typeof (IntType).GetConstructor(
                                new[] {typeof (int)});
                        Ctx.Sink.Emit(OpCodes.Newobj, ctor);
                        break;
                    }
                    case TemplateLexer.SignedLong:
                    {
                        var result = Int64.Parse(child.Text);
                        Ctx.Sink.Emit(OpCodes.Ldc_I8, result);
                        var ctor =
                            typeof (LongType).GetConstructor(
                                new[] {typeof (Int64)});
                        Ctx.Sink.Emit(OpCodes.Newobj, ctor);
                        break;
                    }
                    case TemplateLexer.UnsignedInteger:
                    {
                        var result = UInt64.Parse(child.Text);
                        Ctx.Sink.Emit(OpCodes.Ldc_I8, (long) result);
                        var ctor =
                            typeof (UIntType).GetConstructor(
                                new[] {typeof (Int64)});
                        Ctx.Sink.Emit(OpCodes.Newobj, ctor);
                        break;
                    }
                    case TemplateLexer.Double:
                    {
                        var d = double.Parse(child.Text);
                        Ctx.Sink.Emit(
                            OpCodes.Ldc_R8, d);
                        var ctor =
                            typeof (DoubleType).GetConstructor(
                                new[] {typeof (double)});
                        Ctx.Sink.Emit(OpCodes.Newobj, ctor);
                        break;
                    }
                    case TemplateLexer.Decimal:
                    {
                        Ctx.Sink.Emit(OpCodes.Ldstr, child.Text);
                        Ctx.Sink.EmitDecimalParse();
                        var ctor =
                            typeof (DecimalType).GetConstructor(
                                new[] {typeof (decimal)});
                        Ctx.Sink.Emit(OpCodes.Newobj, ctor);
                        break;
                    }
                    case TemplateLexer.Hex:
                    {
                        var result = Convert.ToInt64(child.Text, 16);
                        Ctx.Sink.Emit(OpCodes.Ldc_I8, result);
                        var ctor =
                            typeof (LongType).GetConstructor(
                                new[] {typeof (Int64)});
                        Ctx.Sink.Emit(OpCodes.Newobj, ctor);
                        break;
                    }
                    case TemplateLexer.StringLiteral:
                    {
                        Ctx.Sink.Emit(OpCodes.Ldstr, child.Text);
                        var ctor =
                            typeof (StringType).GetConstructor(
                                new[] {typeof (string)});
                        Ctx.Sink.Emit(OpCodes.Newobj, ctor);
                        break;
                    }
                    case TemplateLexer.True:
                    {
                        Ctx.EmitTrue();
                        break;
                    }
                    case TemplateLexer.False:
                    {
                        Ctx.EmitFalse();
                        break;
                    }
                    case TemplateLexer.Null:
                    {
                        Ctx.EmitNullType();
                        break;
                    }
                    case TemplateParser.ConstDict:
                    {
                        var dictCtor =
                            typeof (Dictionary<string, object>).GetConstructor(
                                new Type[0]);
                        Ctx.Sink.Emit(OpCodes.Newobj, dictCtor);

                        if (child.Children[0].Type
                            != TemplateParser.Empty)
                        {
                            var method = typeof (Dictionary<string, object>).
                                GetMethod(
                                    "Add",
                                    BindingFlags.Public | BindingFlags.Instance);
                            var swap = Ctx.Sink.DeclareLocal(
                                typeof (Dictionary<string, object>));
                            Ctx.Sink.FastEmitStoreLocal(swap.LocalIndex);
                            for (var i = 0; i < child.Children.Count; i += 2)
                            {
                                Ctx.Sink.FastEmitLoadLocal(swap.LocalIndex);
                                var key = child.Children[i].Text;
                                Ctx.Sink.Emit(OpCodes.Ldstr, key);
                                var value = (CommonTree) child.Children[i + 1];
                                var exp = new Expression(value);
                                Errors.AddRange(exp.Generate(Ctx));
                                Ctx.Sink.Emit(OpCodes.Callvirt, method);
                            }
                            Ctx.Sink.FastEmitLoadLocal(swap.LocalIndex);
                        }
                        var ctor =
                            typeof (DictType).GetConstructor(
                                new[] {typeof (Dictionary<string, object>)});
                        Ctx.Sink.Emit(OpCodes.Newobj, ctor);
                        break;
                    }
                    case TemplateParser.ConstList:
                    {
                        var listCtor =
                            typeof (List<object>).GetConstructor(
                                new Type[0]);
                        Ctx.Sink.Emit(OpCodes.Newobj, listCtor);
                        if (child.Children[0].Type
                            != TemplateParser.Empty)
                        {
                            var method = typeof (List<object>).GetMethod(
                                "Add",
                                BindingFlags.Public | BindingFlags.Instance);
                            var swap = Ctx.Sink.DeclareLocal(
                                typeof (List<object>));
                            Ctx.Sink.FastEmitStoreLocal(swap.LocalIndex);
                            foreach (var element in child.Children)
                            {
                                Ctx.Sink.FastEmitLoadLocal(swap.LocalIndex);
                                var exp = new Expression(element);
                                Errors.AddRange(exp.Generate(Ctx));
                                Ctx.Sink.Emit(OpCodes.Callvirt, method);
                            }
                            Ctx.Sink.FastEmitLoadLocal(swap.LocalIndex);
                        }
                        var ctor =
                            typeof (ListType).GetConstructor(
                                new[] {typeof (List<object>)});
                        Ctx.Sink.Emit(OpCodes.Newobj, ctor);
                        break;
                    }
                    default:
                    {
                        Errors.ErrorUnknownKeyword(child.Text);
                        Ctx.EmitNullRef();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Ctx.EmitNullRef();
                Errors.Add(
                    Error.NewError(
                        "R999", "Unable to create constant expression", e));
            }
        }
    }
}