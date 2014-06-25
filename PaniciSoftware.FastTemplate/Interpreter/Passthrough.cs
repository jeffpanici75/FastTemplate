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
    internal class Passthrough : Evaluator
    {
        public Passthrough(ITree tree)
            : base(tree)
        {
        }

        protected override void OnEval(out ITemplateType o)
        {
            var c = (CommonTree) T.Children[0];
            switch (c.Type)
            {
                case 0:
                {
                    Errors.ErrorParse((CommonErrorNode) c);
                    o = null;
                    break;
                }
                case TemplateLexer.MPass:
                case TemplateLexer.EPass:
                {
                    o = EvalPassThrough(c);
                    if (c.Type == TemplateLexer.MPass)
                    {
                        string obj;
                        if (!o.TryConvert(out obj))
                        {
                            Errors.ErrorMacrosOnlyAcceptStrings(c.Text);
                            return;
                        }
                        var i = new Interpreter(obj);

                        var result = i.Apply(S);
                        Errors.AddRange(result.Errors);

                        o = Errors.ContainsError()
                            ? StringType.Empty()
                            : StringType.New(result.Output);
                    }
                    break;
                }
                default:
                {
                    o = StringType.Empty();
                    break;
                }
            }
        }

        private ITemplateType EvalPassThrough(CommonTree p)
        {
            if (p.Children == null || p.Children.Count < 1)
                return StringType.Empty();
            var args = (CommonTree) p.Children[0];
            if (args.Children == null || args.Children.Count < 1)
                return StringType.Empty();
            var buffer = new StringBuilder();
            foreach (var t in args.Children)
            {
                var exp = new Expression(t);
                ITemplateType o;
                Errors.AddRange(exp.Eval(Ctx, Interpreter, out o));
                if (Errors.ContainsError())
                    continue;
                o.WriteTo(buffer);
            }
            return StringType.New(buffer.ToString());
        }
    }
}