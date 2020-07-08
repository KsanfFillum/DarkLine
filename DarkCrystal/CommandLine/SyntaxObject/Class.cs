
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DarkCrystal.CommandLine
{
    public class Class : BaseTypeObject
    {
        protected override BindingFlags Flags => BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod;
        protected override Expression Instance => null;
        
        public Class(Type @class, Token token) : base(@class, token)
        {
        }

        public override SyntaxObject GetMember(Token token)
        {
            var memberName = token.Data as string;
            var nestedClass = Type.GetNestedType(memberName);
            if (nestedClass != null)
            {
                return new Class(nestedClass, token);
            }

            return base.GetMember(token);
        }

        public override string AutoCompleteMember(string startText)
        {
            foreach (var nestedClass in Type.GetNestedTypes())
            {
                if (!nestedClass.IsSpecialName && nestedClass.Name.StartsWith(startText))
                {
                    return nestedClass.Name;
                }
            }

            return base.AutoCompleteMember(startText);
        }

        public override string ToString()
        {
            return String.Format("{0} class", Type.Name);
        }
    }
}