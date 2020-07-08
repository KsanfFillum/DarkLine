
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DarkCrystal.CommandLine
{
    public partial class Function : SyntaxObject
    {
        private Type CollerType;
        private string FunctionSignature;
        private Cache.MethodInfoData MethodInfo;
        private Expression FuncExpression;
        private Expression Caller;

        public Function(string signature, Type collerType, Expression caller, Token token) : base(token)
        {
            this.CollerType = collerType;
            this.FunctionSignature = signature;
            this.Caller = caller;
        }

        public void FinalizeFunction(Value[] arguments)
        {
            int bestScore = int.MaxValue;
            foreach (var method in Cache.GetFor(CollerType, FunctionSignature))
            {
                var parameters = method.MethodInfo.GetParameters().ToList();
                if (method.IsExtension)
                {
                    parameters.RemoveAt(0);
                }

                int convertationScore = 0;
                if (arguments.Length > parameters.Count)
                {
                    continue;
                }

                if (parameters.Count != arguments.Length)
                {
                    if (parameters[arguments.Length].IsOptional)
                    {
                        convertationScore += 64;
                    }
                    else
                    {
                        continue;
                    }
                }

                int i = -1;
                bool invalid = false;

                while (++i < arguments.Length)
                {
                    if (parameters[i].ParameterType.IsGenericParameter)
                    {
                        if (!arguments[i].Type.MeetTheConstraints(parameters[i].ParameterType))
                        {
                            invalid = true;
                            break;
                        }
                    }
                    else if (parameters[i].ParameterType == arguments[i].Type)
                    {
                    }
                    else if (arguments[i].Type.IsCastableTo(parameters[i].ParameterType))
                    {
                        ++convertationScore;
                    }
                    else
                    {
                        invalid = true;
                        break;
                    }
                }

                if (invalid)
                {
                    continue;
                }

                if (bestScore > convertationScore)
                {
                    bestScore = convertationScore;
                    if (method.MethodInfo.IsGenericMethodDefinition)
                    {
                        var genericType = new Dictionary<Type, Type>();
                        foreach (var param in parameters)
                        {
                            if (param.ParameterType.IsGenericParameter)
                            {
                                genericType[param.ParameterType] = arguments[param.Position].Type;
                            }
                        }

                        MethodInfo = new Cache.MethodInfoData(
                            method.MethodInfo.MakeGenericMethod(
                                method.MethodInfo.GetGenericArguments().ConvertCollection(g => genericType[g]).ToArray()),
                            method.IsExtension);

                    }
                    else
                    {
                        MethodInfo = method;
                    }

                    if (convertationScore == 0)
                    {
                        break;
                    }
                    continue;
                }
            }

            if (MethodInfo.MethodInfo == null)
            {
                StringBuilder builder = new StringBuilder("There are no function with given signature. Candidates:");
                builder.AppendLine();
                foreach (var function in Cache.GetFor(CollerType, FunctionSignature))
                {
                    builder.AppendLine(function.MethodInfo.ToString());
                }

                throw new TokenException(builder.ToString(), Token);
            }

            var @params = MethodInfo.MethodInfo.GetParameters();
            Expression[] args = new Expression[@params.Length];
            int counter = 0;
            if (MethodInfo.IsExtension)
            {
                args[counter++] = Caller;
                for (; counter < arguments.Length + 1; counter++)
                {
                    args[counter] = TypeCache.Cast(arguments[counter - 1].ValueExpression, @params[counter].ParameterType);
                }

                for (; counter < @params.Length; counter++)
                {
                    args[counter] = Expression.Constant(@params[counter].DefaultValue);
                }
                this.FuncExpression = Expression.Call(MethodInfo.MethodInfo, args).ReduceIfPossible();
            }
            else
            {
                for (; counter < arguments.Length; counter++)
                {
                    args[counter] = TypeCache.Cast(arguments[counter].ValueExpression, @params[counter].ParameterType);
                }

                for (; counter < @params.Length; counter++)
                {
                    args[counter] = Expression.Constant(@params[counter].DefaultValue);
                }
            }

            if (MethodInfo.MethodInfo.IsStatic)
            {
                this.FuncExpression = Expression.Call(MethodInfo.MethodInfo, args).ReduceIfPossible();
            }
            else
            {
                this.FuncExpression = Expression.Call(Caller, MethodInfo.MethodInfo, args).ReduceIfPossible();
            }
        }

        public override Value GetValue() => new Value(MethodInfo.MethodInfo.ReturnType, FuncExpression, Token);

        public override string ToString()
        {
            return String.Format("Function '{0}'", MethodInfo.MethodInfo?.Name ?? ("Unfinished " + FunctionSignature));
        }
    }
}