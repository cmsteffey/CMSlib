using System;
using System.Linq;
using System.Runtime.Loader;
using CMSlib.ConsoleModule;

ModuleManager manager = new();
StandardInputModule input = new("INPUT", 0, 0, Console.WindowWidth / 2, Console.WindowHeight - 1);
LogModule output = new("OUTPUT", Console.WindowWidth / 2, 0, Console.WindowWidth / 2, Console.WindowHeight - 1);
TaskBarModule bar = new("NIU", 0, Console.WindowHeight - 1, Console.WindowWidth, 1, 10);
LogModule logging = new("LOGGING", 0,0,Console.WindowWidth, Console.WindowHeight - 1);
ModulePage pageOne = new()
{
    DisplayName = "IO"
};
pageOne.Add(input); 
pageOne.Add(output); 
pageOne.Add(bar);
manager.Add(pageOne);
ModulePage pageTwo = new()
{
    DisplayName = "LOGGING"
};
pageTwo.Add(logging);
pageTwo.Add(bar);
manager.Add(pageTwo);
manager.RefreshAll();

manager.LineEntered += async (sender, args) =>
{
    (sender as BaseModule)?.AddText(args.Line);
};
System.Threading.Tasks.Task.Delay(-1).GetAwaiter().GetResult();