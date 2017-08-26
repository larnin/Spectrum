using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class IntComparison
    {
        public enum Comparison { Equal, Greater, Lesser, GreaterEqual, LesserEqual };
        public Comparison comparison;
        public int number;
        public IntComparison(int number)
        {
            comparison = Comparison.Equal;
            this.number = number;
        }
        public IntComparison(int number, Comparison comparison)
        {
            this.number = number;
            this.comparison = comparison;
        }
        public bool Compare(int number)
        {
            switch (comparison)
            {
                case Comparison.Equal:
                    return number == this.number;
                case Comparison.Greater:
                    return number > this.number;
                case Comparison.Lesser:
                    return number < this.number;
                case Comparison.GreaterEqual:
                    return number >= this.number;
                case Comparison.LesserEqual:
                    return number <= this.number;
                default:
                    return false;
            }
        }

        public delegate bool TryGetValueDelegate(string input, out int value);

        public static bool DefaultTryGetValue(string input, out int value)
        {
            return int.TryParse(input.Trim(), out value);
        }

        public static IntComparison ParseString(string input)
        {
            return ParseString(input, DefaultTryGetValue);
        }

        public static IntComparison ParseString(string input, TryGetValueDelegate parser)
        {
            Comparison comparison;
            string numSubstring;
            if (input.Length > 1)
            {
                switch (input.ElementAt(0))
                {
                    case '=':
                        comparison = Comparison.Equal;
                        numSubstring = input.Substring(1);
                        break;
                    case '>':
                        if (input.Length > 2 && input.ElementAt(1) == '=')
                        {
                            comparison = Comparison.GreaterEqual;
                            numSubstring = input.Substring(2);
                        }
                        else
                        {
                            comparison = Comparison.Greater;
                            numSubstring = input.Substring(1);
                        }
                        break;
                    case '<':
                        if (input.Length > 2 && input.ElementAt(1) == '=')
                        {
                            comparison = Comparison.LesserEqual;
                            numSubstring = input.Substring(2);
                        }
                        else
                        {
                            comparison = Comparison.Lesser;
                            numSubstring = input.Substring(1);
                        }
                        break;
                    default:
                        comparison = Comparison.Equal;
                        numSubstring = input;
                        break;
                }
            }
            else
            {
                comparison = Comparison.Equal;
                numSubstring = input;
            }
            int num;
            if (parser(numSubstring, out num))
            {
                return new IntComparison(num, comparison);
            }
            return null;
        }
    }
}
