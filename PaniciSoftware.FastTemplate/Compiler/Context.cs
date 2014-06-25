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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Antlr.Runtime.Tree;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Compiler
{
    internal class Context
    {
        private readonly Stack<JumpScope> _scopes = new Stack<JumpScope>();

        public Context(ILGenerator il)
        {
            Sink = new AssemblyBuilder(il);
            EndOfTemplate = Sink.DefineLabel();
        }

        public AssemblyBuilder Sink { get; private set; }

        public OptimizeLevel OptimizeLevel { get; set; }
        public Label EndOfTemplate { get; set; }

        public bool InJumpableBlock
        {
            get { return _scopes.Count > 0; }
        }

        public Label GetContinueLabel()
        {
            return _scopes.Peek().Continue;
        }

        public Label GetBreakLabel()
        {
            return _scopes.Peek().Break;
        }

        public void PushNewJumpScope(Label cont, Label brk)
        {
            _scopes.Push(new JumpScope {Break = brk, Continue = cont});
        }

        public void PopJumpScope()
        {
            _scopes.Pop();
        }

        public void EmitIterMoveNext()
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var method = typeof (IEnumerator).GetMethod(
                "MoveNext", flags);
            Sink.Emit(OpCodes.Callvirt, method, flags);
        }

        public void EmitAppendToBuffer()
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var methodInfo = typeof (VirtualTemplate).GetMethod(
                "AppendToBuffer", flags);
            Sink.Emit(OpCodes.Callvirt, methodInfo, flags);
        }

        public void EmitAppendLiteralToBuffer()
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var methodInfo = typeof (VirtualTemplate).GetMethod(
                "AppendLiteral", flags);
            Sink.Emit(OpCodes.Callvirt, methodInfo, flags);
        }

        public void EmitEmptyString()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            var t = typeof (StringType).GetMethod(
                "Empty", flags);
            Sink.Emit(OpCodes.Call, t, flags);
        }

        public void EmitStringArray(string[] arr)
        {
            var local = Sink.DeclareLocal(typeof (string[]));
            Sink.FastEmitConstInt(arr.Length);
            Sink.Emit(OpCodes.Newarr, typeof (string));
            Sink.FastEmitStoreLocal(local.LocalIndex);
            for (var i = 0; i < arr.Length; i++)
            {
                Sink.FastEmitLoadLocal(local.LocalIndex);
                Sink.FastEmitConstInt(i);
                Sink.Emit(OpCodes.Ldstr, arr[i]);
                Sink.Emit(OpCodes.Stelem_Ref);
            }
            Sink.FastEmitLoadLocal(local.LocalIndex);
        }

        public ErrorList EmitFlatArgList(CommonTree fnArgs)
        {
            var errors = new ErrorList();
            var args = fnArgs.Children;
            for (var i = 0; i < args.Count; i++)
            {
                var exp = new Expression(args[i]);
                errors.AddRange(exp.Generate(this));
            }
            return errors;
        }

        public ErrorList EmitArgList(CommonTree fnArgs)
        {
            var errors = new ErrorList();
            if (fnArgs.Type == 0)
            {
                EmitNullRef();
                errors.ErrorParse((CommonErrorNode) fnArgs);
                return errors;
            }
            var args = fnArgs.Children;
            var size = args.Count;
            var local = Sink.DeclareLocal(typeof (ITemplateType[]));
            Sink.FastEmitConstInt(size);
            Sink.Emit(OpCodes.Newarr, typeof (ITemplateType));
            Sink.FastEmitStoreLocal(local.LocalIndex);
            for (var i = 0; i < args.Count; i++)
            {
                Sink.FastEmitLoadLocal(local.LocalIndex);
                Sink.FastEmitConstInt(i);
                var exp = new Expression(args[i]);
                errors.AddRange(exp.Generate(this));
                Sink.Emit(OpCodes.Stelem_Ref);
            }
            Sink.FastEmitLoadLocal(local.LocalIndex);
            return errors;
        }

        public ErrorList EmitArgList(CommonTree fnArgs, Action<Context> last)
        {
            var errors = new ErrorList();
            if (fnArgs.Type == 0)
            {
                EmitNullRef();
                errors.ErrorParse((CommonErrorNode) fnArgs);
                return errors;
            }
            var args = fnArgs.Children;
            var size = args.Count + 1;
            var local = Sink.DeclareLocal(typeof (ITemplateType[]));
            Sink.FastEmitConstInt(size);
            Sink.Emit(OpCodes.Newarr, typeof (ITemplateType));
            Sink.FastEmitStoreLocal(local.LocalIndex);
            for (var i = 0; i < args.Count; i++)
            {
                Sink.FastEmitLoadLocal(local.LocalIndex);
                Sink.FastEmitConstInt(i);
                var exp = new Expression(args[i]);
                errors.AddRange(exp.Generate(this));
                Sink.Emit(OpCodes.Stelem_Ref);
            }
            Sink.FastEmitLoadLocal(local.LocalIndex);
            Sink.FastEmitConstInt(fnArgs.Children.Count);
            last(this);
            Sink.Emit(OpCodes.Stelem_Ref);

            Sink.FastEmitLoadLocal(local.LocalIndex);
            return errors;
        }

        public void EmitEmptyArgList()
        {
            Sink.FastEmitConstInt(0);
            Sink.Emit(OpCodes.Newarr, typeof (ITemplateType));
        }

        public ErrorList EmitSingleArgList(Action<Context> last)
        {
            var errors = new ErrorList();
            const int size = 1;
            var local = Sink.DeclareLocal(typeof (ITemplateType[]));
            Sink.FastEmitConstInt(size);
            Sink.Emit(OpCodes.Newarr, typeof (ITemplateType));
            Sink.FastEmitStoreLocal(local.LocalIndex);
            Sink.FastEmitLoadLocal(local.LocalIndex);

            Sink.FastEmitConstInt(0);
            last(this);
            Sink.Emit(OpCodes.Stelem_Ref);

            Sink.FastEmitLoadLocal(local.LocalIndex);
            return errors;
        }

        public void EmitConstIntType(int value)
        {
            Sink.Emit(OpCodes.Ldc_I4, value);
            var ctor =
                typeof (IntType).GetConstructor(
                    new[] {typeof (int)});
            Sink.Emit(OpCodes.Newobj, ctor);
        }

        public void EmitVTFunctionCall(string name)
        {
            var info = typeof (VirtualTemplate).GetMethod(
                name,
                BindingFlags.NonPublic | BindingFlags.Public
                    | BindingFlags.Instance);
            Sink.Emit(OpCodes.Callvirt, info);
        }

        public void EmitVTFunctionCallIgnoreReturn(string name)
        {
            var info = typeof (VirtualTemplate).GetMethod(
                name,
                BindingFlags.NonPublic | BindingFlags.Public
                    | BindingFlags.Instance);
            Sink.Emit(OpCodes.Call, info);
            Sink.Emit(OpCodes.Pop);
        }

        public void EmitUnwrapBool()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            var info = typeof (Context).GetMethod(
                "UnwrapBooleanType", flags);
            Sink.Emit(OpCodes.Call, info, flags);
        }

        public void EmitCombineStringParts()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            var info = typeof (Context).GetMethod(
                "CombineStringParts", flags);
            Sink.Emit(OpCodes.Call, info, flags);
        }

        public void EmitApplyBinary(Operator op)
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            var info = typeof (Context).GetMethod(
                "ApplyBinary", flags);
            Sink.Emit(OpCodes.Ldc_I4, (int) op);
            var fieldInfo =
                typeof (VirtualTemplate).GetField(
                    "_errors",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            Sink.Emit(OpCodes.Ldarg_0);
            Sink.Emit(OpCodes.Ldfld, fieldInfo);
            Sink.Emit(OpCodes.Call, info, flags);
        }

        public void EmitApplyUnary(Operator op)
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            var info = typeof (Context).GetMethod(
                "ApplyUnary", flags);
            Sink.Emit(OpCodes.Ldc_I4, (int) op);
            var fieldInfo =
                typeof (VirtualTemplate).GetField(
                    "_errors",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            Sink.Emit(OpCodes.Ldarg_0);
            Sink.Emit(OpCodes.Ldfld, fieldInfo);
            Sink.Emit(OpCodes.Call, info, flags);
        }

        public void EmitTrue()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            var t = typeof (BooleanType).GetMethod(
                "True", flags);
            Sink.Emit(OpCodes.Call, t, flags);
        }

        public void EmitFalse()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            var t = typeof (BooleanType).GetMethod(
                "False", flags);
            Sink.Emit(OpCodes.Call, t, flags);
        }

        public void EmitNullType()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            var t = typeof (NullType).GetMethod(
                "New", flags);
            Sink.Emit(OpCodes.Call, t, flags);
        }

        public void EmitNullRef()
        {
            Sink.Emit(OpCodes.Ldnull);
        }

        public static ITemplateType ApplyBinary(ITemplateType lhs, ITemplateType rhs, int id, ErrorList errors)
        {
            if (rhs == null || lhs == null)
                return null;
            var op = (Operator) id;
            try
            {
                var rt = lhs.ApplyBinary(op, rhs);
                return PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                errors.ErrorRuntimeError(e);
            }
            return null;
        }

        public static ITemplateType ApplyUnary(ITemplateType lhs, int id, ErrorList errors)
        {
            if (lhs == null)
                return null;
            var op = (Operator) id;
            try
            {
                return PrimitiveType.Create(lhs.ApplyUnary(op));
            }
            catch (Exception e)
            {
                errors.ErrorRuntimeError(e);
            }
            return null;
        }

        public static ITemplateType CombineStringParts(object[] parts)
        {
            if (parts == null)
                return null;
            var buffer = new StringBuilder();
            foreach (var o in parts)
            {
                if (o is string)
                {
                    buffer.Append(o);
                }
                else
                {
                    ((ITemplateType) o).WriteTo(buffer);
                }
            }
            return StringType.New(buffer.ToString());
        }

        public static int UnwrapBooleanType(ITemplateType operand)
        {
            if (operand == null)
                return 0;
            bool flag;
            if (!operand.TryConvert(out flag))
            {
                return 0;
            }
            return flag ? 1 : 0;
        }
    }
}