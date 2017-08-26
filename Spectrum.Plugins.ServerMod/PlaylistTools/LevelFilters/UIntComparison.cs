using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class UIntComparison
    {
        public enum Comparison { Equal, Greater, Lesser, GreaterEqual, LesserEqual };
        public Comparison comparison;
        public uint number;
        public UIntComparison(uint number)
        {
            comparison = Comparison.Equal;
            this.number = number;
        }
        public UIntComparison(uint number, Comparison comparison)
        {
            this.number = number;
            this.comparison = comparison;
        }
        public bool Compare(uint number)
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

        public delegate bool TryGetValueDelegate(string input, out uint value);

        public static bool DefaultTryGetValue(string input, out uint value)
        {
            return uint.TryParse(input.Trim(), out value);
        }

        public static UIntComparison ParseString(string input)
        {
            return ParseString(input, DefaultTryGetValue);
        }

        public static UIntComparison ParseString(string input, TryGetValueDelegate parser)
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
            uint num;
            if (parser(numSubstring, out num))
            {
                return new UIntComparison(num, comparison);
            }
            return null;
        }
    }
}
