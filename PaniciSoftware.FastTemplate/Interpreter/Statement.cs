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
    internal enum StatementType
    {
        Expansion,
        Macro,
        EFunction,
        MFunction,
        EIndex,
        MIndex,
        EPass,
        MPass,
    }

    internal class Statement : Evaluator
    {
        public Statement(ITree tree) : base(tree)
        {
        }

        public ErrorList SetTargetObject(
            Interpreter terp,
            ITemplateType source)
        {
            Errors.Clear();
            Interpreter = terp;
            InternalSetTargetObject((CommonTree) T.Children[0], source);
            return Errors;
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
                case TemplateLexer.MStart:
                case TemplateLexer.EStart:
                {
                    o = EvalStatement(c);
                    if (c.Type == TemplateLexer.MStart)
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

        private bool InvokeDelegate(
            ITemplateType d,
            CommonTree argTree,
            out ITemplateType result)
        {
            var func = (Delegate) d.RawValue;
            var args = GetArgs(argTree);

            if (func.Method.ReturnType == typeof (void))
            {
                try
                {
                    func.DynamicInvoke(args);
                    result = new VoidType();
                    return true;
                }
                catch (Exception e)
                {
                    Errors.ErrorRuntimeError(e);
                    result = null;
                    return false;
                }
            }
            try
            {
                var rt = func.DynamicInvoke(args);
                result = PrimitiveType.Create(rt);
                return true;
            }
            catch (Exception e)
            {
                Errors.ErrorRuntimeError(e);
                result = null;
                return false;
            }
        }

        private bool InvokeFunc(
            ITemplateType that,
            string name,
            CommonTree argTree,
            out ITemplateType result)
        {
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                Errors.ErrorFunctionNotFound(type, name);
                result = StringType.Empty();
                return false;
            }
            var args = GetArgs(argTree);
            if (method.ReturnType == typeof (void))
            {
                try
                {
                    method.Invoke(that.RawValue, args);
                    result = new VoidType();
                    return true;
                }
                catch (Exception e)
                {
                    Errors.ErrorRuntimeError(e);
                    result = null;
                    return false;
                }
            }
            try
            {
                var rt = method.Invoke(that.RawValue, args);
                result = PrimitiveType.Create(rt);
                return true;
            }
            catch (Exception e)
            {
                Errors.ErrorRuntimeError(e);
                result = null;
                return false;
            }
        }

        private bool Index(
            ITemplateType that,
            CommonTree argTree,
            out ITemplateType result)
        {
            var type = that.UnderlyingType;
            var indexer = type.GetProperty("Item");
            if (indexer == null)
            {
                Errors.WarningPropertyNotFound(type, "'Index based property'");
                result = StringType.Empty();
                return false;
            }
            var args = GetArgs(argTree);
            try
            {
                var item = indexer.GetValue(that.RawValue, args);
                result = PrimitiveType.Create(item);
                return true;
            }
            catch (Exception e)
            {
                Errors.ErrorRuntimeError(e);
                result = null;
                return false;
            }
        }

        private void SetIndex(
            ITemplateType that,
            ITemplateType source,
            CommonTree argTree)
        {
            var type = that.UnderlyingType;
            var indexer = type.GetProperty("Item");
            if (indexer == null)
            {
                Errors.WarningPropertyNotFound(type, "'Index based property'");
                return;
            }
            var args = GetArgs(argTree);
            try
            {
                indexer.SetValue(that.RawValue, source.RawValue, args);
            }
            catch (Exception e)
            {
                Errors.ErrorRuntimeError(e);
            }
        }

        private void Set(
            ITemplateType that,
            string name,
            ITemplateType source)
        {
            var type = that.UnderlyingType;
            var prop = type.GetProperty(name);
            if (prop == null)
            {
                Errors.WarningPropertyNotFound(type, name);
                return;
            }
            try
            {
                prop.SetValue(that.RawValue, source.RawValue, null);
            }
            catch (Exception e)
            {
                Errors.ErrorRuntimeError(e);
            }
        }

        private bool Get(
            ITemplateType that,
            string name,
            out ITemplateType result)
        {
            var type = that.UnderlyingType;
            var prop = type.GetProperty(name);
            if (prop == null)
            {
                Errors.WarningPropertyNotFound(type, name);
                result = StringType.Empty();
                return false;
            }
            try
            {
                result = PrimitiveType.Create(prop.GetValue(that.RawValue, null));
                return true;
            }
            catch (Exception e)
            {
                Errors.ErrorRuntimeError(e);
                result = null;
                return false;
            }
        }

        private object[] GetArgs(CommonTree args)
        {
            if (args.Children == null || args.Children.Count < 1)
                return null;
            var fnArgs = (CommonTree) args.Children[0];
            var arr = new object[fnArgs.Children.Count];
            for (var i = 0; i < fnArgs.Children.Count; i++)
            {
                var exp = new Expression(fnArgs.Children[i]);
                ITemplateType o;
                Errors.AddRange(exp.Eval(Ctx, Interpreter, out o));
                if (Errors.ContainsError())
                    continue;
                arr[i] = o.RawValue;
            }
            return arr;
        }

        private void InternalSetTargetObject(
            CommonTree s,
            ITemplateType source)
        {
            var root = s.Children[0].Text;
            if (s.Children.Count < 2)
            {
                S.SetValue(root, source);
                return;
            }
            ITemplateType r;
            if (!S.TryGetValue(root, out r))
            {
                Errors.ErrorMissingItemInChain(S.GetType(), root);
                return;
            }

            var that = r;
            for (var i = 1; i < s.Children.Count; i++)
            {
                var current = (CommonTree) s.Children[i];
                ITemplateType result;
                switch (current.Type)
                {
                    case 0:
                    {
                        Errors.ErrorParse((CommonErrorNode) current);
                        return;
                    }
                    case TemplateParser.Invoke:
                    {
                        if (i == s.Children.Count - 1)
                        {
                            Errors.ErrorUnableToSetIntoFunctionResult(
                                that.UnderlyingType.Name, current.Text);
                            return;
                        }
                        //that should be a delegate
                        if (!that.CanInvoke())
                        {
                            Errors.ErrorNonInvokableObject(that.ToString());
                            return;
                        }
                        if (!InvokeDelegate(that, current, out result))
                        {
                            return;
                        }
                        that = result;
                        break;
                    }
                    case TemplateParser.Indexer:
                    {
                        if (i == s.Children.Count - 1)
                        {
                            SetIndex(that, source, current);
                            return;
                        }

                        if (!Index(that, current, out result))
                        {
                            return;
                        }
                        that = result;
                        break;
                    }
                    case TemplateParser.Prop:
                    {
                        if (i == s.Children.Count - 1)
                        {
                            Set(that, current.Text, source);
                            return;
                        }
                        var next = (CommonTree) s.Children[i + 1];
                        switch (next.Type)
                        {
                            case TemplateParser.Invoke:
                            {
                                //consume this prop and the args
                                // then invoke
                                var funcName = current.Text;
                                var args = next;
                                if (!InvokeFunc(
                                    that,
                                    funcName,
                                    args,
                                    out result))
                                {
                                    return;
                                }
                                that = result;
                                i++; //move to args
                                break;
                            }
                            default:
                            {
                                if (!Get(
                                    that,
                                    current.Text,
                                    out result))
                                {
                                    return;
                                }
                                that = result;
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            Errors.ErrorUnableToSetIntoFunctionResult(
                that.UnderlyingType.Name,
                s.Children[s.Children.Count - 2].Text);
        }

        private ITemplateType EvalStatement(CommonTree s)
        {
            var root = s.Children[0].Text;
            ITemplateType r;
            if (!S.TryGetValue(root, out r))
            {
                Errors.ErrorKeyNotFound(root);
                return StringType.Empty();
            }
            var that = r;
            for (var i = 1; i < s.Children.Count; i++)
            {
                var current = (CommonTree) s.Children[i];
                ITemplateType result;
                switch (current.Type)
                {
                    case 0:
                    {
                        Errors.ErrorParse((CommonErrorNode) current);
                        return null;
                    }
                    case TemplateParser.Invoke:
                    {
                        //that should be a delegate
                        if (!that.CanInvoke())
                        {
                            Errors.ErrorNonInvokableObject(that.ToString());
                            return null;
                        }
                        if (!InvokeDelegate(that, current, out result))
                        {
                            return null;
                        }
                        that = result;
                        break;
                    }
                    case TemplateParser.Indexer:
                    {
                        if (!Index(that, current, out result))
                        {
                            return StringType.Empty();
                        }
                        that = result;
                        break;
                    }
                    case TemplateParser.Prop:
                    {
                        if (i == s.Children.Count - 1)
                        {
                            if (!Get(
                                that,
                                current.Text,
                                out result))
                            {
                                return StringType.Empty();
                            }
                            that = result;
                            break;
                        }
                        var next = (CommonTree) s.Children[i + 1];
                        switch (next.Type)
                        {
                            case TemplateParser.Invoke:
                            {
                                //consume this prop and the args
                                // then invoke
                                var funcName = current.Text;
                                var args = next;
                                if (!InvokeFunc(
                                    that,
                                    funcName,
                                    args,
                                    out result))
                                {
                                    return StringType.Empty();
                                }
                                that = result;
                                i++; //move to args
                                break;
                            }
                            default:
                            {
                                if (!Get(
                                    that,
                                    current.Text,
                                    out result))
                                {
                                    return StringType.Empty();
                                }
                                that = result;
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            return that;
        }
    }
}