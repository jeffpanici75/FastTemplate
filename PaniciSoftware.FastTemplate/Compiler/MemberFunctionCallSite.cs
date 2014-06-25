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
using System.Reflection;
using System.Reflection.Emit;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Compiler
{
    internal class MemberFunctionCallSite : CallSite
    {
        private readonly InvokeMemberFunction _delegate;

        public MemberFunctionCallSite(MethodInfo info)
        {
            var t = info.DeclaringType;
            var isVoid = info.ReturnType == typeof (void);
            Method = new DynamicMethod(
                string.Format("__{0}_call{1}", t.Name, info.Name),
                typeof (object),
                new[] {typeof (object), typeof (object[])},
                typeof (VirtualTemplate),
                true);
            var generator = Method.GetILGenerator();
            if (!info.IsStatic)
            {
                generator.Emit(OpCodes.Ldarg_0);
            }

            var parameters = new List<ParameterInfo>(info.GetParameters());

            for (var i = 0; i < parameters.Count; i++)
            {
                generator.Emit(OpCodes.Ldarg_1);
                generator.FastEmitConstInt(i);
                generator.Emit(OpCodes.Ldelem_Ref);
                var pType = parameters[i].ParameterType;
                generator.Emit(OpCodes.Castclass, pType);
                if (pType.IsValueType)
                {
                    generator.Emit(OpCodes.Unbox_Any, pType);
                }
            }

            generator.Emit(
                info.IsVirtual
                    ? OpCodes.Callvirt
                    : OpCodes.Call,
                info);
            if (isVoid)
            {
                generator.Emit(OpCodes.Ldnull);
            }
            else
            {
                if (info.ReturnType.IsValueType)
                {
                    generator.Emit(OpCodes.Box, info.ReturnType);
                }
            }
            generator.Emit(OpCodes.Ret);
            _delegate = (InvokeMemberFunction) Method.CreateDelegate(typeof (InvokeMemberFunction));
        }

        public override object Invoke(object that, object[] args)
        {
            return _delegate(that, args);
        }

        internal delegate object InvokeMemberFunction(object that, object[] args);
    }
}