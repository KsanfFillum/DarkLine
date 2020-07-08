
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DarkCrystal.CommandLine
{
    public partial class Function
    {
        public static class ExtentionCache
        {
            private static Dictionary<Type, HashSet<MethodInfo>> m_cache = new Dictionary<Type, HashSet<MethodInfo>>();
            private static Dictionary<Type, HashSet<MethodInfo>> m_genericsCache = new Dictionary<Type, HashSet<MethodInfo>>();
            static ExtentionCache()
            {
                HashSet<MethodInfo> Get(Dictionary<Type, HashSet<MethodInfo>> dict, Type key)
                {
                    if (dict.TryGetValue(key, out var set))
                    {
                        return set;
                    }

                    set = new HashSet<MethodInfo>();
                    dict[key] = set;
                    return set;
                }

                foreach (var type in DarkExtensions.GetTypes())
                {
                    if (type.IsStatic() && !type.IsGenericType && !type.IsNested)
                    {
                        foreach (var method in type.GetMethods())
                        {
                            if (method.IsDefined(typeof(ExtensionAttribute), false))
                            {
                                var mainParameter = method.GetParameters()[0].ParameterType;
                                if (mainParameter.IsGenericParameter)
                                {
                                    bool valid = true;
                                    foreach (var arg in method.GetGenericArguments())
                                    {
                                        if (arg != mainParameter)
                                        {
                                            valid = false;
                                            break;
                                        }
                                    }

                                    if (valid)
                                    {
                                        Get(m_genericsCache, mainParameter).Add(method);
                                    }
                                }
                                else
                                {
                                    if (!method.ContainsGenericParameters)
                                    {
                                        Get(m_cache, mainParameter).Add(method);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public static IEnumerable<MethodInfo> GetExtentionsFor(Type t)
            {
                if (m_cache.TryGetValue(t, out var set))
                {
                    foreach (var elem in set)
                    {
                        yield return elem;
                    }
                }

                foreach (var pair in m_genericsCache)
                {
                    if (t.MeetTheConstraints(pair.Key))
                    {
                        foreach (var elem in pair.Value)
                        {
                            yield return elem.MakeGenericMethod(t);
                        }
                    }
                }
            }
        }
    }
}