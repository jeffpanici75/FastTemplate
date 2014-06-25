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
    internal class Assert : Evaluator
    {
        public Assert(ITree tree)
            : base(tree)
        {
        }

        protected override void OnEval(out ITemplateType result)
        {
            var cond = T.Children[0];
            var message = T.Children[1];

            var exp = new Expression(cond);
            ITemplateType flag;
            Errors.AddRange(exp.Eval(Ctx, Interpreter, out flag));
            if (Errors.ContainsError())
            {
                result = StringType.Empty();
                return;
            }

            bool o;
            if (!flag.TryConvert(out o))
            {
                result = StringType.Empty();
                return;
            }

            if (o)
            {
                var mExp = new Expression(message);
                Errors.AddRange(mExp.Eval(Ctx, Interpreter, out result));
            }
            else
            {
                result = StringType.Empty();
            }
        }
    }
}