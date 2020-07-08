
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEditor;
using UnityEngine;
using DarkCrystal.CommandLine;

namespace DarkCrystal.Sample
{
    public class CommandLineWindowWithExpectedType : CommandLineWindow
    {
        [MenuItem("Tools/CommandLineWindowWithExpectedType")]
        static void Init()
        {
            (EditorWindow.GetWindow(typeof(CommandLineWindowWithExpectedType)) as CommandLineWindowWithExpectedType).Show();
        }

        protected override void Run()
        {
            try
            {
                var result = CommandLine.Execute<float>(CommandLineText, null);
                OutputText = result.ToString();
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

        protected override void DrawCommandLine()
        {
            CommandLine.Draw<float>(ref CommandLineText, ref OutputText, Resolver);
        }

        protected override void OnGUIInternal()
        {
            GUILayout.Label("Here we expect float, so lines like\n" +
                "`Player.Name` won't work\n\n");
            base.OnGUIInternal();
        }
    }
}