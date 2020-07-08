
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace DarkCrystal.CommandLine
{
    public class Value : BaseTypeObject
    {
        public Expression ValueExpression { get; private set; }

        protected override BindingFlags Flags => BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.Static |BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod;
        protected override Expression Instance => ValueExpression;

        public object Get() => ValueExpression;

        public Value(Type valueType, object m_value, Token token) : base(valueType, token)
        {
            this.ValueExpression = Expression.Constant(m_value, valueType).ReduceIfPossible();
        }
        public Value(Type valueType, Expression expression, Token token) : base(valueType, token)
        {
            this.ValueExpression = expression.ReduceIfPossible();
        }

        public override Value GetValue()
        {
            return this;
        }

        public override string ToString()
        {
            if (ValueExpression == null)
            {
                return String.Format("{0} instance", Type.Name);
            }
            else
            {
                return ValueExpression.ToString();
            }
        }

        public Property GetThroughAccessor(Value accessor)
        {
            if (Type.IsArray)
            {
                var accessorExpr = Expression.ArrayAccess(ValueExpression, accessor.ValueExpression);
                return new Property(accessorExpr, accessor.Token);
            }
            else
            {
                return new Property(Expression.MakeIndex(ValueExpression, Type.GetProperty("Item"), new[] { accessor.ValueExpression }), accessor.Token);
            }
        }
    }
}
