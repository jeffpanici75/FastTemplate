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
using System.Text;

namespace PaniciSoftware.FastTemplate.Common
{
    public enum Operator
    {
        Plus = 1,
        Minus,
        GT,
        LT,
        GE,
        LE,
        Div,
        Mod,
        Mul,
        Not,
        EQ,
        NEQ,
        Unknown
    }

    public abstract class PrimitiveType : ITemplateType
    {
        public abstract bool TryConvert<T>(out T o);

        public abstract object ApplyBinary(
            Operator op,
            ITemplateType rhs);

        public abstract object ApplyUnary(Operator op);

        public abstract Type UnderlyingType { get; }

        public abstract object RawValue { get; }

        public virtual bool CanInvoke()
        {
            return false;
        }

        public virtual bool WriteTo(
            StringBuilder builder,
            TrimMode mode = TrimMode.FollowingNewLine)
        {
            string value;
            if (!TryConvert(out value)) return false;
            switch (mode)
            {
                case TrimMode.FollowingNewLine:
                {
                    builder.Append(value);
                    break;
                }
                case TrimMode.Greedy:
                {
                    builder.Append(value.GreedyTrim());
                    break;
                }
                default:
                {
                    builder.Append(value);
                    break;
                }
            }
            return true;
        }

        public static ITemplateType Create(object o)
        {
            var typedInstance = o as ITemplateType;
            if (typedInstance != null) return typedInstance;

            if (o == null)
            {
                return new NullType();
            }

            if (o is long)
            {
                return new LongType {Value = (Int64) o};
            }
            var @delegate = o as Delegate;
            if (@delegate != null)
            {
                return new DelegateType {Value = @delegate};
            }
            if (o is ulong)
            {
                var i = (ulong) o;
                return new UIntType {Value = i};
            }
            if (o is int)
            {
                var i = (int) o;
                return new IntType {Value = i};
            }
            if (o is short)
            {
                var i = (short) o;
                return new IntType {Value = i};
            }
            if (o is uint)
            {
                Int64 i = (uint) o;
                return new LongType {Value = i};
            }
            if (o is ushort)
            {
                var i = (ushort) o;
                return new IntType {Value = i};
            }
            if (o is byte)
            {
                var i = (byte) o;
                return new IntType {Value = i};
            }
            if (o is sbyte)
            {
                var i = (sbyte) o;
                return new IntType {Value = i};
            }
            if (o is float)
            {
                double d = (float) o;
                return new DoubleType {Value = d};
            }
            if (o is decimal)
            {
                var d = (decimal) o;
                return new DecimalType {Value = d};
            }
            if (o is double)
            {
                return new DoubleType {Value = (double) o};
            }
            var value = o as string;
            if (value != null)
            {
                return new StringType {Value = value};
            }
            if (o is char)
            {
                return new StringType {Value = "" + (char) o};
            }
            if (o is bool)
            {
                return new BooleanType {Value = (bool) o};
            }
            var dictionary = o as Dictionary<string, object>;
            if (dictionary != null)
            {
                return new DictType {Value = dictionary};
            }
            var objects = o as List<object>;
            if (objects != null)
            {
                return new ListType {Value = objects};
            }
            return new ObjectType {Value = o};
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PrimitiveType))
                return false;
            return RawValue.Equals(((PrimitiveType) obj).RawValue);
        }

        public override int GetHashCode()
        {
            return RawValue.GetHashCode();
        }

        public override string ToString()
        {
            return RawValue == null ? string.Empty : RawValue.ToString();
        }
    }
}