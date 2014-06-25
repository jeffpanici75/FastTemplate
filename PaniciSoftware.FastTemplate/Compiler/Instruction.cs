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
using System.IO;
using System.Reflection;
using System.Text;

namespace PaniciSoftware.FastTemplate.Compiler
{
    public abstract class Instruction
    {
        public abstract byte Type { get; }

        public abstract void Emit(AssemblyContext c);

        public abstract void WriteTo(BinaryWriter s);

        public abstract void ReadFrom(BinaryReader s);

        protected static void WriteType(Type t, BinaryWriter w)
        {
            var name = t.AssemblyQualifiedName;
            WriteString(name, w);
        }

        protected static void WriteString(string s, BinaryWriter w)
        {
            var buffer = Encoding.UTF8.GetBytes(s);
            w.Write(buffer.Length);
            w.Write(buffer);
        }

        protected static void WriteCtorInfo(ConstructorInfo i, BinaryWriter w)
        {
            var type = i.DeclaringType;
            var parameters = i.GetParameters();
            WriteType(type, w);
            var count = parameters.Length;
            w.Write(count);
            foreach (var p in parameters)
                WriteType(p.ParameterType, w);
        }

        protected static void WriteFieldInfo(FieldInfo i, BinaryWriter w)
        {
            var type = i.DeclaringType;
            var name = i.Name;
            WriteType(type, w);
            WriteString(name, w);
        }

        protected static void WriteMethodInfo(MethodInfo i, BinaryWriter w)
        {
            var type = i.DeclaringType;
            var name = i.Name;
            WriteType(type, w);
            WriteString(name, w);
        }

        protected static Type ReadType(BinaryReader w)
        {
            var name = ReadString(w);
            return System.Type.GetType(name);
        }

        protected static string ReadString(BinaryReader w)
        {
            var size = w.ReadInt32();
            var buffer = new byte[size];
            w.Read(buffer, 0, size);
            return Encoding.UTF8.GetString(buffer);
        }

        protected static ConstructorInfo ReadCtorInfo(BinaryReader w)
        {
            var type = ReadType(w);
            var count = w.ReadInt32();
            var args = new Type[count];
            for (var i = 0; i < count; i++)
            {
                args[i] = ReadType(w);
            }
            var ctor = type.GetConstructor(args);
            return ctor;
        }

        protected static FieldInfo ReadFieldInfo(BinaryReader w)
        {
            var type = ReadType(w);
            var name = ReadString(w);
            return type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        protected static MethodInfo ReadMethodInfo(BinaryReader w, uint flags)
        {
            var type = ReadType(w);
            var name = ReadString(w);
            return type.GetMethod(name, (BindingFlags) flags);
        }
    }
}