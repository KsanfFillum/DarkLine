
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace DarkCrystal.CommandLine
{
    public partial class Function
    {
        public static class Cache
        {
            public struct MethodInfoData
            {
                public readonly MethodInfo MethodInfo;
                public readonly bool IsExtension;
                public MethodInfoData(MethodInfo methodInfo, bool isExtension)
                {
                    this.MethodInfo = methodInfo;
                    this.IsExtension = isExtension;
                }
            }

            private static Dictionary<Type, Dictionary<string, HashSet<MethodInfoData>>> m_cache = new Dictionary<Type, Dictionary<string, HashSet<MethodInfoData>>>();
            private static Dictionary<Type, Dictionary<string, HashSet<MethodInfoData>>> m_genericCache = new Dictionary<Type, Dictionary<string, HashSet<MethodInfoData>>>();
            public static IEnumerable<MethodInfoData> GetFor(Type t, string name)
            {
                if (GetMap(t).TryGetValue(name, out var array))
                {
                    foreach (var elem in array)
                    {
                        yield return elem;
                    }
                }
            }

            public static IEnumerable<MethodInfoData> GetFor(Type t)
            {
                foreach (var value in GetMap(t).Values)
                {
                    foreach (var elem in value)
                    {
                        yield return elem;
                    }
                }
            }

            private static Dictionary<string, HashSet<MethodInfoData>> GetMap(Type t)
            {
                if (!m_cache.TryGetValue(t, out var dict))
                {
                    dict = new Dictionary<string, HashSet<MethodInfoData>>();
                    foreach (var method in t.GetMethods())
                    {
                        dict.EnsureValue(method.Name).Add(new MethodInfoData(method, false));
                    }

                    foreach (var method in ExtentionCache.GetExtentionsFor(t))
                    {
                        dict.EnsureValue(method.Name).Add(new MethodInfoData(method, true));
                    }

                    m_cache[t] = dict;
                }

                return dict;
            }
        }
    }
}