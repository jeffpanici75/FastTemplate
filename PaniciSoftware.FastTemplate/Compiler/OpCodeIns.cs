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
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace PaniciSoftware.FastTemplate.Compiler
{
    internal class OpCodeIns : Instruction
    {
        private static Dictionary<string, OpCode> _nameToOpCode;

        public OpCode OpCode { get; set; }

        public override byte Type
        {
            get { return 9; }
        }

        public override void Emit(AssemblyContext c)
        {
            c.Generator.Emit(OpCode);
        }

        public override void ReadFrom(BinaryReader s)
        {
            if (_nameToOpCode == null)
            {
                _nameToOpCode = InitOpCodeMap();
            }
            var str = ReadString(s);
            OpCode = _nameToOpCode[str];
        }

        public override void WriteTo(BinaryWriter s)
        {
            var name = OpCode.Name;
            WriteString(name, s);
        }

        private Dictionary<string, OpCode> InitOpCodeMap()
        {
            var d = new Dictionary<string, OpCode>();
            var fields = typeof (OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var f in fields)
            {
                var op = (OpCode) f.GetValue(null);
                d[op.Name] = op;
            }
            return d;
        }
    }
}