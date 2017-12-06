using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.CmdSettings
{
    abstract class CmdSettingBool : CmdSetting<bool>
    {
        public override string UsageParameters { get; } = "<true/false>";
        public override UpdateResult<bool> UpdateFromString(string input)
        {
            input = input.ToLower();
            if (input == "true" || input == "1" || input == "t" || input == "yes" || input == "y")
            {
                return new UpdateResult<bool>(true, true);
            }
            else if (input == "false" || input == "0" || input == "f" || input == "no" || input == "n")
            {
                return new UpdateResult<bool>(true, false);
            }
            else
            {
                return new UpdateResult<bool>(false, this.Value, "Valid options: true/false, yes/no, y/n");
            }
        }
        public override UpdateResult<bool> UpdateFromObject(object input)
        {
            if (input is bool)
                return new UpdateResult<bool>(true, (bool)input);
            else
                return new UpdateResult<bool>(false, Default, "Type should be a bool (true/false)");
        }
    }
}
