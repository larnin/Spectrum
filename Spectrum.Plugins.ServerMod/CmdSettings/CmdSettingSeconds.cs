using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.CmdSettings
{
    // TODO: Make this evaluate "mm:ss" as well as just seconds.
    abstract class CmdSettingSeconds : CmdSettingInt
    {
        public override string UsageParameters { get; } = "<seconds>";
    }
}
