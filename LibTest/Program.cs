using System;
using CMSlib.ConsoleModule;

ModuleManager manager = new();
manager.AddModule("INPUT", 0, 0, Console.WindowWidth / 2, Console.WindowHeight);
manager.AddModule("OUTPUT", Console.WindowWidth / 2, 0, Console.WindowWidth / 2, Console.WindowHeight / 2);
manager.AddModule("LOGGING", Console.WindowWidth / 2, Console.WindowHeight / 2, Console.WindowWidth / 2, Console.WindowHeight / 2);
manager.LineEntered += async (@object, args) =>
{
    Module? module = @object as Module;
    module?.AddText($"{args.Line}");
};
System.Threading.Tasks.Task.Delay(-1).GetAwaiter().GetResult();