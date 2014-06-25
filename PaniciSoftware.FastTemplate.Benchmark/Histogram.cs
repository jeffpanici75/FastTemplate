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

namespace PaniciSoftware.FastTemplate.Benchmark
{
    public class Histogram
    {
        private const int BucketCount = 154;

        private readonly double[] _bucketLimits =
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 14, 16, 18, 20, 25, 30, 35, 40, 45,
            50, 60, 70, 80, 90, 100, 120, 140, 160, 180, 200, 250, 300, 350, 400, 450,
            500, 600, 700, 800, 900, 1000, 1200, 1400, 1600, 1800, 2000, 2500, 3000,
            3500, 4000, 4500, 5000, 6000, 7000, 8000, 9000, 10000, 12000, 14000,
            16000, 18000, 20000, 25000, 30000, 35000, 40000, 45000, 50000, 60000,
            70000, 80000, 90000, 100000, 120000, 140000, 160000, 180000, 200000,
            250000, 300000, 350000, 400000, 450000, 500000, 600000, 700000, 800000,
            900000, 1000000, 1200000, 1400000, 1600000, 1800000, 2000000, 2500000,
            3000000, 3500000, 4000000, 4500000, 5000000, 6000000, 7000000, 8000000,
            9000000, 10000000, 12000000, 14000000, 16000000, 18000000, 20000000,
            25000000, 30000000, 35000000, 40000000, 45000000, 50000000, 60000000,
            70000000, 80000000, 90000000, 100000000, 120000000, 140000000, 160000000,
            180000000, 200000000, 250000000, 300000000, 350000000, 400000000,
            450000000, 500000000, 600000000, 700000000, 800000000, 900000000,
            1000000000, 1200000000, 1400000000, 1600000000, 1800000000, 2000000000,
            2500000000.0, 3000000000.0, 3500000000.0, 4000000000.0, 4500000000.0,
            5000000000.0, 6000000000.0, 7000000000.0, 8000000000.0, 9000000000.0, 1e200,
        };

        private readonly double[] _buckets = new double[154];
        private double _sumsquares;
        private double _max;
        private double _min;
        private double _num;
        private double _sum;

        public Histogram()
        {
            _min = double.MaxValue;
            _max = 0;
            _num = 0;
            _sum = 0;
            _sumsquares = 0;
            for (var i = 0; i < BucketCount; i++)
            {
                _buckets[i] = 0;
            }
        }

        public void Add(double value)
        {
            var b = 0;
            while (b < BucketCount - 1 && _bucketLimits[b] <= value)
            {
                b++;
            }

            _buckets[b] += 1.0;

            if (_min > value)
                _min = value;

            if (_max < value)
                _max = value;

            _num++;
            _sum += value;
            _sumsquares += value*value;
        }

        public double Median()
        {
            return Percentile(50.0);
        }

        public double Percentile(double p)
        {
            var threshold = _num*(p/100.0);
            double sum = 0;
            for (var b = 0; b < BucketCount; b++)
            {
                sum += _buckets[b];
                if (sum >= threshold)
                {
                    // Scale linearly within this bucket
                    var leftPoint = (b == 0) ? 0 : _bucketLimits[b - 1];
                    var rightPoint = _bucketLimits[b];
                    var leftSum = sum - _buckets[b];
                    var rightSum = sum;
                    var pos = (threshold - leftSum)/(rightSum - leftSum);
                    var r = leftPoint + (rightPoint - leftPoint)*pos;
                    if (r < _min) r = _min;
                    if (r > _max) r = _max;
                    return r;
                }
            }
            return _max;
        }

        private double Average()
        {
            if (Math.Abs(_num - 0.0) < Double.Epsilon) return 0;
            return _sum/_num;
        }

        public double StandardDeviation()
        {
            if (Math.Abs(_num - 0.0) < Double.Epsilon) return 0;
            var variance = (_sumsquares*_num - _sum*_sum)/(_num*_num);
            return Math.Sqrt(variance);
        }

        public override string ToString()
        {
            var r = new StringBuilder();
            r.AppendFormat("Count: {0} Average: {1} StdDev: {2}\n", _num, Average(), StandardDeviation());
            r.AppendFormat("Min: {0} Median: {1} Max: {2}\n", _num == 0.0 ? 0.0 : _min, Median(), _max);
            r.Append("------------------------------------------------------\n");
            var mult = 100.0/_num;
            double sum = 0;
            for (var b = 0; b < BucketCount; b++)
            {
                if (_buckets[b] <= 0.0) continue;
                sum += _buckets[b];
                r.AppendFormat(
                    "[ {0,7}, {1,7} ) {2,7} {3,7} % {4,7} ",
                    ((b == 0) ? 0.0 : _bucketLimits[b - 1]),
                    // left
                    _bucketLimits[b],
                    // right
                    _buckets[b],
                    // count
                    mult*_buckets[b],
                    // percentage
                    mult*sum); // cumulative percentage
                // Add hash marks based on percentage; 20 marks for 100%.
                var marks = (int) (20*(_buckets[b]/_num) + 0.5);
                r.Append('#', marks);
                r.Append('\n');
            }
            return r.ToString();
        }
    }
}