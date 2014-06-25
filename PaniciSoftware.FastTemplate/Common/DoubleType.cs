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
using System.Globalization;

namespace PaniciSoftware.FastTemplate.Common
{
    public class DoubleType : PrimitiveType
    {
        public DoubleType()
        {
        }

        public DoubleType(double r)
        {
            Value = r;
        }

        public override Type UnderlyingType
        {
            get { return typeof (double); }
        }

        public override object RawValue
        {
            get { return Value; }
        }

        public double Value { get; set; }

        public static explicit operator DoubleType(double d)
        {
            return new DoubleType(d);
        }

        public static explicit operator DoubleType(float d)
        {
            return new DoubleType(d);
        }

        public override object ApplyBinary(Operator op, ITemplateType rhs)
        {
            switch (op)
            {
                case Operator.Div:
                {
                    var intType = rhs as IntType;
                    if (intType != null)
                    {
                        return Value/(intType).Value;
                    }
                    var longType = rhs as LongType;
                    if (longType != null)
                    {
                        return Value/(longType).Value;
                    }
                    var doubleType = rhs as DoubleType;
                    if (doubleType != null)
                    {
                        return Value/(doubleType).Value;
                    }
                    var decimalType = rhs as DecimalType;
                    if (decimalType != null)
                    {
                        return (decimal) Value/(decimalType).Value;
                    }
                    return null;
                }
                case Operator.EQ:
                {
                    var intType = rhs as IntType;
                    if (intType != null)
                    {
                        return Value == (intType).Value;
                    }
                    var longType = rhs as LongType;
                    if (longType != null)
                    {
                        return Value == (longType).Value;
                    }
                    var doubleType = rhs as DoubleType;
                    if (doubleType != null)
                    {
                        return Value == (doubleType).Value;
                    }
                    var decimalType = rhs as DecimalType;
                    if (decimalType != null)
                    {
                        return (decimal) Value == (decimalType).Value;
                    }
                    if (rhs is NullType)
                    {
                        return Value == default(double);
                    }
                    break;
                }
                case Operator.GE:
                {
                    var intType = rhs as IntType;
                    if (intType != null)
                    {
                        return Value >= (intType).Value;
                    }
                    var longType = rhs as LongType;
                    if (longType != null)
                    {
                        return Value >= (longType).Value;
                    }
                    var doubleType = rhs as DoubleType;
                    if (doubleType != null)
                    {
                        return Value >= (doubleType).Value;
                    }
                    var decimalType = rhs as DecimalType;
                    if (decimalType != null)
                    {
                        return (decimal) Value >= (decimalType).Value;
                    }
                    break;
                }
                case Operator.GT:
                {
                    var intType = rhs as IntType;
                    if (intType != null)
                    {
                        return Value > (intType).Value;
                    }
                    var longType = rhs as LongType;
                    if (longType != null)
                    {
                        return Value > (longType).Value;
                    }
                    var doubleType = rhs as DoubleType;
                    if (doubleType != null)
                    {
                        return Value > (doubleType).Value;
                    }
                    var decimalType = rhs as DecimalType;
                    if (decimalType != null)
                    {
                        return (decimal) Value > (decimalType).Value;
                    }
                    break;
                }
                case Operator.LE:
                {
                    var intType = rhs as IntType;
                    if (intType != null)
                    {
                        return Value <= (intType).Value;
                    }
                    var longType = rhs as LongType;
                    if (longType != null)
                    {
                        return Value <= (longType).Value;
                    }
                    var doubleType = rhs as DoubleType;
                    if (doubleType != null)
                    {
                        return Value <= (doubleType).Value;
                    }
                    var decimalType = rhs as DecimalType;
                    if (decimalType != null)
                    {
                        return (decimal) Value <= (decimalType).Value;
                    }
                    break;
                }
                case Operator.LT:
                {
                    var intType = rhs as IntType;
                    if (intType != null)
                    {
                        return Value < (intType).Value;
                    }
                    var longType = rhs as LongType;
                    if (longType != null)
                    {
                        return Value < (longType).Value;
                    }
                    var doubleType = rhs as DoubleType;
                    if (doubleType != null)
                    {
                        return Value < (doubleType).Value;
                    }
                    var decimalType = rhs as DecimalType;
                    if (decimalType != null)
                    {
                        return (decimal) Value < (decimalType).Value;
                    }
                    break;
                }
                case Operator.Minus:
                {
                    var intType = rhs as IntType;
                    if (intType != null)
                    {
                        return Value - (intType).Value;
                    }
                    var longType = rhs as LongType;
                    if (longType != null)
                    {
                        return Value - (longType).Value;
                    }
                    var doubleType = rhs as DoubleType;
                    if (doubleType != null)
                    {
                        return Value - (doubleType).Value;
                    }
                    var decimalType = rhs as DecimalType;
                    if (decimalType != null)
                    {
                        return (decimal) Value - (decimalType).Value;
                    }
                    break;
                }
                case Operator.Mod:
                {
                    var intType = rhs as IntType;
                    if (intType != null)
                    {
                        return Value%(intType).Value;
                    }
                    var longType = rhs as LongType;
                    if (longType != null)
                    {
                        return Value%(longType).Value;
                    }
                    var doubleType = rhs as DoubleType;
                    if (doubleType != null)
                    {
                        return Value%(doubleType).Value;
                    }
                    var decimalType = rhs as DecimalType;
                    if (decimalType != null)
                    {
                        return (decimal) Value%(decimalType).Value;
                    }
                    break;
                }
                case Operator.Mul:
                {
                    var intType = rhs as IntType;
                    if (intType != null)
                    {
                        return Value*(intType).Value;
                    }
                    var longType = rhs as LongType;
                    if (longType != null)
                    {
                        return Value*(longType).Value;
                    }
                    var doubleType = rhs as DoubleType;
                    if (doubleType != null)
                    {
                        return Value*(doubleType).Value;
                    }
                    var decimalType = rhs as DecimalType;
                    if (decimalType != null)
                    {
                        return (decimal) Value*(decimalType).Value;
                    }
                    break;
                }
                case Operator.NEQ:
                {
                    var intType = rhs as IntType;
                    if (intType != null)
                    {
                        return Value != (intType).Value;
                    }
                    var longType = rhs as LongType;
                    if (longType != null)
                    {
                        return Value != (longType).Value;
                    }
                    var doubleType = rhs as DoubleType;
                    if (doubleType != null)
                    {
                        return Value != (doubleType).Value;
                    }
                    var decimalType = rhs as DecimalType;
                    if (decimalType != null)
                    {
                        return (decimal) Value != (decimalType).Value;
                    }
                    if (rhs is NullType)
                    {
                        return Value != default(double);
                    }
                    break;
                }
                case Operator.Plus:
                {
                    var intType = rhs as IntType;
                    if (intType != null)
                    {
                        return Value + (intType).Value;
                    }
                    var longType = rhs as LongType;
                    if (longType != null)
                    {
                        return Value + (longType).Value;
                    }
                    var doubleType = rhs as DoubleType;
                    if (doubleType != null)
                    {
                        return Value + (doubleType).Value;
                    }
                    var stringType = rhs as StringType;
                    if (stringType != null)
                    {
                        return string.Format(
                            "{0}{1}", Value, (stringType).Value);
                    }
                    var decimalType = rhs as DecimalType;
                    if (decimalType != null)
                    {
                        return (decimal) Value + (decimalType).Value;
                    }
                    break;
                }
            }
            throw new InvalidTypeException(
                op.ToString(), UnderlyingType, rhs.UnderlyingType);
        }

        public override bool TryConvert<T>(out T o)
        {
            try
            {
                var t = typeof (T);
                if (t == typeof (UInt64))
                {
                    o = (T) (object) Convert.ToUInt64(Value);
                    return true;
                }
                if (t == typeof (decimal))
                {
                    o = (T) (object) Convert.ToDecimal(Value);
                    return true;
                }
                if (t == typeof (double))
                {
                    o = (T) (object) Value;
                    return true;
                }
                if (t == typeof (string))
                {
                    o = (T) (object) Value.ToString(CultureInfo.InvariantCulture);
                    return true;
                }
            }
            catch (Exception)
            {
                o = default(T);
                return false;
            }
            o = default(T);
            return false;
        }

        public override object ApplyUnary(Operator op)
        {
            switch (op)
            {
                case Operator.Plus:
                {
                    return +Value;
                }
                case Operator.Minus:
                {
                    return -Value;
                }
            }
            throw new InvalidTypeException(
                op.ToString(), UnderlyingType);
        }
    }
}