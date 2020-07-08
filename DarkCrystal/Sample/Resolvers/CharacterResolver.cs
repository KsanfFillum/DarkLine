
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq.Expressions;
using DarkCrystal.CommandLine;

namespace DarkCrystal.Sample
{
    public class CharacterResolver : BaseArgumentObjectResolver<Character>
    {
        public CharacterResolver(IGlobalObjectResolver parent) : base("self", parent)
        {
            // You can use main argument to make an alias for some data
            AddValueGetter("selfHP", typeof(int), Expression.Field(MainArgument, "HP"));
        }
    }
}
