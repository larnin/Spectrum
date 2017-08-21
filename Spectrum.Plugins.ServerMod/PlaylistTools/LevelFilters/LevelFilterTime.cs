using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class LevelFilterTime : LevelFilter
    {
        public override string[] options { get; } = new string[] {"t", "time"};

        public static Dictionary<string, MedalStatus> medalLookup = new Dictionary<string, MedalStatus>
        {
            {"bronze",  MedalStatus.Bronze},
            {"silver",  MedalStatus.Silver},
            {"gold",    MedalStatus.Gold},
            {"diamond", MedalStatus.Diamond},
        };

        MedalStatus medal;
        FloatComparison comparison;

        public LevelFilterTime()
        {
            medal = MedalStatus.None;
            comparison = new FloatComparison(0);
        }

        public LevelFilterTime(MedalStatus medal, FloatComparison comparison)
        {
            this.medal = medal;
            this.comparison = comparison;
        }

        public override void Apply(List<PlaylistLevel> levels)
        {
            var levelSetsManager = G.Sys.LevelSets_;
            foreach (var level in levels)
            {
                var levelInfo = levelSetsManager.GetLevelInfo(level.level.levelNameAndPath_.levelPath_);
                if (level.level.mode_.IsTimeBased() && levelInfo.SupportsMedals(level.level.mode_))
                    level.Mode(mode, comparison.Compare(levelInfo.GetMedalRequirementTime(medal) / 1000f));
                else
                    level.Mode(mode, false);
            }
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            var match = Regex.Match(chatString, @"(\w+) (.+)");
            MedalStatus medal;
            string timeStr;
            if (match.Success)
            {
                if (!TryParseMedal(match.Groups[1].Value, out medal))
                {
                    return new LevelFilterResult("Invalid medal for -time");
                }
                timeStr = match.Groups[2].Value;
            }
            else
            {
                medal = MedalStatus.Bronze;
                timeStr = chatString;
            }
            FloatComparison comparison = FloatComparison.ParseString(timeStr, TryParseTime);
            if (comparison != null)
                return new LevelFilterResult(new LevelFilterTime(medal, comparison));
            return new LevelFilterResult("Invalid comparison/time for -time");
        }

        bool TryParseMedal(string input, out MedalStatus value)
        {
            foreach (KeyValuePair<string, MedalStatus> pair in medalLookup)
                if (pair.Key.Contains(input.ToLower().Trim()))
                {
                    value = pair.Value;
                    return true;
                }
            value = MedalStatus.None;
            return false;
        }

        bool TryParseTime(string input, out float value)
        {
            float seconds = 0;
            List<float> numbers = new List<float>();
            foreach (Match match in Regex.Matches(input, @"\:?(\d*\.?\d+)"))
            {
                float num;
                if (float.TryParse(match.Groups[1].Value, out num))
                    numbers.Insert(0, num);
            }
            if (numbers.Count == 0)
            {
                if (!float.TryParse(input, out seconds))
                {
                    value = 0;
                    return false;
                }
            }
            else
            {
                var multiplier = 1;
                var index = 0;
                foreach (int number in numbers)
                {
                    seconds += number * multiplier;
                    switch (index)
                    {
                        case 2:
                            multiplier *= 24;
                            break;
                        case 3:
                            multiplier *= 7;
                            break;
                        case 4:
                            multiplier *= 52;
                            break;
                        default:
                            multiplier *= 60;
                            break;
                    }
                    index++;
                }
            }
            value = seconds;
            return true;
        }
    }
}
