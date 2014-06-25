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

using System.Collections.Generic;
using System.Reflection.Emit;
using Antlr.Runtime.Tree;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Compiler
{
    internal class If : Emitter
    {
        public If(ITree tree)
            : base(tree)
        {
        }

        protected override void OnGenerate()
        {
            var end = Ctx.Sink.DefineLabel();
            var labels = DiscoverLabels(T, end);
            {
                var cond = (CommonTree) T.Children[0];
                var condExp = new Expression(cond);
                Errors.AddRange(condExp.Generate(Ctx));
                Ctx.EmitUnwrapBool();
                Ctx.Sink.Emit(OpCodes.Brfalse, labels[0]);
                var block = (CommonTree) T.Children[1];
                var blk = new Block(block);
                Errors.AddRange(blk.Generate(Ctx));
                Ctx.Sink.Emit(OpCodes.Br, end);
            }

            var nextJump = 0;
            var hasElse = T.Children.Count > 2 && T.Children[2].Type == TemplateParser.Else;
            var elseIfStart = hasElse ? 3 : 2;
            for (var i = elseIfStart; i < T.Children.Count; i++)
            {
                var elseIf = (CommonTree) T.Children[i];
                var cond = elseIf.Children[0];
                var block = elseIf.Children[1];

                Ctx.Sink.MarkLabel(labels[nextJump]);
                nextJump++;

                var exp = new Expression(cond);
                Errors.AddRange(exp.Generate(Ctx));
                Ctx.EmitUnwrapBool();
                Ctx.Sink.Emit(OpCodes.Brfalse, labels[nextJump]);

                var blk = new Block(block);
                Errors.AddRange(blk.Generate(Ctx));
                Ctx.Sink.Emit(OpCodes.Br, end);
            }

            if (hasElse)
            {
                Ctx.Sink.MarkLabel(labels[labels.Count - 2]);
                var elseTree = (CommonTree) T.Children[2];
                var block = elseTree.Children[0];
                var blk = new Block(block);
                Errors.AddRange(blk.Generate(Ctx));
            }

            Ctx.Sink.MarkLabel(end);
            Ctx.Sink.Emit(OpCodes.Nop);
        }

        private List<Label> DiscoverLabels(CommonTree blocks, Label end)
        {
            var labels = new List<Label>();
            var hasElse = blocks.Children.Count > 2 && blocks.Children[2].Type == TemplateParser.Else;
            var elseIfStart = hasElse ? 3 : 2;
            for (var i = elseIfStart;
                i < blocks.Children.Count;
                i++)
            {
                labels.Add(Ctx.Sink.DefineLabel());
            }
            if (hasElse)
            {
                labels.Add(Ctx.Sink.DefineLabel());
            }
            labels.Add(end);
            return labels;
        }
    }
}