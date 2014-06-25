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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Text;
using Antlr.Runtime;
using PaniciSoftware.FastTemplate.Common;

namespace PaniciSoftware.FastTemplate.Compiler
{
    internal delegate void RunTemplate(VirtualTemplate t);

    [Flags]
    public enum OptimizeLevel
    {
        None = 0x0,
        Callsite = 0x1,
        FixArgList = 0x2,
        All = 0x3
    }

    public class VirtualTemplate : IVirtualTemplate
    {
        private readonly Dictionary<Type, Dictionary<string, FixedArgsCallSite>> _compiledFixedArgs = new Dictionary<Type, Dictionary<string, FixedArgsCallSite>>();
        private readonly Dictionary<Type, Dictionary<string, CallSite>> _compiledFunctionCall = new Dictionary<Type, Dictionary<string, CallSite>>();
        private TemplateDictionary _state;
        private DynamicMethod _template;
        private StringBuilder _buffer;
        private RunTemplate _image;

        private ErrorList _errors;

        public VirtualTemplate(
            string name,
            OptimizeLevel l,
            TemplateCache cache = null)
        {
            Name = name;
            Cache = cache ?? new TemplateCache();
            TrimMode = TrimMode.FollowingNewLine;
            OptimizeLevel = l;
        }

        public VirtualTemplate(
            string name,
            TemplateCache cache = null)
        {
            Name = name;
            OptimizeLevel = OptimizeLevel.All;
            Cache = cache ?? new TemplateCache();
            TrimMode = TrimMode.FollowingNewLine;
        }

        public OptimizeLevel OptimizeLevel { get; private set; }

        public TemplateExecutionResult Execute(TemplateDictionary state)
        {
            var result = new TemplateExecutionResult {Template = this};

            _buffer = new StringBuilder();
            _errors = new ErrorList();
            _state = state;
            try
            {
                _image(this);
                result.Output = _buffer.ToString();
            }
            catch (Exception e)
            {
                result.Output = string.Empty;
                _errors.ErrorRuntimeError(e);
            }

            result.Errors.AddRange(_errors);

            return result;
        }

        public string Name { get; set; }

        public string Source { get; private set; }

        public string SourceUrl { get; set; }

        public IAssembly Assembly { get; set; }

        public TemplateCache Cache { get; set; }

        public ErrorList CompileFromFile(string path)
        {
            return InternalCompile(CreateCharStream(File.OpenRead(path)));
        }

        public ErrorList Compile(string source)
        {
            return InternalCompile(CreateCharStream(source));
        }

        public ErrorList Compile(Stream source)
        {
            return InternalCompile(CreateCharStream(source));
        }

        public ErrorList WarmUp(TemplateDictionary state)
        {
            _buffer = null;
            _errors = new ErrorList();
            _state = state;
            try
            {
                _image(this);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
            }
            return _errors;
        }

        public ErrorList Compile(Assembly a)
        {
            var e = new ErrorList();
            try
            {
                _template = new DynamicMethod(
                    string.Format("__template_{0}", Name),
                    typeof (void),
                    new[] {typeof (VirtualTemplate)},
                    typeof (VirtualTemplate),
                    true);
                var ctx = new AssemblyContext {Generator = _template.GetILGenerator()};
                a.Copmile(ctx);
                _image = (RunTemplate) _template.CreateDelegate(typeof (RunTemplate));
                Assembly = new Assembly(a);
            }
            catch (Exception ex)
            {
                e.ErrorUnhandledCompileError(ex);
            }
            return e;
        }

        private TrimMode TrimMode { get; set; }

        private static ICharStream CreateCharStream(Stream s)
        {
            return new ANTLRInputStream(s);
        }

        private static ICharStream CreateCharStream(string text)
        {
            return new ANTLRStringStream(text);
        }

        private ErrorList InternalCompile(ICharStream stream)
        {
            var errors = new ErrorList();
            var lexer = TemplateLexer.Create(stream);
            var tStream = new CommonTokenStream(lexer);
            var parser = new TemplateParser(tStream) {Errors = errors};
            var docResult = parser.document();

            _template = new DynamicMethod(
                string.Format("__template_{0}", Name),
                typeof (void),
                new[] {typeof (VirtualTemplate)},
                typeof (VirtualTemplate),
                true);

            var emit = _template.GetILGenerator();
            var ctx = new Context(emit) {OptimizeLevel = OptimizeLevel};
            var doc = new Document(docResult.Tree);

            try
            {
                var e = doc.Generate(ctx);
                errors.AddRange(e);
                _image = (RunTemplate) _template.CreateDelegate(typeof (RunTemplate));
                Assembly = ctx.Sink.Build();
            }
            catch (Exception e)
            {
                errors.ErrorUnhandledCompileError(e);
            }

            stream.Seek(0);
            Source = stream.ToString();

            return errors;
        }

