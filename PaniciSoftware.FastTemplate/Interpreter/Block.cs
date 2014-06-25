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
    internal class Block : Evaluator
    {
        public Block(CommonTree t)
            : base(t)
        {
        }

        public Block(ITree t)
            : base(t)
        {
        }

        protected override void OnEval(
            out ITemplateType r)
        {
            var buffer = new StringBuilder();
            if (T.Children == null)
            {
                r = StringType.Empty();
            }
            else
            {
                foreach (var section in T.Children)
                {
                    ITemplateType result;
                    var s = new Section(section);
                    OnChild(s, out result);
                    if (!result.WriteTo(buffer))
                    {
                        Errors.ErrorIEnumerableRequired(string.Format("Unable to convert {0} to a string.",
                            result.RawValue));
                    }
                    if (Ctx.Jump
                        != JumpType.None)
                    {
                        break;
                    }
                }
                r = StringType.New(buffer.ToString());
            }
        }
    }
}