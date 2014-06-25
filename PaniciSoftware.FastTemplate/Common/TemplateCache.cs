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
using System.Collections.Concurrent;
using System.IO;

namespace PaniciSoftware.FastTemplate.Common
{
    public class TemplateCache
    {
        private sealed class TemplateCacheEntry
        {
            public string Hash { get; set; }

            public IVirtualTemplate Instance { get; set; }
        }

        private readonly ConcurrentDictionary<string, TemplateCacheEntry> _templates = new ConcurrentDictionary<string, TemplateCacheEntry>();

        public TemplateCache()
        {
            ResourceResolver = name =>
            {
                var result = new ResourceResolverResult {SourceUrl = name};
                if (!File.Exists(name))
                {
                    result.Errors.Add(Error.NewError(
                        "R404",
                        string.Format("The resource named '{0}' was not found in the current directory.", name),
                        "Ensure that the resource you're attempting to load is accessible in the same directory as all other resources."));
                }
                else
                {
                    result.Stream = File.Open(name, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                return result;
            };
        }

        public Func<string, ResourceResolverResult> ResourceResolver { get; set; }

        public void Clear()
        {
            _templates.Clear();
        }

        public TemplateCacheResult GetTemplate(
            string name,
            Func<string, ResourceResolverResult> resolver = null)
        {
            var result = new TemplateCacheResult();

            TemplateCacheEntry cacheEntry;
            _templates.TryGetValue(name, out cacheEntry);

            var resolverResult = resolver != null ? resolver(name) : ResourceResolver(name);
            if (!resolverResult.Success)
            {
                result.Errors.AddRange(resolverResult.Errors);
                return result;
            }

            using (var stream = resolverResult.Stream)
            {
                var value = new StreamReader(stream).ReadToEnd();
                var hash = value.ToAsciiSha1Hash();
                if (cacheEntry != null && cacheEntry.Hash == hash)
                {
                    result.FromCache = true;
                    result.Template = cacheEntry.Instance;
                    return result;
                }

                var compilerResult = Template.Compile(name, value);
                if (!compilerResult.Success)
                    result.Errors.AddRange(compilerResult.Errors);
                else
                {
                    compilerResult.Template.Cache = this;
                    compilerResult.Template.SourceUrl = resolverResult.SourceUrl;
                    result.Template = compilerResult.Template;
                    _templates[name] = new TemplateCacheEntry
                    {
                        Hash = hash,
                        Instance = compilerResult.Template
                    };
                }
            }

            return result;
        }
    }
}