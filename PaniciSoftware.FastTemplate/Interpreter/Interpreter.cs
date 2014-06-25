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
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Interpreter
{
    public class Interpreter
    {
        private sealed class DocumentResult
        {
            public readonly ErrorList Errors = new ErrorList();
            public readonly StringBuilder Buffer = new StringBuilder();
        }

        private readonly ErrorList _parserErrors = new ErrorList();
        private readonly CommonTree _ast;

        public Interpreter(
            Stream stream,
            TemplateCache cache = null)
        {
            Cache = cache ?? new TemplateCache();
            var charStream = new ANTLRInputStream(stream);
            var lexer = TemplateLexer.Create(charStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new TemplateParser(tokenStream)
            {
                TraceDestination = Console.Out,
                Errors = _parserErrors
            };
            var docResult = parser.document();
            _ast = docResult.Tree;
        }

        public Interpreter(
            string text,
            TemplateCache cache = null)
        {
            Cache = cache ?? new TemplateCache();
            var stream = new ANTLRStringStream(text);
            var lexer = TemplateLexer.Create(stream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new TemplateParser(tokenStream)
            {
                TraceDestination = Console.Out,
                Errors = _parserErrors
            };
            var docResult = parser.document();
            _ast = docResult.Tree;
        }

        public TemplateDictionary State { get; set; }

        public TemplateCache Cache { get; set; }

        public TemplateExecutionResult Apply(TemplateDictionary state)
        {
            var result = new TemplateExecutionResult();
            result.Errors.AddRange(_parserErrors);
            if (result.Errors.ContainsError() || result.Errors.ContainsWarning())
            {
                return result;
            }

            State = state;
            var documentResult = Document(_ast);

            result.Output = documentResult.Buffer.ToString();
            result.Errors.AddRange(documentResult.Errors);

            return result;
        }

        private DocumentResult Document(CommonTree tree)
        {
            var result = new DocumentResult();
            var context = new Context();

            if (tree.Children == null) return result;

            foreach (var section in tree.Children)
            {
                ITemplateType o;
                var s = new Section(section);
                var e = s.Eval(context, this, out o);
                result.Errors.AddRange(e);
                if (e.ContainsError())
                    continue;
                o.WriteTo(result.Buffer);
            }

            return result;
        }
    }
}