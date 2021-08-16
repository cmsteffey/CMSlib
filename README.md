# CMSlib
CMSlib (Console Module System) is a c# library for making graphical user interfaces, all inside a console window!\

The structure of a standard program goes as follows:\
ModuleManager instance\
|-ModulePage instance\
|-BaseModule instance\
  |-...\
  |-BaseModule instance\
  |-TaskBarModule instance\
|-ModulePage instance\
  |-BaseModule instance\
  |-...\
  |-BaseModule instance\
  |-TaskBarModule instance (same reference as in the first ModulePage)\
 
ModuleManager is what controls all drawing, and it is what handles input.\
Only call its constructor *once* in your program, and before you need to do any graphics.\
If you have redirected input, either due to running through a linux `nohup` or from your program being a child process, *don't* call the ModuleManager constructor.

Very important note - DO NOT call Console.ReadLine, Console.Read, Console.ReadKey, Console.Write, or Console.WriteLine after you call the ModuleManager constructor.\
Instead, use BaseModule#AddText on Module instances for writing, and ModuleManager#ReadLineAsync() or InputModule.ReadLineAsync() for reading input.\
ModuleManager also has LineEntered and KeyEntered events for handling input, if you prefer event based rather than input loops.

The required parameter of ModuleManager's constructor is an object that implements ITerminal - This is what is used to handle all display, I/O, pre-display setup, and other console functions.\
The library as it is comes with two ITerminals - WinTerminal and StdTerminal. WinTerminal should be used when Windows is being used to run the program. It handles win32 api calls to properly setup the terminal to render line drawing, as well as other changes so that the graphics render properly. It also handles click and mouse events, so if you want mouse interface on Windows, use Winterminal. StdTerminal is a wrapper of the standard System.Console static class - it gets input through Console.ReadKey, and uses Console.Write to handle output. This should be used for all non-Windows applications.
If someone wants to make a linux specific terminal that uses /dev/input for mouse clicks, or an IOKit terminal to handle mouse input on Macs, feel free, but I'm only supporting Windows mouse input at the moment.

The main members of ModuleManager:\
Add(ModulePage):\
  This is how you add pages to your ModuleManager. ModulePages contain modules, and pages are viewed one at a time.\
  (Note - ModuleManager also supports collection initializers of ModulePages)\
RefreshAll(bool):\
  This optionally clears the screen, then redraws all modules on the current page.\ 
  Modules individually have their own WriteOutput, which is faster to use when you only need to redraw one module.\
InputModule:
  This gets the BaseModule that is currently selected and supports input. This property's value changes when a different module is selected (either by Tab key press, or mouse click), or a different page is selected. When a Module that doesn't inherit InputModule is selected, or no Module is selected at all, this property returns `null`.\
SelectedModule:\
  This gets the BaseModule that is currently selected. This property's value changes when a different Module is selected (either by Tab key press, or mouse click),
  or a different page is selected. When no Module is selected, this property returns `null`.

The main members of ModulePage:\
Add(BaseModule):\
  This is how you add modules to a page. When using the Tab key, Modules are selected in the order that they are added to the currently viewed page, or if\
  the collection initializer is used, they are selected in the order that they appear in the collection initializer.\
DisplayName:\
  This string is intended to represent the page. It doesn't do anything by itself, but should be used in modules that need a representation of the page.\
  This string is displayed on a TaskbarModule to represent the page. If this property is not set, the index of this page + 1 is rendered in the TaskbarModule.

The main members of BaseModule:\
X, Y, Width, and Height\
  These get the dimensions and location of the module. When writing Module classes, returned line width from ToOutputLines should never be wider than Width, and the number of lines returned should never exceed Height.\
Title:\
  This string is used to represent the module, and is displayed on the module on all default modules, and should be visible on all modules (except TaskBarModule)\
AddText(string):\
  This adds a line (or when using \n in the string, lines) of text to the module. It does not redraw the module however, and WriteOutput() should be called on the module when using AddText().\
WriteOutput():\
  This redraws the module. It should be called as few times as possible, while not leaving the module with undrawn output.\
Log(...)\
  This method is inherited from ILogger, so BaseModules can be used as loggers.
