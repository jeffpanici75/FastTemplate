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

using System.Text;
using Antlr.Runtime.Tree;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Interpreter
{
    internal class DynamicString : Evaluator
    {
        public DynamicString(CommonTree tree)
            : base(tree)
        {
        }

        public DynamicString(ITree tree)
            : base(tree)
        {
        }

        protected override void OnEval(out ITemplateType o)
        {
            if (T.Children == null
                || T.Children.Count < 1)
            {
                o = StringType.Empty();
                return;
            }

            var buffer = new StringBuilder();
            foreach (var part in T.Children)
            {
                switch (part.Type)
                {
                    case TemplateLexer.Literal:
                    {
                        buffer.Append(part.Text);
                        break;
                    }
                    case TemplateParser.Statement:
                    {
                        ITemplateType result;
                        Errors.AddRange(new Statement(part).Eval(Ctx, Interpreter, out result));
                        if (Errors.ContainsError())
                            break;
                        result.WriteTo(buffer);
                        break;
                    }
                    case TemplateParser.Passthrough:
                    {
                        ITemplateType result;
                        Errors.AddRange(new Passthrough(part).Eval(Ctx, Interpreter, out result));
                        if (Errors.ContainsError())
                            break;
                        result.WriteTo(buffer);
                        break;
                    }
                }
            }
            o = StringType.New(buffer.ToString());
        }
    }
}