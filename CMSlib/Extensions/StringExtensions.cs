using System;
using System.Text;

namespace CMSlib.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns a the string, in a form where the length is always <paramref name="maxLength"/> or less.
        /// </summary>
        /// <param name="str">The string to format</param>
        /// <param name="maxLength">The length</param>
        public static string Ellipse(this string str, int maxLength)
        {
            if (maxLength < 4)
                throw new ArgumentException("The length parameter of the Ellipse method must be 4 or greater.");
            if (str.Length > maxLength)
            {
                return str.Substring(0, maxLength - 3) + "...";
            }
            return str;
        }
        

        public static string PadToDivisible(this string str, int divisor)
        {
            if (str.Length % divisor == 0) return str;
            return new StringBuilder(str).Append(' ', divisor - str.Length % divisor).ToString();
        }
        public static string[] SplitOnLength(this string str, int length)
        {
            if (length <= 0) throw new ArgumentException("Length parameter must be at least 1 or greater");
            string[] output = new string[str.Length % length == 0 ? str.Length / length : str.Length / length + 1];
            for(int i = 0; i < str.Length / length; i++)
            {
                output[i] = str.Substring(i * length, length);
            }
            if (str.Length % length != 0) output[^1] = str.Substring((output.Length -1) * (length));
            return output;
            
        }

        public static string Censor(this string str, string word, out bool wordFound , string replacementString = "*")
        {
            if (!str.ToLower().Contains(word.ToLower())) {
                wordFound = false;
                return str;
            }

            StringBuilder output = new StringBuilder(str.Substring(0, str.ToLower().IndexOf(word.ToLower())));
            for(int i = 0;i<word.Length; i++)
            {
                output.Append(replacementString);
            }
            if (str.ToLower().IndexOf(word.ToLower()) + word.Length != str.Length)
                output.Append(str.Substring(str.ToLower().IndexOf(word.ToLower()) + word.Length));
            wordFound = true;
            return output.ToString();
        }

        public static string DiscordMarkdownStrip(this string str)
        {
            return str.Replace("*", "\\*").Replace("|", "\\|").Replace(">", "\\>").Replace("<", "\\<").Replace("@", "\\@").Replace("~", "\\~").Replace("`", "\\`").Replace("#", "\\#").Replace("_", "\\_");
        }

        public static string AddPlural(this string str, int number)
        {

            if (number != -1 && number != 1)
                return str + "s";
            return str;
        }
        public static int ParseInt(this string str)
        {
            return int.Parse(str);
        }
        public static void KeybdType(this string str)
        {
            for(int i = 0; i < str.Length; i++)
            {
                char current = str[i];
                if(current >= 'A' && current <= 'Z')
                {
                    ((byte)0x10).HoldKey();
                    ((byte)(0x41 + current - 'A')).KeyPress();
                    ((byte)0x10).ReleaseKey();
                    continue;
                }
                if(current >= 'a' && current <= 'z')
                {
                    ((byte)(0x41 + current - 'a')).KeyPress();
                    continue;
                }
                if(current >= '0' && current <= '9')
                {
                    ((byte)(0x30 + current - '0')).KeyPress();
                }
            }
        }


    }
    
}
