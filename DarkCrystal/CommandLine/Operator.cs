
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DarkCrystal.CommandLine
{
    public class Operator
    {
        public delegate Expression HandlerType(Expression a, Expression b);

        private static Dictionary<string, Operator> Operators;
        public string Name { get; private set; }
        public HandlerType Handler { get; private set; }
        public int Priority { get; private set; }
        public Type ArgumentType1 { get; private set; }
        public Type ArgumentType2 { get; private set; }
        public Type ResultType { get; private set; }

        static Operator()
        {
            Operators = new Dictionary<string, Operator>();
            int priority = 0;
            AddOperator<object, object, object>("=", Expression.Assign, priority);
            priority++;
            AddOperator<bool, bool, bool>("||", Expression.Or, priority);
            priority++;
            AddOperator<bool, bool, bool>("&&", Expression.And, priority);
            priority++;
            AddOperator<object, object, bool>("==", Expression.Equal, priority);
            AddOperator<object, object, bool>("!=", Expression.NotEqual, priority);
            priority++;
            AddOperator<float, float, bool>("<", Expression.LessThan, priority);
            AddOperator<float, float, bool>(">", Expression.GreaterThan, priority);
            AddOperator<float, float, bool>("<=", Expression.LessThanOrEqual, priority);
            AddOperator<float, float, bool>(">=", Expression.GreaterThanOrEqual, priority);
            priority++;
            AddOperator<float, float, float>("+", Expression.Add, priority);
            AddOperator<float, float, float>("-", Expression.Subtract, priority);
            priority++;
            AddOperator<float, float, float>("*", Expression.Multiply, priority);
            AddOperator<float, float, float>("/", Expression.Divide, priority);
        }

        public Value Evaluate(Expression lValue, Expression rValue, Token token)
        {
            TypeCache.Cast(ref lValue, ref rValue);
            return new Value(ResultType, Handler(lValue, rValue), token);
        }

        private Operator()
        {
        }

        public static Operator Get(string name) => Operators[name];
        
        private static void AddOperator<ArgT1, ArgT2, ResultT>(string name, HandlerType handler, int priority)
        {
            var @operator = new Operator();
            @operator.Name = name;
            @operator.Handler = handler;
            @operator.Priority = priority;
            @operator.ArgumentType1 = typeof(ArgT1);
            @operator.ArgumentType2 = typeof(ArgT2);
            @operator.ResultType = typeof(ResultT);
            Operators[name] = @operator;
        }
    }
}