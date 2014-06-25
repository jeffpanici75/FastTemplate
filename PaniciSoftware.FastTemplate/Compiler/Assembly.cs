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
using System.IO;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Compiler
{
    public class Assembly : IAssembly
    {
        private const short Magic = 0xbe;

        private readonly List<Instruction> _template = new List<Instruction>();

        public Assembly(Assembly a)
        {
            _template = new List<Instruction>(a._template);
        }

        public Assembly(Stream s)
        {
            var dict = new Dictionary<byte, Type>();
            var types = typeof (Assembly).Assembly.GetTypes();
            foreach (var t in types)
            {
                if (!t.IsSubclassOf(typeof (Instruction)))
                    continue;
                var instance = (Instruction) Activator.CreateInstance(t);
                var id = instance.Type;
                dict[id] = t;
            }

            using (var reader = new BinaryReader(s))
            {
                ReaderHeader(reader);
                while (reader.PeekChar() > -1)
                {
                    var type = reader.ReadByte();
                    var ins = dict[type];
                    var i = (Instruction) Activator.CreateInstance(ins);
                    i.ReadFrom(reader);
                    _template.Add(i);
                }
            }
        }

        public Assembly()
        {
        }

        public int MajorVersion
        {
            get { return 1; }
        }

        public int MinorVersion
        {
            get { return 0; }
        }

        public void Write(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                WriteHeader(w);
                foreach (var i in _template)
                {
                    w.Write(i.Type);
                    i.WriteTo(w);
                }
                //footer?
            }
        }

        public void Add(Instruction i)
        {
            _template.Add(i);
        }

        public void Clear()
        {
            _template.Clear();
        }

        internal void Copmile(AssemblyContext ctx)
        {
            foreach (var i in _template)
            {
                i.Emit(ctx);
            }
        }

        private void WriteHeader(BinaryWriter s)
        {
            s.Write(Magic);
            s.Write(MajorVersion);
            s.Write(MinorVersion);
            var createdOn = DateTime.UtcNow;
            s.Write(createdOn.Ticks);
            s.Write(0L);
            s.Write(0L);
            s.Write(0L);
            s.Write(0L);
            s.Write(0L);
            s.Write(0L);
            s.Write(0L);
            s.Write(0L);
        }

        private void ReaderHeader(BinaryReader s)
        {
            var magic = s.ReadInt16();
            if (magic != Magic)
                throw new ApplicationException("Invalid file type.");
            var major = s.ReadInt32();
            if (major != MajorVersion)
                throw new ApplicationException(string.Format("Major Version: {0} not supported.", major));
            var minor = s.ReadInt32();
            if (minor != MinorVersion)
                throw new ApplicationException(string.Format("Minor Version: {0} not supported.", minor));
            var ticks = s.ReadInt64();
            s.ReadInt64(); //1
            s.ReadInt64(); //2
            s.ReadInt64(); //3
            s.ReadInt64(); //4
            s.ReadInt64(); //5
            s.ReadInt64(); //6
            s.ReadInt64(); //7
            s.ReadInt64(); //8
        }
    }
}