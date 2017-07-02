using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.CmdSettings
{
    abstract class CmdSettingString : CmdSetting
    {
        public override string UsageParameters { get; } = "<text>";
        public override UpdateResult UpdateFromString(string input)
        {
            return new UpdateResult(true, input);
        }
        public override UpdateResult UpdateFromObject(object input)
        {
            if (input.GetType() == typeof(string))
                return new UpdateResult(true, input);
            else
                return new UpdateResult(false, Default, "Type should be a string (\"text\")");
        }
    }
}
