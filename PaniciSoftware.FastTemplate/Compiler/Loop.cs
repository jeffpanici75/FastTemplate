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
using System.Reflection.Emit;
using Antlr.Runtime.Tree;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Compiler
{
    internal class Loop : Emitter
    {
        public Loop(ITree tree)
            : base(tree)
        {
        }

        protected override void OnGenerate()
        {
            var loopArgs = (CommonTree) T.Children[0];
            var lower = (CommonTree) loopArgs.Children[0];
            if (lower.Type == 0)
            {
                Errors.ErrorParse((CommonErrorNode) lower);
                return;
            }

            var upper = (CommonTree) loopArgs.Children[1];
            if (upper.Type == 0)
            {
                Errors.ErrorParse((CommonErrorNode) upper);
                return;
            }

            var var = (CommonTree) loopArgs.Children[2];
            if (var.Type == 0)
            {
                Errors.ErrorParse((CommonErrorNode) var);
                return;
            }

            CommonTree stepTree;
            if (loopArgs.ChildCount > 3)
            {
                stepTree = (CommonTree) loopArgs.Children[3];
            }
            else
            {
                stepTree = null;
            }

            var defaultBlock = (CommonTree) T.Children[1];
            var blocks = new Dictionary<LoopDirective, CommonTree>();
            for (var i = 2; i < T.Children.Count; i++)
            {
                var targeted = (CommonTree) T.Children[i];
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
            {
                return;
            }

            var endOfLoop = Ctx.Sink.DefineLabel();
            var continuePoint = Ctx.Sink.DefineLabel();
            var checkCond = Ctx.Sink.DefineLabel();
            var top = Ctx.Sink.DefineLabel();
            var oddEvenVar = Ctx.Sink.DeclareLocal(typeof (int));

            Ctx.PushNewJumpScope(continuePoint, endOfLoop);
            try
            {
                //init part here
                var stateVar = Ctx.Sink.DeclareLocal(typeof (ITemplateType));
                var step = Ctx.Sink.DeclareLocal(typeof (ITemplateType));

                var lowerVar = Ctx.Sink.DeclareLocal(typeof (ITemplateType));
                var upperVar = Ctx.Sink.DeclareLocal(typeof (ITemplateType));

                Ctx.Sink.FastEmitConstInt(0);
                Ctx.Sink.FastEmitStoreLocal(oddEvenVar.LocalIndex);

                if (stepTree == null)
                {
                    Ctx.EmitConstIntType(1);
                }
                else
                {
                    var stepExp = new Expression(stepTree);
                    Errors.AddRange(stepExp.Generate(Ctx));
                }
                Ctx.Sink.FastEmitStoreLocal(step.LocalIndex);

                var lowerExp = new Expression(lower);
                Errors.AddRange(lowerExp.Generate(Ctx));
                Ctx.Sink.FastEmitStoreLocal(lowerVar.LocalIndex);

                var upperExp = new Expression(upper);
                Errors.AddRange(upperExp.Generate(Ctx));
                Ctx.Sink.FastEmitStoreLocal(upperVar.LocalIndex);

                var initVar = new Statement(var);
                Errors.AddRange(
                    initVar.GenerateSet(Ctx, lowerVar.LocalIndex));

                Ctx.Sink.FastEmitLoadLocal(lowerVar.LocalIndex);
                Ctx.Sink.FastEmitStoreLocal(stateVar.LocalIndex);

                var noDataSkip = Ctx.Sink.DefineLabel();

                Ctx.Sink.FastEmitLoadLocal(lowerVar.LocalIndex);
                Ctx.Sink.FastEmitLoadLocal(upperVar.LocalIndex);
                Ctx.EmitApplyBinary(Operator.GT);
                Ctx.EmitUnwrapBool();
                Ctx.Sink.Emit(OpCodes.Brtrue, noDataSkip);

                TryEmitBlock(LoopDirective.BeforeAll, blocks);

                Ctx.Sink.Emit(OpCodes.Br, checkCond);

                Ctx.Sink.MarkLabel(top);

                TryEmitBlock(LoopDirective.Each, blocks);

                if (blocks.ContainsKey(LoopDirective.Between))
                {
                    var skipFirstBetween = Ctx.Sink.DefineLabel();
                    Ctx.Sink.FastEmitLoadLocal(oddEvenVar.LocalIndex);
                    Ctx.Sink.FastEmitConstInt(0);
                    Ctx.Sink.Emit(OpCodes.Ceq);
                    Ctx.Sink.Emit(OpCodes.Brtrue, skipFirstBetween);
                    TryEmitBlock(LoopDirective.Between, blocks);
                    Ctx.Sink.MarkLabel(skipFirstBetween);
                }

                TryEmitBlock(LoopDirective.Before, blocks);

                //even odd
                if (blocks.ContainsKey(LoopDirective.Even)
                    || blocks.ContainsKey(LoopDirective.Odd))
                {
                    var oddBlock = Ctx.Sink.DefineLabel();
                    var endOddEvenBlock = Ctx.Sink.DefineLabel();

                    Ctx.Sink.FastEmitLoadLocal(oddEvenVar.LocalIndex);
                    Ctx.Sink.FastEmitConstInt(2);
                    Ctx.Sink.Emit(OpCodes.Rem);
                    Ctx.Sink.FastEmitConstInt(0);
                    Ctx.Sink.Emit(OpCodes.Ceq);
                    Ctx.Sink.Emit(OpCodes.Brfalse, oddBlock);

                    TryEmitBlock(LoopDirective.Even, blocks);
                    Ctx.Sink.Emit(OpCodes.Br, endOddEvenBlock);

                    Ctx.Sink.MarkLabel(oddBlock);
                    TryEmitBlock(LoopDirective.Odd, blocks);
                    Ctx.Sink.MarkLabel(endOddEvenBlock);
                }

                var defaultBlk = new Block(defaultBlock.Children[0]);
                Errors.AddRange(defaultBlk.Generate(Ctx));

                TryEmitBlock(LoopDirective.After, blocks);

                Ctx.Sink.MarkLabel(continuePoint);

                //increment odd even var
                Ctx.Sink.Emit(OpCodes.Ldc_I4_1);
                Ctx.Sink.FastEmitLoadLocal(oddEvenVar.LocalIndex);
                Ctx.Sink.Emit(OpCodes.Add);
                Ctx.Sink.FastEmitStoreLocal(oddEvenVar.LocalIndex);

                //increment
                Ctx.Sink.FastEmitLoadLocal(stateVar.LocalIndex);
                Ctx.Sink.FastEmitLoadLocal(step.LocalIndex);
                Ctx.EmitApplyBinary(Operator.Plus);
                Ctx.Sink.FastEmitStoreLocal(stateVar.LocalIndex);
                var s = new Statement(var);
                Errors.AddRange(s.GenerateSet(Ctx, stateVar.LocalIndex));


                //check condidtion
                Ctx.Sink.MarkLabel(checkCond);

                Ctx.Sink.FastEmitLoadLocal(stateVar.LocalIndex);
                Ctx.Sink.FastEmitLoadLocal(upperVar.LocalIndex);
                Ctx.EmitApplyBinary(Operator.LE);
                Ctx.EmitUnwrapBool();
                Ctx.Sink.Emit(OpCodes.Brtrue, top);

                Ctx.Sink.MarkLabel(endOfLoop);

                TryEmitBlock(LoopDirective.AfterAll, blocks);
                var endOfBlock = Ctx.Sink.DefineLabel();
                Ctx.Sink.Emit(OpCodes.Br, endOfBlock);

                Ctx.Sink.MarkLabel(noDataSkip);
                TryEmitBlock(LoopDirective.NoData, blocks);
                Ctx.Sink.MarkLabel(endOfBlock);
            }
            finally
            {
                Ctx.PopJumpScope();
            }
        }

        private void TryEmitBlock(LoopDirective d, Dictionary<LoopDirective, CommonTree> blocks)
        {
            CommonTree block;
            if (!blocks.TryGetValue(d, out block))
            {
                Ctx.Sink.Emit(OpCodes.Nop);
                return;
            }
            var b = new Block(block);
            Errors.AddRange(b.Generate(Ctx));
        }
    }
}