using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CMSlib.Extensions;
using Microsoft.Extensions.Logging;

namespace CMSlib.ConsoleModule
{
    public sealed class ModuleManager : ILoggerFactory, IEnumerable<ModulePage>
    {
        public List<ModulePage> Pages { get; } = new();
        internal int selected = 0;
        internal object writeLock = new();
        
        private readonly object dictSync = new();

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
                    BaseModule selectedModule = SelectedModule;
                    InputModule inputModule = selectedModule as InputModule;
                    AsyncEventHandler<KeyEnteredEventArgs> handler = KeyEntered;

                    if (inputModule is not null)
                    {
                        KeyEnteredEventArgs e = new()
                        {
                            Module = inputModule,
                            KeyInfo = key
                        };
                        if(handler is not null)
                            await handler(inputModule, e);
                        await inputModule.FireKeyEnteredAsync(e);
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

        public void Add(ModulePage page)
        {
            lock (dictSync)
            {
                page.SetParent(this);
                Pages.Add(page);
                
            }
        }

        public IEnumerator<ModulePage> GetEnumerator()
        {
            return Pages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        

        /// <summary>
        /// The currently selected module. Returns null if there is no module currently selected;
        /// </summary>

        public BaseModule? SelectedModule

            => Pages[selected].SelectedModule;
        

        private Queue<BaseModule> loggerQueue = new();

        /// <summary>
        /// The currently selected module that has input enabled. Returns null if there isn't one.
        /// </summary>
        public BaseModule? InputModule => SelectedModule as InputModule;

        public ModulePage? SelectedPage
        {
            get { lock(dictSync) return selected < 0 || selected >= Pages.Count ? null : Pages[selected]; }
        }

        /// <summary>
        /// The title of the current input module
        /// </summary>
        public string? InputModuleTitle
        {
            get => InputModule?.Title;
        }
        /// <summary>
        /// Refreshes all modules in this manager, ensuring that the latest output is displayed in all of them.
        /// </summary>
        /// <param name="clear">Whether to clear the console before writing.</param>
        public void RefreshAll(bool clear = true)
        {
            if (clear) Console.Clear();
            ModulePage selectedPage = SelectedPage;
            if (selectedPage is null) return;
            foreach (BaseModule module in selectedPage)
            {
                module.WriteOutput();
            }
        }
        /// <summary>
        /// Refreshes a module by its title. This ensures that the latest output is displayed.
        /// </summary>
        /// <param name="title">The title of the module to refresh</param>
        /// <returns>Whether this operation was successful. It will not succeed if this manager does not have a module with the supplied title.</returns>
        public bool RefreshModule(string title)
        {
            BaseModule module = GetModule(title);
            if (module is null) return false;
            module.WriteOutput();
            return true;
        }
        /// <summary>
        /// Removes a module by its title.
        /// </summary>
        /// <param name="title"></param>
        /// <returns>Whether this operation was successful. It will not succeed if this manager does not have a module with the supplied title.</returns>
        public bool RemoveModule(string title)
        {
            return false;
            //TODO shift selected back down once module is removed
            /*
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
            */

        }
        
        
        /// <summary>
        /// Adds a module to the logger queue by title. This queue is accessed when this is called to CreateLogger.
        /// </summary>
        /// <param name="moduleTitle">The title of the module to add to the queue</param>
        /// <returns>Whether this operation was successful. It will not succeed if this manager does not have a module with the supplied title.</returns>
        public bool EnqueueLoggerModule(string moduleTitle)
        {
            BaseModule module = GetModule(moduleTitle);
            if(module is null) return false;
            loggerQueue.Enqueue(module);
            return true;
        }
        /// <summary>
        /// Adds a module to this manager.
        /// </summary>
        
        
        /// <summary>
        /// Gets a module by title
        /// </summary>
        /// <param name="title">The title of the module to get</param>
        public BaseModule this[string title] => GetModule(title);
        /// <summary>
        /// Gets a module by title
        /// </summary>
        /// <param name="title">The title of the module to get</param>
        /// <returns>The module with that title</returns>
        public BaseModule? GetModule(string title)
        {
            return Pages.FirstOrDefault(x => x.ContainsTitle(title))?[title];
        }

        /// <summary>
        /// Event fired when a line is entered in any module.
        /// </summary>
        public event AsyncEventHandler<LineEnteredEventArgs> LineEntered;
        /// <summary>
        /// Event fired when a key is pressed.
        /// </summary>
        public event AsyncEventHandler<KeyEnteredEventArgs> KeyEntered;

        /// <summary>
        /// Tries to get the next queued module to be used as a logger, and if the queue is empty return the input module.
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Thrown when there are no modules created yet, and none in the queue.</exception>

        ILogger ILoggerFactory.CreateLogger(string categoryName)
        {
            lock (dictSync)
            {
                if (loggerQueue.TryDequeue(out var next))
                {
                    return next;
                }
                else if (Pages.Count > 0 && this.Pages.FirstOrDefault()?.FirstOrDefault() is BaseModule module)
                {
                    return module;
                }
                else
                {
                    throw new Exception("No modules created, and no modules queued.");
                }
            }
        }

        void ILoggerFactory.AddProvider(ILoggerProvider provider)
        {
            throw new NotImplementedException();
        }
        
        public void Dispose()
        {
            Pages.Clear();
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
            ModulePage currentPage;
            lock (dictSync)
            {
                currentPage = Pages[selected];
                int currentSelected = currentPage.selected;
                pastSelected = currentSelected;
                currentSelected++;
                newSelected = (++currentSelected).Modulus(currentPage.Count + 1) - 1;
                currentPage.selected = newSelected;
                refreshPast = pastSelected >= 0;
                if (refreshPast)
                {
                    lock(currentPage.dictSync)
                        currentPage[pastSelected].selected = false;
                }
                
                refreshNew = newSelected >= 0;
                if (refreshNew)
                {
                    lock(currentPage.dictSync)
                        currentPage[newSelected].selected = true;
                }
            }
            if(refreshPast)
                currentPage[pastSelected].WriteOutput();
            if(refreshNew)
                currentPage[newSelected].WriteOutput();
        }

        public void NextPage()
        {
            lock(dictSync)
                selected = (++selected).Modulus(Pages.Count);
            RefreshAll();
        }
        public void PrevPage()
        {
            lock(dictSync)
                selected = (--selected).Modulus(Pages.Count);
            RefreshAll();
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
            ModulePage currentPage;
            lock (dictSync)
            {
                currentPage = Pages[selected];
                int currentSelected = currentPage.selected;
                pastSelected = currentSelected;
                newSelected = currentSelected.Modulus(currentPage.Count + 1) - 1;
                currentPage.selected = newSelected;
                refreshPast = pastSelected >= 0;
                if (refreshPast)
                {
                    currentPage[pastSelected].selected = false;
                }
                
                refreshNew = newSelected >= 0;
                if (refreshNew)
                {
                    currentPage[newSelected].selected = true;
                }
            }
            if(refreshPast)
                currentPage[pastSelected].WriteOutput();
            if(refreshNew)
                currentPage[newSelected].WriteOutput();
        }
        /// <summary>
        /// Quits the app, properly returning to the main buffer and clearing all possible cursor/format options.
        /// </summary>
        public static void QuitApp()
        {
            
            Console.Write(AnsiEscape.MainScreenBuffer);
            Console.Write(AnsiEscape.SoftReset);
            Console.Write(AnsiEscape.EnableCursorBlink);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        public async Task HandleKeyAsync(ConsoleKeyInfo key, BaseModule selectedModule)
        {
            Dictionary<string, bool> mods = key.Modifiers.ToStringDictionary<ConsoleModifiers>();
            if (mods[Alt])
                return;
            InputModule inputModule = selectedModule as InputModule;
            switch (key.Key)
            {
                case ConsoleKey.C when mods[Ctrl]:
                    QuitApp();
                    break;
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
                    selectedModule?.PageUp();
                    break;
                case ConsoleKey.PageDown:
                    selectedModule?.PageDown();
                    break;
                case ConsoleKey.UpArrow when mods[Ctrl]:
                    selectedModule?.ScrollUp(1);
                    break;
                case ConsoleKey.DownArrow when mods[Ctrl]:
                    selectedModule?.ScrollDown(1);
                    break;
                case ConsoleKey.UpArrow:
                    inputModule?.inputString?.Clear();
                    inputModule?.ScrollHistory(1);
                    break;
                case ConsoleKey.DownArrow:
                    inputModule?.inputString?.Clear();
                    inputModule?.ScrollHistory(-1);
                    break;
                case ConsoleKey.OemMinus when mods[Ctrl]:
                case ConsoleKey.Tab when mods[Ctrl] && mods[Shift]:
                    PrevPage();
                    break;
                case ConsoleKey.OemPlus when mods[Ctrl]:
                case ConsoleKey.Tab when mods[Ctrl]:
                    NextPage();
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
                    inputModule?.Backspace();
                    return;
                default:
                    inputModule?.AddChar(key.KeyChar);
                    
                    break;
            }
        }

        private async Task EnterLineAsync(InputModule inputModule, bool scrollToBottom)
        {
            if (inputModule is null) return;
            string line;
            AsyncEventHandler<LineEnteredEventArgs> handler;
            lock (this.writeLock)
            {
                handler = LineEntered;
                line = inputModule.inputString.ToString();
                inputModule.inputString.Clear();
                inputModule.lrCursorPos = 0;
                inputModule.AddToHistory(line);
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
            await inputModule.FireLineEnteredAsync(e);
            await inputModule.FireReadLineLineEntered(e);
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
        
        public InputModule Module { get; internal init; }
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
        
        public BaseModule Module { get; internal init; }
    }
    public class WinConsoleConfiguerer
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
        
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        
        public void SetupConsole()
        {
            IntPtr outputHandle = GetStdHandle(-11);
            IntPtr inputHandle = GetStdHandle(-10);
            GetConsoleMode(outputHandle, out uint outmode);
            GetConsoleMode(inputHandle, out uint inMode);
            outmode |= 4;
            SetConsoleMode(outputHandle, outmode);
            inMode = (uint)(inMode & ~64);
            SetConsoleMode(inputHandle, inMode);
        }
    }
    public delegate Task AsyncEventHandler<in T>(object sender, T eventArgs);
}
