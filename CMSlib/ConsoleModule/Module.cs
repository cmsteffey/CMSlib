using System;
using CMSlib.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMSlib.ConsoleModule
{
    internal class Module
    {
        private List<string> text = new();
        private readonly char borderCharacter;
        internal int x, y, width, height;
        private bool isInput;
        internal Module(char borderCharacter, int x, int y, int width, int height, string text, bool isInput)
        {
            (
                this.borderCharacter,
                this.x,
                this.y,
                this.height,
                this.width,
                this.isInput) =
                (borderCharacter,
                x,
                y,
                height,
                width,
                isInput
            );
            this.AddText(text);
        }

        public void AddText(string text)
        {
            foreach (string line in text.SplitOnLength(width))
            {
                this.text.Add(line.PadToDivisible(width));
            }
        }

        public string ToString(string title)
        {
            StringBuilder output = new StringBuilder();
            output.Append(borderCharacter);
            output.Append(title.Ellipse(width));
            output.Append(borderCharacter, width - title.Ellipse(width).Length + 1);
            for (int i = 0; i < height - text.Count - (isInput?2:0); i++)
            {
                output.Append(borderCharacter);
                output.Append(' ', width);
                output.Append(borderCharacter);
            }

            foreach (string line in text.TakeLast(height - (isInput?2:0)))
            {
                output.Append(borderCharacter);
                output.Append(line);    
                output.Append(borderCharacter);
            }
            output.Append(borderCharacter, width + 2);
            if (isInput) output.Append(borderCharacter).Append(' ', width).Append(borderCharacter, width + 3);
            return output.ToString();
        }

        public string[] ToOutputLines(string title)
        {
            return ToString(title).SplitOnLength(width + 2);
        }

        public void WriteOutput(string title)
        {
            string[] outputLines = this.ToOutputLines(title);
            
            for (int i = 0; i < outputLines.Length; i++)
            {
                Console.SetCursorPosition(x, i + y);
                Console.Write(outputLines[i]);
            }
            if (isInput)
            
                Console.SetCursorPosition(this.x + 1, this.y + height);
            
        }
        public override string ToString()
        {
            return ToString("");
        }

    }
}

