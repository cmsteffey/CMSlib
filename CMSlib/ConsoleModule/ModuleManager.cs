using System;
using System.Collections.Generic;
using System.Linq;

namespace CMSlib.ConsoleModule
{
    public class ModuleManager
    {
        private readonly Dictionary<string, Module> modules = new();
        
        public void RefreshAll()
        {
            
            Console.Clear();
            foreach(string title in modules.Keys)
            {
                RefreshModule(title);
            }
            if (modules.Count > 0)
                Console.SetCursorPosition(modules.ElementAt(0).Value.x + 1, modules.ElementAt(0).Value.y + modules.ElementAt(0).Value.height);
        }
        public bool RefreshModule(string title)
        {
            if (!modules.Keys.Contains(title))
            {
                return false;
            }
            modules[title].WriteOutput(title);
            Console.SetCursorPosition(modules.ElementAt(0).Value.x + 1, modules.ElementAt(0).Value.y + modules.ElementAt(0).Value.height);

            return true;
        }
        public bool RemoveModule(string title)
        {
            return modules.Remove(title);
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

        public bool AddModule(string title, char borderChar, int x, int y, int width, int height, string startingText = "")
        {
            if (modules.Keys.Contains(title))
            {
                return false;
            }
            modules.Add(title, new(borderChar, x, y, width, height, startingText,modules.Count == 0));
            return true;
        }
    }
}
