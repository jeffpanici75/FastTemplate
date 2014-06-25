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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime.Tree;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Interpreter
{
    internal class Control : Evaluator
    {
        public Control(ITree tree)
            : base(tree)
        {
        }

        protected override void OnEval(out ITemplateType result)
        {
            result = StringType.Empty();
            var child = (CommonTree) T.Children[0];
            switch (child.Type)
            {
                case 0:
                {
                    Errors.ErrorParse((CommonErrorNode) child);
                    result = StringType.Empty();
                    break;
                }
                case TemplateLexer.Assert:
                {
                    var a = new Assert(child);
                    Errors.AddRange(a.Eval(Ctx, Interpreter, out result));
                    break;
                }
                case TemplateLexer.Pragma:
                {
                    var a = new Pragma(child);
                    Errors.AddRange(a.Eval(Ctx, Interpreter, out result));
                    break;
                }
                case TemplateLexer.Continue:
                {
                    if (!Ctx.AreJumpsValid())
                    {
                        Errors.ErrorInvalidJump("#continue", child.Token);
                    }
                    else
                    {
                        Ctx.Jump = JumpType.Continue;
                    }
                    result = StringType.Empty();
                    break;
                }
                case TemplateLexer.Break:
                {
                    if (!Ctx.AreJumpsValid())
                    {
                        Errors.ErrorInvalidJump("#break", child.Token);
                    }
                    else
                    {
                        Ctx.Jump = JumpType.Break;
                    }
                    result = StringType.Empty();
                    break;
                }
                case TemplateLexer.Stop:
                {
                    Ctx.Jump = JumpType.Stop;
                    result = StringType.Empty();
                    break;
                }
                case TemplateLexer.Parse:
                {
                    result = DoFileArgs(child, false);
                    break;
                }
                case TemplateLexer.Include:
                {
                    result = DoFileArgs(child, true);
                    break;
                }
                case TemplateLexer.Set:
                {
                    var target = child.Children[0];
                    if (target.Type == 0)
                    {
                        Errors.ErrorParse((CommonErrorNode) target);
                        break;
                    }

                    var source = child.Children[1];
                    if (source.Type == 0)
                    {
                        Errors.ErrorParse((CommonErrorNode) source);
                        break;
                    }

                    var exp = new Expression(source);
                    ITemplateType o;
                    var e = exp.Eval(Ctx, Interpreter, out o);
                    Errors.AddRange(e);
                    if (e.ContainsError())
                        break;
                    var s = new Statement(target);
                    Errors.AddRange(s.SetTargetObject(Interpreter, o));
                    result = StringType.Empty();
                    break;
                }
                case TemplateLexer.Loop:
                {
                    var l = new Loop(child);
                    Errors.AddRange(l.Eval(Ctx, Interpreter, out result));
                    break;
                }
                case TemplateLexer.Foreach:
                {
                    Ctx.EnterValidJumpScope();
                    Ctx.UseJump();
                    var var = (CommonTree) child.Children[0];
                    if (var.Type == 0)
                    {
                        Errors.ErrorParse((CommonErrorNode) var);
                        result = StringType.Empty();
                        break;
                    }
                    var exp = (CommonTree) child.Children[1];
                    if (exp.Type == 0)
                    {
                        Errors.ErrorParse((CommonErrorNode) exp);
                        result = StringType.Empty();
                        break;
                    }

                    var buffer = new StringBuilder();
                    var defaultBlock = (CommonTree) child.Children[2];
                    var blocks = new Dictionary<LoopDirective, CommonTree>();
                    for (var i = 3; i < child.Children.Count; i++)
                    {
                        var targeted = (CommonTree) child.Children[i];
                        var ins = targeted.Children[0];
                        var tBlock = (CommonTree) targeted.Children[1];
                        LoopDirective d;
                        if (!Enum.TryParse(ins.Text, true, out d))
                        {
                            Errors.ErrorUnknownLoopDirective(ins.Text);
                            result = StringType.Empty();
                            break;
                        }
                        blocks[d] = tBlock;
                    }
                    if (Errors.ContainsError())
                        break;

                    ITemplateType o;
                    var expErrors = new Expression(exp).Eval(Ctx, Interpreter, out o);
                    Errors.AddRange(expErrors);
                    if (expErrors.ContainsError())
                    {
                        result = StringType.Empty();
                        break;
                    }

                    if (o is NullType)
                    {
                        TryDirective(
                            LoopDirective.NoData,
                            blocks,
                            buffer);
                        if (Ctx.Jump == JumpType.Stop)
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (!(o.RawValue is IEnumerable))
                        {
                            Errors.ErrorIEnumerableRequired(exp.Text);
                            result = StringType.Empty();
                            break;
                        }
                        var seq = (IEnumerable) o.RawValue;
                        var list = seq.Cast<object>().ToList();
                        if (list.Count < 1)
                        {
                            TryDirective(
                                LoopDirective.NoData,
                                blocks,
                                buffer);
                            if (Ctx.Jump == JumpType.Stop)
                            {
                                return;
                            }
                        }
                        else
                        {
                            TryDirective(
                                LoopDirective.BeforeAll,
                                blocks,
                                buffer);
                            if (Ctx.Jump == JumpType.Stop)
                            {
                                return;
                            }
                            var parityIndex = -1;
                            foreach (var i in list)
                            {
                                parityIndex++;
                                if (i is char)
                                {
                                    var s = new Statement(var);
                                    Errors.AddRange(s.SetTargetObject(Interpreter,
                                        StringType.New(new string((char) i, 0))));
                                }
                                else
                                {
                                    var s = new Statement(var);
                                    Errors.AddRange(s.SetTargetObject(Interpreter, PrimitiveType.Create(i)));
                                }
                                TryDirective(
                                    LoopDirective.Each,
                                    blocks,
                                    buffer);
                                if (Ctx.Jump == JumpType.Stop)
                                {
                                    return;
                                }
                                if (Ctx.Jump == JumpType.Break)
                                {
                                    Ctx.UseJump();
                                    break;
                                }
                                if (Ctx.Jump == JumpType.Continue)
                                {
                                    Ctx.UseJump();
                                    continue;
                                }

                                TryDirective(
                                    LoopDirective.Before,
                                    blocks,
                                    buffer);
                                if (Ctx.Jump == JumpType.Stop)
                                {
                                    return;
                                }
                                if (Ctx.Jump == JumpType.Break)
                                {
                                    Ctx.UseJump();
                                    break;
                                }
                                if (Ctx.Jump == JumpType.Continue)
                                {
                                    Ctx.UseJump();
                                    continue;
                                }
                                if (parityIndex%2 == 0)
                                {
                                    TryDirective(
                                        LoopDirective.Even,
                                        blocks,
                                        buffer);
                                    if (Ctx.Jump == JumpType.Stop)
                                    {
                                        return;
                                    }
                                    if (Ctx.Jump == JumpType.Break)
                                    {
                                        Ctx.UseJump();
                                        break;
                                    }
                                    if (Ctx.Jump == JumpType.Continue)
                                    {
                                        Ctx.UseJump();
                                        continue;
                                    }
                                }
                                else
                                {
                                    TryDirective(
                                        LoopDirective.Odd,
                                        blocks,
                                        buffer);
                                    if (Ctx.Jump == JumpType.Stop)
                                    {
                                        return;
                                    }
                                    if (Ctx.Jump == JumpType.Break)
                                    {
                                        Ctx.UseJump();
                                        break;
                                    }
                                    if (Ctx.Jump == JumpType.Continue)
                                    {
                                        Ctx.UseJump();
                                        continue;
                                    }
                                }

                                ITemplateType iter;
                                var block = defaultBlock.Children[0];
                                var e = new Block(block).Eval(Ctx, Interpreter, out iter);
                                if (e.ContainsError())
                                    break;
                                Errors.AddRange(e);
                                if (!iter.WriteTo(buffer))
                                {
                                    Errors.ErrorIEnumerableRequired(string.Format("Unable to convert {0} to a string.",
                                        iter.RawValue));
                                }
                                if (Ctx.Jump == JumpType.Stop)
                                {
                                    return;
                                }
                                if (Ctx.Jump == JumpType.Break)
                                {
                                    Ctx.UseJump();
                                    break;
                                }
                                if (Ctx.Jump == JumpType.Continue)
                                {
                                    Ctx.UseJump();
                                    continue;
                                }
                                TryDirective(
                                    LoopDirective.After,
                                    blocks,
                                    buffer);
                                if (Ctx.Jump == JumpType.Stop)
                                {
                                    return;
                                }
                                if (Ctx.Jump == JumpType.Break)
                                {
                                    Ctx.UseJump();
                                    break;
                                }
                                if (Ctx.Jump == JumpType.Continue)
                                {
                                    Ctx.UseJump();
                                    continue;
                                }
                                TryDirective(
                                    LoopDirective.Between,
                                    blocks,
                                    buffer);
                                if (Ctx.Jump == JumpType.Stop)
                                {
                                    return;
                                }
                                if (Ctx.Jump == JumpType.Break)
                                {
                                    Ctx.UseJump();
                                    break;
                                }
                                if (Ctx.Jump == JumpType.Continue)
                                {
                                    Ctx.UseJump();
                                }
                            }
                            TryDirective(
                                LoopDirective.AfterAll,
                                blocks,
                                buffer);
                            if (Ctx.Jump == JumpType.Stop)
                            {
                                return;
                            }
                        }
                    }
                    result = StringType.New(buffer.ToString());
                    Ctx.ExitValidJumpScope();
                    break;
                }
                case TemplateLexer.If:
                {
                    bool obj;
                    ITemplateType o;
                    {
                        var cond = (CommonTree) child.Children[0];
                        var expression = new Expression(cond);
                        var errors = expression.Eval(Ctx, Interpreter, out o);
                        Errors.AddRange(errors);
                        if (Errors.ContainsError())
                            break;
                    }

                    if (!o.TryConvert(out obj))
                    {
                        Errors.ErrorTypeMismatch("bool", o.GetType().Name);
                        break;
                    }
                    var ifCondResult = obj;
                    if (ifCondResult)
                    {
                        var block = (CommonTree) child.Children[1];
                        var e = new Block(block).Eval(Ctx, Interpreter, out result);
                        Errors.AddRange(e);
                        if (Ctx.Jump == JumpType.Stop)
                        {
                            return;
                        }
                    }
                    else if (child.Children.Count > 2)
                    {
                        var hasElse = child.Children[2].Type == TemplateParser.Else;
                        var doElse = hasElse;
                        var elseIfStart = hasElse ? 3 : 2;
                        for (var i = elseIfStart;
                            i < child.Children.Count;
                            i++)
                        {
                            var elseIf = (CommonTree) child.Children[i];
                            var cond = (CommonTree) elseIf.Children[0];
                            var expression = new Expression(cond);
                            var errors = expression.Eval(Ctx, Interpreter, out o);
                            Errors.AddRange(errors);
                            if (Errors.ContainsError())
                                break;
                            if (!o.TryConvert(out obj))
                            {
                                Errors.ErrorTypeMismatch("bool", o.UnderlyingType.Name);
                            }
                            var elseIfCondResult = obj;
                            if (elseIfCondResult)
                            {
                                var block = (CommonTree) elseIf.Children[1];
                                var e = new Block(block).Eval(Ctx, Interpreter, out result);
                                Errors.AddRange(e);
                                if (Ctx.Jump == JumpType.Stop)
                                {
                                    return;
                                }
                                doElse = false;
                                break;
                            }
                        }
                        if (doElse)
                        {
                            var elseTree = (CommonTree) child.Children[2];
                            if (elseTree.Type == 0)
                            {
                                Errors.ErrorParse((CommonErrorNode) elseTree);
                            }
                            else
                            {
                                var block = (CommonTree) elseTree.Children[0];
                                var e = new Block(block).Eval(Ctx, Interpreter, out result);
                                Errors.AddRange(e);
                                if (Ctx.Jump == JumpType.Stop)
                                {
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        result = StringType.Empty();
                    }
                    break;
                }
            }
        }

        private ITemplateType DoFileArgs(
            CommonTree child,
            bool isRaw)
        {
            var args = child.Children[0];
            if (args.Type == 0)
            {
                Errors.ErrorParse((CommonErrorNode) args);
                return StringType.Empty();
            }

            var buffer = new StringBuilder();
            var templates = FnArgs.Extract((CommonTree) args);

            foreach (var e in templates)
            {
                ITemplateType o;

                var errors = e.Eval(Ctx, Interpreter, out o);
                Errors.AddRange(errors);

                if (errors.ContainsError())
                    break;

                string templateName;
                if (!o.TryConvert(out templateName))
                {
                    Errors.ErrorTypeMismatch("string", o.RawValue.GetType().Name);
                    break;
                }

                var cacheResult = Interpreter.Cache.GetTemplate(templateName);
                if (!cacheResult.Success)
                {
                    Errors.ErrorRuntimeError(string.Format("Template: {0} not found by specified resource resolver.", templateName));
                    Errors.AddRange(cacheResult.Errors);
                    break;
                }

                var virtualTemplate = cacheResult.Template;
                if (virtualTemplate != null)
                {
                    if (isRaw)
                    {
                        buffer.Append(cacheResult.Template.Source);
                    }
                    else
                    {
                        var result = virtualTemplate.Execute(S);
                        Errors.AddRange(result.Errors);
                        if (!Errors.ContainsError())
                        {
                            buffer.Append(result.Output);
                        }
                    }
                }
            }

            return StringType.New(buffer.ToString());
        }

        private void TryDirective(
            LoopDirective d,
            Dictionary<LoopDirective, CommonTree> blocks,
            StringBuilder buffer)
        {
            CommonTree b;
            if (blocks.TryGetValue(d, out b))
            {
                ITemplateType iter;
                var block = new Block(b);
                var e = block.Eval(Ctx, Interpreter, out iter);
                if (e.ContainsError())
                {
                    return;
                }
                Errors.AddRange(e);
                if (!iter.WriteTo(buffer))
                {
                    Errors.ErrorTypeMismatch("string", iter.UnderlyingType.Name);
                }
            }
        }
    }
}