        private void SetPragma(string pragma)
        {
            if (string.IsNullOrEmpty(pragma))
                return;
            switch (pragma)
            {
                case "trim":
                {
                    TrimMode = TrimMode.Greedy;
                    break;
                }
                case "notrim":
                {
                    TrimMode = TrimMode.FollowingNewLine;
                    break;
                }
            }
        }

        private void AppendToBuffer(ITemplateType t)
        {
            if (t == null)
                return;
            if (_buffer == null)
                return;
            t.WriteTo(_buffer);
        }

        private void AppendLiteral(string l)
        {
            if (string.IsNullOrEmpty(l))
                return;
            if (_buffer == null)
                return;
            switch (TrimMode)
            {
                case TrimMode.FollowingNewLine:
                {
                    _buffer.Append(l);
                    break;
                }
                case TrimMode.Greedy:
                {
                    _buffer.Append(l.GreedyTrim());
                    break;
                }
                default:
                {
                    _buffer.Append(l);
                    break;
                }
            }
        }

        private ITemplateType GetRootObject(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            ITemplateType obj;
            if (!_state.TryGetValue(name, out obj))
            {
                _errors.ErrorKeyNotFound(name);
                return new NullType();
            }
            return obj;
        }

        private void InvokeParse(IEnumerable<ITemplateType> files)
        {
            foreach (var f in files)
            {
                string s;
                if (!f.TryConvert(out s))
                    continue;
                try
                {
                    var cacheResult = Cache.GetTemplate(s);
                    if (!cacheResult.Success)
                    {
                        _errors.AddRange(cacheResult.Errors);
                        continue;
                    }
                    var result = cacheResult.Template.Execute(_state);
                    _errors.AddRange(result.Errors);
                    if (_buffer != null)
                        _buffer.Append(result.Output);
                }
                catch (Exception e)
                {
                    _errors.ErrorRuntimeError(e);
                }
            }
        }

        private void InvokeInclude(IEnumerable<ITemplateType> files)
        {
            foreach (var f in files)
            {
                string s;
                if (!f.TryConvert(out s))
                    continue;
                try
                {
                    var resolverResult = Cache.ResourceResolver(s);
                    if (!resolverResult.Success)
                    {
                        _errors.AddRange(resolverResult.Errors);
                        continue;
                    }
                    var read = new StreamReader(resolverResult.Stream);
                    var content = read.ReadToEnd();
                    if (_buffer != null)
                        _buffer.Append(content);
                }
                catch (Exception e)
                {
                    _errors.ErrorRuntimeError(e);
                }
            }
        }

        private ITemplateType InvokeFunction(
            ITemplateType that,
            string name,
            ITemplateType[] args)
        {
            if (that == null)
                return null;
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                _errors.ErrorFunctionNotFound(type, name);
                return null;
            }
            var isVoid = method.ReturnType == typeof (void);
            if ((OptimizeLevel & OptimizeLevel.Callsite) == OptimizeLevel.Callsite)
            {
                CallSite site;
                if (!TryGetCompiledFunctionCall(type, name, out site))
                {
                    site = new MemberFunctionCallSite(method);
                    SetCompiledFunctionCall(type, name, site);
                }
                try
                {
                    var rt = site.Invoke(that.RawValue, args.ToRaw());
                    return isVoid ? new VoidType() : PrimitiveType.Create(rt);
                }
                catch (Exception e)
                {
                    _errors.ErrorRuntimeError(e);
                    return null;
                }
            }
            try
            {
                var rt = method.Invoke(that.RawValue, args.ToRaw());
                return isVoid ? new VoidType() : PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
                return null;
            }
        }

        private ITemplateType InvokePassthrough(IEnumerable<ITemplateType> args)
        {
            var buffer = new StringBuilder();
            foreach (var a in args)
            {
                string obj;
                if (a.TryConvert(out obj))
                {
                    buffer.Append(obj);
                }
            }
            return StringType.New(buffer.ToString());
        }

