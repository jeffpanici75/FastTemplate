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

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Antlr.Runtime;

namespace PaniciSoftware.FastTemplate.Common
{
    public enum TemplateLexerMode
    {
        Outside,
        Inside,
        DynamicString,
        Statement
    }

    public enum TemplateTokenType
    {
        Expansion,
        Macro
    }

    public enum ConstructType
    {
        Expression,
        ArgList,
        DynamicString,
        Comment,
        FindLP,
        FindControlLP,
        ControlArgList,
        FreeStatement,
        FormalStatement
    }

    public class TemplateLexer : ITokenSource
    {
        public const char EOF = char.MaxValue;
        public const int EOF_TYPE = -1;

        public const int Literal = 4;
        public const int EStart = 5;
        public const int MStart = 6;
        public const int Prop = 7;
        public const int EPass = 8;
        public const int MPass = 9;
        public const int EQ = 10;
        public const int NEQ = 11;
        public const int LE = 12;
        public const int GE = 13;
        public const int LT = 14;
        public const int GT = 15;
        public const int OR = 16;
        public const int AND = 17;
        public const int Add = 18;
        public const int Minus = 19;
        public const int Div = 20;
        public const int Mul = 21;
        public const int Mod = 22;
        public const int RP = 23;
        public const int LP = 24;
        public const int RBrace = 25;
        public const int LBrace = 26;
        public const int RBracket = 27;
        public const int LBracket = 28;
        public const int SignedLong = 29;
        public const int Float = 30;
        public const int Assign = 31;
        public const int StringLiteral = 32;
        public const int DynamicString = 33;
        public const int UnsignedInteger = 34;
        public const int Double = 35;
        public const int Hex = 36;
        public const int Decimal = 37;
        public const int Comma = 38;
        public const int Unparsed = 39;
        public const int LoopDirective = 40;
        public const int If = 41;
        public const int ElseIf = 42;
        public const int Else = 43;
        public const int Loop = 44;
        public const int Foreach = 45;
        public const int Set = 46;
        public const int Parse = 47;
        public const int Include = 48;
        public const int Keyword = 49;
        public const int Root = 50;
        public const int End = 51;
        public const int True = 52;
        public const int False = 53;
        public const int As = 54;
        public const int To = 55;
        public const int Step = 56;
        public const int In = 57;
        public const int Null = 58;
        public const int Break = 59;
        public const int Stop = 60;
        public const int Not = 61;
        public const int Inc = 62;
        public const int Dec = 63;
        public const int Continue = 64;
        public const int UnexpectedChar = 65;
        public const int Integer = 66;
        public const int Assert = 67;
        public const int Pragma = 68;

        private readonly Stack<ConstructType> _scope = new Stack<ConstructType>();
        private TemplateLexerMode _lexerMode = TemplateLexerMode.Outside;
        private bool _shouldStripNextNewLine;
        private readonly ICharStream _input;

        private TemplateLexer(ICharStream stream)
        {
            TokenBuffer = new Queue<IToken>();
            _input = stream;
            Current = (char) _input.LA(1);
            Buffer = new StringBuilder();
        }

        private int CurrentStart { get; set; }

        private char Current { get; set; }

        private StringBuilder Buffer { get; set; }

        private Queue<IToken> TokenBuffer { get; set; }

        public IToken NextToken()
        {
            if (TokenBuffer.Count > 0)
            {
                var t = TokenBuffer.Dequeue();
                if (!(t.Type == Literal && _lexerMode == TemplateLexerMode.Inside))
                {
                    return t;
                }
            }

            CurrentStart = _input.CharPositionInLine;
            switch (_lexerMode)
            {
                case TemplateLexerMode.DynamicString:
                {
                    return NextDynamicString();
                }
                case TemplateLexerMode.Inside:
                {
                    return NextInside();
                }
                case TemplateLexerMode.Outside:
                {
                    return NextOutside();
                }
                case TemplateLexerMode.Statement:
                {
                    return NextStatement();
                }
                default:
                {
                    //not reachable?
                    return null;
                }
            }
        }

        public string SourceName
        {
            get { return _input.SourceName; }
        }

        public string[] TokenNames
        {
            get { return TemplateParser.tokenNames; }
        }

