using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CMSlib.Extensions;
using Microsoft.Extensions.Logging;

namespace CMSlib.ConsoleModule;

public class TextLineModule : BaseModule
{
    
    private string[] lines;
    public TextLineModule(string title, int x, int y, int width, int height) : base(title, x, y, width, height,
        LogLevel.None)
    {
        lines = new string[height];
        for (int i = 0; i < height; ++i)
        {
            lines[i] = string.Empty;
        }
    }
    public string this[int index]
    {
        get => lines[index];
        set => lines[index] = value;
    } 
    public override void AddText(string text)
    {
        var idx = lines.FindFirst(string.Empty);
        if (idx == -1) idx = lines.FindFirst(null);
        if (idx == -1) return;
        lines[idx] = text;
    }

    public override void ScrollUp(int amt)
    {
    }

    public override void ScrollTo(int line)
    {
    }

    public override void PageUp()
    {
    }

    public override void PageDown()
    {
    }

    public override void Clear(bool refresh = true)
    {
        for (int i = 0; i < lines.Length; ++i)
        {
            lines[i] = string.Empty;
        }
    }

    internal override async Task HandleClickAsync(InputRecord record, ButtonState? before)
    {
    }

    internal override async Task HandleKeyAsync(ConsoleKeyInfo info)
    {
    }

    protected override IEnumerable<string> ToOutputLines()
    {
        int width = Math.Min(Width, Console.WindowWidth - X);
        int height = Math.Min(Height, Console.WindowHeight - Y);
        for (int i = 0; i < height; ++i)
        {
            if (lines[i] == string.Empty)
            {
                yield return string.Empty;
                continue;
            }

            var line = lines[i];
            if (line is null) goto YR;
            var vislen = line.VisibleLength();
            if(vislen < width)
                line += new string(' ', width - vislen);
            else
                line = line.SplitOnNonEscapeLength(width).FirstOrDefault();
            YR:
            yield return line ?? string.Empty;
        }
    }
}