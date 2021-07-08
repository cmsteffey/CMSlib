using System;
using CMSlib.ConsoleModule;

ModuleManager manager = new();
StandardInputModule input = new("INPUT", 0, 0, Console.WindowWidth / 2, Console.WindowHeight);
StandardInputModule output = new("OUTPUT", Console.WindowWidth / 2, 0, Console.WindowWidth / 2, Console.WindowHeight / 2);
LogModule logging = new("LOGGING", Console.WindowWidth / 2, Console.WindowHeight / 2, Console.WindowWidth / 2, Console.WindowHeight / 2);
ModulePage page = new();
page.Add(input); //input module, full left side
page.Add(output); //output module, top right
page.Add(logging); //logger module, no input, bottom right
manager.Add(page);
manager.RefreshAll();

while (true)
{
    
    var inputEnteredEventArgs = await input.ReadLineAsync();
    logging.AddText(inputEnteredEventArgs.Line);
    logging.WriteOutput();
}

System.Threading.Tasks.Task.Delay(-1).GetAwaiter().GetResult();