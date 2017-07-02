using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.CmdSettings
{
    abstract class CmdSetting
    {
        public abstract string FileId { get; }  // for settings.json
        public virtual string SettingsId { get { return FileId; } }  // for !settings
        
        public abstract string DisplayName { get; }  // for automated help messages
        public abstract string HelpShort { get; }
        public abstract string HelpLong { get; }
        public virtual string HelpMarkdown { get { return HelpLong; } }
        public virtual string UsageParameters { get; } = "<option>";

        public object Value { get; set; }
        public abstract object Default { get; }

        public abstract UpdateResult UpdateFromString(string input);
        public abstract UpdateResult UpdateFromObject(object input);

        public CmdSetting()
        {
            Value = Default;
        }
    }
}
