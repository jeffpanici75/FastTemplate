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
using System.Reflection.Emit;
using Antlr.Runtime.Tree;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Compiler
{
    internal class Statement : Emitter
    {
        public Statement(ITree tree)
            : base(tree)
        {
        }

        public ErrorList GenerateSet(Context ctx, Expression exp)
        {
            return GenerateSet(ctx, c => Errors.AddRange(exp.Generate(c)));
        }

        public ErrorList GenerateSet(Context ctx, int localIndex)
        {
            return GenerateSet(
                ctx,
                c => c.Sink.FastEmitLoadLocal(
                    localIndex));
        }

        public ErrorList GenerateSet(Context ctx, Action<Context> exp)
        {
            Ctx = ctx;
            Errors.Clear();
            var errors = new ErrorList();
            if (T.Type == 0)
            {
                var error = (CommonErrorNode) T;
                errors.ErrorParse(error);
                return Errors;
            }
            var child = (CommonTree) T.Children[0];
            switch (child.Type)
            {
                case 0:
                {
                    var error = (CommonErrorNode) child;
                    errors.ErrorParse(error);
                    break;
                }
                case TemplateLexer.MStart:
                case TemplateLexer.EStart:
                {
                    EmitSet(child, exp);
                    //pop off voidtype from final function in chain
                    break;
                }
                default:
                {
                    break;
                }
            }
            return Errors;
        }

        protected override void OnGenerate()
        {
            var errors = new ErrorList();
            if (T.Type == 0)
            {
                var error = (CommonErrorNode) T;
                errors.ErrorParse(error);
                Ctx.EmitNullRef();
                return;
            }
            var child = (CommonTree) T.Children[0];
            switch (child.Type)
            {
                case 0:
                {
                    var error = (CommonErrorNode) child;
                    errors.ErrorParse(error);
                    Ctx.EmitNullRef();
                    break;
                }
                case TemplateLexer.MStart:
                case TemplateLexer.EStart:
                {
                    EmitStatement(child);
                    if (child.Type == TemplateLexer.MStart)
                    {
                        EmitDoMacro();
                    }
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        private void EmitDoMacro()
        {
            var swap = Ctx.Sink.DeclareLocal(typeof (ITemplateType));
            Ctx.Sink.FastEmitStoreLocal(swap.LocalIndex);
            Ctx.Sink.Emit(OpCodes.Ldarg_0);
            Ctx.Sink.FastEmitLoadLocal(swap.LocalIndex);
            Ctx.EmitVTFunctionCall("EvalMacro");
        }

        private void EmitSet(CommonTree s, Action<Context> emitRVal)
        {
            var root = s.Children[0].Text;
            if (s.Children.Count < 2)
            {
                Ctx.Sink.Emit(OpCodes.Ldarg_0);
                Ctx.Sink.Emit(OpCodes.Ldstr, root);
                emitRVal(Ctx);
                Ctx.EmitVTFunctionCall("SetState");
                return;
            }
            Ctx.Sink.Emit(OpCodes.Ldarg_0);
            Ctx.Sink.Emit(OpCodes.Ldstr, root);
            Ctx.EmitVTFunctionCall("GetRootObject");
            var swapSpace = Ctx.Sink.DeclareLocal(typeof (ITemplateType));
            for (var i = 1; i < s.Children.Count; i++)
            {
                var current = (CommonTree) s.Children[i];
                switch (current.Type)
                {
                    case 0:
                    {
                        Errors.ErrorParse((CommonErrorNode) current);
                        break;
                    }
                    case TemplateParser.Invoke:
                    {
                        if (i == s.Children.Count - 1)
                        {
                            Errors.ErrorUnableToSetIntoFunctionResult(
                                "unknown type", current.Text);
                            return;
                        }
                        EmitFunctionInvoke(swapSpace, "Invoke", current);
                        break;
                    }
                    case TemplateParser.Indexer:
                    {
                        if (i == s.Children.Count - 1)
                        {
                            EmitFunctionInvoke(swapSpace, "set_Item", current, emitRVal);
                            Ctx.Sink.Emit(OpCodes.Pop);
                            return;
                        }
                        EmitFunctionInvoke(swapSpace, "get_Item", current);
                        break;
                    }
                    case TemplateParser.Prop:
                    {
                        if (i == s.Children.Count - 1)
                        {
                            EmitFunctionInvoke(
                                swapSpace,
                                string.Format("set_{0}",
                                    current.Text),
                                null,
                                emitRVal);
                            Ctx.Sink.Emit(OpCodes.Pop);
                            return;
                        }
                        var next = (CommonTree) s.Children[i + 1];
                        switch (next.Type)
                        {
                            case TemplateParser.Invoke:
                            {
                                EmitFunctionInvoke(swapSpace, current.Text, next);
                                i++;
                                break;
                            }
                            default:
                            {
                                EmitFunctionInvoke(
                                    swapSpace,
                                    string.Format("get_{0}",
                                        current.Text),
                                    null);
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            Errors.ErrorUnableToSetIntoFunctionResult(
                "type only known at runtime",
                s.Children[s.Children.Count - 2].Text);
        }

        private void EmitStatement(CommonTree s)
        {
            var root = s.Children[0].Text;
            Ctx.Sink.Emit(OpCodes.Ldarg_0);
            Ctx.Sink.Emit(OpCodes.Ldstr, root);
            Ctx.EmitVTFunctionCall("GetRootObject");
            var swapSpace = Ctx.Sink.DeclareLocal(typeof (ITemplateType));
            for (var i = 1; i < s.Children.Count; i++)
            {
                var current = (CommonTree) s.Children[i];
                switch (current.Type)
                {
                    case 0:
                    {
                        Errors.ErrorParse((CommonErrorNode) current);
                        Ctx.EmitNullRef();
                        break;
                    }
                    case TemplateParser.Invoke:
                    {
                        EmitFunctionInvoke(swapSpace, "Invoke", current);
                        break;
                    }
                    case TemplateParser.Indexer:
                    {
                        EmitFunctionInvoke(swapSpace, "get_Item", current);
                        break;
                    }
                    case TemplateParser.Prop:
                    {
                        if (i == s.Children.Count - 1)
                        {
                            EmitFunctionInvoke(
                                swapSpace,
                                string.Format("get_{0}",
                                    current.Text),
                                null);
                            break;
                        }
                        var next = (CommonTree) s.Children[i + 1];
                        switch (next.Type)
                        {
                            case TemplateParser.Invoke:
                            {
                                EmitFunctionInvoke(swapSpace, current.Text, next);
                                i++;
                                break;
                            }
                            default:
                            {
                                EmitFunctionInvoke(
                                    swapSpace,
                                    string.Format("get_{0}",
                                        current.Text),
                                    null);
                                break;
                            }
                        }
                        break;
                    }
                }
            }
        }

        private void EmitFunctionInvoke(
            LocalBuilder swapSpace,
            string name,
            CommonTree next)
        {
            EmitFunctionInvoke(swapSpace, name, next, null);
        }

        private void EmitFunctionInvoke(
            LocalBuilder swapSpace,
            string name,
            CommonTree next,
            Action<Context> lastArg)
        {
            Ctx.Sink.FastEmitStoreLocal(swapSpace.LocalIndex);
            Ctx.Sink.Emit(OpCodes.Ldarg_0);
            Ctx.Sink.FastEmitLoadLocal(swapSpace.LocalIndex);
            Ctx.Sink.Emit(OpCodes.Ldstr, name);
            var args = next;
            if (args != null
                && args.Children != null
                && args.Children.Count > 0)
            {
                var fnArgs = (CommonTree) args.Children[0];
                if ((Ctx.OptimizeLevel & OptimizeLevel.FixArgList) == OptimizeLevel.FixArgList)
                {
                    if (fnArgs.Type == 0)
                    {
                        Errors.ErrorParse((CommonErrorNode) fnArgs);
                        Ctx.EmitNullRef();
                        Ctx.EmitVTFunctionCall("InvokeFunction1");
                        return;
                    }
                    var hasLast = lastArg != null;
                    var argCount = hasLast ? fnArgs.Children.Count + 1 : fnArgs.Children.Count;
                    switch (argCount)
                    {
                        case 0:
                            Ctx.EmitVTFunctionCall("InvokeFunction0");
                            break;
                        case 1:
                            Errors.AddRange(Ctx.EmitFlatArgList(fnArgs));
                            if (lastArg != null)
                                lastArg(Ctx);
                            Ctx.EmitVTFunctionCall("InvokeFunction1");
                            break;
                        case 2:
                            Errors.AddRange(Ctx.EmitFlatArgList(fnArgs));
                            if (lastArg != null)
                                lastArg(Ctx);
                            Ctx.EmitVTFunctionCall("InvokeFunction2");
                            break;
                        case 3:
                            Errors.AddRange(Ctx.EmitFlatArgList(fnArgs));
                            if (lastArg != null)
                                lastArg(Ctx);
                            Ctx.EmitVTFunctionCall("InvokeFunction3");
                            break;
                        case 4:
                            Errors.AddRange(Ctx.EmitFlatArgList(fnArgs));
                            if (lastArg != null)
                                lastArg(Ctx);
                            Ctx.EmitVTFunctionCall("InvokeFunction4");
                            break;
                        case 5:
                            Errors.AddRange(Ctx.EmitFlatArgList(fnArgs));
                            if (lastArg != null)
                                lastArg(Ctx);
                            Ctx.EmitVTFunctionCall("InvokeFunction5");
                            break;
                        case 6:
                            Errors.AddRange(Ctx.EmitFlatArgList(fnArgs));
                            if (lastArg != null)
                                lastArg(Ctx);
                            Ctx.EmitVTFunctionCall("InvokeFunction6");
                            break;
                        case 7:
                            Errors.AddRange(Ctx.EmitFlatArgList(fnArgs));
                            if (lastArg != null)
                                lastArg(Ctx);
                            Ctx.EmitVTFunctionCall("InvokeFunction7");
                            break;
                        case 8:
                            Errors.AddRange(Ctx.EmitFlatArgList(fnArgs));
                            if (lastArg != null)
                                lastArg(Ctx);
                            Ctx.EmitVTFunctionCall("InvokeFunction8");
                            break;
                        case 9:
                            Errors.AddRange(Ctx.EmitFlatArgList(fnArgs));
                            if (lastArg != null)
                                lastArg(Ctx);
                            Ctx.EmitVTFunctionCall("InvokeFunction9");
                            break;
                        case 10:
                            Errors.AddRange(Ctx.EmitFlatArgList(fnArgs));
                            if (lastArg != null)
                                lastArg(Ctx);
                            Ctx.EmitVTFunctionCall("InvokeFunction10");
                            break;
                        case 11:
                            Errors.AddRange(Ctx.EmitFlatArgList(fnArgs));
                            if (lastArg != null)
                                lastArg(Ctx);
                            Ctx.EmitVTFunctionCall("InvokeFunction11");
                            break;
                        case 12:
                            Errors.AddRange(Ctx.EmitFlatArgList(fnArgs));
                            if (lastArg != null)
                                lastArg(Ctx);
                            Ctx.EmitVTFunctionCall("InvokeFunction12");
                            break;
                        case 13:
                            Errors.AddRange(Ctx.EmitFlatArgList(fnArgs));
                            if (lastArg != null)
                                lastArg(Ctx);
                            Ctx.EmitVTFunctionCall("InvokeFunction13");
                            break;
                        case 14:
                            Errors.AddRange(Ctx.EmitFlatArgList(fnArgs));
                            if (lastArg != null)
                                lastArg(Ctx);
                            Ctx.EmitVTFunctionCall("InvokeFunction14");
                            break;
                        case 15:
                            Errors.AddRange(Ctx.EmitFlatArgList(fnArgs));
                            if (lastArg != null)
                                lastArg(Ctx);
                            Ctx.EmitVTFunctionCall("InvokeFunction15");
                            break;
                        case 16:
                            Errors.AddRange(Ctx.EmitFlatArgList(fnArgs));
                            if (lastArg != null)
                                lastArg(Ctx);
                            Ctx.EmitVTFunctionCall("InvokeFunction16");
                            break;
                        default:
                        {
                            ErrorList e;
                            if (lastArg == null)
                            {
                                e = Ctx.EmitArgList(fnArgs);
                            }
                            else
                            {
                                e = Ctx.EmitArgList(fnArgs, lastArg);
                            }
                            Errors.AddRange(e);
                            Ctx.EmitVTFunctionCall("InvokeFunction");
                            break;
                        }
                    }
                }
                else
                {
                    if (lastArg == null)
                    {
                        var e = Ctx.EmitArgList(fnArgs);
                        Errors.AddRange(e);
                    }
                    else
                    {
                        var e = Ctx.EmitArgList(fnArgs, lastArg);
                        Errors.AddRange(e);
                    }
                    Ctx.EmitVTFunctionCall("InvokeFunction");
                }
            }
            else
            {
                if ((Ctx.OptimizeLevel & OptimizeLevel.FixArgList) == OptimizeLevel.FixArgList)
                {
                    if (lastArg != null)
                    {
                        lastArg(Ctx);
                        Ctx.EmitVTFunctionCall("InvokeFunction1");
                    }
                    else
                    {
                        Ctx.EmitVTFunctionCall("InvokeFunction0");
                    }
                }
                else
                {
                    if (lastArg == null)
                    {
                        Ctx.EmitEmptyArgList();
                        Ctx.EmitVTFunctionCall("InvokeFunction");
                    }
                    else
                    {
                        Errors.AddRange(Ctx.EmitSingleArgList(lastArg));
                        Ctx.EmitVTFunctionCall("InvokeFunction");
                    }
                }
            }
        }
    }
}