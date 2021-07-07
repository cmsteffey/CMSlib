using System;
using CMSlib.ConsoleModule;

ModuleManager manager = new();
StandardInputModule input = new("INPUT", 0, 0, Console.WindowWidth / 2, Console.WindowHeight);
StandardInputModule output = new("OUTPUT", Console.WindowWidth / 2, 0, Console.WindowWidth / 2, Console.WindowHeight / 2);
LogModule logging = new("LOGGING", Console.WindowWidth / 2, Console.WindowHeight / 2, Console.WindowWidth / 2, Console.WindowHeight / 2);

manager.Add(input); //input module, full left side
manager.Add(output); //output module, top right
manager.Add(logging); //logger module, no input, bottom right

while (true)
{
    
    var inputEnteredEventArgs = await input.ReadLineAsync();
    input.AddText(inputEnteredEventArgs.Line);
}

System.Threading.Tasks.Task.Delay(-1).GetAwaiter().GetResult();