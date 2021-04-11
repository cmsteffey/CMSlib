using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMSlib.Extensions;
using System.Threading.Tasks;

namespace CMSlib.Tables
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// This will return a padded table column with the given width in characters, containing the string. The optional parameters adjust the formatting.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="innerWidth">The width of the column in characters, excluding the optional pipes on the sides.</param>
        /// <param name="adjust">Left will pad spaces on the right, pushing text to the left. Right will pad spaces to the left, pushing text to the right. Center will attempt to evenly pad spaces on both sides, centering the text.</param>
        /// <param name="ellipse">If the string does not fit into the column, this decides whether to replace the last three spaces in the column with "..."</param>
        /// <param name="leftPipe">Set to true to append | to the left side of the column</param>
        /// <param name="rightPipe">Set to true to append | to the right side of the column</param>
        /// <returns></returns>
        public static string TableColumn(this string str, int innerWidth, ColumnAdjust adjust = ColumnAdjust.Left, bool ellipse = true, bool leftPipe = false, bool rightPipe = false)
        {
            if (str.Length > innerWidth)
            {
                return ellipse ? (leftPipe ? "|" : string.Empty) + str.Ellipse(innerWidth) + (rightPipe ? "|" : string.Empty)
                    : (leftPipe ? "|" : string.Empty) + str.Substring(0, innerWidth) + (rightPipe ? "|" : string.Empty);
            }
            int spaces = innerWidth - str.Length;
            if (adjust == ColumnAdjust.Left)
            {
                return new StringBuilder().Append(leftPipe ? "|" : null).Append(str).Append(' ', spaces).Append(rightPipe ? '|' : null).ToString();
            }
            else if (adjust == ColumnAdjust.Right)
            {
                return new StringBuilder().Append(leftPipe ? "|" : null).Append(' ', spaces).Append(str).Append(rightPipe ? '|' : null).ToString();
            }
            else
            {
                return new StringBuilder().Append(leftPipe ? "|" : null).Append(' ', spaces / 2).Append(str).Append(' ', spaces / 2 + spaces % 2).Append(rightPipe ? '|' : null).ToString();
            }
        }
        public enum ColumnAdjust
        {
            Left, Right, Center
        }
    }
}
