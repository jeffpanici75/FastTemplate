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

using System.IO;
using System.Text;

namespace PaniciSoftware.FastTemplate.Common
{
    public static class StringExtensions
    {
        public static string GreedyTrim(this string str)
        {
            var buffer = new StringBuilder();
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            var skipping = false;
            var lastWs = char.MaxValue;
            foreach (var c in str)
            {
                if (IsWhiteSpace(c))
                {
                    lastWs = c;
                    if (skipping)
                    {
                        continue;
                    }
                    skipping = true;
                }
                else
                {
                    if (skipping)
                    {
                        buffer.Append(lastWs);
                        skipping = false;
                    }
                    buffer.Append(c);
                }
            }
            if (skipping)
            {
                buffer.Append(lastWs);
            }
            return buffer.ToString();
        }

        private static bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t' || c == '\r' || c == '\n'
                || c == '\u000C';
        }

        public static string ToAsciiSha1Hash(
                        this string value,
                        Encoding encoding = null)
        {
            if (value == null) return "0000000000000000000000000000000000000000";
            if (encoding == null)
            {
                encoding = Encoding.ASCII;
            }
            return encoding.GetBytes(value).ToAsciiSha1Hash();
        }

        public static Stream ToStream(this string value, Encoding encoding = null)
        {
            if (value == null) return null;
            if (encoding == null)
            {
                encoding = Encoding.ASCII;
            }
            return new MemoryStream(encoding.GetBytes(value));
        }
    }
}