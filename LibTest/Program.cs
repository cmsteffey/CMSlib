using System;
using CMSlib.ConsoleModule;

ModuleManager manager = new();
StandardInputModule input = new("INPUT", 0, 0, Console.WindowWidth / 2, Console.WindowHeight);
LogModule output = new("OUTPUT", Console.WindowWidth / 2, 0, Console.WindowWidth / 2, Console.WindowHeight);
LogModule logging = new("LOGGING", 0,0,Console.WindowWidth, Console.WindowHeight);
ModulePage pageOne = new();
pageOne.Add(input); //input module, full left side
pageOne.Add(output); //output module, top right
manager.Add(pageOne);
ModulePage pageTwo = new();
pageTwo.Add(logging);
manager.Add(pageTwo);
manager.RefreshAll();

while (true)
{
    
    var inputEnteredEventArgs = await input.ReadLineAsync();
    logging.AddText(inputEnteredEventArgs.Line);
    logging.WriteOutput();
}

System.Threading.Tasks.Task.Delay(-1).GetAwaiter().GetResult();