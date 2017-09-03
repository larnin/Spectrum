using Spectrum.Plugins.ServerMod.Utilities;
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
        public virtual ServerModVersion UpdatedOnVersion { get; } = new ServerModVersion("C.7.3.1");
        
        public abstract object ValueTypeless { get; set; }
        public virtual object SaveValue { get { return ValueTypeless; } }
        public abstract object DefaultTypeless { get; }

        public abstract UpdateResult UpdateFromStringTypeless(string input);
        public abstract UpdateResult UpdateFromObjectTypeless(object input);
    }
    abstract class CmdSetting<T> : CmdSetting
    {
        private bool useDefault = true;
        public T value;
        public virtual T Value
        {
            get { return useDefault ? Default : this.value;  }
            set { this.value = value; useDefault = false; }
        }
        public override object ValueTypeless
        {
            get { return Value; }
            set { Value = (T) value; }
        }
        public abstract T Default { get; }
        public override object DefaultTypeless
        {
            get { return Default; }
        }

        public abstract UpdateResult<T> UpdateFromString(string input);
        public abstract UpdateResult<T> UpdateFromObject(object input);

        public override UpdateResult UpdateFromStringTypeless(string input)
        {
            return UpdateFromString(input);
        }
        public override UpdateResult UpdateFromObjectTypeless(object input)
        {
            return UpdateFromObject(input);
        }
    }
}