        private int CheckIsEnumerable(ITemplateType t)
        {
            if (t.RawValue is IEnumerable)
            {
                return 1;
            }
            _errors.ErrorIEnumerableRequired(t.UnderlyingType.Name);
            return 0;
        }

        private ITemplateType CurrentToTType(IEnumerator e)
        {
            var c = e.Current;
            return PrimitiveType.Create(c);
        }

        private IEnumerator PrimeIter(ITemplateType t)
        {
            var iter = (IEnumerable) t.RawValue;
            return iter.GetEnumerator();
        }

        private void SetState(
            string name,
            ITemplateType rvalue)
        {
            if (string.IsNullOrEmpty(name)
                || rvalue == null)
                return;
            _state.SetValue(name, rvalue);
        }

        private ITemplateType EvalMacro(ITemplateType source)
        {
            if (source == null) return StringType.Empty();
            string obj;
            if (!source.TryConvert(out obj))
            {
                _errors.ErrorMacrosOnlyAcceptStrings(source.UnderlyingType, string.Empty);
                return StringType.Empty();
            }
            var i = new Interpreter.Interpreter(obj, Cache);
            var result = i.Apply(_state);
            _errors.AddRange(result.Errors);
            return StringType.New(result.Output);
        }

        private void SetCompiledFunctionCall(
            Type t,
            string name,
            CallSite site)
        {
            Dictionary<string, CallSite> sites;
            if (!_compiledFunctionCall.TryGetValue(t, out sites))
            {
                sites = new Dictionary<string, CallSite>();
                _compiledFunctionCall[t] = sites;
            }
            sites[name] = site;
        }

        private bool TryGetCompiledFunctionCall(
            Type t,
            string name,
            out CallSite site)
        {
            Dictionary<string, CallSite> sites;
            if (_compiledFunctionCall.TryGetValue(t, out sites)
                && sites.TryGetValue(name, out site))
            {
                return true;
            }
            site = null;
            return false;
        }

        private void SetCompiledFixedArgs(
            Type t,
            string name,
            FixedArgsCallSite site)
        {
            Dictionary<string, FixedArgsCallSite> sites;
            if (!_compiledFixedArgs.TryGetValue(t, out sites))
            {
                sites = new Dictionary<string, FixedArgsCallSite>();
                _compiledFixedArgs[t] = sites;
            }
            sites[name] = site;
        }

        private bool TryGetCompiledFixedArgs(
            Type t,
            string name,
            out FixedArgsCallSite site)
        {
            Dictionary<string, FixedArgsCallSite> sites;
            if (_compiledFixedArgs.TryGetValue(t, out sites)
                && sites.TryGetValue(name, out site))
            {
                return true;
            }
            site = null;
            return false;
        }

        #region Fixed Length Functions

