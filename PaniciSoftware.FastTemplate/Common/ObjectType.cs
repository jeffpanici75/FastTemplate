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
using System.Text;

namespace PaniciSoftware.FastTemplate.Common
{
    public class ObjectType : PrimitiveType
    {
        public override Type UnderlyingType
        {
            get { return Value.GetType(); }
        }

        public override object RawValue
        {
            get { return Value; }
        }

        public object Value { get; set; }

        public override object ApplyBinary(
            Operator op,
            ITemplateType rhs)
        {
            switch (op)
            {
                case Operator.Div:
                {
                    break;
                }
                case Operator.EQ:
                {
                    var comparable = Value as IComparable;
                    if (comparable != null) return (comparable).CompareTo(rhs) == 0;
                    if (rhs is NullType)
                        return Value == null;
                    break;
                }
                case Operator.GE:
                {
                    var comparable = Value as IComparable;
                    if (comparable != null) return (comparable).CompareTo(rhs) >= 0;
                    break;
                }
                case Operator.GT:
                {
                    var comparable = Value as IComparable;
                    if (comparable != null) return (comparable).CompareTo(rhs) > 0;
                    break;
                }
                case Operator.LE:
                {
                    var comparable = Value as IComparable;
                    if (comparable != null) return (comparable).CompareTo(rhs) <= 0;
                    break;
                }
                case Operator.LT:
                {
                    var comparable = Value as IComparable;
                    if (comparable != null) return (comparable).CompareTo(rhs) < 0;
                    break;
                }
                case Operator.Minus:
                {
                    break;
                }
                case Operator.Mod:
                {
                    break;
                }
                case Operator.Mul:
                {
                    break;
                }
                case Operator.NEQ:
                {
                    var comparable = Value as IComparable;
                    if (comparable != null) return (comparable).CompareTo(rhs) != 0;
                    if (rhs is NullType)
                        return Value != null;
                    break;
                }
                case Operator.Plus:
                {
                    break;
                }
            }
            throw new InvalidTypeException(op.ToString(), UnderlyingType, rhs.UnderlyingType);
        }

        public override object ApplyUnary(Operator op)
        {
            throw new InvalidTypeException(op.ToString(), UnderlyingType);
        }

        public override bool TryConvert<T>(out T o)
        {
            var t = typeof (T);
            if (t == typeof (string))
            {
                o = (T) ((object) Value.ToString());
                return true;
            }
            o = default(T);
            return false;
        }

        public override bool WriteTo(
            StringBuilder builder,
            TrimMode mode = TrimMode.FollowingNewLine)
        {
            var templateWriter = RawValue as ITemplateWriter;
            if (templateWriter != null)
            {
                templateWriter.WriteTo(builder);
                return true;
            }
            return base.WriteTo(builder, mode);
        }
    }
}