using System;
using System.Linq;
using System.Text;
using CMSlib.CollectionTypes;
using CMSlib.ConsoleModule;
using CMSlib.ConsoleModule.InputStates;
using CMSlib.Extensions;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

ModuleManager manager = new(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new WinTerminal() : new StdTerminal());
StandardInputModule input = new("INPUT", 0, 0, Console.WindowWidth / 2, Console.WindowHeight - 2);
LogModule output = new("OUTPUT", Console.WindowWidth / 2, 0, Console.WindowWidth / 2, Console.WindowHeight - 2);
TaskBarModule bar = new("NIU", 0, Console.WindowHeight - 2, Console.WindowWidth, 2, 10);
ToggleModule toggle = new("Color", Math.Max(Console.WindowWidth - 12, 0), 0, 12, 3, true, "Green", "Red");
ButtonModule btn = new("Button!", Math.Max(Console.WindowWidth - 12, 0), 3, 12, 3, "Clear"){DisplayName = ""};

ModulePage pageOne = new()
{
    DisplayName = "IO"
};
pageOne.Add(input); 
pageOne.Add(output); 
pageOne.Add(bar);
manager.Add(pageOne);
ModulePage pageThree = new("Canvas")
{
    new CanvasModule("Canvas", 0, 0, Console.WindowWidth, Console.WindowHeight - bar.Height),
    toggle,
    btn,
    bar
};
manager.Add(pageThree);
manager.RefreshAll();
int count = 0;
FifoBuffer<int> ints = new FifoBuffer<int>(10);
(int x, int y) cachedCoords = (-1, -1);
pageThree.First().MouseInputReceived += async (sender, eventArgs) =>
{
    
    if (eventArgs.InputState is ClickInputState state && state.MouseCoordinates != cachedCoords)
    {
        CanvasModule module = sender as CanvasModule;
        module?.SetCell(state.MouseCoordinates.X - 1 - module.X, state.MouseCoordinates.Y - 1 - module.Y, toggle.Enabled ? AnsiEscape.SgrBrightGreenBackGround + " " : AnsiEscape.SgrBrightRedBackGround + " ");
        module?.QuickWriteOutput();
        count++;
        cachedCoords = state.MouseCoordinates;
        
    }
};

manager.LineEntered += async (sender, args) =>
{
    StringBuilder builder = new();
    int visLen = args.Line.VisibleLength();
    builder.Append(AnsiEscape.LineDrawingMode).Append(AnsiEscape.UpperLeftCorner)
        .Append(AnsiEscape.HorizontalLine, visLen).Append(AnsiEscape.UpperRightCorner).Append('\n');
    builder.Append(AnsiEscape.LineDrawingMode).Append(AnsiEscape.VerticalLine).Append(AnsiEscape.AsciiMode)
        .Append(args.Line).Append(AnsiEscape.LineDrawingMode).Append(AnsiEscape.VerticalLine).Append('\n');
    builder.Append(AnsiEscape.LineDrawingMode).Append(AnsiEscape.LowerLeftCorner)
        .Append(AnsiEscape.HorizontalLine, visLen).Append(AnsiEscape.LowerRightCorner);
    (sender as BaseModule)?.AddText(builder);
    (sender as BaseModule)?.WriteOutput();
};
btn.Clicked += async (sender, args) =>
{
    pageThree.FirstOrDefault()?.As<CanvasModule>()?.Clear();
    manager.RefreshAll(false);
};
System.Threading.Tasks.Task.Delay(-1).GetAwaiter().GetResult();
