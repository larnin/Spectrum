using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class FloatComparison
    {
        public enum Comparison { Equal, Greater, Lesser, GreaterEqual, LesserEqual };
        public Comparison comparison;
        public float number;
        public FloatComparison(float number)
        {
            comparison = Comparison.Equal;
            this.number = number;
        }
        public FloatComparison(float number, Comparison comparison)
        {
            this.number = number;
            this.comparison = comparison;
        }
        public bool Compare(float number)
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

        public delegate bool TryGetValueDelegate(string input, out float value);

        public static FloatComparison ParseString(string input)
        {
            return ParseString(input, float.TryParse);
        }

        public static FloatComparison ParseString(string input, TryGetValueDelegate parser)
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
            float num;
            if (parser(numSubstring, out num))
            {
                return new FloatComparison(num, comparison);
            }
            return null;
        }
    }
}
