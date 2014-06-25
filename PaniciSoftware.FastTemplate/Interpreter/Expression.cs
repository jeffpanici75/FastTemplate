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

using System;
using Antlr.Runtime.Tree;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Interpreter
{
    internal class Expression : Evaluator
    {
        public Expression(ITree tree)
            : base(tree)
        {
        }

        protected override void OnEval(
            out ITemplateType o)
        {
            o = LogicalOr(T);
            if (o == null && !Errors.ContainsError()
                && !Errors.ContainsWarning())
            {
                Errors.ErrorRuntimeError();
            }
        }

        private ITemplateType LogicalOr(CommonTree tree)
        {
            if (CheckTree(tree))
            {
                return null;
            }
            var root = tree;
            ITemplateType o;
            if (TryPrimary(root, out o))
            {
                return o;
            }
            switch (root.Type)
            {
                case TemplateLexer.OR:
                {
                    var lhs = LogicalOr((CommonTree) root.Children[0]);
                    CheckType(lhs, typeof (BooleanType));
                    if (Errors.ContainsError())
                        return null;
                    var lRaw = (bool) lhs.RawValue;
                    if (lRaw)
                        return BooleanType.True();
                    var rhs =
                        LogicalOr(
                            (CommonTree) root.Children[root.ChildCount - 1]);
                    CheckType(rhs, typeof (BooleanType));
                    if (Errors.ContainsError())
                        return null;
                    var rRaw = (bool) rhs.RawValue;
                    return new BooleanType {Value = rRaw};
                }
                default:
                {
                    return LogicalAnd(root);
                }
            }
        }

        private ITemplateType LogicalAnd(CommonTree tree)
        {
            if (CheckTree(tree))
            {
                return null;
            }
            var root = tree;
            ITemplateType o;
            if (TryPrimary(root, out o))
            {
                return o;
            }
            switch (root.Type)
            {
                case TemplateLexer.AND:
                {
                    var lhs = LogicalAnd((CommonTree) root.Children[0]);
                    CheckType(lhs, typeof (BooleanType));
                    if (Errors.ContainsError())
                        return null;
                    var lRaw = (bool) lhs.RawValue;
                    if (!lRaw)
                        return BooleanType.False();
                    var rhs =
                        LogicalAnd(
                            (CommonTree) root.Children[root.ChildCount - 1]);
                    CheckType(rhs, typeof (BooleanType));
                    if (Errors.ContainsError())
                        return null;
                    var rRaw = (bool) rhs.RawValue;
                    return new BooleanType {Value = rRaw};
                }
                default:
                {
                    return Equality(root);
                }
            }
        }

        private ITemplateType Equality(CommonTree tree)
        {
            if (CheckTree(tree))
            {
                return null;
            }
            var root = tree;
            ITemplateType o;
            if (TryPrimary(root, out o))
            {
                return o;
            }
            switch (root.Type)
            {
                case TemplateLexer.NEQ:
                case TemplateLexer.EQ:
                {
                    var lhs = Equality((CommonTree) root.Children[0]);
                    var rhs = Equality((CommonTree) root.Children[root.ChildCount - 1]);
                    if (lhs == null || rhs == null)
                        return null;
                    try
                    {
                        return new BooleanType
                        {
                            Value = (bool) lhs.ApplyBinary(ExpressionHelper.TokenToOperator(root.Type), rhs)
                        };
                    }
                    catch (InvalidTypeException t)
                    {
                        Errors.ErrorTypeMismatch(t);
                    }
                    catch (NullReferenceException n)
                    {
                        Errors.ErrorRuntimeErrorNullRef(n);
                    }
                    catch (Exception e)
                    {
                        Errors.ErrorRuntimeError(e);
                    }
                    return null;
                }
                default:
                {
                    return Relational(root);
                }
            }
        }

        private ITemplateType Relational(CommonTree tree)
        {
            if (CheckTree(tree))
            {
                return null;
            }
            var root = tree;
            ITemplateType o;
            if (TryPrimary(root, out o))
            {
                return o;
            }
            switch (root.Type)
            {
                case TemplateLexer.GE:
                case TemplateLexer.LE:
                case TemplateLexer.LT:
                case TemplateLexer.GT:
                {
                    var lhs = Relational((CommonTree) root.Children[0]);
                    var rhs = Relational((CommonTree) root.Children[root.ChildCount - 1]);
                    if (lhs == null || rhs == null)
                        return null;
                    try
                    {
                        return new BooleanType
                        {
                            Value = (bool) lhs.ApplyBinary(ExpressionHelper.TokenToOperator(root.Type), rhs)
                        };
                    }
                    catch (InvalidTypeException t)
                    {
                        Errors.ErrorTypeMismatch(t);
                    }
                    catch (NullReferenceException n)
                    {
                        Errors.ErrorRuntimeErrorNullRef(n);
                    }
                    catch (Exception e)
                    {
                        Errors.ErrorRuntimeError(e);
                    }
                    return null;
                }
                default:
                {
                    return Additive(root);
                }
            }
        }

        private ITemplateType Additive(CommonTree tree)
        {
            if (CheckTree(tree))
            {
                return null;
            }
            var root = tree;
            ITemplateType o;
            if (TryPrimary(root, out o))
            {
                return o;
            }
            switch (root.Type)
            {
                case TemplateLexer.Add:
                case TemplateLexer.Minus:
                {
                    var lhs = Additive((CommonTree) root.Children[0]);
                    var rhs = Additive((CommonTree) root.Children[root.ChildCount - 1]);
                    if (lhs == null || rhs == null)
                        return null;
                    try
                    {
                        return PrimitiveType.Create(lhs.ApplyBinary(ExpressionHelper.TokenToOperator(root.Type), rhs));
                    }
                    catch (InvalidTypeException t)
                    {
                        Errors.ErrorTypeMismatch(t);
                    }
                    catch (NullReferenceException n)
                    {
                        Errors.ErrorRuntimeErrorNullRef(n);
                    }
                    catch (Exception e)
                    {
                        Errors.ErrorRuntimeError(e);
                    }
                    return null;
                }
                default:
                {
                    return Multiplicative(root);
                }
            }
        }

        private ITemplateType Multiplicative(CommonTree tree)
        {
            if (CheckTree(tree))
            {
                return null;
            }
            var root = tree;
            ITemplateType o;
            if (TryPrimary(tree, out o))
            {
                return o;
            }
            switch (root.Type)
            {
                case TemplateLexer.Mod:
                case TemplateLexer.Mul:
                case TemplateLexer.Div:
                {
                    var lhs = Multiplicative((CommonTree) root.Children[0]);
                    var rhs = Multiplicative((CommonTree) root.Children[root.ChildCount - 1]);
                    if (lhs == null || rhs == null)
                        return null;
                    try
                    {
                        return PrimitiveType.Create(lhs.ApplyBinary(ExpressionHelper.TokenToOperator(root.Type), rhs));
                    }
                    catch (InvalidTypeException t)
                    {
                        Errors.ErrorTypeMismatch(t);
                    }
                    catch (NullReferenceException n)
                    {
                        Errors.ErrorRuntimeErrorNullRef(n);
                    }
                    catch (Exception e)
                    {
                        Errors.ErrorRuntimeError(e);
                    }
                    break;
                }
                default:
                {
                    return Unary(root);
                }
            }
            return null;
        }

        private ITemplateType Unary(CommonTree tree)
        {
            if (CheckTree(tree))
            {
                return null;
            }
            var root = tree;
            ITemplateType o;
            if (TryPrimary(tree, out o))
            {
                return o;
            }
            switch (root.Type)
            {
                case TemplateParser.Unary:
                {
                    var op = (CommonTree) root.Children[0];
                    if (CheckTree(op))
                    {
                        return null;
                    }
                    var operand = Unary((CommonTree) root.Children[1]);
                    if (operand == null)
                        return null;
                    try
                    {
                        return PrimitiveType.Create(operand.ApplyUnary(ExpressionHelper.TokenToOperator(op.Type)));
                    }
                    catch (InvalidTypeException t)
                    {
                        Errors.ErrorTypeMismatch(t);
                    }
                    catch (NullReferenceException n)
                    {
                        Errors.ErrorRuntimeErrorNullRef(n);
                    }
                    catch (Exception e)
                    {
                        Errors.ErrorRuntimeError(e);
                    }
                    break;
                }
            }
            return null;
        }

        private bool TryPrimary(CommonTree tree, out ITemplateType o)
        {
            switch (tree.Type)
            {
                case 0:
                {
                    Errors.ErrorParse((CommonErrorNode) tree);
                    o = null;
                    return true;
                }
                case TemplateParser.DynamicString:
                {
                    Errors.AddRange(new DynamicString(tree).Eval(Ctx, Interpreter, out o));
                    return true;
                }
                case TemplateParser.Constant:
                {
                    Errors.AddRange(new Constant(tree).Eval(Ctx, Interpreter, out o));
                    return true;
                }
                case TemplateParser.Statement:
                {
                    Errors.AddRange(new Statement(tree).Eval(Ctx, Interpreter, out o));
                    return true;
                }
                case TemplateParser.Passthrough:
                {
                    Errors.AddRange(new Passthrough(tree).Eval(Ctx, Interpreter, out o));
                    return true;
                }
                case TemplateParser.Nested:
                {
                    var expression = (CommonTree) tree.Children[0];
                    o = LogicalOr(expression);
                    return true;
                }
                case TemplateParser.Error:
                {
                    Errors.ErrorUnknownKeyword(tree.Children[0].Text);
                    o = null;
                    return true;
                }
                default:
                {
                    o = null;
                    return false;
                }
            }
        }

        private void CheckType(object o, Type expected)
        {
            if (o == null || o.GetType() != expected)
            {
                Errors.ErrorTypeMismatch(
                    expected.ToString(),
                    o == null ? "(null)" : o.GetType().ToString());
            }
        }

        private bool CheckTree(CommonTree tree)
        {
            if (tree.Type == 0)
            {
                Errors.ErrorParse((CommonErrorNode) tree);
                return true;
            }
            return false;
        }
    }
}