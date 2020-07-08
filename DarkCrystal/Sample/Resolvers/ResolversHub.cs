
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace DarkCrystal.Sample
{
    public static class ResolversHub
    {
        public static DefaultGlobalResolver DefaultGlobalResolver { get; private set; }
        public static CharacterResolver CharacterResolver { get; private set; }

        static ResolversHub()
        {
            DefaultGlobalResolver = new DefaultGlobalResolver();
            CharacterResolver = new CharacterResolver(DefaultGlobalResolver);
        }
    }
}
