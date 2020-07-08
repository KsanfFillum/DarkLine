
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace DarkCrystal.CommandLine
{
    public class GlobalObjectResolver : BaseGlobalObjectResolver, IArgumentlessResolver
    {
        public GlobalObjectResolver(IGlobalObjectResolver parent = null) : base(parent) { }
        public override Delegate GetThrowerDelegate(Exception ex) => (Action)(() => throw ex);
    }
}