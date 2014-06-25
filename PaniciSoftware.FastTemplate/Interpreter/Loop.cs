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
using System.Text;
using Antlr.Runtime.Tree;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Interpreter
{
    internal class Loop : Evaluator
    {
        public Loop(ITree tree) : base(tree)
        {
        }

        protected override void OnEval(out ITemplateType result)
        {
            result = StringType.Empty();
            var child = T;
            Ctx.EnterValidJumpScope();
            Ctx.UseJump();
            var loopArgs = (CommonTree) child.Children[0];
            var lower = (CommonTree) loopArgs.Children[0];
            if (lower.Type == 0)
            {
                Errors.ErrorParse((CommonErrorNode) lower);
                return;
            }

            int lowerValue;
            if (!ToInt(lower, out lowerValue))
                return;

            var upper = (CommonTree) loopArgs.Children[1];
            if (upper.Type == 0)
            {
                Errors.ErrorParse((CommonErrorNode) upper);
                return;
            }

            int upperValue;
            if (!ToInt(upper, out upperValue))
                return;

            var var = (CommonTree) loopArgs.Children[2];
            if (var.Type == 0)
            {
                Errors.ErrorParse((CommonErrorNode) var);
                return;
            }

            int step;
            if (loopArgs.ChildCount > 3)
            {
                var stepExp = (CommonTree) loopArgs.Children[3];
                if (!ToInt(stepExp, out step))
                    return;
            }
            else
            {
                step = 1;
            }

            var buffer = new StringBuilder();
            var defaultBlock = (CommonTree) child.Children[1];
            var blocks = new Dictionary<LoopDirective, CommonTree>();
            for (var i = 2; i < child.Children.Count; i++)
            {
                var targeted = (CommonTree) child.Children[i];
                var ins = targeted.Children[0];
                var tBlock = (CommonTree) targeted.Children[1];
                LoopDirective d;
                if (!Enum.TryParse(ins.Text, true, out d))
                {
                    Errors.ErrorUnknownLoopDirective(ins.Text);
                    break;
                }
                blocks[d] = tBlock;
            }

            if (Errors.ContainsError())
                return;
            if (lowerValue >= upperValue)
            {
                var s = new Statement(var);
                Errors.AddRange(s.SetTargetObject(Interpreter, new IntType {Value = lowerValue}));
                TryDirective(
                    LoopDirective.NoData,
                    blocks,
                    buffer);
                if (Ctx.Jump == JumpType.Stop)
                {
                    Ctx.UseJump();
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
                    Ctx.UseJump();
                    return;
                }
                var parityIndex = -1;
                var s = new Statement(var);
                for (var i = lowerValue; i <= upperValue; i += step)
                {
                    Errors.AddRange(s.SetTargetObject(Interpreter, new IntType {Value = i}));
                    parityIndex++;

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
                    var e = new Block(defaultBlock.Children[0]).Eval(Ctx, Interpreter, out iter);
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
                        continue;
                    }
                }
                Errors.AddRange(s.SetTargetObject(Interpreter, new IntType {Value = upperValue}));
                TryDirective(
                    LoopDirective.AfterAll,
                    blocks,
                    buffer);
                if (Ctx.Jump == JumpType.Stop)
                {
                    return;
                }
            }
            result = StringType.New(buffer.ToString());
            Ctx.ExitValidJumpScope();
        }

        private bool ToInt(
            CommonTree tree,
            out int i)
        {
            i = 0;
            var expression = new Expression(tree);
            ITemplateType o;
            var e = expression.Eval(Ctx, Interpreter, out o);
            Errors.AddRange(e);
            if (e.ContainsError())
                return false;
            int conversion;
            if (!o.TryConvert(out conversion))
            {
                Errors.ErrorTypeMismatch("integer", o.UnderlyingType.Name);
                return false;
            }
            i = conversion;
            return true;
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