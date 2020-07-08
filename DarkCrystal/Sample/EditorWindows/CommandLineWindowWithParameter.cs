
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEditor;
using UnityEngine;
using DarkCrystal.CommandLine;

namespace DarkCrystal.Sample
{
    public class CommandLineWindowWithParameter : CommandLineWindow
    {
        protected override string DescriptionText =>
            "Type command. Examples:\n" +
            " self.Name\n" +
            " self.Name = \"My Enemy\"\n" +
            " As well as all default resolver options\n\n";
        protected override IGlobalObjectResolver Resolver => ResolversHub.CharacterResolver;
        private int CurrentEnemy;

        [MenuItem("Tools/CommandLineWindowWithParameter")]
        static void Init()
        {
            (EditorWindow.GetWindow(typeof(CommandLineWindowWithParameter)) as CommandLineWindowWithParameter).Show();
        }

        protected override void OnGUIInternal()
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < World.Enemies.Length; i++)
            {
                GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
                if (i == CurrentEnemy)
                {
                    guiStyle.normal.textColor = Color.red;
                }
                if (GUILayout.Button("Enemy " + i, guiStyle))
                    CurrentEnemy = i;
            }

            GUILayout.EndHorizontal();

            base.OnGUIInternal();
        }

        protected override void Run()
        {
            try
            {
                var result = CommandLine.Execute(CommandLineText, World.Enemies[CurrentEnemy], ResolversHub.CharacterResolver);
                OutputText = result?.ToString() ?? "<null>";
            }
            catch (TokenException exception)
            {
                OutputText = exception.ToString();
            }
            catch (Exception exception)
            {
                OutputText = exception.ToString();
            }
        }
    }
}