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

using System.Reflection.Emit;
using Antlr.Runtime.Tree;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Compiler
{
    internal class DynamicString : Emitter
    {
        public DynamicString(ITree tree)
            : base(tree)
        {
        }

        protected override void OnGenerate()
        {
            if (T.Children == null
                || T.Children.Count < 1)
            {
                Ctx.Sink.Emit(OpCodes.Nop);
                return;
            }
            var size = T.Children.Count;
            var local = Ctx.Sink.DeclareLocal(typeof (object[]));
            Ctx.Sink.FastEmitConstInt(size);
            Ctx.Sink.Emit(OpCodes.Newarr, typeof (object));
            Ctx.Sink.FastEmitStoreLocal(local.LocalIndex);
            var index = 0;
            foreach (var part in T.Children)
            {
                switch (part.Type)
                {
                    case TemplateLexer.Literal:
                    {
                        Ctx.Sink.FastEmitLoadLocal(local.LocalIndex);
                        Ctx.Sink.FastEmitConstInt(index);
                        Ctx.Sink.Emit(OpCodes.Ldstr, part.Text);
                        Ctx.Sink.Emit(OpCodes.Stelem_Ref);
                        break;
                    }
                    case TemplateParser.Statement:
                    {
                        Ctx.Sink.FastEmitLoadLocal(local.LocalIndex);
                        Ctx.Sink.FastEmitConstInt(index);
                        var s = new Statement(part);
                        Errors.AddRange(s.Generate(Ctx));
                        Ctx.Sink.Emit(OpCodes.Stelem_Ref);
                        break;
                    }
                    case TemplateParser.Passthrough:
                    {
                        Ctx.Sink.FastEmitLoadLocal(local.LocalIndex);
                        Ctx.Sink.FastEmitConstInt(index);
                        var s = new Passthrough(part);
                        Errors.AddRange(s.Generate(Ctx));
                        Ctx.Sink.Emit(OpCodes.Stelem_Ref);
                        break;
                    }
                }
                index++;
            }
            Ctx.Sink.FastEmitLoadLocal(local.LocalIndex);
            Ctx.EmitCombineStringParts();
        }
    }
}