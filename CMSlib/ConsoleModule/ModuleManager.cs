using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private AutoResetEvent lockReset = new AutoResetEvent(true);
        public Module SelectedModule
        {
            get { lock(dictSync)return selected == -1 ? null : modules[dictKeys[selected]]; }
        }

        private Queue<Module> loggerQueue = new();
        public Module InputModule
        {
            get { lock(dictSync) return modules.Count > 0 ? modules[dictKeys[0]] : null; }
        }

        public string InputModuleTitle
        {
            get { lock(dictSync) return dictKeys.Count > 0 ? dictKeys[0] : null; }
        }

        private readonly object dictSync = new();
        public void RefreshAll(bool clear = true)
        {
            if (clear) Console.Clear();
            foreach(string title in modules.Keys)
            {
                RefreshModule(title);
            }

        }
        public bool RefreshModule(string title)
        {
            if (!modules.ContainsKey(title))
            {
                return false;
            }
            modules[title].WriteOutput();

            return true;
        }
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
        public bool AddToModule(string moduleTitle, string text)
        {
            if(modules.TryGetValue(moduleTitle, out Module module))
            {
                module.AddText(text);
                return true;
            }
            return false;
        }

        

        public bool EnqueueLoggerModule(string moduleTitle)
        {
            if (!modules.TryGetValue(moduleTitle, out Module toQueue)) return false;
            loggerQueue.Enqueue(toQueue);
            return true;
        }
        
        public bool AddModule(string title, int x, int y, int width, int height, string startingText = "", char? borderChar = null, LogLevel minimumLogLevel = LogLevel.Information, bool immediateOutput = true)
        {
            if (modules.ContainsKey(title))
            {
                return false;
            }
            lock (dictSync)
            {
                Module toAdd = new(this, title, x, y, width, height, startingText, modules.Count == 0, borderChar, minimumLogLevel);
                modules.Add(title, toAdd);
                dictKeys.Add(title);
                if (dictKeys.Count - 1 == selected)
                    toAdd.selected = true;
                if (immediateOutput) RefreshModule(title);
            }
            return true;
        }
        

        public Module this[string title] => GetModule(title);
        public Module this[int index] =>  modules[dictKeys[index]];
        
        public Module GetModule(string title)
        {
            return modules[title];
        }
        
        public event Module.AsyncEventHandler<LineEnteredEventArgs> LineEntered
        {
            add { if(InputModule is not null) InputModule.LineEntered += value; }
            remove { if(InputModule is not null) InputModule.LineEntered -= value; }
        }

        public event Module.AsyncEventHandler<KeyEnteredEventArgs> KeyEntered
        {
            add { if(InputModule is not null) InputModule.KeyEntered += value; }
            remove { if(InputModule is not null) InputModule.KeyEntered -= value; }
        } 

        public ILogger CreateLogger(string categoryName)
        {
            if (loggerQueue.TryDequeue(out Module next))
            {
                return next;
            }
            else
            {
                throw new InvalidOperationException("There are no modules in the logger queue");
            }
        }

        public void AddProvider(ILoggerProvider provider)
        {
            
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            dictKeys.Clear();
            modules.Clear();
            this.RefreshAll();
        }

        public void SelectNext()
        {
            int newSelected;
            lock (dictSync)
            {
                
                int pastSelected = selected;
                selected++;
                newSelected = ++selected % (dictKeys.Count + 1) - 1;
                

                if (pastSelected >= 0)
                {
                    modules[dictKeys[pastSelected]].selected = false;
                    modules[dictKeys[pastSelected]].WriteOutput();
                }

                if (newSelected >= 0)
                {
                    modules[dictKeys[newSelected]].selected = true;
                    modules[dictKeys[newSelected]].WriteOutput();
                }

                selected = newSelected;

            }
        }
    }

    public class LineEnteredEventArgs : EventArgs{
        internal LineEnteredEventArgs(){}
        public string Line { get; }
        
        internal LineEnteredEventArgs(string line)
        {
            this.Line = line;
        }
    }
    public class KeyEnteredEventArgs : EventArgs{
        internal KeyEnteredEventArgs(){}
        public ConsoleKeyInfo KeyInfo { get; }
        
        internal KeyEnteredEventArgs(ConsoleKeyInfo key)
        {
            KeyInfo = key;
        }
    }
}
