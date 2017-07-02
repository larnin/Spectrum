using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.CmdSettings
{
    abstract class CmdSettingBool : CmdSetting
    {
        public override string UsageParameters { get; } = "<true/false>";
        public override UpdateResult UpdateFromString(string input)
        {
            input = input.ToLower();
            if (input == "true" || input == "1" || input == "t" || input == "yes" || input == "y")
            {
                return new UpdateResult(true, true);
            }
            else if (input == "false" || input == "0" || input == "f" || input == "no" || input == "n")
            {
                return new UpdateResult(true, false);
            }
            else
            {
                return new UpdateResult(false, this.Value, "Valid options: true/false, yes/no, y/n");
            }
        }
        public override UpdateResult UpdateFromObject(object input)
        {
            if (input.GetType() == typeof(bool))
                return new UpdateResult(true, input);
            else
                return new UpdateResult(false, Default, "Type should be a bool (true/false)");
        }
    }
}
