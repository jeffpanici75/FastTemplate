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
    internal class Passthrough : Emitter
    {
        public Passthrough(ITree tree)
            : base(tree)
        {
        }

        protected override void OnGenerate()
        {
            var errors = new ErrorList();
            if (T.Type == 0)
            {
                var error = (CommonErrorNode) T;
                errors.ErrorParse(error);
                return;
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
                case TemplateLexer.MPass:
                case TemplateLexer.EPass:
                {
                    EmitPassthrough(child);
                    if (child.Type == TemplateLexer.MPass)
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

        private void EmitPassthrough(CommonTree p)
        {
            if (p.Children == null || p.Children.Count < 1)
            {
                Ctx.EmitEmptyString();
                return;
            }
            var args = (CommonTree) p.Children[0];
            if (args.Children == null || args.Children.Count < 1)
            {
                Ctx.EmitEmptyString();
                return;
            }
            Ctx.Sink.Emit(OpCodes.Ldarg_0);
            Ctx.EmitArgList(args);
            Ctx.EmitVTFunctionCall("InvokePassthrough");
        }
    }
}