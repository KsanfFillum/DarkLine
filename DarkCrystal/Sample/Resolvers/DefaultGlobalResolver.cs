
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq.Expressions;
using DarkCrystal.CommandLine;

namespace DarkCrystal.Sample
{
    public class DefaultGlobalResolver : GlobalObjectResolver
    {
        public DefaultGlobalResolver()
        {
            AddClass("World", typeof(World));
            AddClass("PartType", typeof(PartType));
            AddClass("CharacterUtils", typeof(CharacterExtensions));

            AddValueGetter("Player", typeof(Character),  Expression.Field(null, typeof(World), "Player")); // static field
            AddValueGetter("Enemies", typeof(Character[]), Expression.Field(null, typeof(World), "Enemies")); // static field
        }
    }
}