        public static TemplateLexer Create(ICharStream stream)
        {
            return new TemplateLexer(stream);
        }

        private IToken NextStatement()
        {
            TrySkipwhitespace();
            switch (Current)
            {
                case EOF:
                {
                    return EOFToken();
                }
                case '.':
                case '(':
                case '[':
                    return NextInside();
                case '}':
                {
                    Next();
                    ExitConstruct();
                    return NewToken(RBrace, '}');
                }
                default:
                {
                    ExitConstruct();
                    return NextToken();
                }
            }
        }

        private void TrySkipwhitespace()
        {
            var c = _scope.Peek();
            if (c == ConstructType.FreeStatement)
            {
                return;
            }
            if (c == ConstructType.FormalStatement)
                IgnoreWS();
        }

        private IToken NextInside()
        {
            IgnoreWS();
            switch (Current)
            {
                case EOF:
                {
                    return EOFToken();
                }
                case '$':
                case '@':
                {
                    var next = (char) _input.LA(2);
                    if (next != '('
                        && next != '{'
                        && !IsIDChar(next))
                    {
                        var controlChar = Current;
                        Next();
                        return NewToken(Literal, controlChar);
                    }
                    var startChar = Current;
                    var type = Current == '@'
                        ? TemplateTokenType.Macro
                        : TemplateTokenType.Expansion;
                    Next();
                    if (Current == '(')
                    {
                        Next(); //skip (
                        EnterConstruct(ConstructType.ArgList);
                        return PassthroughFunction(type);
                    }

                    var formal = Current == '{';
                    if (formal)
                    {
                        TokenBuffer.Enqueue(
                            NewToken(LBrace, '{'));
                        Next();
                        IgnoreWS();
                        var root = RootToken();
                        TokenBuffer.Enqueue(root);
                        EnterConstruct(ConstructType.FormalStatement);
                    }
                    else
                    {
                        var root = RootToken();
                        TokenBuffer.Enqueue(root);
                        EnterConstruct(ConstructType.FreeStatement);
                    }
                    return NewToken(
                        type == TemplateTokenType.Expansion
                            ? EStart
                            : MStart,
                        startChar);
                }
                case '#':
                {
                    var next = (char) _input.LA(2);
                    if (next != '['
                        && next != '#'
                        && next != '*'
                        && next != '{'
                        && !IsIDChar(next))
                    {
                        Next();
                        return NewToken(Literal, "#");
                    }

                    Next();
                    bool formal;
                    if (Current == '{')
                    {
                        Next();
                        formal = true;
                    }
                    else
                    {
                        formal = false;
                    }

                    switch (Current)
                    {
                        case '#':
                        {
                            Next();
                            IgnoreLineComment();
                            return NextToken();
                        }
                        case '*':
                        {
                            Next(); //skip *
                            IgnoreBlockComment();
                            return NextToken();
                        }
                        case '[':
                        {
                            Next(); //[
                            return UnparsedToken();
                        }

                        default:
                        {
                            var d = formal
                                ? FormalDirectiveToken()
                                : DirectiveToken();
                            switch (d)
                            {
                                case "assert":
                                {
                                    EnterConstruct(
                                        ConstructType.FindControlLP);
                                    return NewToken(
                                        Assert, formal ? "#{assert}" : "#assert");
                                }
                                case "pragma":
                                {
                                    EnterConstruct(
                                        ConstructType.FindControlLP);
                                    return NewToken(
                                        Pragma, formal ? "#{pragma}" : "#pragma");
                                }
                                case "loop":
                                {
                                    EnterConstruct(
                                        ConstructType.FindControlLP);
                                    return NewToken(
                                        Loop, formal ? "#{loop}" : "#loop");
                                }
                                case "foreach":
                                {
                                    EnterConstruct(
                                        ConstructType.FindControlLP);
                                    return NewToken(
                                        Foreach,
                                        formal ? "#{foreach}" : "#foreach");
                                }
                                case "if":
                                {
                                    EnterConstruct(
                                        ConstructType.FindControlLP);
                                    return NewToken(
                                        If, formal ? "#{if}" : "#if");
                                }
                                case "elseif":
                                {
                                    EnterConstruct(
                                        ConstructType.FindControlLP);
                                    return NewToken(
                                        ElseIf, formal ? "#{elseif}" : "#elseif");
                                }
                                case "else":
                                {
                                    SetStripNextNewLine();
                                    return NewToken(
                                        Else, formal ? "#{else}" : "#else");
                                }
                                case "set":
                                {
                                    EnterConstruct(
                                        ConstructType.FindControlLP);
                                    return NewToken(
                                        Set, formal ? "#{set}" : "#set");
                                }
                                case "include":
                                {
                                    EnterConstruct(
                                        ConstructType.FindControlLP);
                                    return NewToken(
                                        Include,
                                        formal ? "#{include}" : "#include");
                                }
                                case "parse":
                                {
                                    EnterConstruct(
                                        ConstructType.FindControlLP);
                                    return NewToken(
                                        Parse, formal ? "#{parse}" : "#parse");
                                }
                                case "end":
                                {
                                    SetStripNextNewLine();
                                    return NewToken(
                                        End, formal ? "#{end}" : "#end");
                                }
                                case "stop":
                                {
                                    SetStripNextNewLine();
                                    return NewToken(Stop, "stop");
                                }
                                case "break":
                                {
                                    SetStripNextNewLine();
                                    return NewToken(Break, "break");
                                }
                                case "continue":
                                {
                                    SetStripNextNewLine();
                                    return NewToken(Continue, "continue");
                                }
                                case "afterall":
                                case "beforeall":
                                case "each":
                                case "before":
                                case "odd":
                                case "even":
                                case "between":
                                case "nodata":
                                {
                                    SetStripNextNewLine();
                                    return NewToken(LoopDirective, d);
                                }
                                default:
                                {
                                    if (formal)
                                        return NewToken(
                                            Literal,
                                            string.Format("#{{{0}}}", d));
                                    return NewToken(
                                        Literal, string.Format("#{0}", d));
                                }
                            }
                        }
                    }
                }
                case '.':
                {
                    Next();
                    TrySkipwhitespace();
                    return Property();
                }
                case '!':
                {
                    Next();
                    if (Current == '=')
                    {
                        Next();
                        return DelimToken(NEQ, "!=");
                    }
                    return DelimToken(Not, '!');
                }
                case ',':
                {
                    Next();
                    return DelimToken(Comma, ',');
                }
                case '{':
                {
                    Next();
                    EnterConstruct();
                    var t = DelimToken(LBrace, '{');
                    return t;
                }
                case '}':
                {
                    Next();
                    var t = DelimToken(RBrace, '}');
                    ExitConstruct();
                    return t;
                }
                case '[':
                {
                    Next();
                    EnterConstruct();
                    var t = DelimToken(LBracket, '[');
                    return t;
                }
                case ']':
                {
                    Next();
                    var t = DelimToken(RBracket, ']');
                    ExitConstruct();
                    return t;
                }
                case '(':
                {
                    FoundLP();
                    Next();
                    return DelimToken(LP, '(');
                }
                case ')':
                {
                    ExitConstruct();
                    Next();
                    return DelimToken(RP, ')');
                }
                case '=':
                {
                    var next = (char) _input.LA(2);
                    if (next == '=')
                    {
                        Next();
                        Next();
                        return DelimToken(EQ, "==");
                    }
                    Next();
                    return DelimToken(Assign, "=");
                }
                case '<':
                {
                    var next = (char) _input.LA(2);
                    if (next == '=')
                    {
                        Next();
                        Next();
                        return DelimToken(LE, "<=");
                    }
                    Next();
                    return DelimToken(LT, "<");
                }
                case '>':
                {
                    var next = (char) _input.LA(2);
                    if (next == '=')
                    {
                        Next();
                        Next();
                        return DelimToken(GE, "<=");
                    }
                    Next();
                    return DelimToken(GT, ">");
                }
                case '&':
                {
                    var next = (char) _input.LA(2);
                    if (next == '&')
                    {
                        Next();
                        Next();
                        return DelimToken(AND, "&&");
                    }
                    Next();
                    return DelimToken(AND, "&&");
                }
                case '|':
                {
                    var next = (char) _input.LA(2);
                    if (next == '|')
                    {
                        Next();
                        Next();
                        return DelimToken(OR, "||");
                    }
                    Next();
                    return DelimToken(OR, "||");
                }
                case '+':
                {
                    Next();
                    return DelimToken(Add, "+");
                }
                case '-':
                {
                    Next();
                    return DelimToken(Minus, '-');
                }
                case '/':
                {
                    Next();
                    return DelimToken(Div, '/');
                }
                case '%':
                {
                    Next();
                    return DelimToken(Mod, '%');
                }
                case '*':
                {
                    Next();
                    return DelimToken(Mul, '*');
                }
                case '\'':
                {
                    Next();
                    return StringLiteralToken();
                }
                case '"':
                {
                    Next();
                    EnterConstruct(ConstructType.DynamicString);
                    return DelimToken(DynamicString, '"');
                }
                default:
                {
                    if (char.IsDigit(Current))
                    {
                        return NumberToken();
                    }
                    if (IsIDChar(Current))
                    {
                        var word = DirectiveToken();
                        switch (word)
                        {
                            case "as":
                            {
                                return NewToken(As, "as");
                            }
                            case "to":
                            {
                                return NewToken(To, "to");
                            }
                            case "step":
                            {
                                return NewToken(Step, "step");
                            }
                            case "in":
                            {
                                return NewToken(In, "in");
                            }
                            case "true":
                            {
                                return NewToken(True, "true");
                            }
                            case "false":
                            {
                                return NewToken(False, "false");
                            }
                            case "null":
                            {
                                return NewToken(Null, "null");
                            }
                            default:
                            {
                                return NewToken(Keyword, word);
                            }
                        }
                    }
                    Next();
                    return NewToken(UnexpectedChar, Current);
                }
            }
        }

