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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PaniciSoftware.FastTemplate.Common
{
    public class TemplateDictionary : IEnumerable<KeyValuePair<string, object>>
    {
        private readonly Dictionary<string, ITemplateType> _dict = new Dictionary<string, ITemplateType>();

        public object this[string key]
        {
            get { return _dict[key].RawValue; }
            set { _dict[key] = PrimitiveType.Create(value); }
        }

        public ICollection<ITemplateType> Values
        {
            get { return _dict.Values; }
        }

        public ICollection<string> Keys
        {
            get { return _dict.Keys; }
        }

        public int Count
        {
            get { return _dict.Count; }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            var items = _dict.Select(kv => new KeyValuePair<string, object>(kv.Key, kv.Value.RawValue)).ToList();
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            _dict.Clear();
        }

        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key);
        }

        public void Add(
            string key,
            object value)
        {
            _dict.Add(key, PrimitiveType.Create(value));
        }

        public void AddType(
            string key,
            ITemplateType t)
        {
            _dict.Add(key, t);
        }

        public bool Remove(string key)
        {
            return _dict.Remove(key);
        }

        public bool TryGetRawValue(
            string key,
            out object value)
        {
            ITemplateType t;
            if (_dict.TryGetValue(key, out t))
            {
                value = t.RawValue;
                return true;
            }
            value = null;
            return false;
        }

        public bool TryGetValue(
            string key,
            out ITemplateType value)
        {
            return _dict.TryGetValue(key, out value);
        }

        public void SetValue(
            string key,
            ITemplateType value)
        {
            _dict[key] = value;
        }
    }
}