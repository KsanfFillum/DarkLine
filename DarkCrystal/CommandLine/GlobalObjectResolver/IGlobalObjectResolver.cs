
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq.Expressions;

namespace DarkCrystal.CommandLine
{
    public interface IGlobalObjectResolver
    {
        SyntaxObject GetObject(Token token);
        string AutoComplete(string startText);
        Delegate GetThrowerDelegate(Exception ex);
    }

    public interface IArgumentlessResolver : IGlobalObjectResolver
    {
    }

    public interface IArgumentResolver : IGlobalObjectResolver
    {
        ParameterExpression MainArgument { get; }
    }

    public interface IArgumentResolver<T> : IArgumentResolver
    {
    }
}