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
using System.IO;
using System.Text;
using PaniciSoftware.FastTemplate.Common;
using PaniciSoftware.FastTemplate.Compiler;

namespace PaniciSoftware.FastTemplate
{
    public class Template : IDisposable
    {
        private VirtualTemplate _template;
        private Stream _text;

        public Template(string s)
        {
            _text = new MemoryStream(Encoding.UTF8.GetBytes(s));
        }

        public Template(Stream s)
        {
            _text = s;
        }

        public Template(Assembly a)
        {
            _text = null;
            _template = new VirtualTemplate("template");
            _template.Compile(a);
        }

        ~Template()
        {
            Dispose();
        }

        public static CompilerResult Compile(
            string name,
            string text)
        {
            var result = new CompilerResult();
            var v = new VirtualTemplate(name);
            result.Errors.AddRange(v.Compile(text));
            result.Template = v;
            return result;
        }

        public static CompilerResult Compile(
            string name,
            Stream stream)
        {
            var result = new CompilerResult();
            var v = new VirtualTemplate(name);
            result.Errors.AddRange(v.Compile(stream));
            result.Template = v;
            return result;
        }

        public static CompilerResult Compile(
            string name,
            string text,
            OptimizeLevel l)
        {
            var result = new CompilerResult();
            var v = new VirtualTemplate(name, l);
            result.Errors.AddRange(v.Compile(text));
            result.Template = v;
            return result;
        }

        public static CompilerResult Compile(
            string name,
            Stream stream,
            OptimizeLevel l)
        {
            var result = new CompilerResult();
            var v = new VirtualTemplate(name, l);
            result.Errors.AddRange(v.Compile(stream));
            result.Template = v;
            return result;
        }

        public static TemplateExecutionResult CompileAndRun(
            string name,
            string text,
            TemplateDictionary d)
        {
            var compilerResult = Compile(name, text);
            if (compilerResult.Errors.ContainsError())
            {
                var result = new TemplateExecutionResult();
                result.Errors.AddRange(compilerResult.Errors);
                return result;
            }
            return compilerResult.Template.Execute(d);
        }

        public static TemplateExecutionResult CompileAndRun(
            string name,
            Stream s,
            TemplateDictionary d)
        {
            var compilerResult = Compile(name, s);
            if (compilerResult.Errors.ContainsError())
            {
                var result = new TemplateExecutionResult();
                result.Errors.AddRange(compilerResult.Errors);
                return result;
            }
            return compilerResult.Template.Execute(d);
        }

        public static TemplateExecutionResult ImmediateApply(
            TemplateCache cache,
            TemplateDictionary state,
            string template)
        {
            var interpreter = new Interpreter.Interpreter(template)
            {
                Cache = cache
            };
            return interpreter.Apply(state);
        }

        public static TemplateExecutionResult ImmediateApply(
            TemplateDictionary state,
            string template)
        {
            var interpreter = new Interpreter.Interpreter(template)
            {
                Cache = new TemplateCache()
            };
            return interpreter.Apply(state);
        }

        public TemplateExecutionResult Execute(TemplateDictionary state)
        {
            if (_template == null)
            {
                var i = new Interpreter.Interpreter(_text);
                return i.Apply(state);
            }
            return _template.Execute(state);
        }

        public ErrorList Compile()
        {
            if (_template != null)
                return new ErrorList();
            _template = new VirtualTemplate("template");
            return _template.Compile(_text);
        }

        public void Dispose()
        {
            if (_text == null) return;

            _text.Dispose();
            _text = null;
            GC.SuppressFinalize(this);
        }
    }
}