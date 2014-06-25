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
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Compiler
{
    public class AssemblyBuilder
    {
        private readonly Dictionary<Label, int> _labels = new Dictionary<Label, int>();
        private Assembly _assembly;
        private ILGenerator _il;

        public AssemblyBuilder(ILGenerator g)
        {
            _il = g;
            _assembly = new Assembly();
        }

        public Assembly Build()
        {
            _il = null;
            _labels.Clear();
            var a = _assembly;
            _assembly = null;
            return a;
        }

        public void EmitDecimalParse()
        {
            var ins = new DecimalParseIns();
            _assembly.Add(ins);
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            var decimalParse = typeof (decimal).GetMethod(
                "Parse",
                flags,
                null,
                new[] {typeof (string)},
                null);
            _il.Emit(OpCodes.Call, decimalParse);
        }

        public void FastEmitStoreLocal(int i)
        {
            var ins = new StoreLocalIns {Id = i};
            _assembly.Add(ins);
            _il.FastEmitStoreLocal(i);
        }

        public void FastEmitLoadLocal(int i)
        {
            var ins = new LoadLocalIns {Id = i};
            _assembly.Add(ins);
            _il.FastEmitLoadLocal(i);
        }

        public void FastEmitConstInt(int i)
        {
            var ins = new FastConstIntIns {Value = i};
            _assembly.Add(ins);
            _il.FastEmitConstInt(i);
        }

        public LocalBuilder DeclareLocal(Type type)
        {
            var l = _il.DeclareLocal(type);
            var declarLocal = new DeclareLocalIns {Id = l.LocalIndex, LocalType = type};
            _assembly.Add(declarLocal);
            return l;
        }

        public Label DefineLabel()
        {
            var l = _il.DefineLabel();
            var define = new DefineLabel {Id = l.GetHashCode()};
            _labels[l] = l.GetHashCode();
            _assembly.Add(define);
            return l;
        }

        public void MarkLabel(Label l)
        {
            var ins = new MarkLabel {Id = _labels[l]};
            _assembly.Add(ins);
            _il.MarkLabel(l);
        }

        public void Emit(OpCode c, Type t)
        {
            var i = new TypeArgIns {OpCode = c, Arg = t};
            _assembly.Add(i);
            _il.Emit(c, t);
        }

        public void Emit(OpCode c, Label l)
        {
            var ins = new LabelArgIns {OpCode = c, Id = _labels[l]};
            _assembly.Add(ins);
            _il.Emit(c, l);
        }

        public void Emit(OpCode c)
        {
            var i = new OpCodeIns {OpCode = c};
            _assembly.Add(i);
            _il.Emit(c);
        }

        public void Emit(OpCode c, string arg)
        {
            var ins = new StringArgIns {OpCode = c, Arg = arg};
            _assembly.Add(ins);
            _il.Emit(c, arg);
        }

        public void Emit(OpCode c, int arg)
        {
            var i = new IntArgIns {OpCode = c, Arg = arg};
            _assembly.Add(i);
            _il.Emit(c, arg);
        }

        public void Emit(OpCode c, ConstructorInfo info)
        {
            var i = new CtorInfoArgIns {OpCode = c, Arg = info};
            _assembly.Add(i);
            _il.Emit(c, info);
        }

        public void Emit(OpCode c, FieldInfo info)
        {
            var i = new FieldInfoArgIns {OpCode = c, Arg = info};
            _assembly.Add(i);
            _il.Emit(c, info);
        }

        public void Emit(
            OpCode c,
            MethodInfo info,
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        {
            var i = new MethodInfoArgIns {Arg = info, Flags = (uint) flags, OpCode = c};
            _assembly.Add(i);
            _il.Emit(c, info);
        }

        public void Emit(OpCode c, Int64 arg)
        {
            var i = new Int64ArgIns {OpCode = c, Arg = arg};
            _assembly.Add(i);
            _il.Emit(c, arg);
        }

        public void Emit(OpCode c, double arg)
        {
            var i = new DoubleArgIns {OpCode = c, Arg = arg};
            _assembly.Add(i);
            _il.Emit(c, arg);
        }
    }
}