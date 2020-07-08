
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DarkCrystal.CommandLine
{
    public abstract class BaseGlobalObjectResolver : IGlobalObjectResolver
    {
        private IGlobalObjectResolver Parent;
        private Dictionary<string, ObjectBinding> Bindings;


        public BaseGlobalObjectResolver(IGlobalObjectResolver parent = null)
        {
            this.Parent = parent;
        }

        public SyntaxObject GetObject(Token token)
        {
            var identifier = token.Data as string;
            ObjectBinding binding;
            if (Bindings.TryGetValue(identifier, out binding))
            {
                return binding.GetSyntaxObject(token);
            }

            if (this.Parent != null)
            {
                return Parent.GetObject(token);
            }

            throw new System.Exception(String.Format("Can't resolve global object {0}", identifier));
        }

        public virtual string AutoComplete(string startText)
        {
            if (!String.IsNullOrEmpty(startText))
            {
                if (Bindings != null)
                {
                    foreach (var name in Bindings.Keys)
                    {
                        if (name.StartsWith(startText))
                        {
                            return name;
                        }
                    }
                }
            }

            return Parent?.AutoComplete(startText);
        }
        
        public void AddClass(string name, Type type)
        {
            AddBinding(name, new ObjectBinding(BindType.Class, type, null));
        }

        public void AddValue(string name, Type type, object value)
        {
            AddBinding(name, new ObjectBinding(BindType.Value, type, value));
        }

        public void AddValueGetter(string name, Type type, Expression getter)
        {
            AddBinding(name, new ObjectBinding(BindType.ValueGetter, type, getter));
        }

        public void AddNamespace(string name, string @namespace)
        {
            var namespaceInstance = Namespace.Get(@namespace);
            AddBinding(name, new ObjectBinding(BindType.Namespace, null, namespaceInstance));
        }

        protected void AddBinding(string name, ObjectBinding binding)
        {
            if (Bindings == null)
            {
                Bindings = new Dictionary<string, ObjectBinding>();
            }
            Bindings[name] = binding;
        }

        public abstract Delegate GetThrowerDelegate(Exception ex);
    }
}