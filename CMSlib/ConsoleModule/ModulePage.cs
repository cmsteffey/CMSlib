using System.Collections;
using System.Collections.Generic;

namespace CMSlib.ConsoleModule
{
    public class ModulePage : IEnumerable<BaseModule>
    {
        private Dictionary<string, BaseModule> modules = new();
        private List<string> dictKeys = new();
        private object dictSync = new();
        private int _selected = 0;
        private ModuleManager parent = null;

        internal void SetParent(ModuleManager parent)
        {
            lock (dictSync){
                this.parent = parent;
                foreach (var module in modules.Values)
                {
                    module.parent = this.parent;
                }
            }
        }
        
        internal int selected {
            get
            {
                lock (dictSync) return _selected;
            }
            set
            {
                lock (dictSync) _selected = value;
            }
            
        }

        public void Add(BaseModule module)
        {
            lock (dictSync)
            {
                module.parent = this.parent;
                modules.Add(module.Title, module);
                dictKeys.Add(module.Title);
                module.WriteOutput();
            }
        }

        public BaseModule this[string title]
        {
            get
            {
                lock (dictSync)
                {
                    if (!modules.ContainsKey(title)) return null;
                    return modules[title];
                }
            }
        }
        public BaseModule this[int index]
        {
            get
            {
                lock (dictSync)
                {
                    return modules[dictKeys[index]];
                }
            }
        }

        internal int Count
        {
            get
            {
                lock (dictSync)
                    return modules.Count;
            }
        }

        internal bool ContainsTitle(string title)
        {
            lock (dictSync)
                return modules.ContainsKey(title);
        }

        internal BaseModule SelectedModule
        {
            get
            {
                lock(dictSync)
                    return _selected == -1 ? null : modules[dictKeys[_selected]];
            }
            
        }
        

        public IEnumerator<BaseModule> GetEnumerator()
        {
            lock(dictSync)
                return modules.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}