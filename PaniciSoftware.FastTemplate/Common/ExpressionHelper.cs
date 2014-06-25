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

namespace PaniciSoftware.FastTemplate.Common
{
    public static class ExpressionHelper
    {
        public static Operator TokenToOperator(int token)
        {
            switch (token)
            {
                case TemplateLexer.Add:
                    return Operator.Plus;
                case TemplateLexer.Minus:
                    return Operator.Minus;
                case TemplateLexer.Div:
                    return Operator.Div;
                case TemplateLexer.Mul:
                    return Operator.Mul;
                case TemplateLexer.Mod:
                    return Operator.Mod;
                case TemplateLexer.LE:
                    return Operator.LE;
                case TemplateLexer.GE:
                    return Operator.GE;
                case TemplateLexer.LT:
                    return Operator.LT;
                case TemplateLexer.GT:
                    return Operator.GT;
                case TemplateLexer.Not:
                    return Operator.Not;
                case TemplateLexer.EQ:
                    return Operator.EQ;
                case TemplateLexer.NEQ:
                    return Operator.NEQ;
            }
            return Operator.Unknown;
        }
    }
}