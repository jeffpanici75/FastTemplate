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
    public static class ErrorMessageHelper
    {
        public static string TokenTypeToDisplayString(int type)
        {
            switch (type)
            {
                case TemplateLexer.RP:
                {
                    return "Right Parentheses, ')'";
                }
                case TemplateLexer.LP:
                {
                    return "Left Parentheses, ')'";
                }
                case TemplateLexer.AND:
                {
                    return "Logical And Symbol, '&&'";
                }
                case TemplateLexer.Add:
                {
                    return "Add Operator, '+'";
                }
                case TemplateLexer.As:
                {
                    return "As Operator, 'as'";
                }
                case TemplateLexer.Assert:
                {
                    return "Assert function, '#assert'";
                }
                case TemplateLexer.Assign:
                {
                    return "Assign operator, '='";
                }
                case TemplateLexer.Break:
                {
                    return "Break operator, '#break'";
                }
                case TemplateLexer.Comma:
                {
                    return "Comma seperator, ','";
                }
                case TemplateLexer.Continue:
                {
                    return "Continue operator, '#continue'";
                }
                case TemplateLexer.Dec:
                {
                    return "Continue operator, '#continue'";
                }
                case TemplateLexer.Decimal:
                {
                    return "Decimal Number";
                }
                case TemplateLexer.Div:
                {
                    return "Division operator, '/'";
                }
                case TemplateLexer.Double:
                {
                    return "Double precision floating point number";
                }
                case TemplateLexer.DynamicString:
                {
                    return "A dynamic string";
                }
                case TemplateLexer.EOF:
                {
                    return "End of File";
                }
                case TemplateLexer.EPass:
                {
                    return "Expansinon Passthrough statement";
                }
                case TemplateLexer.EQ:
                {
                    return "Equality operator, '=='";
                }
                case TemplateLexer.EStart:
                {
                    return "expansion statement, '$(<key-name>)'";
                }
                case TemplateLexer.Else:
                {
                    return "Start of an else block, '#else'";
                }
                case TemplateLexer.ElseIf:
                {
                    return "Start of an if else block, '#elseif'";
                }
                case TemplateLexer.End:
                {
                    return "End of a block, '#end'";
                }
                case TemplateLexer.False:
                {
                    return "Boolean false constant, 'false'";
                }
                case TemplateLexer.Float:
                {
                    return "Floating point number";
                }
                case TemplateLexer.Foreach:
                {
                    return "Start of a foreach block, '#foreach('";
                }
                case TemplateLexer.GE:
                {
                    return "Greater than or equal, '>='";
                }
                case TemplateLexer.GT:
                {
                    return "Greater than, '>'";
                }
                case TemplateLexer.Hex:
                {
                    return "Hexidecimal constant";
                }
                case TemplateLexer.If:
                {
                    return "Start of an if block";
                }
                case TemplateLexer.In:
                {
                    return "'in' operator used in #foreach( <statement> in <expression> )";
                }
                case TemplateLexer.Include:
                {
                    return "#include function.";
                }
                case TemplateLexer.Integer:
                {
                    return "an integer value";
                }
                case TemplateLexer.Keyword:
                {
                    return "a keyword";
                }
                case TemplateLexer.LBrace:
                {
                    return "left brace, '{'";
                }
                case TemplateLexer.LBracket:
                {
                    return "left bracket, '['";
                }
                case TemplateLexer.LE:
                {
                    return "less than or equal, '<='";
                }
                case TemplateLexer.LT:
                {
                    return "less than, '<'";
                }
                case TemplateLexer.Literal:
                {
                    return "a literal string";
                }
                case TemplateLexer.Loop:
                {
                    return "a loop block starting with: '#loop'";
                }
            }
            return "";
        }
    }
}