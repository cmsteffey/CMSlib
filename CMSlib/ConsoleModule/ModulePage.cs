using System.Collections;
using System.Collections.Generic;

namespace CMSlib.ConsoleModule
{
    public class ModulePage : IEnumerable<BaseModule>
    {
        private List<BaseModule> modules;
        internal int selected = 0;
        public void Add(BaseModule module)
        {
            modules.Add(module);
        }
        public IEnumerator<BaseModule> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class Enumerator : IEnumerator<BaseModule>
        {
            private readonly ModulePage page;
            private int pointer;

            private Enumerator()
            {
            }

            internal Enumerator(ModulePage page)
            {
                this.page = page;
            }
            object IEnumerator.Current => Current;
            public BaseModule Current
            {
                get => page.modules[pointer];
            }

            public void Dispose()
            {
                
            }

            public bool MoveNext()
            {
                return ++pointer < page.modules.Count;
            }
            public void Reset()
            {
                pointer = 0;
            }
        }
    }
}