        private IToken Property()
        {
            Buffer.Clear();
            while (IsIDChar(Current))
            {
                Buffer.Append(Current);
                Next();
            }
            var prop = NewToken(Prop, Buffer.ToString());
            Buffer.Clear();
            return prop;
        }

        private IToken RootToken()
        {
            Buffer.Clear();
            while (IsIDChar(Current))
            {
                Buffer.Append(Current);
                Next();
            }
            var root = NewToken(Root, Buffer.ToString());
            Buffer.Clear();
            return root;
        }

        private void TryEnterFreeStatement()
        {
            if (_scope.Count > 0)
                return;
            EnterConstruct(ConstructType.FreeStatement);
        }

        private void IgnoreLineComment()
        {
            while (Current != '\n'
                && Current != EOF)
                Next();
            if (Current != EOF)
            {
                Next(); //skip \n
            }
        }

        private void IgnoreBlockComment()
        {
            var n = (char) _input.LA(2);
            while (!(Current == '*' && n == '#')
                && Current != EOF)
            {
                if (Current == '#'
                    && n == '*')
                {
                    Next();
                    Next();
                    IgnoreBlockComment();
                }
                else
                {
                    Next();
                    n = (char) _input.LA(2);
                }
            }
            if (Current != EOF)
            {
                Next(); //*
                Next(); //#
            }
        }

