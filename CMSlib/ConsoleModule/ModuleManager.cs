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

        public ModuleManager()
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    var key = Console.ReadKey(true);
                    await HandleKeyAsync(key);
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
                    return selected == -1 ? null : modules[dictKeys[selected]].isInput ? modules[dictKeys[selected]] : null;
            }
        }
        /// <summary>
        /// The title of the current input module
        /// </summary>
        public string? InputModuleTitle
        {
            get { lock(dictSync) return dictKeys.Count > 0 ? dictKeys[0] : null; }
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
            bool success;
            lock (dictSync)
            {
                dictKeys.Remove(title);
                success = modules.Remove(title);
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
        public bool AddModule(string title, int x, int y, int width, int height, string startingText = "", char? borderChar = null, LogLevel minimumLogLevel = LogLevel.Information, bool immediateOutput = true)
        {
            if (modules.ContainsKey(title))
            {
                return false;
            }
            lock (dictSync)
            {
                Module toAdd = new(this, title, x, y, width-2, height-2, startingText, modules.Count == 0, borderChar, minimumLogLevel);
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
        /// Event fired when a line is entered, by pressing enter when an input module is focused
        /// </summary>
        public event Module.AsyncEventHandler<LineEnteredEventArgs> LineEntered;
        //todo Abstract to key when any module is inputmodule
        /// <summary>
        /// Event fired when a key is pressed
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
            if (loggerQueue.TryDequeue(out Module next))
            {
                return next;
            }
            else if(modules.Count > 0)
            {
                return this.InputModule;
            }
            else
            {
                throw new Exception("No modules created, and no modules queued.");
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
                newSelected = ++selected % (dictKeys.Count + 1) - 1;
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
                newSelected = selected % (dictKeys.Count + 1) - 1;

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

        public async Task HandleKeyAsync(ConsoleKeyInfo key)
        {
            if (key.Modifiers.HasFlag(ConsoleModifiers.Alt))
                            return;
            
            switch (key.Key)
            {
                case ConsoleKey.RightArrow:
                    break;
                case ConsoleKey.LeftArrow:
                    break;
                case ConsoleKey.PageUp:
                    this.SelectedModule?.ScrollUp((SelectedModule.height - (SelectedModule.isInput ? 2 : 0)));
                    break;
                case ConsoleKey.PageDown:
                    this.SelectedModule?.ScrollDown((SelectedModule.height - (SelectedModule.isInput ? 2 : 0)));
                    break;
                case ConsoleKey.UpArrow when key.Modifiers.HasFlag(ConsoleModifiers.Shift):
                    this.SelectedModule?.ScrollUp(1);
                    break;
                case ConsoleKey.DownArrow when key.Modifiers.HasFlag(ConsoleModifiers.Shift):
                    this.SelectedModule?.ScrollDown(1);
                    break;
                case ConsoleKey.Tab when key.Modifiers.HasFlag(ConsoleModifiers.Shift):
                    this.SelectPrev();
                    break;
                case ConsoleKey.Tab:
                    this.SelectNext();
                    break;
                case ConsoleKey.C when key.Modifiers.HasFlag(ConsoleModifiers.Control):
                    ModuleManager.QuitApp();
                    break;
                case ConsoleKey.Enter:
                    if (InputModule is null) return;
                    string line;
                    Module.AsyncEventHandler<LineEnteredEventArgs> handler;
                    lock (this.writeLock)
                    {
                        handler = LineEntered;
                        line = InputModule.inputString.ToString();
                        InputModule.inputString.Clear();
                        InputModule.lrCursorPos = 0;
                        InputModule.scrolledLines = 0;
                        InputModule.unread = false;
                    }
                    if (handler != null)
                    {
                        var e = new LineEnteredEventArgs(line);
                        await handler(InputModule, e);
                    }
                    this.InputModule.WriteOutput();
                    return;
                case ConsoleKey.Backspace when InputModule?.inputString.Length.Equals(0) ?? false:
                    return;
                case ConsoleKey.Backspace when key.Modifiers.HasFlag(ConsoleModifiers.Control):
                    goto NotImpl;
                    if (InputModule is null) return;
                    bool? isPrevSpace = InputModule.inputString[^1] == ' ';
                    int i;
                    for (i = InputModule.inputString.Length - 2; i >= 0; i--)
                    {
            
                        if (isPrevSpace.Value && InputModule.inputString[i] != ' ')
                            break;
                        if (InputModule.inputString[i] == ' ' && !isPrevSpace.Value)
                            isPrevSpace = true;
                    }
            
                    //TODO fix this
                    NotImpl:
                    break;
                case ConsoleKey.Backspace:
                    if (InputModule is null) return;
                    lock (this.writeLock)
                    {
                        InputModule.inputString.Remove(InputModule.inputString.Length - 1, 1);
                        InputModule.lrCursorPos--;
                        Console.Write("\b \b");
                    }
            
                    return;
                default:
                    if (InputModule is null) return;
                    if (key.KeyChar == '\u0000') return;
                    if (InputModule.inputString.Length < InputModule.width)
                    {
                        lock (this.writeLock)
                        {
                            InputModule.inputString.Append(key.KeyChar);
                            Console.Write(key.KeyChar);
                            InputModule.lrCursorPos++;
                        }
                    }
                    break;
            }
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
        public string Line { get; }
        
        internal LineEnteredEventArgs(string line)
        {
            this.Line = line;
        }
    }
    /// <summary>
    /// EventArgs for the KeyEntered Event
    /// </summary>
    public class KeyEnteredEventArgs : EventArgs{
        internal KeyEnteredEventArgs(){}
        /// <summary>
        /// Info about the key pressed.
        /// </summary>
        public ConsoleKeyInfo KeyInfo { get; }
        
        internal KeyEnteredEventArgs(ConsoleKeyInfo key)
        {
            KeyInfo = key;
        }
    }
}
