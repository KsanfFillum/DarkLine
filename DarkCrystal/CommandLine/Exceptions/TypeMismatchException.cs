
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace DarkCrystal.CommandLine
{
    public class TypeMismatchException : System.Exception
    {
        public IEnumerable<Type> ExpectedTypes;
        public IEnumerable<Type> GottenTypes;
        
        public TypeMismatchException(IEnumerable<Type> expectedTypes, IEnumerable<Type> gottenTypes)
        {
            this.ExpectedTypes = expectedTypes;
            this.GottenTypes = gottenTypes;
        }
    }
}