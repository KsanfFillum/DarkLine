
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEditor;
using UnityEngine;
using DarkCrystal.CommandLine;

namespace DarkCrystal.Sample
{
    public class CommandLineWindow : EditorWindow
    {
        protected virtual string DescriptionText =>
            "Type command. Examples:\n" +
            " Player.HP\n" +
            " Player.HP = 14 + 3 * 2\n" +
            " World.Enemies[2].GetPartInfo(PartType.Head)\n" +
            " Player.GetTypeName()\n" +
            " Player.IsAlive()\n" +
            " CharacterUtils.GetCharacterNormalizedHP(Player)\n\n";
        protected virtual IGlobalObjectResolver Resolver => null; // null fallback to default

        protected CommandLine.CommandLine CommandLine;
        protected string CommandLineText;
        protected string OutputText;

        private void OnEnable()
        {
            CommandLine = new CommandLine.CommandLine(ResolversHub.DefaultGlobalResolver);
        }

        private void OnGUI()
        {
            OnGUIInternal();
        }

        protected virtual void OnGUIInternal()
        {
            GUILayout.Label(DescriptionText);

            DrawCommandLine();
            if (OutputText == null)
            {
                OutputText = String.Format("<color=green>{0}</color>\nWell formed", CommandLineText);
            }

            var style = new GUIStyle(GUI.skin.label);
            style.richText = true;
            GUILayout.TextArea(OutputText, style);
            if (GUILayout.Button("Run"))
            {
                Run();
            }
        }

        protected virtual void DrawCommandLine()
        {
            CommandLine.Draw(ref CommandLineText, ref OutputText, Resolver);
        }

        protected virtual void Run()
        {
            try
            {
                var result = CommandLine.Execute(CommandLineText, null);
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