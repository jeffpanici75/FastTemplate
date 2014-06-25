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
    internal class Control : Emitter
    {
        public Control(ITree tree)
            : base(tree)
        {
        }

        protected override void OnGenerate()
        {
            Errors.Clear();
            var child = (CommonTree) T.Children[0];
            switch (child.Type)
            {
                case 0:
                {
                    Ctx.EmitNullRef();
                    Errors.ErrorParse((CommonErrorNode) child);
                    break;
                }
                case TemplateLexer.Pragma:
                {
                    var a = new Pragma(child);
                    Errors.AddRange(a.Generate(Ctx));
                    break;
                }
                case TemplateLexer.Assert:
                {
                    Ctx.Sink.Emit(OpCodes.Ldarg_0);
                    var a = new Assert(child);
                    Errors.AddRange(a.Generate(Ctx));
                    Ctx.EmitAppendToBuffer();
                    break;
                }
                case TemplateLexer.Continue:
                {
                    if (!Ctx.InJumpableBlock)
                    {
                        Errors.ErrorInvalidJump("continue", child.Token);
                        break;
                    }
                    Ctx.Sink.Emit(OpCodes.Br, Ctx.GetContinueLabel());
                    break;
                }
                case TemplateLexer.Break:
                {
                    if (!Ctx.InJumpableBlock)
                    {
                        Errors.ErrorInvalidJump("break", child.Token);
                        break;
                    }
                    Ctx.Sink.Emit(OpCodes.Br, Ctx.GetBreakLabel());
                    break;
                }
                case TemplateLexer.Stop:
                {
                    Ctx.Sink.Emit(OpCodes.Br, Ctx.EndOfTemplate);
                    break;
                }
                case TemplateLexer.Parse:
                {
                    var args = (CommonTree) child.Children[0];
                    if (args.Type == 0)
                    {
                        Errors.ErrorParse((CommonErrorNode) args);
                        break;
                    }
                    Ctx.Sink.Emit(OpCodes.Ldarg_0);
                    Ctx.EmitArgList(args);
                    Ctx.EmitVTFunctionCall("InvokeParse");
                    break;
                }
                case TemplateLexer.Include:
                {
                    var args = (CommonTree) child.Children[0];
                    if (args.Type == 0)
                    {
                        Errors.ErrorParse((CommonErrorNode) args);
                        break;
                    }
                    Ctx.Sink.Emit(OpCodes.Ldarg_0);
                    Ctx.EmitArgList(args);
                    Ctx.EmitVTFunctionCall("InvokeInclude");
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
                    var s = new Statement(target);
                    var exp = new Expression(source);
                    Errors.AddRange(s.GenerateSet(Ctx, exp));
                    break;
                }
                case TemplateLexer.If:
                {
                    var ifControl = new If(child);
                    Errors.AddRange(ifControl.Generate(Ctx));
                    break;
                }
                case TemplateLexer.Loop:
                {
                    var loop = new Loop(child);
                    Errors.AddRange(loop.Generate(Ctx));
                    break;
                }
                case TemplateLexer.Foreach:
                {
                    var loop = new Foreach(child);
                    Errors.AddRange(loop.Generate(Ctx));
                    break;
                }
            }
        }
    }
}