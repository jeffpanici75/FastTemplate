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
using Antlr.Runtime;
using Antlr.Runtime.Tree;

namespace PaniciSoftware.FastTemplate.Common
{
    public static class ErrorListExtensions
    {
        public static void ErrorRecognition(this ErrorList errors, string header, string message)
        {
            errors.Add(
                Error.NewError(
                    "R999",
                    header,
                    message));
        }

        public static void ErrorKeyNotFound(
            this ErrorList errors,
            string name)
        {
            errors.Add(
                Error.NewWarning(
                    "E100",
                    string.Format(
                        "The key: {0} does not exist in the state table provied.",
                        name),
                    "This expansion has resolved to the empty string."));
        }

        public static void WarningPropertyNotFound(
            this ErrorList errors,
            Type t,
            string name)
        {
            errors.Add(
                Error.NewWarning(
                    "E101",
                    string.Format(
                        "The property: {0} does not exist on the type: {1}.",
                        name,
                        t),
                    "This property access has resolved to the empty string."));
        }

        public static void ErrorFunctionNotFound(
            this ErrorList errors,
            Type t,
            string name)
        {
            errors.Add(
                Error.NewWarning(
                    "E101",
                    string.Format(
                        "The function: {0} does not exist on the type: {1}.",
                        name,
                        t),
                    "This function access has resolved to the empty string."));
        }

        public static void ErrorTypeMismatch(
            this ErrorList errors,
            string expected,
            string found)
        {
            errors.Add(
                Error.NewError(
                    "TM00",
                    string.Format(
                        "Type mismatch. Expected: {0} Found: {1}.",
                        expected,
                        found),
                    "Please resolve type mismatch to continue."));
        }

        public static void ErrorTypeMismatch(
            this ErrorList errors,
            InvalidTypeException e)
        {
            errors.Add(
                Error.NewError(
                    "TM01",
                    "Type mismatch. Unable to apply operator.",
                    FormatException(e)));
        }

        public static void ErrorIEnumerableRequired(
            this ErrorList errors,
            string text)
        {
            errors.Add(
                Error.NewError(
                    "TM01",
                    "IEnumerable required in foreach statement.",
                    string.Format("In expression: {0}", text)));
        }

        public static void ErrorNonInvokableObject(
            this ErrorList errors,
            string name)
        {
            errors.Add(
                Error.NewError(
                    "TM01",
                    "Attempting to invoke an root object that is not invokable.",
                    string.Format("In expression: {0}", name)));
        }

        public static void ErrorMacrosOnlyAcceptStrings(
            this ErrorList errors,
            string name)
        {
            errors.Add(
                Error.NewError(
                    "E101",
                    string.Format(
                        "The key: {0} did not resolve to a string. Only able to macro expand strings.",
                        name),
                    "This macro has resolved to the empty string."));
        }

        public static void ErrorUnknownConstant(
            this ErrorList errors,
            string name)
        {
            errors.Add(
                Error.NewError(
                    "C101",
                    string.Format(
                        "Unknown constant found: {0}.",
                        name),
                    "Unrecoverable error."));
        }

        public static void ErrorUnhandledCompileError(
            this ErrorList errors,
            Exception e)
        {
            errors.Add(
                Error.NewError(
                    "C101",
                    FormatException(e),
                    "Unknown compile error."));
        }

        public static void ErrorMacrosOnlyAcceptStrings(
            this ErrorList errors,
            Type t,
            string name)
        {
            errors.Add(
                Error.NewError(
                    "E101",
                    string.Format(
                        "The property: {0} of type: {1} is not a string. Only able to macro expand strings.",
                        name,
                        t),
                    "This macro has resolved to the empty string."));
        }

        public static void ErrorRuntimeError(
            this ErrorList errors,
            Type t,
            string name,
            Exception e)
        {
            errors.Add(
                Error.NewError(
                    "R999",
                    string.Format(
                        "Runtime Error. Method/Property: {0} on Type: {1} threw an unhandled exception.",
                        name,
                        t),
                    FormatException(e)));
        }

        public static void ErrorRuntimeError(
            this ErrorList errors, string reason = "")
        {
            errors.Add(
                Error.NewError(
                    "R999",
                    "Runtime Error.",
                    reason));
        }

        public static void ErrorRuntimeError(
            this ErrorList errors,
            Exception e)
        {
            errors.Add(
                Error.NewError(
                    "R999",
                    "Runtime Error.",
                    FormatException(e)));
        }

        public static void ErrorRuntimeErrorNullRef(
            this ErrorList errors,
            NullReferenceException e)
        {
            errors.Add(
                Error.NewError(
                    "R990",
                    string.Format(
                        "Runtime Error. Null reference found."),
                    FormatException(e)));
        }

        public static void ErrorZeroArgsToIndexer(
            this ErrorList errors,
            string name)
        {
            errors.Add(
                Error.NewError(
                    "I001",
                    string.Format(
                        "Index into Property: {0} must have at least one argument.",
                        name),
                    ""));
        }

        public static void ErrorParse(
            this ErrorList errors,
            CommonErrorNode error)
        {
            errors.Add(
                Error.NewError(
                    "P999",
                    error.ToString(),
                    "Error in source file."));
        }

        public static void ErrorDuplicateLoopDirective(
            this ErrorList errors, string duplicate)
        {
            errors.Add(
                Error.NewError(
                    "P800",
                    string.Format(
                        "Duplicate loop directive found: {0}", duplicate),
                    "Only one of each directive allowed."));
        }

        public static void ErrorUnknownLoopDirective(this ErrorList errors, string name)
        {
            errors.Add(
                Error.NewError(
                    "P801",
                    string.Format("Unkown loop directive found: {0}", name),
                    "Valid directives: #before #after #each #between #odd #even #nodata #afterall #beforeall."));
        }

        public static void ErrorUnknownKeyword(this ErrorList errors, string text)
        {
            errors.Add(
                Error.NewError(
                    "P802",
                    string.Format("Unkown keyword directive found: {0}", text),
                    "Supported keywords: as in to step true false null."));
        }

        public static void ErrorInvalidJump(
            this ErrorList errors, string name, IToken t)
        {
            errors.Add(
                Error.NewError(
                    "P801",
                    string.Format(
                        "Invalid use of {0} between {1} and {2} on line: {3}.",
                        name,
                        t.StartIndex,
                        t.StopIndex,
                        t.Line),
                    "Inavlid use of jump statement. Only valid within loops."));
        }

        public static void ErrorMissingItemInChain(this ErrorList errors, Type t, string name)
        {
            errors.Add(
                Error.NewError(
                    "E101",
                    string.Format(
                        "The property: {0} of type: {1} does not exist. Can not perform set operation.",
                        name,
                        t),
                    "This set operation has failed."));
        }

        public static void ErrorUnableToSetIntoFunctionResult(this ErrorList errors, string type, string name)
        {
            errors.Add(
                Error.NewError(
                    "E101",
                    string.Format(
                        "The function named: {0} on type: {1} is not a valid lvalue. Can not perform set operation.",
                        name,
                        type),
                    "This set operation has failed."));
        }

        public static string FormatException(Exception e)
        {
            var builder = new StringBuilder();
            builder.Append(e);
            if (e.InnerException != null)
            {
                builder.AppendFormat(
                    "\r\n\r\nInner Exception:\r\n---------------------------\r\n{0}\r\n",
                    e.InnerException);
            }
            builder.AppendFormat(
                "\r\n\r\nStack Trace:\r\n----------------------------\r\n{0}\r\n",
                e.StackTrace);
            return builder.ToString();
        }
    }
}