        private ITemplateType InvokeFunction16(
            ITemplateType that,
            string name,
            ITemplateType arg0,
            ITemplateType arg1,
            ITemplateType arg2,
            ITemplateType arg3,
            ITemplateType arg4,
            ITemplateType arg5,
            ITemplateType arg6,
            ITemplateType arg7,
            ITemplateType arg8,
            ITemplateType arg9,
            ITemplateType arg10,
            ITemplateType arg11,
            ITemplateType arg12,
            ITemplateType arg13,
            ITemplateType arg14,
            ITemplateType arg15)
        {
            if (that == null)
                return null;
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                _errors.ErrorFunctionNotFound(type, name);
                return null;
            }
            var isVoid = method.ReturnType == typeof (void);
            FixedArgsCallSite site;
            if (!TryGetCompiledFixedArgs(type, name, out site))
            {
                site = new FixedArgsCallSite(16, method);
                SetCompiledFixedArgs(type, name, site);
            }
            try
            {
                var rt = site.Invoke(
                    that.RawValue,
                    arg0.RawValue,
                    arg1.RawValue,
                    arg2.RawValue,
                    arg3.RawValue,
                    arg4.RawValue,
                    arg5.RawValue,
                    arg6.RawValue,
                    arg7.RawValue,
                    arg8.RawValue,
                    arg9.RawValue,
                    arg10.RawValue,
                    arg11.RawValue,
                    arg12.RawValue,
                    arg13.RawValue,
                    arg14.RawValue,
                    arg15.RawValue);
                return isVoid ? new VoidType() : PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
                return null;
            }
        }

        private ITemplateType InvokeFunction15(
            ITemplateType that,
            string name,
            ITemplateType arg0,
            ITemplateType arg1,
            ITemplateType arg2,
            ITemplateType arg3,
            ITemplateType arg4,
            ITemplateType arg5,
            ITemplateType arg6,
            ITemplateType arg7,
            ITemplateType arg8,
            ITemplateType arg9,
            ITemplateType arg10,
            ITemplateType arg11,
            ITemplateType arg12,
            ITemplateType arg13,
            ITemplateType arg14)
        {
            if (that == null)
                return null;
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                _errors.ErrorFunctionNotFound(type, name);
                return null;
            }
            var isVoid = method.ReturnType == typeof (void);
            FixedArgsCallSite site;
            if (!TryGetCompiledFixedArgs(type, name, out site))
            {
                site = new FixedArgsCallSite(15, method);
                SetCompiledFixedArgs(type, name, site);
            }
            try
            {
                var rt = site.Invoke(
                    that.RawValue,
                    arg0.RawValue,
                    arg1.RawValue,
                    arg2.RawValue,
                    arg3.RawValue,
                    arg4.RawValue,
                    arg5.RawValue,
                    arg6.RawValue,
                    arg7.RawValue,
                    arg8.RawValue,
                    arg9.RawValue,
                    arg10.RawValue,
                    arg11.RawValue,
                    arg12.RawValue,
                    arg13.RawValue,
                    arg14.RawValue);
                return isVoid ? new VoidType() : PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
                return null;
            }
        }

        private ITemplateType InvokeFunction14(
            ITemplateType that,
            string name,
            ITemplateType arg0,
            ITemplateType arg1,
            ITemplateType arg2,
            ITemplateType arg3,
            ITemplateType arg4,
            ITemplateType arg5,
            ITemplateType arg6,
            ITemplateType arg7,
            ITemplateType arg8,
            ITemplateType arg9,
            ITemplateType arg10,
            ITemplateType arg11,
            ITemplateType arg12,
            ITemplateType arg13)
        {
            if (that == null)
                return null;
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                _errors.ErrorFunctionNotFound(type, name);
                return null;
            }
            var isVoid = method.ReturnType == typeof (void);
            FixedArgsCallSite site;
            if (!TryGetCompiledFixedArgs(type, name, out site))
            {
                site = new FixedArgsCallSite(14, method);
                SetCompiledFixedArgs(type, name, site);
            }
            try
            {
                var rt = site.Invoke(
                    that.RawValue,
                    arg0.RawValue,
                    arg1.RawValue,
                    arg2.RawValue,
                    arg3.RawValue,
                    arg4.RawValue,
                    arg5.RawValue,
                    arg6.RawValue,
                    arg7.RawValue,
                    arg8.RawValue,
                    arg9.RawValue,
                    arg10.RawValue,
                    arg11.RawValue,
                    arg12.RawValue,
                    arg13.RawValue);
                return isVoid ? new VoidType() : PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
                return null;
            }
        }

        private ITemplateType InvokeFunction13(
            ITemplateType that,
            string name,
            ITemplateType arg0,
            ITemplateType arg1,
            ITemplateType arg2,
            ITemplateType arg3,
            ITemplateType arg4,
            ITemplateType arg5,
            ITemplateType arg6,
            ITemplateType arg7,
            ITemplateType arg8,
            ITemplateType arg9,
            ITemplateType arg10,
            ITemplateType arg11,
            ITemplateType arg12)
        {
            if (that == null)
                return null;
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                _errors.ErrorFunctionNotFound(type, name);
                return null;
            }
            var isVoid = method.ReturnType == typeof (void);
            FixedArgsCallSite site;
            if (!TryGetCompiledFixedArgs(type, name, out site))
            {
                site = new FixedArgsCallSite(13, method);
                SetCompiledFixedArgs(type, name, site);
            }
            try
            {
                var rt = site.Invoke(
                    that.RawValue,
                    arg0.RawValue,
                    arg1.RawValue,
                    arg2.RawValue,
                    arg3.RawValue,
                    arg4.RawValue,
                    arg5.RawValue,
                    arg6.RawValue,
                    arg7.RawValue,
                    arg8.RawValue,
                    arg9.RawValue,
                    arg10.RawValue,
                    arg11.RawValue,
                    arg12.RawValue);

                return isVoid ? new VoidType() : PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
                return null;
            }
        }

        private ITemplateType InvokeFunction12(
            ITemplateType that,
            string name,
            ITemplateType arg0,
            ITemplateType arg1,
            ITemplateType arg2,
            ITemplateType arg3,
            ITemplateType arg4,
            ITemplateType arg5,
            ITemplateType arg6,
            ITemplateType arg7,
            ITemplateType arg8,
            ITemplateType arg9,
            ITemplateType arg10,
            ITemplateType arg11)
        {
            if (that == null)
                return null;
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                _errors.ErrorFunctionNotFound(type, name);
                return null;
            }
            var isVoid = method.ReturnType == typeof (void);
            FixedArgsCallSite site;
            if (!TryGetCompiledFixedArgs(type, name, out site))
            {
                site = new FixedArgsCallSite(12, method);
                SetCompiledFixedArgs(type, name, site);
            }
            try
            {
                var rt = site.Invoke(
                    that.RawValue,
                    arg0.RawValue,
                    arg1.RawValue,
                    arg2.RawValue,
                    arg3.RawValue,
                    arg4.RawValue,
                    arg5.RawValue,
                    arg6.RawValue,
                    arg7.RawValue,
                    arg8.RawValue,
                    arg9.RawValue,
                    arg10.RawValue,
                    arg11.RawValue);

                return isVoid ? new VoidType() : PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
                return null;
            }
        }

        private ITemplateType InvokeFunction11(
            ITemplateType that,
            string name,
            ITemplateType arg0,
            ITemplateType arg1,
            ITemplateType arg2,
            ITemplateType arg3,
            ITemplateType arg4,
            ITemplateType arg5,
            ITemplateType arg6,
            ITemplateType arg7,
            ITemplateType arg8,
            ITemplateType arg9,
            ITemplateType arg10)
        {
            if (that == null)
                return null;
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                _errors.ErrorFunctionNotFound(type, name);
                return null;
            }
            var isVoid = method.ReturnType == typeof (void);
            FixedArgsCallSite site;
            if (!TryGetCompiledFixedArgs(type, name, out site))
            {
                site = new FixedArgsCallSite(11, method);
                SetCompiledFixedArgs(type, name, site);
            }
            try
            {
                var rt = site.Invoke(
                    that.RawValue,
                    arg0.RawValue,
                    arg1.RawValue,
                    arg2.RawValue,
                    arg3.RawValue,
                    arg4.RawValue,
                    arg5.RawValue,
                    arg6.RawValue,
                    arg7.RawValue,
                    arg8.RawValue,
                    arg9.RawValue,
                    arg10.RawValue);

                return isVoid ? new VoidType() : PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
                return null;
            }
        }

        private ITemplateType InvokeFunction10(
            ITemplateType that,
            string name,
            ITemplateType arg0,
            ITemplateType arg1,
            ITemplateType arg2,
            ITemplateType arg3,
            ITemplateType arg4,
            ITemplateType arg5,
            ITemplateType arg6,
            ITemplateType arg7,
            ITemplateType arg8,
            ITemplateType arg9)
        {
            if (that == null)
                return null;
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                _errors.ErrorFunctionNotFound(type, name);
                return null;
            }
            var isVoid = method.ReturnType == typeof (void);
            FixedArgsCallSite site;
            if (!TryGetCompiledFixedArgs(type, name, out site))
            {
                site = new FixedArgsCallSite(10, method);
                SetCompiledFixedArgs(type, name, site);
            }
            try
            {
                var rt = site.Invoke(
                    that.RawValue,
                    arg0.RawValue,
                    arg1.RawValue,
                    arg2.RawValue,
                    arg3.RawValue,
                    arg4.RawValue,
                    arg5.RawValue,
                    arg6.RawValue,
                    arg7.RawValue,
                    arg8.RawValue,
                    arg9.RawValue);

                return isVoid ? new VoidType() : PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
                return null;
            }
        }

        private ITemplateType InvokeFunction9(
            ITemplateType that,
            string name,
            ITemplateType arg0,
            ITemplateType arg1,
            ITemplateType arg2,
            ITemplateType arg3,
            ITemplateType arg4,
            ITemplateType arg5,
            ITemplateType arg6,
            ITemplateType arg7,
            ITemplateType arg8)
        {
            if (that == null)
                return null;
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                _errors.ErrorFunctionNotFound(type, name);
                return null;
            }
            var isVoid = method.ReturnType == typeof (void);
            FixedArgsCallSite site;
            if (!TryGetCompiledFixedArgs(type, name, out site))
            {
                site = new FixedArgsCallSite(9, method);
                SetCompiledFixedArgs(type, name, site);
            }
            try
            {
                var rt = site.Invoke(
                    that.RawValue,
                    arg0.RawValue,
                    arg1.RawValue,
                    arg2.RawValue,
                    arg3.RawValue,
                    arg4.RawValue,
                    arg5.RawValue,
                    arg6.RawValue,
                    arg7.RawValue,
                    arg8.RawValue);

                return isVoid ? new VoidType() : PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
                return null;
            }
        }

        private ITemplateType InvokeFunction8(
            ITemplateType that,
            string name,
            ITemplateType arg0,
            ITemplateType arg1,
            ITemplateType arg2,
            ITemplateType arg3,
            ITemplateType arg4,
            ITemplateType arg5,
            ITemplateType arg6,
            ITemplateType arg7)
        {
            if (that == null)
                return null;
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                _errors.ErrorFunctionNotFound(type, name);
                return null;
            }
            var isVoid = method.ReturnType == typeof (void);
            FixedArgsCallSite site;
            if (!TryGetCompiledFixedArgs(type, name, out site))
            {
                site = new FixedArgsCallSite(8, method);
                SetCompiledFixedArgs(type, name, site);
            }
            try
            {
                var rt = site.Invoke(
                    that.RawValue,
                    arg0.RawValue,
                    arg1.RawValue,
                    arg2.RawValue,
                    arg3.RawValue,
                    arg4.RawValue,
                    arg5.RawValue,
                    arg6.RawValue,
                    arg7.RawValue);

                return isVoid ? new VoidType() : PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
                return null;
            }
        }

        private ITemplateType InvokeFunction7(
            ITemplateType that,
            string name,
            ITemplateType arg0,
            ITemplateType arg1,
            ITemplateType arg2,
            ITemplateType arg3,
            ITemplateType arg4,
            ITemplateType arg5,
            ITemplateType arg6)
        {
            if (that == null)
                return null;
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                _errors.ErrorFunctionNotFound(type, name);
                return null;
            }
            var isVoid = method.ReturnType == typeof (void);
            FixedArgsCallSite site;
            if (!TryGetCompiledFixedArgs(type, name, out site))
            {
                site = new FixedArgsCallSite(7, method);
                SetCompiledFixedArgs(type, name, site);
            }
            try
            {
                var rt = site.Invoke(
                    that.RawValue,
                    arg0.RawValue,
                    arg1.RawValue,
                    arg2.RawValue,
                    arg3.RawValue,
                    arg4.RawValue,
                    arg5.RawValue,
                    arg6.RawValue);

                return isVoid ? new VoidType() : PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
                return null;
            }
        }

        private ITemplateType InvokeFunction6(
            ITemplateType that,
            string name,
            ITemplateType arg0,
            ITemplateType arg1,
            ITemplateType arg2,
            ITemplateType arg3,
            ITemplateType arg4,
            ITemplateType arg5)
        {
            if (that == null)
                return null;
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                _errors.ErrorFunctionNotFound(type, name);
                return null;
            }
            var isVoid = method.ReturnType == typeof (void);
            FixedArgsCallSite site;
            if (!TryGetCompiledFixedArgs(type, name, out site))
            {
                site = new FixedArgsCallSite(6, method);
                SetCompiledFixedArgs(type, name, site);
            }
            try
            {
                var rt = site.Invoke(
                    that.RawValue,
                    arg0.RawValue,
                    arg1.RawValue,
                    arg2.RawValue,
                    arg3.RawValue,
                    arg4.RawValue,
                    arg5.RawValue);

                return isVoid ? new VoidType() : PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
                return null;
            }
        }

        private ITemplateType InvokeFunction5(
            ITemplateType that,
            string name,
            ITemplateType arg0,
            ITemplateType arg1,
            ITemplateType arg2,
            ITemplateType arg3,
            ITemplateType arg4)
        {
            if (that == null)
                return null;
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                _errors.ErrorFunctionNotFound(type, name);
                return null;
            }
            var isVoid = method.ReturnType == typeof (void);
            FixedArgsCallSite site;
            if (!TryGetCompiledFixedArgs(type, name, out site))
            {
                site = new FixedArgsCallSite(5, method);
                SetCompiledFixedArgs(type, name, site);
            }
            try
            {
                var rt = site.Invoke(
                    that.RawValue,
                    arg0.RawValue,
                    arg1.RawValue,
                    arg2.RawValue,
                    arg3.RawValue,
                    arg4.RawValue);

                return isVoid ? new VoidType() : PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
                return null;
            }
        }

        private ITemplateType InvokeFunction4(
            ITemplateType that,
            string name,
            ITemplateType arg0,
            ITemplateType arg1,
            ITemplateType arg2,
            ITemplateType arg3)
        {
            if (that == null)
                return null;
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                _errors.ErrorFunctionNotFound(type, name);
                return null;
            }
            var isVoid = method.ReturnType == typeof (void);
            FixedArgsCallSite site;
            if (!TryGetCompiledFixedArgs(type, name, out site))
            {
                site = new FixedArgsCallSite(4, method);
                SetCompiledFixedArgs(type, name, site);
            }
            try
            {
                var rt = site.Invoke(
                    that.RawValue,
                    arg0.RawValue,
                    arg1.RawValue,
                    arg2.RawValue,
                    arg3.RawValue);

                return isVoid ? new VoidType() : PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
                return null;
            }
        }

        private ITemplateType InvokeFunction3(
            ITemplateType that,
            string name,
            ITemplateType arg0,
            ITemplateType arg1,
            ITemplateType arg2)
        {
            if (that == null)
                return null;
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                _errors.ErrorFunctionNotFound(type, name);
                return null;
            }
            var isVoid = method.ReturnType == typeof (void);
            FixedArgsCallSite site;
            if (!TryGetCompiledFixedArgs(type, name, out site))
            {
                site = new FixedArgsCallSite(3, method);
                SetCompiledFixedArgs(type, name, site);
            }
            try
            {
                var rt = site.Invoke(
                    that.RawValue,
                    arg0.RawValue,
                    arg1.RawValue,
                    arg2.RawValue);

                return isVoid ? new VoidType() : PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
                return null;
            }
        }

        private ITemplateType InvokeFunction2(
            ITemplateType that,
            string name,
            ITemplateType arg0,
            ITemplateType arg1)
        {
            if (that == null)
                return null;
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                _errors.ErrorFunctionNotFound(type, name);
                return null;
            }
            var isVoid = method.ReturnType == typeof (void);
            FixedArgsCallSite site;
            if (!TryGetCompiledFixedArgs(type, name, out site))
            {
                site = new FixedArgsCallSite(2, method);
                SetCompiledFixedArgs(type, name, site);
            }
            try
            {
                var rt = site.Invoke(
                    that.RawValue,
                    arg0.RawValue,
                    arg1.RawValue);

                return isVoid ? new VoidType() : PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
                return null;
            }
        }

        private ITemplateType InvokeFunction1(
            ITemplateType that,
            string name,
            ITemplateType arg1)
        {
            if (that == null)
                return null;
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                _errors.ErrorFunctionNotFound(type, name);
                return null;
            }
            var isVoid = method.ReturnType == typeof (void);
            FixedArgsCallSite site;
            if (!TryGetCompiledFixedArgs(type, name, out site))
            {
                site = new FixedArgsCallSite(1, method);
                SetCompiledFixedArgs(type, name, site);
            }
            try
            {
                var rt = site.Invoke(that.RawValue, arg1.RawValue);
                return isVoid ? new VoidType() : PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
                return null;
            }
        }

        private ITemplateType InvokeFunction0(
            ITemplateType that,
            string name)
        {
            if (that == null)
                return null;
            var type = that.UnderlyingType;
            var method = type.GetMethod(name);
            if (method == null)
            {
                _errors.ErrorFunctionNotFound(type, name);
                return null;
            }
            var isVoid = method.ReturnType == typeof (void);
            FixedArgsCallSite site;
            if (!TryGetCompiledFixedArgs(type, name, out site))
            {
                site = new FixedArgsCallSite(0, method);
                SetCompiledFixedArgs(type, name, site);
            }
            try
            {
                var rt = site.Invoke(that.RawValue);
                return isVoid ? new VoidType() : PrimitiveType.Create(rt);
            }
            catch (Exception e)
            {
                _errors.ErrorRuntimeError(e);
                return null;
            }
        }

        #endregion
    }
}