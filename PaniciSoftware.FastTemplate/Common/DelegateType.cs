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
    public class DelegateType : PrimitiveType
    {
        public override Type UnderlyingType
        {
            get { return Value.GetType(); }
        }

        public override object RawValue
        {
            get { return Value; }
        }

        public Delegate Value { get; set; }

        public static explicit operator DelegateType(Delegate d)
        {
            return new DelegateType {Value = d};
        }

        public override bool TryConvert<T>(out T o)
        {
            throw new NotImplementedException();
        }

        public override object ApplyBinary(Operator op, ITemplateType rhs)
        {
            switch (op)
            {
                case Operator.EQ:
                {
                    var delegateType = rhs as DelegateType;
                    if (delegateType != null)
                    {
                        return Value == (delegateType).Value;
                    }
                    if (rhs is NullType)
                    {
                        return Value == null;
                    }
                    break;
                }
                case Operator.NEQ:
                {
                    var delegateType = rhs as DelegateType;
                    if (delegateType != null)
                    {
                        return Value != (delegateType).Value;
                    }
                    if (rhs is NullType)
                    {
                        return Value != null;
                    }
                    break;
                }
            }
            throw new InvalidTypeException(op.ToString(), UnderlyingType, rhs.UnderlyingType);
        }

        public override object ApplyUnary(Operator op)
        {
            throw new NotImplementedException();
        }

        public override bool CanInvoke()
        {
            return true;
        }
    }
}