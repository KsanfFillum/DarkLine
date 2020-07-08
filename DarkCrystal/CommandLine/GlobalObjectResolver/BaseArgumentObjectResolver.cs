
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DarkCrystal.CommandLine
{
    public class BaseArgumentObjectResolver<T> : BaseGlobalObjectResolver, IArgumentResolver<T>
    {
        public ParameterExpression MainArgument { get; protected set; }

        public BaseArgumentObjectResolver(string argumentName, IGlobalObjectResolver parent) : base(parent)
        {
            MainArgument = Expression.Parameter(typeof(T), argumentName);
            AddBinding(argumentName, new ObjectBinding(BindType.Argument, typeof(T), MainArgument));
        }

        public void AddArgument(string name, Type type, Expression getter)
        {
            AddBinding(name, new ObjectBinding(BindType.Argument, type, getter));
        }

        public override Delegate GetThrowerDelegate(Exception ex) => (Action<T>)((T arg) => throw ex);
    }
}