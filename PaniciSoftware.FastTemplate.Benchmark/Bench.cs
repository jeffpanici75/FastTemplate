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
using PaniciSoftware.FastTemplate.Common;
using PaniciSoftware.FastTemplate.Compiler;

namespace PaniciSoftware.FastTemplate.Benchmark
{
    public class Bench
    {
        public static Histogram RunInterpreter(
            string source, 
            TemplateDictionary s, 
            int count = 1000)
        {
            var h = new Histogram();
            for (var i = 0; i < count; i++)
            {
                TemplateExecutionResult result;
                using (var timer = new ExecutionTimer())
                {
                    var inter = new Interpreter.Interpreter(source);
                    result = inter.Apply(s);
                    var ticks = (double) timer.ElapsedTicks;
                    var microSecond = ticks/(TimeSpan.TicksPerMillisecond/1000.0);
                    h.Add(microSecond);
                }
                if (result.Errors.ContainsError())
                    break;
            }
            return h;
        }

        public static void CompileOnceAndRun(
            string source,
            TemplateDictionary s,
            int count,
            out Histogram h,
            out Histogram m)
        {
            CompileOnceAndRun(source, s, count, OptimizeLevel.All, out h, out m);
        }

        public static void CompileOnceAndRun(
            string source,
            TemplateDictionary s,
            int count,
            OptimizeLevel l,
            out Histogram h,
            out Histogram m)
        {
            var compilerResult = Template.Compile("test", source, l);
            h = new Histogram();
            m = new Histogram();
            for (var i = 0; i < count; i++)
            {
                TemplateExecutionResult result;
                using (var timer = new ExecutionTimer())
                {
                    result = compilerResult.Template.Execute(s);
                    var ticks = (double) timer.ElapsedTicks;
                    var microSecond = ticks/(TimeSpan.TicksPerMillisecond/1000.0);
                    h.Add(microSecond);
                }

                if (result.Errors.ContainsError())
                {
                    Console.Error.WriteLine(result.Errors);
                    break;
                }
                m.Add(GC.GetTotalMemory(false));
            }
        }

        public static Histogram CompileOnceAndRunOnce(
            string source, 
            TemplateDictionary s, 
            int count = 1000)
        {
            var h = new Histogram();
            for (var i = 0; i < count; i++)
            {
                TemplateExecutionResult result;
                using (var timer = new ExecutionTimer())
                {
                    var compilerResult = Template.Compile("test", source);
                    result = compilerResult.Template.Execute(s);
                    var ticks = (double) timer.ElapsedTicks;
                    var microSecond = ticks/(TimeSpan.TicksPerMillisecond/1000.0);
                    h.Add(microSecond);
                }
                if (result.Errors.ContainsError())
                {
                    Console.Error.WriteLine(result.Errors);
                    break;
                }
            }
            return h;
        }
    }
}