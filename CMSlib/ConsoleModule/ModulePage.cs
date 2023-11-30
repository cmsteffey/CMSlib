using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMSlib.ConsoleModule
{
    public class ModulePage : IEnumerable<BaseModule>
    {
        //TODO add page selected event, to modules too 
        private Dictionary<string, BaseModule> modules = new();
        private List<string> dictKeys = new();
        internal object dictSync = new();
        internal int selected = 0;
        private ModuleManager parent = null;
        private string _displayName;
        internal Guid id = Guid.NewGuid();
        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;
                RefreshAll();
            }
        }

        public ModulePage(string displayName = null)
        {
            this._displayName = displayName;
        }
        public void RefreshAll(bool clear = true)
        {
            if (parent is null) return;
            lock (parent.writeLock)
            {
                if(clear)System.Console.Clear();
                Dictionary<string, BaseModule>.ValueCollection modules;
                lock(dictSync)
                    modules = this.modules.Values;
                foreach (var modulesValue in modules)
                {
                    modulesValue.WriteOutput(false);
                }
                parent.Flush();
            }
        }

        internal void SetParent(ModuleManager parent)
        {
            lock (dictSync){
                this.parent = parent;
                foreach (var module in modules.Values)
                {
                    module.Parent = this.parent;
                    module.parentPages.Add(this.id);
                }
            }
        }
        
       

        public void Add(BaseModule module)
        {
            lock (dictSync)
            {
                module.Parent = this.parent;
                module.parentPages.Add(this.id);
                modules.Add(module.Title, module);
                dictKeys.Add(module.Title);
                if (selected == modules.Count - 1)
                    module.selected = true;
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
                    return selected == -1 ? null : modules[dictKeys[selected]];
            }
            
        }
	
        public event EventHandler<PageSelectedEventArgs> PageSelected;

        internal void FirePageSelected(PageSelectedEventArgs e)
        {
            var handler = PageSelected;
            if (handler is not null)
                handler(this, e);
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
	public override bool Equals(object? o){
	    return (o is ModulePage mp) && mp.id == this.id;
	}

    }

    public class PageSelectedEventArgs : EventArgs
    {
        public BaseModule NewSelectedModule { get; }

        internal PageSelectedEventArgs()
        {
        }

        internal PageSelectedEventArgs(BaseModule newSelectedModule)
        {
            NewSelectedModule = newSelectedModule;
        }
    }
}