# DarkerLine

Command line with autocomplete for C# or Unity projects

Proud fork of DarkCrystal.DarkLine, originally placed in
https://github.com/Alprog/DarkLine/tree/master

Mainly used with Unity3D, but core code has no limitation to use it outside.

## Origin Features

* Execute void functions return values
* Binding typenames, enums and values
* Access to all class fields, properties and methods
* Autocomplete helpers
* Validation by syntax errors and specified return type 
* Math and Boolean operations, brackets

## Fork Features

* Using delegates and cache. Originally, that command line used Lexer for each call and it generated a lot of garbage. Moreover, execution time was dramatically depended from line lenght. Now it takes a bit more time to compilation, but it really fast to execute it after.
* Nested classed access
* Namespace binding
* [] operator access (for example, array/list single index access)
* Extension methods access
* Deduceable generic methods access
* Easy type converting

## Arguments

Original version only used static getters, so you could bind static members to get a value. Now it allowed to bind pass argument as execution context and use one resolver for different instance of same type.

## Technology

I used Linq.Expression for runtime compilation. It allows to make complex dynamic expressions, compile them and use as delegates. It takes a bit more time for compilation, but running time comparative to the simple code syntax and allows to cache the result.

## Limitation

Namespaces, Generics and Extensions all depends from assembly. If you do not use multiple assemblies or command line code is part of the main one it is ok, but if you do, you have to enumerate your assemblies inside `DarkExtensions.GetTypes()`.

Each instance of command line use its own cache, so I encourage you to use command line as Singleton. 

## How to use

Copy whole code into Unity Project. You can open 
* Tools/CommandLineWindow
* Tools/CommandLineWindowWithParameter
* Tools/CommandLineWindowWithExpectedType
and see the difference between them.

Good start point will be folder `Sample/Resolvers`. 

Simple start kit is:
* Define DefaultResolver. It should be argumentless, good practice to inherent it from `GlobalObjectResolver`. You just have to add all bindings into constructor.
* Define command line's entry point. It could be hub or singleton, you name it.

And that's all. You can execute your lines with `CommandLine.Execute()`, Validate with `CommandLine.Validate()` and get Autocomplete hints with `CommandLine.Autocomplete()`.

Some example commands for sample:
* Player.HP 						// Field read access
* Player.HP = Player.HP - 10 			// Field write access
* Enemies[2].GetPartInfo(PartType.Head)	// Method call
* Player.IsAlive()					// Extension method
* Player.GetTypeName()				// Generic Extension

Example commands for CMD with parameter (as well as all up):
* self.HP
* self.HP = self.HP - 10
* self.IsAlive()
* self.GetTypeName()