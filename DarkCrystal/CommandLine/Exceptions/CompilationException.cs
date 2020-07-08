
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace DarkCrystal.CommandLine
{
    public class CompilationException : Exception
    {
        public CompilationException(Exception innerException) : base("Compilation Exception", innerException)
        {
        }
    }
}