        private IToken UnparsedToken()
        {
            var n = (char) _input.LA(2);
            while (!(Current == ']' && n == '#')
                && Current != EOF)
            {
                Buffer.Append(Current);
                Next();
                n = (char) _input.LA(2);
            }
            if (Current != EOF)
            {
                Next(); //]
                Next(); //#
            }
            var t = new CommonToken(
                _input,
                Unparsed,
                TokenChannels.Default,
                CurrentStart,
                _input.CharPositionInLine)
            {
                Text = Buffer.ToString(),
                Line = _input.Line,
            };
            Buffer.Clear();
            return t;
        }

        private IToken NewToken(int type, string text)
        {
            return new CommonToken(
                _input,
                type,
                TokenChannels.Default,
                CurrentStart,
                _input.CharPositionInLine)
            {
                Text = text,
                Line = _input.Line
            };
        }

        private IToken NewToken(int type, char text)
        {
            return new CommonToken(
                _input,
                type,
                TokenChannels.Default,
                CurrentStart,
                _input.CharPositionInLine)
            {
                Text = new string(text, 1),
                Line = _input.Line
            };
        }

        private string FormalDirectiveToken()
        {
            IgnoreWS();
            while (Current != '}'
                && Current != EOF)
            {
                Buffer.Append(Current);
                Next();
            }
            if (Current != EOF)
            {
                Next(); //skip }
            }
            var s = Buffer.ToString();
            Buffer.Clear();
            return s;
        }

