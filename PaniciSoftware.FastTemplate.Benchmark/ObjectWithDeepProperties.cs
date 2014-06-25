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

namespace PaniciSoftware.FastTemplate.Benchmark
{
    public class ObjectWithDeepProperties
    {
        public ObjectWithDeepProperties()
        {
            Field1 = new FieldObjectOne(10);
            Field2 = new FieldObjectTwo(10);
            Field3 = new FieldObjectThree(10);
            Field4 = new FieldObjectFour(10);
            IntField = 2;
            StringField = "empty string";
            DoubleField = 1.0;
            DecimalField = 2.0M;
            BoolField = false;
        }

        public FieldObjectOne Field1 { get; set; }
        public FieldObjectTwo Field2 { get; set; }
        public FieldObjectThree Field3 { get; set; }
        public FieldObjectFour Field4 { get; set; }

        public int IntField { get; set; }
        public string StringField { get; set; }
        public double DoubleField { get; set; }
        public decimal DecimalField { get; set; }
        public bool BoolField { get; set; }
    }

    public class FieldObjectOne
    {
        public FieldObjectOne(int count)
        {
            if (count > 0)
            {
                var n = count - 1;
                Field2 = new FieldObjectTwo(n);
                Field3 = new FieldObjectThree(n);
                Field4 = new FieldObjectFour(n);
            }
            IntField = 2;
            StringField = "empty string";
            DoubleField = 1.0;
            DecimalField = 2.0M;
            BoolField = false;
        }

        public FieldObjectTwo Field2 { get; set; }
        public FieldObjectThree Field3 { get; set; }
        public FieldObjectFour Field4 { get; set; }

        public int IntField { get; set; }
        public string StringField { get; set; }
        public double DoubleField { get; set; }
        public decimal DecimalField { get; set; }
        public bool BoolField { get; set; }
    }

    public class FieldObjectTwo
    {
        public FieldObjectTwo(int count)
        {
            if (count > 0)
            {
                var n = count - 1;
                Field1 = new FieldObjectOne(n);
                Field3 = new FieldObjectThree(n);
                Field4 = new FieldObjectFour(n);
            }
            IntField = 2;
            StringField = "empty string";
            DoubleField = 1.0;
            DecimalField = 2.0M;
            BoolField = false;
        }

        public FieldObjectOne Field1 { get; set; }
        public FieldObjectThree Field3 { get; set; }
        public FieldObjectFour Field4 { get; set; }

        public int IntField { get; set; }
        public string StringField { get; set; }
        public double DoubleField { get; set; }
        public decimal DecimalField { get; set; }
        public bool BoolField { get; set; }
    }

    public class FieldObjectThree
    {
        public FieldObjectThree(int count)
        {
            if (count > 0)
            {
                var n = count - 1;
                Field1 = new FieldObjectOne(n);
                Field2 = new FieldObjectTwo(n);
                Field4 = new FieldObjectFour(n);
            }
            IntField = 2;
            StringField = "empty string";
            DoubleField = 1.0;
            DecimalField = 2.0M;
            BoolField = false;
        }

        public FieldObjectOne Field1 { get; set; }
        public FieldObjectTwo Field2 { get; set; }
        public FieldObjectFour Field4 { get; set; }

        public int IntField { get; set; }
        public string StringField { get; set; }
        public double DoubleField { get; set; }
        public decimal DecimalField { get; set; }
        public bool BoolField { get; set; }
    }

    public class FieldObjectFour
    {
        public FieldObjectFour(int count)
        {
            if (count > 1)
            {
                Field1 = new FieldObjectOne(count - 1);
                Field2 = new FieldObjectTwo(count - 1);
                Field3 = new FieldObjectThree(count - 1);
            }
            IntField = 2;
            StringField = "empty string";
            DoubleField = 1.0;
            DecimalField = 2.0M;
            BoolField = false;
        }

        public FieldObjectOne Field1 { get; set; }
        public FieldObjectTwo Field2 { get; set; }
        public FieldObjectThree Field3 { get; set; }

        public int IntField { get; set; }
        public string StringField { get; set; }
        public double DoubleField { get; set; }
        public decimal DecimalField { get; set; }
        public bool BoolField { get; set; }
    }
}