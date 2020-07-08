
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace DarkCrystal.CommandLine
{
    public class Namespace : SyntaxObject
    {
        public static Namespace Empty = new Namespace(string.Empty, new Token(TokenType.Identifier, 0, 0, string.Empty));
        private static readonly Dictionary<string, List<SyntaxObject>> AvalaibleNamespaces;

        static Namespace()
        {
            AvalaibleNamespaces = new Dictionary<string, List<SyntaxObject>>();

            foreach (var type in DarkExtensions.GetTypes())
            {
                if (ParseTypeToSyntaxObject(type, out var syntaxObject))
                {
                    PushNamespaceRecursive((type.Namespace ?? "").Split('.').ToList(), syntaxObject);
                }
            }
        }

        private static bool ParseTypeToSyntaxObject(Type type, out SyntaxObject obj)
        {
            if (type.IsClass || type.IsEnum)
            {
                if (!type.IsAbstract || (type.IsAbstract && type.IsSealed))
                {
                    obj = new Class(type, new Token(TokenType.Identifier, type.Name.Length, type.Name.Length));
                    return true;
                }
            }

            obj = null;
            return false;
        }

        private static void PushNamespaceRecursive(List<string> currentNamespaceList, SyntaxObject @object)
        {
            var currentNamespace = string.Join(".", currentNamespaceList);
            if (!AvalaibleNamespaces.TryGetValue(currentNamespace, out List<SyntaxObject> CurrentAssemblyList))
            {
                CurrentAssemblyList = new List<SyntaxObject>();
                AvalaibleNamespaces[currentNamespace] = CurrentAssemblyList;
            }

            CurrentAssemblyList.Add(@object);
            if (currentNamespaceList.Count > 0)
            {
                var ownName = currentNamespaceList[currentNamespaceList.Count - 1];
                currentNamespaceList.RemoveAt(currentNamespaceList.Count - 1);
                var token = new Token(TokenType.Value, 0, 0, ownName);

                PushNamespaceRecursive(currentNamespaceList, new Namespace(string.Join(".", currentNamespaceList), token));
            }
        }

        public static IEnumerable<SyntaxObject> GetAllClassesForNamespace(string @namespace)
        {
            if (AvalaibleNamespaces.TryGetValue(@namespace, out var list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    yield return list[i];
                }
            }
        }

        public static Namespace Get(string @namespace)
        {
            if (string.IsNullOrEmpty(@namespace))
            {
                return Empty;
            }

            var stack = new List<string>(@namespace.Split('.'));
            var ownNamespace = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);
            var parentNamespace = string.Join(".", stack);

            var namespaceInstance = Empty;

            var foundNamespace = AvalaibleNamespaces[parentNamespace].Find(o => (o as Namespace)?.OwnNamespace == ownNamespace) as Namespace;
            return foundNamespace;
        }

        public static List<SyntaxObject> Get(Namespace space)
        {
            return AvalaibleNamespaces[space.CurrentNamespace];
        }

        protected string CurrentNamespace;
        protected string OwnNamespace;

        private Namespace(string parent, Token token) : base(token)
        {
            OwnNamespace = token.Data.ToString();
            CurrentNamespace = string.Format("{0}.{1}", parent, OwnNamespace);
        }


        public override SyntaxObject GetMember(Token token)
        {
            var avalaileForThisNamespace = AvalaibleNamespaces[CurrentNamespace];
            var memberName = token.Data as string;
            var propertyInfo = avalaileForThisNamespace.Find(sObj =>
            {
                switch (sObj)
                {
                    case Namespace @namespace:
                        if (@namespace.OwnNamespace == memberName)
                        {
                            return true;
                        }
                        break;

                    case BaseTypeObject @object:
                        if (@object.Type.Name == memberName)
                        {
                            return true;
                        }
                        break;
                }

                return false;
            });

            if (propertyInfo != null)
            {
                return propertyInfo;
            }

            return base.GetMember(token);
        }

        public override string AutoCompleteMember(string startText)
        {
            var avalaileForThisNamespace = AvalaibleNamespaces[CurrentNamespace];
            foreach (var syntaxObject in avalaileForThisNamespace)
            {
                switch (syntaxObject)
                {
                    case Namespace @namespace:
                        if (@namespace.OwnNamespace.StartsWith(startText))
                        {
                            return @namespace.OwnNamespace;
                        }
                        break;

                    case BaseTypeObject @object:
                        if (@object.Type.Name.StartsWith(startText))
                        {
                            return @object.Type.Name;
                        }
                        break;
                }
            }
            
            return null;
        }
    }
}