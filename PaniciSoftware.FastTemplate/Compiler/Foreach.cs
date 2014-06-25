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
using System.Reflection.Emit;
using Antlr.Runtime.Tree;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Compiler
{
    internal class Foreach : Emitter
    {
        public Foreach(ITree tree)
            : base(tree)
        {
        }

        protected override void OnGenerate()
        {
            var var = (CommonTree) T.Children[0];
            if (var.Type == 0)
            {
                Errors.ErrorParse((CommonErrorNode) var);
                return;
            }
            var exp = (CommonTree) T.Children[1];
            if (exp.Type == 0)
            {
                Errors.ErrorParse((CommonErrorNode) exp);
                return;
            }

            var defaultBlock = (CommonTree) T.Children[2];
            var blocks = new Dictionary<LoopDirective, CommonTree>();
            for (var i = 3; i < T.Children.Count; i++)
            {
                var targeted = (CommonTree) T.Children[i];
                var ins = targeted.Children[0];
                var tBlock = (CommonTree) targeted.Children[1];
                LoopDirective d;
                if (!Enum.TryParse(ins.Text, true, out d))
                {
                    Errors.ErrorUnknownLoopDirective(ins.Text);
                    continue;
                }
                blocks[d] = tBlock;
            }
            if (Errors.ContainsError())
                return;

            var endOfLoop = Ctx.Sink.DefineLabel();
            var continuePoint = Ctx.Sink.DefineLabel();
            var top = Ctx.Sink.DefineLabel();
            var endOfBlock = Ctx.Sink.DefineLabel();
            var oddEvenVar = Ctx.Sink.DeclareLocal(typeof (int));

            var iter = Ctx.Sink.DeclareLocal(typeof (IEnumerator));
            var current = Ctx.Sink.DeclareLocal(typeof (ITemplateType));

            Ctx.PushNewJumpScope(continuePoint, endOfLoop);
            try
            {
                //init part here
                var enumerableExp = new Expression(exp);
                Errors.AddRange(enumerableExp.Generate(Ctx));
                Ctx.Sink.FastEmitStoreLocal(current.LocalIndex);

                Ctx.Sink.Emit(OpCodes.Ldarg_0);
                Ctx.Sink.FastEmitLoadLocal(current.LocalIndex);
                Ctx.EmitVTFunctionCall("CheckIsEnumerable");
                Ctx.Sink.Emit(OpCodes.Brfalse, endOfBlock);

                Ctx.Sink.Emit(OpCodes.Ldarg_0);
                Ctx.Sink.FastEmitLoadLocal(current.LocalIndex);
                Ctx.EmitVTFunctionCall("PrimeIter");
                Ctx.Sink.FastEmitStoreLocal(iter.LocalIndex);

                Ctx.Sink.FastEmitConstInt(0);
                Ctx.Sink.FastEmitStoreLocal(oddEvenVar.LocalIndex);

                var noDataSkip = Ctx.Sink.DefineLabel();
                Ctx.Sink.FastEmitLoadLocal(iter.LocalIndex);
                Ctx.EmitIterMoveNext();
                Ctx.Sink.Emit(OpCodes.Brfalse, noDataSkip);

                var skipStart = Ctx.Sink.DefineLabel();

                Ctx.Sink.Emit(OpCodes.Ldarg_0);
                Ctx.Sink.FastEmitLoadLocal(iter.LocalIndex);
                Ctx.EmitVTFunctionCall("CurrentToTType");
                Ctx.Sink.FastEmitStoreLocal(current.LocalIndex);
                var s = new Statement(var);
                Errors.AddRange(s.GenerateSet(Ctx, current.LocalIndex));

                TryEmitBlock(LoopDirective.BeforeAll, blocks);
                Ctx.Sink.Emit(OpCodes.Br, skipStart);

                Ctx.Sink.MarkLabel(top);
                Ctx.Sink.Emit(OpCodes.Nop);

                Ctx.Sink.Emit(OpCodes.Ldarg_0);
                Ctx.Sink.FastEmitLoadLocal(iter.LocalIndex);
                Ctx.EmitVTFunctionCall("CurrentToTType");
                Ctx.Sink.FastEmitStoreLocal(current.LocalIndex);
                s = new Statement(var);
                s.GenerateSet(Ctx, current.LocalIndex);

                Ctx.Sink.MarkLabel(skipStart);
                Ctx.Sink.Emit(OpCodes.Nop);

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

                TryEmitBlock(LoopDirective.Each, blocks);

                TryEmitBlock(LoopDirective.After, blocks);

                Ctx.Sink.MarkLabel(continuePoint);

                //increment odd even var
                Ctx.Sink.Emit(OpCodes.Ldc_I4_1);
                Ctx.Sink.FastEmitLoadLocal(oddEvenVar.LocalIndex);
                Ctx.Sink.Emit(OpCodes.Add);
                Ctx.Sink.FastEmitStoreLocal(oddEvenVar.LocalIndex);

                //increment
                //check condidtion
                Ctx.Sink.FastEmitLoadLocal(iter.LocalIndex);
                Ctx.EmitIterMoveNext();
                Ctx.Sink.Emit(OpCodes.Brtrue, top);

                Ctx.Sink.MarkLabel(endOfLoop);

                TryEmitBlock(LoopDirective.AfterAll, blocks);

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