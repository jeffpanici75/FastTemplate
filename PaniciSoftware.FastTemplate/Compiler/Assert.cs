﻿//
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

namespace PaniciSoftware.FastTemplate.Compiler
{
    internal class Assert : Emitter
    {
        public Assert(ITree tree)
            : base(tree)
        {
        }

        protected override void OnGenerate()
        {
            var cond = T.Children[0];
            var message = T.Children[1];
            var end = Ctx.Sink.DefineLabel();
            var messageLabel = Ctx.Sink.DefineLabel();
            var condExp = new Expression(cond);
            Errors.AddRange(condExp.Generate(Ctx));
            Ctx.EmitUnwrapBool();
            Ctx.Sink.Emit(OpCodes.Brtrue, messageLabel);
            Ctx.EmitEmptyString();
            Ctx.Sink.Emit(OpCodes.Br, end);
            Ctx.Sink.MarkLabel(messageLabel);
            var messageExp = new Expression(message);
            Errors.AddRange(messageExp.Generate(Ctx));
            Ctx.Sink.MarkLabel(end);
        }
    }
}