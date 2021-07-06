using System;
using CMSlib.ConsoleModule;

ModuleManager manager = new();
StandardInputModule input = new("INPUT", 0, 0, Console.WindowWidth / 2, Console.WindowHeight);
StandardInputModule output = new("OUTPUT", Console.WindowWidth / 2, 0, Console.WindowWidth / 2, Console.WindowHeight / 2);
LogModule logging = new("LOGGING", Console.WindowWidth / 2, Console.WindowHeight / 2, Console.WindowWidth / 2, Console.WindowHeight / 2);
manager.Add(input);
manager.Add(output);
manager.Add(logging);
manager.LineEntered += async (@object, args) =>
{
    InputModule? module = @object as InputModule;
    module?.AddText($"{args.Line}");
};
System.Threading.Tasks.Task.Delay(-1).GetAwaiter().GetResult();