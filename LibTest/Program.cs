using System;
using System.Dynamic;
using System.Linq;
using System.Runtime.Loader;
using CMSlib.ConsoleModule;
using CMSlib.ConsoleModule.InputStates;

ModuleManager manager = new();
StandardInputModule input = new("INPUT", 0, 0, Console.WindowWidth / 2, Console.WindowHeight - 2);
LogModule output = new("OUTPUT", Console.WindowWidth / 2, 0, Console.WindowWidth / 2, Console.WindowHeight - 2);
TaskBarModule bar = new("NIU", 0, Console.WindowHeight - 1, Console.WindowWidth, 1, 10);
LogModule logging = new("LOGGING", 0,0,Console.WindowWidth, Console.WindowHeight - 2);
ToggleModule toggle = new("TEST1", Math.Max(Console.WindowWidth - 9, 0), 0, 9, 3, true);
ToggleModule toggle2 = new("TEST2", Math.Max(Console.WindowWidth - 9, 0), 3, 9, 3, true);
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
    DisplayName = "1234567890"
};
pageTwo.Add(logging);
pageTwo.Add(bar);
pageTwo.Add(toggle);
pageTwo.Add(toggle2);
manager.Add(pageTwo);
manager.RefreshAll();
input.MouseInputReceived += async (sender, eventArgs) =>
{
    if (eventArgs.InputState is ClickInputState)
    {
        (sender as BaseModule)?.AddText("click!");
        manager.RefreshAll();
    }
};
manager.LineEntered += async (sender, args) =>
{
    (sender as BaseModule)?.AddText(args.Line);
};
System.Threading.Tasks.Task.Delay(-1).GetAwaiter().GetResult();