
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DarkCrystal.CommandLine
{
    public class Property : SyntaxObject
    {
        private Expression MemberExpression;
        private Type PropertyType;
        private string PropertyName;

        public Property(PropertyInfo propertyInfo, Expression holder, Token token) : base(token)
        {
            this.MemberExpression = Expression.Property(holder, propertyInfo).ReduceIfPossible();
            this.PropertyType = propertyInfo.PropertyType;
            this.PropertyName = propertyInfo.Name;
        }

        public Property(FieldInfo fieldInfo, Expression holder, Token token) : base(token)
        {
            this.MemberExpression = Expression.Field(holder, fieldInfo).ReduceIfPossible();
            this.PropertyType = fieldInfo.FieldType;
            this.PropertyName = fieldInfo.Name;
        }

        public Property(Expression memberExpression, Token token) : base(token)
        {
            this.MemberExpression = memberExpression;
            this.PropertyType = memberExpression.Type;
            this.PropertyName = "Special Property";
        }

        public override Value GetValue()
        {
            return new Value(PropertyType, MemberExpression, Token);
        }

        public override SyntaxObject GetMember(Token token)
        {
            return GetValue().GetMember(token);
        }

        public override string AutoCompleteMember(string startText)
        {
            return GetValue().AutoCompleteMember(startText);
        }

        public override string ToString()
        {
            return String.Format("Property '{0}'", PropertyName);
        }
    }
}