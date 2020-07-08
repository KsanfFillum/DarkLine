
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace DarkCrystal
{
    public class TypeHash<T>
    {
        public static readonly int Hash = typeof(T).GetHashCode();
    }
}
