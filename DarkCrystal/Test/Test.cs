
// Copyright (c) Dark Crystal Games. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using DarkCrystal.CommandLine;
using DarkCrystal.Sample;

using UnityEditor;

using UnityEngine;

namespace DarkCrystal.Test
{
    public static class Test
    {
        private const int TestsCount = 10000;

        private const string SimpleTestLine = "Player.Name";
        private const string ComplexTestLine = "Enemies[0].GetPartInfo(PartType.Hands)";
        private const string AgrumentTestLine = "self.GetPartInfo(PartType.Hands)";

        [MenuItem("Tools/TimeTest")]
        public static void TestFunction()
        {
            Debug.LogWarning("-----------------  Warm Up!  -----------------");
            InternalTest("Enemies[0].Name", ResolversHub.DefaultGlobalResolver);
            InternalTest("self.Name", ResolversHub.CharacterResolver, World.Enemies[0]);

            Debug.LogWarning("Reference Values:");
            var name = World.Player.Name;
            var before = DateTime.Now;
            for (int i = 0; i < TestsCount; i++)
            {
                name = World.Player.Name;
            }
            var after = DateTime.Now;
            Debug.LogWarningFormat("Simple Test: {0} ticks", (after - before).Ticks / (double)TestsCount);

            name = World.Enemies[0].GetPartInfo(PartType.Hands);
            before = DateTime.Now;
            for (int i = 0; i < TestsCount; i++)
            {
                name = World.Enemies[0].GetPartInfo(PartType.Hands);
            }
            after = DateTime.Now;
            Debug.LogWarningFormat("Compex and argument Tests: {0} ticks", (after - before).Ticks / (double)TestsCount);


            Debug.LogWarning("-----------------  SimpleTest1!  -----------------");
            InternalTest(SimpleTestLine, ResolversHub.DefaultGlobalResolver);

            Debug.LogWarning("-----------------  SimpleTest2!  -----------------");
            InternalTest(SimpleTestLine, ResolversHub.DefaultGlobalResolver);

            Debug.LogWarning("-----------------  ComplexTest1!  -----------------");
            InternalTest(ComplexTestLine, ResolversHub.DefaultGlobalResolver);

            Debug.LogWarning("-----------------  ComplexTest2!  -----------------");
            InternalTest(ComplexTestLine, ResolversHub.DefaultGlobalResolver);

            Debug.LogWarning("-----------------  AgrumentTest1!  -----------------");
            InternalTest(AgrumentTestLine, ResolversHub.CharacterResolver, World.Enemies[0]);

            Debug.LogWarning("-----------------  AgrumentTest2!  -----------------");
            InternalTest(AgrumentTestLine, ResolversHub.CharacterResolver, World.Enemies[0]);
        }

        private static void InternalTest(string line, IArgumentlessResolver resolver)
        {
            var commandLine = new CommandLine.CommandLine(ResolversHub.DefaultGlobalResolver);
            var currentTime = DateTime.Now;

            commandLine.Execute(line, resolver);
            var firstExeccutionTime = DateTime.Now;

            for(int i = 0; i < TestsCount; i++)
            {
                commandLine.Execute(line, resolver);
            }
            var totalExecutionTime = DateTime.Now;

            var hundredExecutionsTime = ((firstExeccutionTime - currentTime).Ticks + (totalExecutionTime - firstExeccutionTime).Ticks / (double)TestsCount * 99);

            Debug.Log("Test Results:");
            Debug.LogFormat("Compilation Time: {0} ticks", (firstExeccutionTime - currentTime).Ticks);
            Debug.LogFormat("Execution Time (median for {0} Tests): {1} ticks", TestsCount, (totalExecutionTime - firstExeccutionTime).Ticks / (double)TestsCount);
            Debug.LogFormat("Total Time for 100 executions: {0} ticks; median for 1 execution: {1} ticks",
                hundredExecutionsTime,
                hundredExecutionsTime / 100);
        }

        private static void InternalTest<T>(string line, IArgumentResolver<T> resolver, T arg)
        {
            var commandLine = new CommandLine.CommandLine(ResolversHub.DefaultGlobalResolver);
            var currentTime = DateTime.Now;

            commandLine.Execute(line, arg, resolver);
            var firstExeccutionTime = DateTime.Now;

            for (int i = 0; i < TestsCount; i++)
            {
                commandLine.Execute(line, arg, resolver);
            }
            var totalExecutionTime = DateTime.Now;

            var hundredExecutionsTime = ((firstExeccutionTime - currentTime).Ticks + (totalExecutionTime - firstExeccutionTime).Ticks / (double)TestsCount * 99);

            Debug.Log("Test Results:");
            Debug.LogFormat("Compilation Time: {0} ticks", (firstExeccutionTime - currentTime).Ticks);
            Debug.LogFormat("Execution Time (median for {0} Tests): {1} ticks", TestsCount, (totalExecutionTime - firstExeccutionTime).Ticks / (double)TestsCount);
            Debug.LogFormat("Total Time for 100 executions: {0} ticks; median for 1 execution: {1} ticks",
                hundredExecutionsTime,
                hundredExecutionsTime / 100);
        }
    }
}
