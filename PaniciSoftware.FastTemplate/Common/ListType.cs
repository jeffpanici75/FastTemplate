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

namespace PaniciSoftware.FastTemplate.Common
{
    public class ListType : PrimitiveType
    {
        public ListType(List<object> l)
        {
            Value = l;
        }

        public ListType()
        {
        }

        public override Type UnderlyingType
        {
            get { return typeof (List<object>); }
        }

        public override object RawValue
        {
            get { return Value; }
        }

        public List<object> Value { get; set; }

        public static explicit operator ListType(List<object> l)
        {
            return new ListType(l);
        }

        public override object ApplyBinary(Operator op, ITemplateType rhs)
        {
            switch (op)
            {
                case Operator.EQ:
                {
                    var listType = rhs as ListType;
                    if (listType != null)
                    {
                        return Value == (listType).Value;
                    }
                    if (rhs is NullType)
                    {
                        return Value == null;
                    }
                    break;
                }
                case Operator.NEQ:
                {
                    var listType = rhs as ListType;
                    if (listType != null)
                    {
                        return Value != (listType).Value;
                    }
                    if (rhs is NullType)
                    {
                        return Value != null;
                    }
                    break;
                }
            }
            throw new InvalidTypeException(
                op.ToString(), UnderlyingType, rhs.UnderlyingType);
        }

        public override object ApplyUnary(Operator op)
        {
            throw new InvalidTypeException(
                op.ToString(), UnderlyingType);
        }

        public override bool TryConvert<T>(out T o)
        {
            o = default(T);
            return false;
        }
    }
}