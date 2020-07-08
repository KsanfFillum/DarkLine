
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DarkCrystal.CommandLine
{
    public class Compiler
    {
        public delegate bool CompilationDelegate(string line, IGlobalObjectResolver globalObjectResolver, out Delegate result, out Exception exception);

        private Lexer Lexer;
        private Dictionary<CompiledKey, Delegate> CompiledDelegates;
        private IGlobalObjectResolver DefaultResolver;

        public IGlobalObjectResolver GlobalResolver { get; private set; }
        public void SetResolver(IGlobalObjectResolver resolver)
        {
            GlobalResolver = resolver ?? DefaultResolver;
        }

        public Compiler(IGlobalObjectResolver defaultResolver)
        {
            Lexer = new Lexer();
            DefaultResolver = defaultResolver;
            CompiledDelegates = new Dictionary<CompiledKey, Delegate>();
        }

        // ----------------- COMPILATION ---------------
        #region COMPILATION

        public bool TryCompile(string line, IGlobalObjectResolver globalResolver, out Delegate @delegate, out Exception ex)
        {
            return TryCompileInternal(line, globalResolver, (expression) => expression, out @delegate, out ex);
        }

        public bool TryCompile<TOut>(string line, IGlobalObjectResolver globalResolver, out Delegate @delegate, out Exception ex)
        {
            Expression OnParsingEnd(Expression compiledExpression)
            {
                return Expression.Convert(compiledExpression, typeof(TOut));
            }

            return TryCompileInternal(line, globalResolver, OnParsingEnd, out @delegate, out ex);
        }

        private bool TryCompileInternal(string line, IGlobalObjectResolver globalResolver, Func<Expression, Expression> onParsingEnd, out Delegate @delegate, out Exception ex)
        {
            GlobalResolver = globalResolver ?? DefaultResolver;

            ex = null;
            @delegate = default;
            var tokens = Lexer.Tokenize(line);
            try
            {
                tokens.MoveNext();
                var result = EvaluateExpression(tokens);
                var finalExpression = onParsingEnd(result.ValueExpression);

                if (tokens.Current.Type != TokenType.EndOfLine)
                {
                    ex = new UnexpectedTokenException("end of line", tokens.Current);
                    return false;
                }

                ParameterExpression[] parameters = null;
                if (globalResolver is IArgumentResolver argResolver)
                {
                    parameters = new ParameterExpression[] { argResolver.MainArgument };
                }

                var expression = finalExpression.ReduceIfPossible();
                var lambda = Expression.Lambda(expression, parameters);
                @delegate = lambda.Compile();

                return true;
            }
            catch (AutoCompleteException exception)
            {
                ex = exception;
            }
            catch (TokenException exception)
            {
                ex = new FormattedException(exception.Message, line, exception.Token);
            }
            catch (Exception exception)
            {
                ex = new FormattedException(exception.Message, line, tokens.Current, exception);
            }

            return false;
        }

        #endregion
        // ----------------- EXECUTION -----------------
        #region EXECUTION

        public Delegate GetDelegate(string line, IGlobalObjectResolver globalResolver)
        {
            var key = new CompiledKey()
            {
                LineHash = line.GetHashCode(),
                ResolverHash = globalResolver?.GetHashCode() ?? 0
            };

            if (!CompiledDelegates.TryGetValue(key, out var @delegate))
            {
                if (!TryCompile(line, globalResolver, out @delegate, out var error))
                {
                    @delegate = GlobalResolver.GetThrowerDelegate(new CompilationException(error));
                }
                CompiledDelegates[key] = @delegate;
            }

            return @delegate;
        }

        public Delegate GetDelegate<TOut>(string line, IGlobalObjectResolver globalResolver)
        {
            var key = new CompiledKey()
            {
                LineHash = line.GetHashCode(),
                ResolverHash = globalResolver?.GetHashCode() ?? 0,
                TypeHash = TypeHash<TOut>.Hash
            };

            if (!CompiledDelegates.TryGetValue(key, out var @delegate))
            {
                if (!TryCompile<TOut>(line, globalResolver, out @delegate, out var error))
                {
                    @delegate = GlobalResolver.GetThrowerDelegate(new CompilationException(error));
                }
                CompiledDelegates[key] = @delegate;
            }

            return @delegate;
        }

        #endregion
        // ----------------- PARSING ------------
        #region PARSING

        private Value EvaluateExpression(IEnumerator<Token> tokens)
        {
            var operand = EvaluateOperand(tokens);
            if (tokens.Current.IsEndExpressionToken())
            {
                return operand;
            }
            else if (tokens.Current.Type == TokenType.Operator)
            {
                var operands = new List<Value> { operand };
                var operators = new List<Operator> { };
                var operatorTokens = new List<Token> { };
                while (tokens.Current.Type == TokenType.Operator)
                {
                    operators.Add(tokens.Current.Data as Operator);
                    operatorTokens.Add(tokens.Current);
                    tokens.MoveNext();
                    operands.Add(EvaluateOperand(tokens));
                }

                while (operators.Count > 1)
                {
                    int i = GetOperatorIndex(operators);
                    operands[i] = operators[i].Evaluate(operands[i].ValueExpression, operands[i + 1].ValueExpression, operatorTokens[i]);
                    operands.RemoveAt(i + 1);
                    operators.RemoveAt(i);
                    operatorTokens.RemoveAt(i);
                }

                return operators[0].Evaluate(operands[0].ValueExpression, operands[1].ValueExpression, operatorTokens[0]);
            }
            else
            {
                throw new UnexpectedTokenException("end of expression", tokens.Current);
            }
        }

        private Value EvaluateOperand(IEnumerator<Token> tokens)
        {
            switch (tokens.Current.Type)
            {
                case TokenType.Value:
                    var @value = (Value)tokens.Current.Data;
                    tokens.MoveNext();
                    return @value;

                case TokenType.OpenBracket:
                    tokens.MoveNext();
                    var content = EvaluateExpression(tokens);
                    if (tokens.Current.Type != TokenType.ClosedBracket)
                    {
                        throw new UnexpectedTokenException("')'", tokens.Current);
                    }
                    tokens.MoveNext();
                    return content;

                case TokenType.OpenAccessorBracket:
                    tokens.MoveNext();
                    content = EvaluateExpression(tokens);
                    if (tokens.Current.Type != TokenType.ClosedAccessorBracket)
                    {
                        throw new UnexpectedTokenException("']'", tokens.Current);
                    }
                    tokens.MoveNext();
                    return content;

                case TokenType.Identifier:
                    return EvaluateIdentifier(tokens);

                case TokenType.Autocomplete:
                    var startText = tokens.Current.Data as string;
                    var autoCompletedText = GlobalResolver.AutoComplete(startText) ?? startText;
                    throw new AutoCompleteException(autoCompletedText.Substring(startText.Length));

                default:
                    throw new UnexpectedTokenException("start of expression", tokens.Current);
            }
        }

        private Value EvaluateIdentifier(IEnumerator<Token> tokens)
        {
            var currentObject = GlobalResolver.GetObject(tokens.Current);
            tokens.MoveNext();
            while (true)
            {
                if (tokens.Current.Type == TokenType.Dot)
                {
                    tokens.MoveNext();
                    if (tokens.Current.Type == TokenType.Identifier)
                    {
                        currentObject = currentObject.GetMember(tokens.Current);
                        tokens.MoveNext();
                    }
                    else if (tokens.Current.Type == TokenType.Autocomplete)
                    {
                        var startText = tokens.Current.Data as string;
                        var autoCompletedText = currentObject.AutoCompleteMember(startText) ?? startText;
                        throw new AutoCompleteException(autoCompletedText.Substring(startText.Length));
                    }
                    else
                    {
                        throw new UnexpectedTokenException("identifier", tokens.Current);
                    }
                }
                else if (tokens.Current.Type == TokenType.OpenBracket)
                {
                    var function = currentObject as Function;
                    if (function == null)
                    {
                        new TokenException("Try to call non-function object", tokens.Current);
                    }
                    tokens.MoveNext();
                    currentObject = EvaluateFunctionCall(function, tokens);
                }
                else if (tokens.Current.Type == TokenType.OpenAccessorBracket)
                {
                    var value = currentObject.GetValue();
                    if (value == null)
                    {
                        new TokenException("Try to access to non-value object", tokens.Current);
                    }
                    tokens.MoveNext();
                    currentObject = value.GetThroughAccessor(EvaluateExpression(tokens));
                    if (tokens.Current.Type != TokenType.ClosedAccessorBracket)
                    {
                        throw new UnexpectedTokenException("']'", tokens.Current);
                    }
                    tokens.MoveNext();
                }
                else if (tokens.Current.Type == TokenType.AssignmentOperator)
                {
                    var field = currentObject as Property;
                    if (field == null)
                    {
                        new TokenException("Try to assign non-field object", tokens.Current);
                    }
                    tokens.MoveNext();
                    var value = field.GetValue();
                    var newValue = EvaluateExpression(tokens);
                    if (newValue.Type.IsCastableTo(value.ValueExpression.Type))
                    {
                        currentObject = new Value(value.Type, Expression.Assign(value.ValueExpression, TypeCache.Cast(newValue.ValueExpression, value.ValueExpression.Type)), default);
                    }
                }
                else
                {
                    return currentObject.GetValue();
                }
            }
        }

        private Value EvaluateFunctionCall(Function function, IEnumerator<Token> tokens)
        {
            if (tokens.Current.Type == TokenType.ClosedBracket)
            {
                tokens.MoveNext();
                function.FinalizeFunction(EmptyArray<Value>.Value);
                return function.GetValue();
            }
            var arguments = new List<Value>();
            while (true)
            {
                arguments.Add(EvaluateExpression(tokens));
                if (tokens.Current.Type == TokenType.Comma)
                {
                    tokens.MoveNext();
                    continue;
                }
                else if (tokens.Current.Type == TokenType.ClosedBracket)
                {
                    tokens.MoveNext();
                    function.FinalizeFunction(arguments.ToArray());
                    return function.GetValue();
                }
                else
                {
                    throw new UnexpectedTokenException("'('", tokens.Current);
                }
            }
        }

        private int GetOperatorIndex(List<Operator> operators)
        {
            int index = 0;
            int highestPriority = operators[index].Priority;
            for (int i = 1; i < operators.Count; i++)
            {
                int priority = operators[i].Priority;
                if (priority > highestPriority)
                {
                    highestPriority = priority;
                    index = i;
                }
            }
            return index;
        }
        #endregion
    }
}
