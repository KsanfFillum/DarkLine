
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DarkCrystal.CommandLine
{
    public static class TypeCache
    {
        private static Dictionary<KeyValuePair<Type, Type>, bool> m_checkedDict = new Dictionary<KeyValuePair<Type, Type>, bool>();
        private static Type[][] TypeHierarchy = {
                    new Type[] { typeof(Byte),  typeof(SByte), typeof(Char) },
                    new Type[] { typeof(Int16), typeof(UInt16) },
                    new Type[] { typeof(Int32), typeof(UInt32) },
                    new Type[] { typeof(Int64), typeof(UInt64) },
                    new Type[] { typeof(Single) },
                    new Type[] { typeof(Double) }
                };
        public static bool IsCastableTo(this Type from, Type to, bool implicitly = false)
        {
            if (m_checkedDict.TryGetValue(new KeyValuePair<Type, Type>(from, to), out var res))
            {
                return res;
            }

            res = to.IsAssignableFrom(from) || from.HasCastDefined(to, implicitly); ;
            m_checkedDict[new KeyValuePair<Type, Type>(from, to)] = res;
            return res;
        }

        static bool HasCastDefined(this Type from, Type to, bool implicitly)
        {
            if ((from.IsPrimitive || from.IsEnum) && (to.IsPrimitive || to.IsEnum))
            {
                if (!implicitly)
                    return from == to || (from != typeof(Boolean) && to != typeof(Boolean));

               
                var lowerTypes = new HashSet<Type>();
                foreach (Type[] types in TypeHierarchy)
                {
                    if (types.Any(t => t == to))
                        return lowerTypes.Any(t => t == from);
                    lowerTypes.UnionWith(types);
                }

                return false;   // IntPtr, UIntPtr, Enum, Boolean
            }
            return IsCastDefined(to, m => m.GetParameters()[0].ParameterType, _ => from, implicitly, false)
                || IsCastDefined(from, _ => to, m => m.ReturnType, implicitly, true);
        }

        static bool IsCastDefined(Type type, Func<MethodInfo, Type> baseType,
                                Func<MethodInfo, Type> derivedType, bool implicitly, bool lookInBase)
        {
            return Function.Cache.GetFor(type, "op_Implicit").First(m => baseType(m.MethodInfo).IsAssignableFrom(derivedType(m.MethodInfo))).MethodInfo != null
                || (!implicitly && Function.Cache.GetFor(type, "op_Explicit").First(m => baseType(m.MethodInfo).IsAssignableFrom(derivedType(m.MethodInfo))).MethodInfo != null);
        }

        public static void Cast(ref Expression left, ref Expression right)
        {
            var leftTypeCode = Type.GetTypeCode(left.Type);
            var rightTypeCode = Type.GetTypeCode(right.Type);

            if (leftTypeCode == rightTypeCode)
                return;

            if (CheckEnum(ref left, right) || CheckEnum(ref right, left))
            {
                return;
            }

            if (leftTypeCode > rightTypeCode)
                right = Expression.Convert(right, left.Type);
            else
                left = Expression.Convert(left, right.Type);
        }

        private static bool CheckEnum(ref Expression left, Expression right)
        {
            if (left.Type.IsEnum && right.Type == typeof(string))
            {
                left = Expression.Convert(left, typeof(object));
                left = Expression.Convert(left, right.Type, typeof(Convert).GetMethod("ToString", new Type[] { typeof(object) }));
                return true;
            }

            return false;
        }

        public static Expression Cast(Expression expr, Type to)
        {
            if (expr.Type == to)
            {
                return expr;
            }

            return Expression.Convert(expr, to);
        }
    }
}