        private string DirectiveToken()
        {
            while (IsIDChar(Current)
                && Current != EOF)
            {
                Buffer.Append(Current);
                Next();
            }
            var s = Buffer.ToString();
            Buffer.Clear();
            return s;
        }

        private IToken NextOutside()
        {
            while (true)
            {
                TryStripNextNewLine();
                if (Current == EOF)
                {
                    if (Buffer.Length > 0)
                    {
                        return LiteralToken();
                    }
                    return EOFToken();
                }

                if (IsConstructStart())
                {
                    if (Buffer.Length > 0)
                    {
                        return LiteralToken();
                    }
                    return NextInside();
                }

                if (Current == '\\')
                {
                    var next = (char) _input.LA(2);
                    switch (next)
                    {
                        case EOF:
                        {
                            Buffer.Append('\\');
                            Next();
                            return LiteralToken();
                        }
                        case '(':
                        case '[':
                        case '@':
                        case '$':
                        case '#':
                        {
                            Buffer.Append(next);
                            _input.Consume();
                            Next();
                            break;
                        }
                        default:
                        {
                            Buffer.Append('\\');
                            Next();
                            break;
                        }
                    }
                }
                else
                {
                    Buffer.Append(Current);
                    Next();
                }
            }
        }

        private IToken NextDynamicString()
        {
            while (true)
            {
                if (Current == EOF)
                {
                    if (Buffer.Length > 0)
                    {
                        return LiteralToken();
                    }
                    return EOFToken();
                }

                if (Current == '"')
                {
                    if (Buffer.Length > 0)
                    {
                        return LiteralToken();
                    }
                    ExitConstruct();
                    Next(); //exit dynamic string block
                    return DelimToken(DynamicString, '"');
                }

                if (IsStatementStart())
                {
                    if (Buffer.Length > 0)
                    {
                        return LiteralToken();
                    }
                    return NextInside();
                }

                if (Current == '\\')
                {
                    var next = (char) _input.LA(2);
                    switch (next)
                    {
                        case EOF:
                        {
                            Buffer.Append('\\');
                            Next();
                            return LiteralToken();
                        }
                        case '(':
                        case '[':
                        case '@':
                        case '$':
                        case '#':
                        {
                            Buffer.Append(next);
                            Next(); //skip \
                            Next(); //skip char
                            break;
                        }
                        default:
                        {
                            EsacpeProcess('"');
                            break;
                        }
                    }
                }
                else
                {
                    Buffer.Append(Current);
                    Next();
                }
            }
        }

        private IToken PassthroughFunction(TemplateTokenType tokenType)
        {
            var t = new CommonToken(
                _input,
                tokenType == TemplateTokenType.Expansion
                    ? EPass
                    : MPass,
                TokenChannels.Default,
                CurrentStart,
                _input.CharPositionInLine)
            {
                Text =
                    tokenType == TemplateTokenType.Expansion ? "$(" : "@(",
                Line = _input.Line,
            };
            return t;
        }

        public IToken StringLiteralToken()
        {
            while (Current != '\''
                && Current != EOF)
            {
                if (Current == EOF)
                {
                    var eof = new CommonToken(
                        _input,
                        StringLiteral,
                        TokenChannels.Default,
                        CurrentStart,
                        _input.CharPositionInLine)
                    {
                        Text = Buffer.ToString(),
                        Line = _input.Line,
                    };
                    Buffer.Clear();
                    return eof;
                }
                if (Current == '\\')
                {
                    EsacpeProcess('\'');
                }
                else
                {
                    Buffer.Append(Current);
                    Next();
                }
            }
            if (Current != EOF)
            {
                Next();
            }
            var t = new CommonToken(
                _input,
                StringLiteral,
                TokenChannels.Default,
                CurrentStart,
                _input.CharPositionInLine)
            {
                Text = Buffer.ToString(),
                Line = _input.Line,
            };
            Buffer.Clear();
            return t;
        }

