
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq.Expressions;

namespace DarkCrystal.CommandLine
{
    public struct ObjectBinding
    {
        public BindType BindType;
        public Type Type;
        public object Value;

        public ObjectBinding(BindType bindType, Type type, object value)
        {
            this.BindType = bindType;
            this.Type = type;
            this.Value = value;
        }
        
        public SyntaxObject GetSyntaxObject(Token token)
        {
            switch (BindType)
            {
                case BindType.Class:
                    return new Class(Type, token);

                case BindType.Value:
                    return new Value(Type, Value, token);

                case BindType.ValueGetter:
                    return new Value(Type, (Expression)Value, token);

                case BindType.Namespace:
                    return Value as Namespace;

                case BindType.Argument:
                    return new Value(Type, Value as Expression, token);
                    
                default:
                    throw new Exception("Unknown bind type");
            }
        }
    }
}