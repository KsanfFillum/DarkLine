
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace DarkCrystal.CommandLine
{
    public class CommandLine
    {
        private Compiler Compiler;

        public CommandLine(IGlobalObjectResolver defaultGlobalObjectResolver)
        {
            Compiler = new Compiler(defaultGlobalObjectResolver);
        }

        // ----------------- INTERFACE --------------
        #region INTERFACE

        public TOut Execute<TOut>(string line, IArgumentlessResolver globalResolver) => (TOut)Compiler.GetDelegate<TOut>(line, globalResolver).DynamicInvoke();
        public TOut Execute<TOut, TIn>(string line, TIn arg, IArgumentResolver<TIn> globalResolver) => (TOut)Compiler.GetDelegate<TOut>(line, globalResolver).DynamicInvoke(arg);
        public object Execute<TIn>(string line, TIn arg, IArgumentResolver<TIn> globalResolver) => Compiler.GetDelegate(line, globalResolver).DynamicInvoke(arg);
        public object Execute(string line, IArgumentlessResolver globalResolver) => Compiler.GetDelegate(line, globalResolver).DynamicInvoke();

        public bool Validate(string line, out string errorText, IGlobalObjectResolver globalResolver = null) => ValidateInternal(line, Compiler.TryCompile, out errorText, globalResolver);
        public bool Validate<TOut>(string line, out string errorText, IGlobalObjectResolver globalResolver = null) => ValidateInternal(line, Compiler.TryCompile<TOut>, out errorText, globalResolver);

        public string AutoComplete(string line, IGlobalObjectResolver resolver)
        {
            Compiler.SetResolver(resolver);
            try
            {
                InternalFakeExecution(Compiler.TryCompile, line + '…');
            }
            catch (AutoCompleteException exception)
            {
                return exception.AutoCompletedText;
            }
            catch (Exception exception)
            {
                exception.ToString();
            }
            return String.Empty;
        }

        #endregion

        // ---------- VALIDATION & AUTOCOMPLETE -----------------
        #region VALIDATION & AUTOCOMPLETE

        private bool ValidateInternal(string line, Compiler.CompilationDelegate compilationDelegate, out string errorText, IGlobalObjectResolver globalResolver = null)
        {
            try
            {
                if (!String.IsNullOrEmpty(line))
                {
                    Compiler.SetResolver(globalResolver);
                    InternalFakeExecution(compilationDelegate, line);
                }
                errorText = null;
                return true;
            }
            catch (Exception exception)
            {
                errorText = exception.Message;
                return false;
            }
        }

        private void InternalFakeExecution(Compiler.CompilationDelegate compilationDelegate, string line)
        {
            try
            {
                if (!compilationDelegate(line, Compiler.GlobalResolver, out var @delegate, out var exception))
                {
                    if (exception != null)
                    {
                        throw exception;
                    }
                }
            }
            catch (AutoCompleteException exception)
            {
                throw exception;
            }
            catch (TokenException exception)
            {
                throw new FormattedException(exception.Message, line, exception.Token);
            }
        }

        #endregion
    }
}