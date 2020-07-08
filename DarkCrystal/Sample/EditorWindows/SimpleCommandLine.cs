
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;

namespace DarkCrystal.Sample
{
    public class SimpleCommandLine : CommandLineWindow
    {
        [MenuItem("Tools/CommandLineWindow")]
        static void Init()
        {
            (EditorWindow.GetWindow(typeof(SimpleCommandLine)) as SimpleCommandLine).Show();
        }
    }
}