
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace DarkCrystal.Sample
{
    public static class CharacterExtensions
    {
        public static bool IsAlive(this Character character) => character.HP > 0;
        public static string GetTypeName<T>(this T instance) => typeof(T).Name;
        public static float GetCharacterNormalizedHP(Character character) => character.HP / 100f;
    }
}
