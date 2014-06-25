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
    internal class Section : Emitter
    {
        public Section(ITree tree)
            : base(tree)
        {
        }

        protected override void OnGenerate()
        {
            var token = T.Token;
            switch (token.Type)
            {
                case 0:
                {
                    var error = (CommonErrorNode) token;
                    Errors.ErrorParse(error);
                    break;
                }
                case TemplateParser.Statement:
                {
                    Ctx.Sink.Emit(OpCodes.Ldarg_0);
                    var s = new Statement(T);
                    Errors.AddRange(s.Generate(Ctx));

                    Ctx.EmitAppendToBuffer();
                    break;
                }
                case TemplateParser.Passthrough:
                {
                    Ctx.Sink.Emit(OpCodes.Ldarg_0);
                    var s = new Passthrough(T);
                    Errors.AddRange(s.Generate(Ctx));

                    Ctx.EmitAppendToBuffer();
                    break;
                }
                case TemplateParser.Control:
                {
                    var ctl = new Control(T);
                    Errors.AddRange(ctl.Generate(Ctx));
                    break;
                }
                case TemplateLexer.Unparsed:
                case TemplateLexer.Literal:
                {
                    Ctx.Sink.Emit(OpCodes.Ldarg_0);
                    Ctx.Sink.Emit(OpCodes.Ldstr, token.Text);
                    Ctx.EmitAppendLiteralToBuffer();
                    break;
                }
                case TemplateLexer.EOF:
                {
                    break;
                }
            }
        }
    }
}