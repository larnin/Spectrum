using System;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Spectrum.API.Helpers
{
    public static class StringExtensions
    {
        internal static string WordWrap(this string text, int lineLength)
        {
            int position;
            int next;
            var sb = new StringBuilder();

            if (lineLength < 1)
                return text;

            for (position = 0; position < text.Length; position = next)
            {
                int lineEnd = text.IndexOf('\n', position);
                if (lineEnd == -1)
                    next = lineEnd = text.Length;
                else
                    next = lineEnd + 1;

                if (lineEnd > position)
                {
                    do
                    {
                        int length = lineEnd - position;

                        if (length > lineLength)
                            length = LineBreak(text, position, lineLength);

                        sb.Append(text, position, length);
                        sb.Append('\n');

                        position += length;
                        while (position < lineEnd && char.IsWhiteSpace(text[position]))
                            position++;
                    } while (lineEnd > position);
                }
                else sb.Append('\n');
            }
            return sb.ToString();
        }

        private static int LineBreak(string text, int where, int max)
        {
            int i = max;
            while (i >= 0 && !char.IsWhiteSpace(text[where + i]))
                i--;

            if (i < 0)
                return max;

            while (i >= 0 && char.IsWhiteSpace(text[where + i]))
                i--;

            return i + 1;
        }

        public static Color ToColor(this string hexString)
        {
            var actualColorString = hexString.StartsWith("#") ? hexString.Substring(1, hexString.Length - 1) : hexString;

            if (actualColorString.Length % 2 != 0)
            {
                Console.WriteLine("API: Color string invalid.");
                return Color.black;
            }

            if(actualColorString.Length < 6)
            {
                Console.WriteLine("API: Color string too short.");
                return Color.black;
            }

            if (actualColorString.Length > 8)
            {
                Console.WriteLine("API: Color string too long.");
                return Color.black;
            }

            return ParseHex(actualColorString);
        }

        private static Color32 ParseHex(string hexString)
        {
            byte r, g, b, a;

            if (!byte.TryParse(hexString.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out r))
            {
                Console.WriteLine("API: Red color value isn't a byte.");
                return Color.black;
            }

            if (!byte.TryParse(hexString.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out g))
            {
                Console.WriteLine("API: Green color value isn't a byte.");
                return Color.black;
            }

            if (!byte.TryParse(hexString.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b))
            {
                Console.WriteLine("API: Blue color value isn't a byte.");
                return Color.black;
            }

            if (hexString.Length == 8)
            {
                if (!byte.TryParse(hexString.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out a))
                {
                    Console.WriteLine("API: Alpha color value isn't a byte.");
                    return Color.black;
                }
            }
            else
            {
                a = 255;
            }

            return new Color32(r, g, b, a);
        }
    }
}
