using System;
using System.Collections.Generic;
using System.Text;

namespace AllanStevens.DataTranslator.Factory
{
    public enum PaddingDirectionTypes
    {
        Right,
        Left,
    }

    public static class StringExtentions
    {

        public static string Padding(this string text, int size) { return text.Padding(size, PaddingDirectionTypes.Left, " ", true); }

        public static string Padding(this string text, int size, PaddingDirectionTypes paddingDirection, string paddingText, bool trim)
        {
            if (text == null) text = "";

            if (paddingText == null || paddingText == "") paddingText = " ";

            if (trim) text.Trim();

            if (size < 1) return text;
            if (paddingDirection == PaddingDirectionTypes.Left)
            {
                text = text.PadRight(size, char.Parse(paddingText));
                return text.Substring(0, size);
            }
            else
            {
                text = text.PadLeft(size, char.Parse(paddingText));
                return text.Substring(text.Length - size);
            }
        }

        public static string Flatten(this string[] array, string spacer)
        {
            if (array.Length.Equals(0)) return null;

            StringBuilder sbRetun = new StringBuilder();

            foreach (string str in array)
            {
                sbRetun.Append(str + spacer);
            }

            return
                sbRetun.ToString().Substring(0, sbRetun.Length - spacer.Length);
        }

    }
}
