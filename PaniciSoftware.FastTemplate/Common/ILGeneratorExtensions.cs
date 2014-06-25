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

namespace PaniciSoftware.FastTemplate.Common
{
    public static class ILGeneratorExtensions
    {
        public static void FastEmitLoadArg(this ILGenerator sink, int index)
        {
            switch (index)
            {
                case 0:
                    sink.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    sink.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    sink.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    sink.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    sink.Emit(OpCodes.Ldarg, index);
                    break;
            }
        }

        public static void FastEmitConstInt(this ILGenerator sink, int i)
        {
            switch (i)
            {
                case 0:
                    sink.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    sink.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    sink.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    sink.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    sink.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    sink.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    sink.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    sink.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    sink.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    sink.Emit(OpCodes.Ldc_I4, i);
                    break;
            }
        }

        public static void FastEmitLoadLocal(this ILGenerator sink, int index)
        {
            switch (index)
            {
                case 0:
                    sink.Emit(OpCodes.Ldloc_0);
                    break;
                case 1:
                    sink.Emit(OpCodes.Ldloc_1);
                    break;
                case 2:
                    sink.Emit(OpCodes.Ldloc_2);
                    break;
                case 3:
                    sink.Emit(OpCodes.Ldloc_3);
                    break;
                default:
                    sink.Emit(OpCodes.Ldloc, index);
                    break;
            }
        }

        public static void FastEmitStoreLocal(this ILGenerator sink, int index)
        {
            switch (index)
            {
                case 0:
                    sink.Emit(OpCodes.Stloc_0);
                    break;
                case 1:
                    sink.Emit(OpCodes.Stloc_1);
                    break;
                case 2:
                    sink.Emit(OpCodes.Stloc_2);
                    break;
                case 3:
                    sink.Emit(OpCodes.Stloc_3);
                    break;
                default:
                    sink.Emit(OpCodes.Stloc, index);
                    break;
            }
        }
    }
}