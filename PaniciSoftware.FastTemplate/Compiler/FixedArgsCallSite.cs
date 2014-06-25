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
using System.Reflection;
using System.Reflection.Emit;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Compiler
{
    internal class FixedArgsCallSite
    {
        private Invoke0 _0;
        private Invoke1 _1;
        private Invoke2 _2;
        private Invoke3 _3;
        private Invoke4 _4;
        private Invoke5 _5;
        private Invoke6 _6;
        private Invoke7 _7;
        private Invoke8 _8;
        private Invoke9 _9;
        private Invoke10 _10;
        private Invoke11 _11;
        private Invoke12 _12;
        private Invoke13 _13;
        private Invoke14 _14;
        private Invoke15 _15;
        private Invoke16 _16;

        public FixedArgsCallSite(int args, MethodInfo info)
        {
            var argsType = new Type[args + 1];
            argsType[0] = typeof (object);
            for (var i = 0; i < args; i++)
            {
                argsType[i + 1] = typeof (object);
            }
            var t = info.DeclaringType;
            var isVoid = info.ReturnType == typeof (void);
            Method = new DynamicMethod(
                string.Format("__{0}_call{1}", t.Name, info.Name),
                typeof (object),
                argsType,
                typeof (VirtualTemplate),
                true);
            var generator = Method.GetILGenerator();
            if (!info.IsStatic)
            {
                generator.Emit(OpCodes.Ldarg_0);
            }

            var parameters = info.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                generator.FastEmitLoadArg(i + 1);
                var valueType = parameters[i].ParameterType;
                generator.Emit(OpCodes.Castclass, valueType);
                if (valueType.IsValueType)
                {
                    generator.Emit(OpCodes.Unbox_Any, valueType);
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
            SetDelegate(Method, args);
        }

        public DynamicMethod Method { get; set; }

        public void SetDelegate(DynamicMethod method, int argCount)
        {
            switch (argCount)
            {
                case 0:
                    _0 = (Invoke0) method.CreateDelegate(typeof (Invoke0));
                    break;
                case 1:
                    _1 = (Invoke1) method.CreateDelegate(typeof (Invoke1));
                    break;
                case 2:
                    _2 = (Invoke2) method.CreateDelegate(typeof (Invoke2));
                    break;
                case 3:
                    _3 = (Invoke3) method.CreateDelegate(typeof (Invoke3));
                    break;
                case 4:
                    _4 = (Invoke4) method.CreateDelegate(typeof (Invoke4));
                    break;
                case 5:
                    _5 = (Invoke5) method.CreateDelegate(typeof (Invoke5));
                    break;
                case 6:
                    _6 = (Invoke6) method.CreateDelegate(typeof (Invoke6));
                    break;
                case 7:
                    _7 = (Invoke7) method.CreateDelegate(typeof (Invoke7));
                    break;
                case 8:
                    _8 = (Invoke8) method.CreateDelegate(typeof (Invoke8));
                    break;
                case 9:
                    _9 = (Invoke9) method.CreateDelegate(typeof (Invoke9));
                    break;
                case 10:
                    _10 = (Invoke10) method.CreateDelegate(typeof (Invoke10));
                    break;
                case 11:
                    _11 = (Invoke11) method.CreateDelegate(typeof (Invoke11));
                    break;
                case 12:
                    _12 = (Invoke12) method.CreateDelegate(typeof (Invoke12));
                    break;
                case 13:
                    _13 = (Invoke13) method.CreateDelegate(typeof (Invoke13));
                    break;
                case 14:
                    _14 = (Invoke14) method.CreateDelegate(typeof (Invoke14));
                    break;
                case 15:
                    _15 = (Invoke15) method.CreateDelegate(typeof (Invoke15));
                    break;
                case 16:
                    _16 = (Invoke16) method.CreateDelegate(typeof (Invoke16));
                    break;
            }
        }

        public object Invoke(
            object that)
        {
            return _0.Invoke(that);
        }

        public object Invoke(
            object that,
            object a0)
        {
            return _1(that, a0);
        }

        public object Invoke(
            object that,
            object a0,
            object a1)
        {
            return _2(
                that,
                a0,
                a1);
        }


        public object Invoke(
            object that,
            object a0,
            object a1,
            object a2)
        {
            return _3(
                that,
                a0,
                a1,
                a2);
        }

        public object Invoke(
            object that,
            object a0,
            object a1,
            object a2,
            object a3)
        {
            return _4(
                that,
                a0,
                a1,
                a2,
                a3);
        }

        public object Invoke(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4)
        {
            return _5(
                that,
                a0,
                a1,
                a2,
                a3,
                a4);
        }

        public object Invoke(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5)
        {
            return _6(
                that,
                a0,
                a1,
                a2,
                a3,
                a4,
                a5);
        }

        public object Invoke(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6)
        {
            return _7(
                that,
                a0,
                a1,
                a2,
                a3,
                a4,
                a5,
                a6);
        }

        public object Invoke(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6,
            object a7)
        {
            return _8(
                that,
                a0,
                a1,
                a2,
                a3,
                a4,
                a5,
                a6,
                a7);
        }

        public object Invoke(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6,
            object a7,
            object a8)
        {
            return _9(
                that,
                a0,
                a1,
                a2,
                a3,
                a4,
                a5,
                a6,
                a7,
                a8);
        }

        public object Invoke(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6,
            object a7,
            object a8,
            object a9)
        {
            return _10(
                that,
                a0,
                a1,
                a2,
                a3,
                a4,
                a5,
                a6,
                a7,
                a8,
                a9);
        }

        public object Invoke(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6,
            object a7,
            object a8,
            object a9,
            object a10)
        {
            return _11(
                that,
                a0,
                a1,
                a2,
                a3,
                a4,
                a5,
                a6,
                a7,
                a8,
                a9,
                a10);
        }

        public object Invoke(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6,
            object a7,
            object a8,
            object a9,
            object a10,
            object a11)
        {
            return _12(
                that,
                a0,
                a1,
                a2,
                a3,
                a4,
                a5,
                a6,
                a7,
                a8,
                a9,
                a10,
                a11);
        }

        public object Invoke(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6,
            object a7,
            object a8,
            object a9,
            object a10,
            object a11,
            object a12)
        {
            return _13(
                that,
                a0,
                a1,
                a2,
                a3,
                a4,
                a5,
                a6,
                a7,
                a8,
                a9,
                a10,
                a11,
                a12);
        }

        public object Invoke(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6,
            object a7,
            object a8,
            object a9,
            object a10,
            object a11,
            object a12,
            object a13)
        {
            return _14(
                that,
                a0,
                a1,
                a2,
                a3,
                a4,
                a5,
                a6,
                a7,
                a8,
                a9,
                a10,
                a11,
                a12,
                a13);
        }

        public object Invoke(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6,
            object a7,
            object a8,
            object a9,
            object a10,
            object a11,
            object a12,
            object a13,
            object a14)
        {
            return _15(
                that,
                a0,
                a1,
                a2,
                a3,
                a4,
                a5,
                a6,
                a7,
                a8,
                a9,
                a10,
                a11,
                a12,
                a13,
                a14);
        }

        public object Invoke(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6,
            object a7,
            object a8,
            object a9,
            object a10,
            object a11,
            object a12,
            object a13,
            object a14,
            object a15)
        {
            return _16(
                that,
                a0,
                a1,
                a2,
                a3,
                a4,
                a5,
                a6,
                a7,
                a8,
                a9,
                a10,
                a11,
                a12,
                a13,
                a14,
                a15);
        }

        //-------------------------------------------------------------------
        // 
        // Internal Interfaces
        //
        //-------------------------------------------------------------------
        internal delegate object Invoke0(
            object that);

        internal delegate object Invoke1(
            object that,
            object a0);

        internal delegate object Invoke10(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6,
            object a7,
            object a8,
            object a9);

        internal delegate object Invoke11(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6,
            object a7,
            object a8,
            object a9,
            object a10);

        internal delegate object Invoke12(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6,
            object a7,
            object a8,
            object a9,
            object a10,
            object a11);

        internal delegate object Invoke13(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6,
            object a7,
            object a8,
            object a9,
            object a10,
            object a11,
            object a12);

        internal delegate object Invoke14(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6,
            object a7,
            object a8,
            object a9,
            object a10,
            object a11,
            object a12,
            object a13);

        internal delegate object Invoke15(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6,
            object a7,
            object a8,
            object a9,
            object a10,
            object a11,
            object a12,
            object a13,
            object a14);

        internal delegate object Invoke16(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6,
            object a7,
            object a8,
            object a9,
            object a10,
            object a11,
            object a12,
            object a13,
            object a14,
            object a15);

        internal delegate object Invoke2(
            object that,
            object a0,
            object a1);

        internal delegate object Invoke3(
            object that,
            object a0,
            object a1,
            object a2);

        internal delegate object Invoke4(
            object that,
            object a0,
            object a1,
            object a2,
            object a3);

        internal delegate object Invoke5(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4);

        internal delegate object Invoke6(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5);

        internal delegate object Invoke7(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6);

        internal delegate object Invoke8(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6,
            object a7);

        internal delegate object Invoke9(
            object that,
            object a0,
            object a1,
            object a2,
            object a3,
            object a4,
            object a5,
            object a6,
            object a7,
            object a8);
    }
}