        private void EsacpeProcess(char delim)
        {
            var skipAutoForward = false;
            var next = (char) _input.LA(2);
            switch (next)
            {
                case 'b':
                {
                    Buffer.Append('\b');
                    break;
                }
                case 't':
                {
                    Buffer.Append('\t');
                    break;
                }
                case 'n':
                {
                    Buffer.Append('\n');
                    break;
                }
                case 'f':
                {
                    Buffer.Append('\f');
                    break;
                }
                case 'r':
                {
                    Buffer.Append('\r');
                    break;
                }
                case 'u':
                {
                    var sequence = new StringBuilder(4);
                    Next();
                    for (var i = 0; i < 4; i++)
                    {
                        sequence.Append(Current);
                        Next();
                    }
                    var c =
                        (char)
                            int.Parse(
                                sequence.ToString(),
                                NumberStyles.HexNumber);
                    Buffer.Append(c);
                    skipAutoForward = true;
                    break;
                }
                case '\\':
                {
                    Buffer.Append('\\');
                    break;
                }
                default:
                {
                    if (next == delim)
                    {
                        Buffer.Append(delim);
                    }
                    else
                    {
                        Buffer.Append("\\" + next);
                    }
                    break;
                }
            }
            if (skipAutoForward)
                return;
            Next(); // \
            Next(); // control code
        }

        private IToken NumberToken()
        {
            if (Current == '0')
            {
                var next = (char) _input.LA(2);
                if (next == 'x'
                    || next == 'X')
                {
                    return HexToken();
                }
            }
            return DecimalOrFloat();
        }

        private IToken DecimalOrFloat()
        {
            var isDouble = false;
            while (char.IsDigit(Current))
            {
                Buffer.Append(Current);
                Next();
            }

            if (Current == '.')
            {
                isDouble = true;
                Buffer.Append('.');
                Next();
                while (char.IsDigit(Current))
                {
                    Buffer.Append(Current);
                    Next();
                }
            }

            if (Current == 'e'
                || Current == 'E')
            {
                isDouble = true;
                Buffer.Append('e');
                Next();
                if (Current == '-'
                    || Current == '+')
                {
                    Buffer.Append(Current);
                    Next();
                }
                while (char.IsDigit(Current))
                {
                    Buffer.Append(Current);
                    Next();
                }
            }

            IToken t;
            switch (Current)
            {
                case 'L':
                {
                    Next(); //skip suffix
                    t = new CommonToken(
                        _input,
                        SignedLong,
                        TokenChannels.Default,
                        CurrentStart,
                        _input.CharPositionInLine)
                    {
                        Text = Buffer.ToString(),
                        Line = _input.Line,
                    };
                    break;
                }
                case 'D':
                {
                    Next(); //skip suffix
                    t = new CommonToken(
                        _input,
                        Double,
                        TokenChannels.Default,
                        CurrentStart,
                        _input.CharPositionInLine)
                    {
                        Text = Buffer.ToString(),
                        Line = _input.Line,
                    };
                    break;
                }
                case 'U':
                {
                    Next(); //skip suffix
                    t = new CommonToken(
                        _input,
                        UnsignedInteger,
                        TokenChannels.Default,
                        CurrentStart,
                        _input.CharPositionInLine)
                    {
                        Text = Buffer.ToString(),
                        Line = _input.Line,
                    };
                    break;
                }
                case 'M':
                {
                    Next(); //skip suffix
                    t = new CommonToken(
                        _input,
                        Decimal,
                        TokenChannels.Default,
                        CurrentStart,
                        _input.CharPositionInLine)
                    {
                        Text = Buffer.ToString(),
                        Line = _input.Line,
                    };
                    break;
                }
                default:
                {
                    t = new CommonToken(
                        _input,
                        isDouble ? Double : Integer,
                        TokenChannels.Default,
                        CurrentStart,
                        _input.CharPositionInLine)
                    {
                        Text = Buffer.ToString(),
                        Line = _input.Line,
                    };
                    break;
                }
            }
            Buffer.Clear();
            return t;
        }

        private IToken HexToken()
        {
            Next(); //skip 0
            Next(); //skip x/X
            while (char.IsDigit(Current)
                || ('a' <= Current && Current <= 'f')
                || ('A' <= Current && Current <= 'F'))
            {
                Buffer.Append(Current);
            }
            var t = new CommonToken(
                _input,
                Hex,
                TokenChannels.Default,
                CurrentStart,
                _input.CharPositionInLine)
            {
                Text = Buffer.ToString(),
                Line = _input.Line,
            };
            Buffer.Clear();
            return t;
        }

