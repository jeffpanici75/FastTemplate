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

namespace PaniciSoftware.FastTemplate.Common
{
    public class StringType : PrimitiveType
    {
        public StringType()
        {
        }

        public StringType(string v)
        {
            Value = v;
        }

        public override Type UnderlyingType
        {
            get { return typeof (string); }
        }

        public override object RawValue
        {
            get { return Value; }
        }

        public string Value { get; set; }

        public static StringType Empty()
        {
            return new StringType {Value = string.Empty};
        }

        public static StringType New(string s)
        {
            return new StringType {Value = s};
        }

        public static explicit operator StringType(string s)
        {
            return new StringType(s);
        }

        public static explicit operator StringType(char s)
        {
            return new StringType {Value = "" + s};
        }

        public override object ApplyBinary(Operator op, ITemplateType rhs)
        {
            switch (op)
            {
                case Operator.Div:
                {
                    break;
                }
                case Operator.EQ:
                {
                    var stringType = rhs as StringType;
                    if (stringType != null)
                    {
                        return Value.CompareTo((stringType).Value) == 0;
                    }
                    if (rhs is NullType)
                    {
                        return Value == null;
                    }
                    break;
                }
                case Operator.GE:
                {
                    var stringType = rhs as StringType;
                    if (stringType != null)
                    {
                        return Value.CompareTo((stringType).Value) >= 0;
                    }
                    break;
                }
                case Operator.GT:
                {
                    var stringType = rhs as StringType;
                    if (stringType != null)
                    {
                        return Value.CompareTo((stringType).Value) > 0;
                    }
                    break;
                }
                case Operator.LE:
                {
                    var stringType = rhs as StringType;
                    if (stringType != null)
                    {
                        return Value.CompareTo((stringType).Value) <= 0;
                    }
                    break;
                }
                case Operator.LT:
                {
                    var stringType = rhs as StringType;
                    if (stringType != null)
                    {
                        return Value.CompareTo((stringType).Value) < 0;
                    }
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
                    var stringType = rhs as StringType;
                    if (stringType != null)
                    {
                        return Value.CompareTo((stringType).Value) != 0;
                    }
                    if (rhs is NullType)
                    {
                        return Value != null;
                    }
                    break;
                }
                case Operator.Plus:
                {
                    return string.Format(
                        "{0}{1}", Value, rhs.RawValue);
                }
            }
            throw new InvalidTypeException(
                op.ToString(), UnderlyingType, rhs.UnderlyingType);
        }

        public override bool TryConvert<T>(out T o)
        {
            var type = typeof (T);
            if (type == typeof (string))
            {
                o = (T) (object) Value;
                return true;
            }
            if (type == typeof (bool))
            {
                o = (T) (object) !string.IsNullOrEmpty(Value);
                return true;
            }
            o = default(T);
            return false;
        }

        public override object ApplyUnary(Operator op)
        {
            throw new NotImplementedException();
        }
    }
}