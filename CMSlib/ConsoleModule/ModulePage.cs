using System;
using System.Collections;
using System.Collections.Generic;

namespace CMSlib.ConsoleModule
{
    public class ModulePage : IEnumerable<BaseModule>
    {
        //TODO add page selected event, to modules too 
        private readonly Dictionary<string, BaseModule> _modules = new();
        private readonly List<string> _dictKeys = new();
        internal readonly object DictSync = new();
        internal int Selected = 0;
        private ModuleManager _parent;
        private string _displayName;
        internal Guid Id = Guid.NewGuid();

        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value;
                RefreshAll();
            }
        }

        public ModulePage(string displayName = null)
        {
            _displayName = displayName;
        }

        public void RefreshAll(bool clear = true)
        {
            if (_parent is null) return;
            lock (_parent.WriteLock)
            {
                if (clear) Console.Clear();
                Dictionary<string, BaseModule>.ValueCollection modules;
                lock (DictSync)
                    modules = _modules.Values;
                foreach (var modulesValue in modules)
                {
                    modulesValue.WriteOutput(false);
                }

                _parent.Flush();
            }
        }

        internal void SetParent(ModuleManager parent)
        {
            lock (DictSync)
            {
                _parent = parent;
                foreach (var module in _modules.Values)
                {
                    module.Parent = this._parent;
                    module.parentPages.Add(this.Id);
                }
            }
        }


        public void Add(BaseModule module)
        {
            lock (DictSync)
            {
                module.Parent = this._parent;
                _modules.Add(module.Title, module);
                _dictKeys.Add(module.Title);
                if (Selected == _modules.Count - 1)
                    module.Selected = true;
                module.WriteOutput();
            }
        }

        public BaseModule this[string title]
        {
            get
            {
                lock (DictSync)
                {
                    if (!_modules.ContainsKey(title)) return null;
                    return _modules[title];
                }
            }
        }

        public BaseModule this[int index]
        {
            get
            {
                lock (DictSync)
                {
                    return _modules[_dictKeys[index]];
                }
            }
        }

        internal int Count
        {
            get
            {
                lock (DictSync)
                    return _modules.Count;
            }
        }

        internal bool ContainsTitle(string title)
        {
            lock (DictSync)
                return _modules.ContainsKey(title);
        }

        internal BaseModule SelectedModule
        {
            get
            {
                lock (DictSync)
                    return Selected == -1 ? null : _modules[_dictKeys[Selected]];
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
            lock (DictSync)
                return _modules.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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