using System;
using Microsoft.Extensions.Logging;

namespace CMSlib.ConsoleModule
{
    public class CategoricalLogModule<TCategory> : LogModule, ILogger<TCategory>, ICategoricalModule
    {
        public CategoricalLogModule(string title, int x, int y, int width, int height,
            char? borderCharacter = null, LogLevel minimumLogLevel = LogLevel.Information) : base(title, x, y, width,
            height, borderCharacter, minimumLogLevel)
        {
        }

        Type ICategoricalModule.GetCategoryType()
        {
            return typeof(TCategory);
        }
    }
}