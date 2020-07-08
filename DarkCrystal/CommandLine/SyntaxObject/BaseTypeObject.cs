﻿
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DarkCrystal.CommandLine
{
    public abstract class BaseTypeObject : SyntaxObject
    {
        public Type Type { get; protected set; }

        protected abstract BindingFlags Flags { get; }
        protected abstract Expression Instance { get; }
        
        public BaseTypeObject(Type valueType, Token token) : base(token)
        {
            this.Type = valueType;
        }

        public override SyntaxObject GetMember(Token token)
        {
            var memberName = token.Data as string;
            var propertyInfo = Type.GetProperty(memberName, Flags);
            if (propertyInfo != null)
            {
                return new Property(propertyInfo, Instance, token);
            }

            var fieldInfo = Type.GetField(memberName, Flags);
            if (fieldInfo != null)
            {
                return new Property(fieldInfo, Instance, token);
            }

            foreach (var _ in Function.Cache.GetFor(Type, memberName))
            {
                return new Function(memberName, Type, Instance, token);
            }

            return base.GetMember(token);
        }

        public override string AutoCompleteMember(string startText)
        {
            // fields
            foreach (var field in Type.GetFields(Flags))
            {
                if (!field.IsSpecialName && field.Name.StartsWith(startText))
                {
                    return field.Name;
                }
            }

            // properties
            foreach (var propetry in Type.GetProperties(Flags))
            {
                if (!propetry.IsSpecialName && propetry.Name.StartsWith(startText))
                {
                    return propetry.Name;
                }
            }

            // methods
            if (!Type.IsEnum)
            {
                foreach (var method in Function.Cache.GetFor(Type))
                {
                    if (!method.MethodInfo.IsSpecialName && method.MethodInfo.Name.StartsWith(startText))
                    {
                        return method.MethodInfo.Name;
                    }
                }
            }

            return null;
        }
    }
}