        private bool IsConstructStart()
        {
            return Current == '$' || Current == '@' || Current == '#';
        }

        private bool IsStatementStart()
        {
            return Current == '$' || Current == '@';
        }

        private bool IsIDChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '-' || c == '_';
        }

        private IToken LiteralToken()
        {
            var t = new CommonToken(
                _input,
                Literal,
                TokenChannels.Default,
                CurrentStart,
                _input.CharPositionInLine)
            {
                Text = Buffer.ToString(),
                Line = _input.Line,
            };
            Buffer.Clear();
            return t;
        }

        private IToken DelimToken(int type, char delim)
        {
            var t = new CommonToken(
                _input,
                type,
                TokenChannels.Default,
                CurrentStart,
                _input.CharPositionInLine)
            {
                Text = new string(delim, 1),
                Line = _input.Line,
            };
            Buffer.Clear();
            return t;
        }

        private IToken DelimToken(int type, string delim)
        {
            var t = new CommonToken(
                _input,
                type,
                TokenChannels.Default,
                CurrentStart,
                _input.CharPositionInLine)
            {
                Text = delim,
                Line = _input.Line,
            };
            Buffer.Clear();
            return t;
        }

        private IToken EOFToken()
        {
            var t = new CommonToken(
                _input,
                EOF_TYPE,
                TokenChannels.Default,
                CurrentStart,
                _input.CharPositionInLine)
            {
                Text = "EOF",
                Line = _input.Line,
            };
            return t;
        }

        private void Next()
        {
            _input.Consume();
            Current = (char) _input.LA(1);
        }

        private static bool IsWS(char c)
        {
            return c == ' ' || c == '\t' || c == '\r' || c == '\n'
                || c == '\u000C';
        }

        private void IgnoreWS()
        {
            while (IsWS(Current))
                Next();
        }

        private void FoundLP()
        {
            var top = _scope.Peek();
            if (top == ConstructType.FindLP)
            {
                _scope.Pop();
                EnterConstruct(ConstructType.ArgList);
            }
            else if (top == ConstructType.FindControlLP)
            {
                _scope.Pop();
                EnterConstruct(ConstructType.ControlArgList);
            }
            else
            {
                EnterConstruct(ConstructType.ArgList);
            }
        }

        private void EnterConstruct(
            ConstructType type = ConstructType.Expression)
        {
            switch (type)
            {
                case ConstructType.DynamicString:
                    _lexerMode = TemplateLexerMode.DynamicString;
                    break;
                case ConstructType.FormalStatement:
                case ConstructType.FreeStatement:
                    _lexerMode = TemplateLexerMode.Statement;
                    break;
                case ConstructType.FindLP:
                case ConstructType.ArgList:
                case ConstructType.Expression:
                case ConstructType.FindControlLP:
                    _lexerMode = TemplateLexerMode.Inside;
                    break;
            }
            _scope.Push(type);
        }

        private void ExitConstruct()
        {
            var leaving = _scope.Pop();
            switch (leaving)
            {
                case ConstructType.ControlArgList:
                {
                    SetStripNextNewLine();
                    break;
                }
            }
            if (_scope.Count < 1)
            {
                _lexerMode = TemplateLexerMode.Outside;
            }
            else

                switch (_scope.Peek())
                {
                    case ConstructType.DynamicString:
                        _lexerMode = TemplateLexerMode.DynamicString;
                        break;
                    case ConstructType.FreeStatement:
                    case ConstructType.FormalStatement:
                        _lexerMode = TemplateLexerMode.Statement;
                        break;
                    case ConstructType.ControlArgList:
                    case ConstructType.ArgList:
                    case ConstructType.Expression:
                        _lexerMode = TemplateLexerMode.Inside;
                        break;
                }
        }

        private void TryStripNextNewLine()
        {
            if (!_shouldStripNextNewLine)
                return;
            _shouldStripNextNewLine = false;
            if (Current == '\r')
            {
                Next();
            }
            if (Current == '\n')
            {
                Next();
            }
        }

        private void SetStripNextNewLine()
        {
            _shouldStripNextNewLine = true;
        }
    }
}