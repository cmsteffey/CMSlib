using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMSlib.Extensions;
using Microsoft.Extensions.Logging;

namespace CMSlib.ConsoleModule
{
    public class ModuleManager : ILoggerFactory
    {
        
        
        private readonly Dictionary<string, Module> modules = new();
        internal readonly List<string> dictKeys = new();
        internal int selected = 0;
        internal object writeLock = new();

        private const string Ctrl = "Control";
        private const string Alt = "Alt";
        private const string Shift = "Shift";

        public ModuleManager()
        {
            Console.TreatControlCAsInput = true;
            Console.CancelKeyPress += (_, _) => { QuitApp(); };
            if (Environment.OSVersion.Platform.ToString().ToLower().Contains("win"))
                new WinConsoleConfiguerer().SetupConsole();
            Console.Write(AnsiEscape.AlternateScreenBuffer);
            Console.Write(AnsiEscape.DisableCursorBlink);
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    var key = Console.ReadKey(true);
                    Module selectedModule = SelectedModule;
                    Module inputModule = selectedModule?.ToInputModule();
                    Module.AsyncEventHandler<KeyEnteredEventArgs> handler = KeyEntered;

                    if (inputModule is not null)
                    {
                        KeyEnteredEventArgs e = new()
                        {
                            Module = inputModule,
                            KeyInfo = key
                        };
                        if(handler is not null)
                            await handler(inputModule, e);
                        inputModule?.FireKeyEntered(e);
                    }

                    try
                    {
                        await HandleKeyAsync(key, selectedModule);
                    }
                    catch (Exception e)
                    {
                        inputModule?.AddText(e.ToString());
                        inputModule?.WriteOutput();
                    }
                }
            });
        }
        
        
        /// <summary>
        /// The currently selected module. Returns null if there is no module currently selected;
        /// </summary>
        
        public Module? SelectedModule
        {
            get { lock(dictSync)return selected == -1 ? null : modules[dictKeys[selected]]; }
        }

        private Queue<Module> loggerQueue = new();
        /// <summary>
        /// The currently selected module that has input enabled. Returns null if there isn't one.
        /// </summary>
        public Module? InputModule
        {
            get
            {
                lock (dictSync)
                    return selected < 0 || selected >= modules.Count ? null : modules[dictKeys[selected]].isInput ? modules[dictKeys[selected]] : null;
            }
        }
        /// <summary>
        /// The title of the current input module
        /// </summary>
        public string? InputModuleTitle
        {
            get => InputModule?.Title;
        }
        
        private readonly object dictSync = new();
        /// <summary>
        /// Refreshes all modules in this manager, ensuring that the latest output is displayed in all of them.
        /// </summary>
        /// <param name="clear">Whether to clear the console before writing.</param>
        public void RefreshAll(bool clear = true)
        {
            if (clear) Console.Clear();
            foreach(string title in modules.Keys)
            {
                RefreshModule(title);
            }

        }
        /// <summary>
        /// Refreshes a module by its title. This ensures that the latest output is displayed.
        /// </summary>
        /// <param name="title">The title of the module to refresh</param>
        /// <returns>Whether this operation was successful. It will not succeed if this manager does not have a module with the supplied title.</returns>
        public bool RefreshModule(string title)
        {
            if (!modules.ContainsKey(title))
            {
                return false;
            }
            modules[title].WriteOutput();

            return true;
        }
        /// <summary>
        /// Removes a module by its title.
        /// </summary>
        /// <param name="title"></param>
        /// <returns>Whether this operation was successful. It will not succeed if this manager does not have a module with the supplied title.</returns>
        public bool RemoveModule(string title)
        {
            //TODO shift selected back down once module is removed
            bool success;
            lock (dictSync)
            {
                dictKeys.Remove(title);
                success = modules.Remove(title);
                if (selected >= dictKeys.Count)
                {
                    selected = -1;
                }
            }
            return success;
        }
        /// <summary>
        /// Adds the supplied text to the module specified.
        /// </summary>
        /// <param name="moduleTitle">The title of the module to add the text to</param>
        /// <param name="text">The text to add to the module</param>
        /// <returns>Whether this operation was successful. It will not succeed if this manager does not have a module with the supplied title.</returns>
        public bool AddToModule(string moduleTitle, string text)
        {
            if(modules.TryGetValue(moduleTitle, out Module module))
            {
                module.AddText(text);
                return true;
            }
            return false;
        }

        
        /// <summary>
        /// Adds a module to the logger queue by title. This queue is accessed when this is called to CreateLogger.
        /// </summary>
        /// <param name="moduleTitle">The title of the module to add to the queue</param>
        /// <returns>Whether this operation was successful. It will not succeed if this manager does not have a module with the supplied title.</returns>
        public bool EnqueueLoggerModule(string moduleTitle)
        {
            if (!modules.TryGetValue(moduleTitle, out Module toQueue)) return false;
            loggerQueue.Enqueue(toQueue);
            return true;
        }
        /// <summary>
        /// Adds a module to this manager.
        /// </summary>
        /// <param name="title">The internal title of the module.
        /// This is used to get the module with the indexer,
        /// as well as used in AddToModule.
        /// The title must be unique between other modules in this manager.</param>
        /// <param name="x">The zero based x coordinate of the module. 0 is the farthest left column.</param>
        /// <param name="y">The zero based y coordinate of the module. 0 is the topmost row.</param>
        /// <param name="width">The width of the module - this includes the border.</param>
        /// <param name="height">The height of the module - this includes the border.</param>
        /// <param name="startingText">This text is put in the output when the module is initialized</param>
        /// <param name="borderChar">This character is used in every character of the border. Leave as null to have a lined border.</param>
        /// <param name="minimumLogLevel">The minimum log level that is outputted when this module is used as an ILogger</param>
        /// <param name="immediateOutput">Whether to immediately call RefreshModule on this module after construction</param>
        /// <returns>Whether the module was successfully created</returns>
        public bool AddModule(string title, int x, int y, int width, int height, string startingText = "", char? borderChar = null, LogLevel minimumLogLevel = LogLevel.Information, bool immediateOutput = true, bool isInput = true)
        {
            if (modules.ContainsKey(title))
            {
                return false;
            }
            lock (dictSync)
            {
                Module toAdd = new(this, title, x, y, width-2, height-2, startingText, isInput, borderChar, minimumLogLevel);
                modules.Add(title, toAdd);
                dictKeys.Add(title);
                if (dictKeys.Count - 1 == selected)
                    toAdd.selected = true;
                if (immediateOutput) RefreshModule(title);
            }
            return true;
        }
        
        /// <summary>
        /// Gets a module by title
        /// </summary>
        /// <param name="title">The title of the module to get</param>
        public Module this[string title] => GetModule(title);
        /// <summary>
        /// Gets a module by order created/index in the internal list
        /// </summary>
        /// <param name="index">The zero-based index of the module</param>
        public Module this[int index] =>  modules[dictKeys[index]];
        /// <summary>
        /// Gets a module by title
        /// </summary>
        /// <param name="title">The title of the module to get</param>
        /// <returns>The module with that title</returns>
        public Module GetModule(string title)
        {
            if (!modules.ContainsKey(title))
                throw new KeyNotFoundException($"There is no module with the title of {title}");
            return modules[title];
        }

        /// <summary>
        /// Event fired when a line is entered in any module.
        /// </summary>
        public event Module.AsyncEventHandler<LineEnteredEventArgs> LineEntered;
        /// <summary>
        /// Event fired when a key is pressed.
        /// </summary>
        public event Module.AsyncEventHandler<KeyEnteredEventArgs> KeyEntered;

        /// <summary>
        /// Tries to get the next queued module to be used as a logger, and if the queue is empty return the input module.
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Thrown when there are no modules created yet, and none in the queue.</exception>

        public ILogger CreateLogger(string categoryName)
        {
            lock (dictSync)
            {
                if (loggerQueue.TryDequeue(out Module next))
                {
                    return next;
                }
                else if (modules.Count > 0)
                {
                    return this.modules.First().Value;
                }
                else
                {
                    throw new Exception("No modules created, and no modules queued.");
                }
            }
        }

        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotImplementedException();
        }
        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            dictKeys.Clear();
            modules.Clear();
            this.RefreshAll();
        }
        //todo add input for this too
        /// <summary>
        /// Selects the next module - enables scrolling for that module.
        /// </summary>
        public void SelectNext()
        {
            int newSelected;
            int pastSelected;
            bool refreshNew;
            bool refreshPast;
            lock (dictSync)
            {
                pastSelected = selected;
                selected++;
                newSelected = (++selected).Modulus(dictKeys.Count + 1) - 1;
                selected = newSelected;
                refreshPast = pastSelected >= 0;
                if (refreshPast)
                {
                    modules[dictKeys[pastSelected]].selected = false;
                }
                
                refreshNew = newSelected >= 0;
                if (refreshNew)
                {
                    modules[dictKeys[newSelected]].selected = true;
                }
            }
            if(refreshPast)
                modules[dictKeys[pastSelected]].WriteOutput();
            if(refreshNew)
                modules[dictKeys[newSelected]].WriteOutput();
        }
        /// <summary>
        /// Selects the previous module - enables scrolling for that module.
        /// </summary>
        public void SelectPrev()
        {
            int newSelected;
            int pastSelected;
            bool refreshNew;
            bool refreshPast;
            lock (dictSync)
            {
                
                pastSelected = selected;
                newSelected = selected.Modulus(dictKeys.Count + 1) - 1;

                selected = newSelected;
                refreshPast = pastSelected >= 0;
                if (refreshPast)
                {
                    modules[dictKeys[pastSelected]].selected = false;
                }

                refreshNew = newSelected >= 0;
                if (refreshNew)
                {
                    modules[dictKeys[newSelected]].selected = true;
                }
            }
            if(refreshPast)
                modules[dictKeys[pastSelected]].WriteOutput();
            if(refreshNew)
                modules[dictKeys[newSelected]].WriteOutput();
        }
        /// <summary>
        /// Quits the app, properly returning to the main buffer and clearing all possible cursor/format options.
        /// </summary>
        public static void QuitApp()
        {
            Console.Write(AnsiEscape.MainScreenBuffer);
            Console.Write(AnsiEscape.SoftReset);
            Console.Write(AnsiEscape.EnableCursorBlink);
            Environment.Exit(0);
        }

        public async Task HandleKeyAsync(ConsoleKeyInfo key, Module selectedModule)
        {
            Dictionary<string, bool> mods = key.Modifiers.ToStringDictionary<ConsoleModifiers>();
            if (mods[Alt])
                return;
            Module inputModule = selectedModule?.ToInputModule();
            switch (key.Key)
            {
                case ConsoleKey.RightArrow:
                    break;
                case ConsoleKey.LeftArrow:
                    break;
                case ConsoleKey.End when mods[Ctrl]:
                    selectedModule?.ScrollTo(0);
                    break;
                case ConsoleKey.Home when mods[Ctrl]:
                    selectedModule?.ScrollTo(int.MaxValue);
                    break;
                case ConsoleKey.PageUp:
                    selectedModule?.ScrollUp((selectedModule.Height - (selectedModule.isInput ? 2 : 0)));
                    break;
                case ConsoleKey.PageDown:
                    selectedModule?.ScrollDown((selectedModule.Height - (selectedModule.isInput ? 2 : 0)));
                    break;
                case ConsoleKey.UpArrow when mods[Ctrl]:
                    selectedModule?.ScrollUp(1);
                    break;
                case ConsoleKey.DownArrow when mods[Ctrl]:
                    selectedModule?.ScrollDown(1);
                    break;
                case ConsoleKey.Tab when mods[Shift]:
                    this.SelectPrev();
                    break;
                case ConsoleKey.Tab:
                    this.SelectNext();
                    break;
                case ConsoleKey.C when mods[Ctrl]:
                    ModuleManager.QuitApp();
                    break;
                case ConsoleKey.Enter when mods[Shift]:
                    await EnterLineAsync(inputModule, false);
                    break;
                case ConsoleKey.Enter:
                    await EnterLineAsync(inputModule, true);
                    return;
                case ConsoleKey.Backspace when inputModule?.inputString.Length.Equals(0) ?? false:
                    return;
                case ConsoleKey.Backspace when mods[Ctrl]:
                    goto NotImpl;
                    //TODO fix this
                    NotImpl:
                    break;
                case ConsoleKey.Backspace:
                    if (inputModule is null) return;
                    lock (this.writeLock)
                    {
                        inputModule.inputString.Remove(inputModule.inputString.Length - 1, 1);
                        inputModule.lrCursorPos--;
                        Console.Write("\b \b");
                    }
            
                    return;
                default:
                    if (inputModule is null) return;
                    if (key.KeyChar == '\u0000') return;
                    if (inputModule.inputString.Length < inputModule.Width)
                    {
                        lock (this.writeLock)
                        {
                            inputModule.inputString.Append(key.KeyChar);
                            Console.Write(key.KeyChar);
                            inputModule.lrCursorPos++;
                        }
                    }
                    break;
            }
        }

        public async Task EnterLineAsync(Module inputModule, bool scrollToBottom)
        {
            if (inputModule is null) return;
            string line;
            Module.AsyncEventHandler<LineEnteredEventArgs> handler;
            lock (this.writeLock)
            {
                handler = LineEntered;
                line = inputModule.inputString.ToString();
                inputModule.inputString.Clear();
                inputModule.lrCursorPos = 0;
                if (scrollToBottom)
                {
                    inputModule.scrolledLines = 0;
                    inputModule.unread = false;
                }
            }
            var e = new LineEnteredEventArgs()
            {
                Module = inputModule,
                Line = line
            };
            if (handler != null)
                await handler(inputModule, e);
            inputModule.FireLineEntered(e);
            inputModule.WriteOutput();
        }
    }
    /// <summary>
    /// EventArgs for the LineEntered Event
    /// </summary>
    public class LineEnteredEventArgs : EventArgs{
        internal LineEnteredEventArgs(){}
        /// <summary>
        /// The line that was inputted.
        /// </summary>
        public string Line { get; internal init; }
        
        public Module Module { get; internal init; }
    }
    /// <summary>
    /// EventArgs for the KeyEntered Event
    /// </summary>
    public class KeyEnteredEventArgs : EventArgs{
        internal KeyEnteredEventArgs(){}
        /// <summary>
        /// Info about the key pressed.
        /// </summary>
        public ConsoleKeyInfo KeyInfo { get; internal init; }
        
        public Module Module { get; internal init; }
    }
}
