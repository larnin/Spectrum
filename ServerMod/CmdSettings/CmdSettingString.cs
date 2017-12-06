using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.CmdSettings
{
    abstract class CmdSettingString : CmdSetting<string>
    {
        public override string UsageParameters { get; } = "<text>";
        public override UpdateResult<string> UpdateFromString(string input)
        {
            return new UpdateResult<string>(true, input.ToLower() == "clear" ? "" : input);
        }
        public override UpdateResult<string> UpdateFromObject(object input)
        {
            if (input is string)
                return new UpdateResult<string>(true, (string)input);
            else
                return new UpdateResult<string>(false, Default, "Type should be a string (\"text\")");
        }
    }
}
