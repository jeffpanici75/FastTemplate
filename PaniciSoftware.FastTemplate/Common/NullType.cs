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
    public class NullType : PrimitiveType
    {
        public override Type UnderlyingType
        {
            get { return typeof (void); }
        }

        public override object RawValue
        {
            get { return null; }
        }

        public object Value { get; set; }

        public static NullType New()
        {
            return new NullType();
        }

        public override object ApplyBinary(Operator op, ITemplateType rhs)
        {
            switch (op)
            {
                case Operator.EQ:
                {
                    return rhs is NullType;
                }
                case Operator.NEQ:
                {
                    return !(rhs is NullType);
                }
            }
            throw new NullReferenceException(
                string.Format(
                    "Attempted to apply: {0} to a null reference.", op));
        }

        public override object ApplyUnary(Operator op)
        {
            throw new NullReferenceException(
                string.Format(
                    "Attempted to apply: {0} to a null reference.", op));
        }

        public override bool TryConvert<T>(out T o)
        {
            var type = typeof (T);
            if (type == typeof (string))
            {
                o = (T) (object) string.Empty;
                return true;
            }
            if (type == typeof (bool))
            {
                o = (T) (object) false;
                return true;
            }
            o = default(T);
            return false;
        }
    }
}