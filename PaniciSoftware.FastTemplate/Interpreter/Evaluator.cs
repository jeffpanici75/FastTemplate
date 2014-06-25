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

using Antlr.Runtime.Tree;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Interpreter
{
    internal abstract class Evaluator
    {
        protected Evaluator(ITree tree)
        {
            T = (CommonTree) tree;
            Errors = new ErrorList();
        }

        protected Context Ctx { get; private set; }

        protected CommonTree T { get; private set; }

        protected ErrorList Errors { get; private set; }

        protected TemplateDictionary S
        {
            get { return Interpreter.State; }
            set { Interpreter.State = value; }
        }

        protected Interpreter Interpreter { get; set; }

        public ErrorList Eval(
            Context ctx,
            Interpreter interpreter,
            out ITemplateType o)
        {
            if (T == null || T.Type == 0)
            {
                Errors.ErrorParse((CommonErrorNode) T);
                o = null;
                return Errors;
            }
            Ctx = ctx;
            Interpreter = interpreter;
            Errors.Clear();
            OnEval(out o);
            return Errors;
        }

        protected void OnChild(
            Evaluator child,
            out ITemplateType o)
        {
            var e = child.Eval(Ctx, Interpreter, out o);
            Errors.AddRange(e);
        }

        protected abstract void OnEval(out ITemplateType o);
    }
}