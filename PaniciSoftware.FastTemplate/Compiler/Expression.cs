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
    internal class Expression : Emitter
    {
        public Expression(ITree tree)
            : base(tree)
        {
        }

        protected override void OnGenerate()
        {
            LogicalOr(T);
        }

        private void LogicalOr(CommonTree tree)
        {
            if (CheckTree(tree))
            {
                return;
            }
            var root = tree;
            if (TryPrimary(root))
            {
                return;
            }
            switch (root.Type)
            {
                case TemplateLexer.OR:
                {
                    var shortcut = Ctx.Sink.DefineLabel();
                    var end = Ctx.Sink.DefineLabel();

                    LogicalOr((CommonTree) root.Children[0]);
                    Ctx.EmitUnwrapBool();
                    Ctx.Sink.Emit(OpCodes.Brtrue, shortcut);

                    LogicalOr((CommonTree) root.Children[root.ChildCount - 1]);
                    Ctx.EmitUnwrapBool();

                    Ctx.Sink.Emit(OpCodes.Brtrue, shortcut);
                    Ctx.EmitFalse();
                    Ctx.Sink.Emit(OpCodes.Br, end);

                    Ctx.Sink.MarkLabel(shortcut);
                    Ctx.EmitTrue();
                    Ctx.Sink.MarkLabel(end);
                    break;
                }
                default:
                {
                    LogicalAnd(root);
                    break;
                }
            }
        }

        private void LogicalAnd(CommonTree tree)
        {
            if (CheckTree(tree))
            {
                return;
            }
            var root = tree;
            if (TryPrimary(root))
            {
                return;
            }
            switch (root.Type)
            {
                case TemplateLexer.AND:
                {
                    var shortcut = Ctx.Sink.DefineLabel();
                    var end = Ctx.Sink.DefineLabel();

                    LogicalAnd((CommonTree) root.Children[0]);
                    Ctx.EmitUnwrapBool();
                    Ctx.Sink.Emit(OpCodes.Brfalse, shortcut);
                    LogicalAnd((CommonTree) root.Children[root.ChildCount - 1]);
                    Ctx.EmitUnwrapBool();
                    Ctx.Sink.Emit(OpCodes.Brfalse, shortcut);
                    Ctx.EmitTrue();
                    Ctx.Sink.Emit(OpCodes.Br, end);
                    Ctx.Sink.MarkLabel(shortcut);
                    Ctx.EmitFalse();
                    Ctx.Sink.MarkLabel(end);
                    break;
                }
                default:
                {
                    Equality(root);
                    break;
                }
            }
        }

        private void Equality(CommonTree tree)
        {
            if (CheckTree(tree))
            {
                return;
            }
            var root = tree;
            if (TryPrimary(root))
            {
                return;
            }
            switch (root.Type)
            {
                case TemplateLexer.NEQ:
                case TemplateLexer.EQ:
                {
                    Equality((CommonTree) root.Children[0]);
                    Equality((CommonTree) root.Children[root.ChildCount - 1]);
                    Ctx.EmitApplyBinary(root.Type == TemplateLexer.NEQ ? Operator.NEQ : Operator.EQ);
                    return;
                }
                default:
                {
                    Relational(root);
                    break;
                }
            }
        }

        private void Relational(CommonTree tree)
        {
            if (CheckTree(tree))
            {
                return;
            }
            var root = tree;
            if (TryPrimary(root))
            {
                return;
            }
            switch (root.Type)
            {
                case TemplateLexer.GE:
                case TemplateLexer.LE:
                case TemplateLexer.LT:
                case TemplateLexer.GT:
                {
                    Relational((CommonTree) root.Children[0]);
                    Relational((CommonTree) root.Children[root.ChildCount - 1]);
                    Ctx.EmitApplyBinary(
                        ExpressionHelper.TokenToOperator(root.Type));
                    break;
                }
                default:
                {
                    Additive(root);
                    break;
                }
            }
        }

        private void Additive(CommonTree tree)
        {
            if (CheckTree(tree))
            {
                return;
            }
            var root = tree;
            if (TryPrimary(root))
            {
                return;
            }
            switch (root.Type)
            {
                case TemplateLexer.Add:
                case TemplateLexer.Minus:
                {
                    Additive((CommonTree) root.Children[0]);
                    Additive(
                        (CommonTree) root.Children[root.ChildCount - 1]);
                    Ctx.EmitApplyBinary(
                        ExpressionHelper.TokenToOperator(root.Type));
                    break;
                }
                default:
                {
                    Multiplicative(root);
                    break;
                }
            }
        }

        private void Multiplicative(CommonTree tree)
        {
            if (CheckTree(tree))
            {
                return;
            }
            var root = tree;
            if (TryPrimary(tree))
            {
                return;
            }
            switch (root.Type)
            {
                case TemplateLexer.Mod:
                case TemplateLexer.Mul:
                case TemplateLexer.Div:
                {
                    Multiplicative((CommonTree) root.Children[0]);
                    Multiplicative(
                        (CommonTree) root.Children[root.ChildCount - 1]);
                    Ctx.EmitApplyBinary(
                        ExpressionHelper.TokenToOperator(root.Type));
                    break;
                }
                default:
                {
                    Unary(root);
                    break;
                }
            }
        }

        private void Unary(CommonTree tree)
        {
            if (CheckTree(tree))
            {
                return;
            }
            var root = tree;
            if (TryPrimary(tree))
            {
                return;
            }
            switch (root.Type)
            {
                case TemplateParser.Unary:
                {
                    var op = (CommonTree) root.Children[0];
                    if (CheckTree(op))
                    {
                        return;
                    }
                    Unary((CommonTree) root.Children[1]);
                    Ctx.EmitApplyUnary(
                        ExpressionHelper.TokenToOperator(op.Type));
                    break;
                }
            }
        }

        private bool TryPrimary(CommonTree tree)
        {
            switch (tree.Type)
            {
                case 0:
                {
                    Ctx.EmitNullRef();
                    return true;
                }
                case TemplateParser.Constant:
                {
                    var cst = new Constant(tree);
                    cst.Generate(Ctx);
                    return true;
                }
                case TemplateParser.Statement:
                {
                    var st = new Statement(tree);
                    Errors.AddRange(st.Generate(Ctx));
                    return true;
                }
                case TemplateParser.Passthrough:
                {
                    var st = new Passthrough(tree);
                    Errors.AddRange(st.Generate(Ctx));
                    return true;
                }
                case TemplateParser.Nested:
                {
                    var expression = (CommonTree) tree.Children[0];
                    LogicalOr(expression);
                    return true;
                }
                case TemplateParser.Error:
                {
                    Errors.ErrorUnknownKeyword(tree.Children[0].Text);
                    return true;
                }
                case TemplateParser.DynamicString:
                {
                    Errors.AddRange(
                        new DynamicString(tree).Generate(Ctx));
                    return true;
                }
                default:
                {
                    return false;
                }
            }
        }

        private bool CheckTree(CommonTree tree)
        {
            if (tree.Type == 0)
            {
                Errors.ErrorParse((CommonErrorNode) tree);
                Ctx.EmitNullRef();
                return true;
            }
            return false;
        }
    }
}