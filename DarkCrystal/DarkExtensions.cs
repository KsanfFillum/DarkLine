
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace DarkCrystal
{
    public static class DarkExtensions
    {
        public static Expression ReduceIfPossible<TExpr>(this TExpr expr) where TExpr : Expression
        {
            return expr.CanReduce ? expr.Reduce() : expr;
        }

        public static bool MeetTheConstraints(this Type type, Type genericParameter)
        {
            var constraintAttributes = genericParameter.GenericParameterAttributes;
            if (constraintAttributes.Contains(GenericParameterAttributes.NotNullableValueTypeConstraint) && !type.IsValueType)
            {
                return false;
            }

            if (constraintAttributes.Contains(GenericParameterAttributes.ReferenceTypeConstraint) && type.IsValueType)
            {
                return false;
            }

            if (constraintAttributes.Contains(GenericParameterAttributes.DefaultConstructorConstraint) && type.GetConstructor(Type.EmptyTypes) == null && !type.IsValueType)
            {
                return false;
            }

            foreach (var param in genericParameter.GetGenericParameterConstraints())
            {
                if (!param.IsAssignableFrom(type))
                {
                    return false;
                }
            }

            return true;
        }

        // If you using assembly definition in your project, just add all the assemblies here
        // you can simply do it with 
        // foreach (var type in typeof(<any_typename_of_another_assembly>).Assembly.GetTypes())
        // {
        //     yield return type;
        // }
        public static IEnumerable<Type> GetTypes()
        {
            return Assembly.GetCallingAssembly().GetTypes();
        }

        public static bool IsStatic(this Type t) => t.IsAbstract && t.IsSealed;

        public static List<T> ToList<T>(this IEnumerable<T> enumerable)
        {
            return new List<T>(enumerable);
        }

        public static IEnumerable<TOut> ConvertCollection<TIn, TOut>(this IEnumerable<TIn> enumerable, Converter<TIn, TOut> converter)
        {
            foreach (var element in enumerable)
            {
                yield return converter(element);
            }
        }

        public static T[] ToArray<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.ToList().ToArray();
        }

        public static TVal EnsureValue<TKey, TVal>(this Dictionary<TKey, TVal> dictionary, TKey key) where TVal : new()
        {
            if (!dictionary.TryGetValue(key, out var value))
            {
                value = new TVal();
                dictionary[key] = value;
            }

            return value;
        }

        public static bool Contains<TEnum>(this TEnum value, TEnum predicate) where TEnum : Enum, IConvertible
        {
            var convertedPred = predicate.ToInt64(null);
            return (value.ToInt64(null) & convertedPred) == convertedPred;
        }

        public static bool Any<TType>(this IEnumerable<TType> enumerable, Predicate<TType> predicate)
        {
            foreach (var elem in enumerable)
            {
                if (predicate(elem))
                {
                    return true;
                }
            }
            return false;
        }

        public static TType First<TType>(this IEnumerable<TType> enumerable, Predicate<TType> predicate)
        {
            foreach (var elem in enumerable)
            {
                if (predicate(elem))
                {
                    return elem;
                }
            }
            return default;
        }
    }
}
