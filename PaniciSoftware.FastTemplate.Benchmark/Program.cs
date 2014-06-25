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

namespace PaniciSoftware.FastTemplate.Benchmark
{
    internal delegate string TestDelegate(int a, int b, int c, int d);

    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                GeneralTemplate();
                PropertyAccessTemplate();
            }
            else
            {
                switch (args[0])
                {
                    case "gen":
                    {
                        GeneralTemplate();
                        break;
                    }
                    case "get":
                    {
                        PropertyAccessTemplate();
                        break;
                    }
                    case "set":
                    {
                        PropertySetTemplate();
                        break;
                    }
                    case "func":
                    {
                        FunctionCallTemplate();
                        break;
                    }
                    case "dele":
                    {
                        DelegateCallTemplate();
                        break;
                    }
                    case "fixed":
                    {
                        FixedArgListCompare();
                        break;
                    }
                    case "assem":
                    {
                        TestAssembly();
                        break;
                    }
                }
            }
            Console.ReadKey();
        }

        public static string Foo(int a, int b, int c, int d)
        {
            return "adfasdfasdf";
        }

        public static TemplateDictionary BuildState()
        {
            Func<int, int, int, string, string> addSome = (int a, int b, int c, string s) =>
            {
                var buffer = new StringBuilder();
                buffer.Append(a);
                buffer.Append(b);
                buffer.Append(c);
                buffer.Append(s);
                return buffer.ToString();
            };
            Func<int, int, int, string, double, long, string> addSomeMore =
                (int a, int b, int c, string s, double d, long l) =>
                {
                    var buffer = new StringBuilder();
                    buffer.Append(a);
                    buffer.Append(b);
                    buffer.Append(c);
                    buffer.Append(s);
                    buffer.Append(d);
                    buffer.Append(l);
                    return buffer.ToString();
                };

            TestDelegate foo = Foo;

            var dict = new TemplateDictionary
            {
                {"o1", new ObjectWithDeepProperties()},
                {"o2", new ObjectWithDeepProperties()},
                {"o3", new ObjectWithDeepProperties()},
                {"o4", new ObjectWithDeepProperties()},
                {"o5", new ObjectWithDeepProperties()},
                {"o6", new ObjectWithDeepProperties()},
                {"source", new ObjectWithManyFunctions()},
                {"addSome", addSome},
                {"addSomeMore", addSomeMore},
                {"foo", foo}
            };
            return dict;
        }

        public static void TestAssembly()
        {
            var state = BuildState();
            var text = File.ReadAllText("bench.template");
            var compilerResult = Template.Compile("test", text);
            if (compilerResult.Errors.ContainsError() || compilerResult.Errors.ContainsWarning())
            {
                Console.Error.WriteLine(compilerResult.Errors);
                return;
            }
            TemplateExecutionResult result1;
            result1 = compilerResult.Template.Execute(state);
            if (result1.Errors.ContainsError() || result1.Errors.ContainsWarning())
            {
                Console.Error.WriteLine(result1.Errors);
                return;
            }

            using (var file = File.Create("bench.tc"))
            {
                compilerResult.Template.Assembly.Write(file);
            }

            TemplateExecutionResult result2;
            using (var file = File.OpenRead("bench.tc"))
            {
                var a = new Assembly(file);

                var vt = new VirtualTemplate("test");
                var compileErrors = vt.Compile(a);

                if (compileErrors.ContainsError() || compileErrors.ContainsWarning())
                {
                    Console.Error.WriteLine(compileErrors);
                    return;
                }

                result2 = vt.Execute(state);
                if (result2.Errors.ContainsError() || result2.Errors.ContainsWarning())
                {
                    Console.Error.WriteLine(result2.Errors);
                    return;
                }
            }
            if (String.Compare(result1.Output, result2.Output, StringComparison.Ordinal) != 0)
            {
                Console.Out.WriteLine("not the same");
            }
        }

        public static void FunctionCallTemplate()
        {
            Console.Out.WriteLine("Results for function-call.template");
            Console.Out.WriteLine("--------------------------------------------");
            var state = BuildState();
            var text = File.ReadAllText("function-call.template");
            {
                Histogram time;
                Histogram memory;
                Bench.CompileOnceAndRun(text, state, 1000, OptimizeLevel.None, out time, out memory);
                Console.Out.WriteLine("JIT OFF Compiler Timing:");
                Console.Out.WriteLine("Compiler Timing:");
                Console.Out.WriteLine(time);
                Console.Out.WriteLine("Compiler Memory Usage:");
                Console.Out.WriteLine(memory);
                Console.Out.WriteLine("--------------------------------------------");
            }

            {
                Histogram time;
                Histogram memory;
                Bench.CompileOnceAndRun(text, state, 1000, OptimizeLevel.All, out time, out memory);
                Console.Out.WriteLine("JIT ON Compiler Timing:");
                Console.Out.WriteLine("Compiler Timing:");
                Console.Out.WriteLine(time);
                Console.Out.WriteLine("Compiler Memory Usage:");
                Console.Out.WriteLine(memory);
                Console.Out.WriteLine("--------------------------------------------");
            }
        }

        public static void FixedArgListCompare()
        {
            Console.Out.WriteLine("Results for function-call.template");
            Console.Out.WriteLine("--------------------------------------------");
            var state = BuildState();
            var text = File.ReadAllText("function-call.template");
            {
                Histogram time;
                Histogram memory;
                Bench.CompileOnceAndRun(text, state, 1000, OptimizeLevel.Callsite, out time, out memory);
                Console.Out.WriteLine("Just Callsite ON Compiler Timing:");
                Console.Out.WriteLine("Compiler Timing:");
                Console.Out.WriteLine(time);
                Console.Out.WriteLine("Compiler Memory Usage:");
                Console.Out.WriteLine(memory);
                Console.Out.WriteLine("--------------------------------------------");
            }

            {
                Histogram time;
                Histogram memory;
                Bench.CompileOnceAndRun(text, state, 1000, OptimizeLevel.All, out time, out memory);
                Console.Out.WriteLine("All JIT options ON Compiler Timing:");
                Console.Out.WriteLine("Compiler Timing:");
                Console.Out.WriteLine(time);
                Console.Out.WriteLine("Compiler Memory Usage:");
                Console.Out.WriteLine(memory);
                Console.Out.WriteLine("--------------------------------------------");
            }
        }

        public static void DelegateCallTemplate()
        {
            Console.Out.WriteLine("Results for delegate-call.template");
            Console.Out.WriteLine("--------------------------------------------");
            var state = BuildState();
            var text = File.ReadAllText("delegate-call.template");
            {
                Histogram time;
                Histogram memory;
                Bench.CompileOnceAndRun(text, state, 1000, OptimizeLevel.None, out time, out memory);
                Console.Out.WriteLine("JIT OFF Compiler Timing:");
                Console.Out.WriteLine("Compiler Timing:");
                Console.Out.WriteLine(time);
                Console.Out.WriteLine("Compiler Memory Usage:");
                Console.Out.WriteLine(memory);
                Console.Out.WriteLine("--------------------------------------------");
            }

            {
                Histogram time;
                Histogram memory;
                Bench.CompileOnceAndRun(text, state, 1000, OptimizeLevel.All, out time, out memory);
                Console.Out.WriteLine("JIT ON Compiler Timing:");
                Console.Out.WriteLine("Compiler Timing:");
                Console.Out.WriteLine(time);
                Console.Out.WriteLine("Compiler Memory Usage:");
                Console.Out.WriteLine(memory);
                Console.Out.WriteLine("--------------------------------------------");
            }
        }

        public static void PropertyAccessTemplate()
        {
            Console.Out.WriteLine("Results for property-access.template");
            Console.Out.WriteLine("--------------------------------------------");
            var state = BuildState();
            var text = File.ReadAllText("property-access.template");
            {
                Histogram time;
                Histogram memory;
                Bench.CompileOnceAndRun(text, state, 1000, OptimizeLevel.None, out time, out memory);
                Console.Out.WriteLine("JIT OFF Compiler Timing:");
                Console.Out.WriteLine("Compiler Timing:");
                Console.Out.WriteLine(time);
                Console.Out.WriteLine("Compiler Memory Usage:");
                Console.Out.WriteLine(memory);
                Console.Out.WriteLine("--------------------------------------------");
            }

            {
                Histogram time;
                Histogram memory;
                Bench.CompileOnceAndRun(text, state, 1000, OptimizeLevel.All, out time, out memory);
                Console.Out.WriteLine("JIT ON Compiler Timing:");
                Console.Out.WriteLine("Compiler Timing:");
                Console.Out.WriteLine(time);
                Console.Out.WriteLine("Compiler Memory Usage:");
                Console.Out.WriteLine(memory);
                Console.Out.WriteLine("--------------------------------------------");
            }
        }

        public static void PropertySetTemplate()
        {
            Console.Out.WriteLine("Results for property-set.template");
            Console.Out.WriteLine("--------------------------------------------");
            var state = BuildState();
            var text = File.ReadAllText("property-set.template");
            {
                Histogram time;
                Histogram memory;
                Bench.CompileOnceAndRun(text, state, 1000, OptimizeLevel.None, out time, out memory);
                Console.Out.WriteLine("JIT OFF Compiler Timing:");
                Console.Out.WriteLine("Compiler Timing:");
                Console.Out.WriteLine(time);
                Console.Out.WriteLine("Compiler Memory Usage:");
                Console.Out.WriteLine(memory);
                Console.Out.WriteLine("--------------------------------------------");
            }

            {
                Histogram time;
                Histogram memory;
                Bench.CompileOnceAndRun(text, state, 1000, OptimizeLevel.All, out time, out memory);
                Console.Out.WriteLine("JIT ON Compiler Timing:");
                Console.Out.WriteLine("Compiler Timing:");
                Console.Out.WriteLine(time);
                Console.Out.WriteLine("Compiler Memory Usage:");
                Console.Out.WriteLine(memory);
                Console.Out.WriteLine("--------------------------------------------");
            }
        }

        public static void GeneralTemplate()
        {
            Console.Out.WriteLine("Results for bench.template");
            Console.Out.WriteLine("--------------------------------------------");
            var state = BuildState();
            var text = File.ReadAllText("bench.template");
            {
                var time = Bench.RunInterpreter(text, state, 100);
                Console.Out.WriteLine("Interpreter Timing:");
                Console.Out.WriteLine(time);
                Console.Out.WriteLine("--------------------------------------------");
            }
            {
                Histogram time;
                Histogram memory;
                Bench.CompileOnceAndRun(text, state, 100, OptimizeLevel.None, out time, out memory);
                Console.Out.WriteLine("JIT OFF Compiler Timing:");
                Console.Out.WriteLine("Compiler Timing:");
                Console.Out.WriteLine(time);
                Console.Out.WriteLine("--------------------------------------------");
            }

            {
                Histogram time;
                Histogram memory;
                Bench.CompileOnceAndRun(text, state, 100, OptimizeLevel.All, out time, out memory);
                Console.Out.WriteLine("JIT ON Compiler Timing:");
                Console.Out.WriteLine("Compiler Timing:");
                Console.Out.WriteLine(time);
                Console.Out.WriteLine("--------------------------------------------");
            }
        }
    }
}