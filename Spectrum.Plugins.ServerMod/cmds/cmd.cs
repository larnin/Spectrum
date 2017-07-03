
using System;
using System.Collections.Generic;

namespace Spectrum.Plugins.ServerMod.cmds
{
    enum PermType
    {
        ALL,
        HOST,
        LOCAL
    };

    abstract class cmd
    {
        public abstract string name { get; }
        public abstract PermType perm { get; }
        public abstract bool canUseAsClient { get; }

        public abstract void help(ClientPlayerInfo p);
        public abstract void use(ClientPlayerInfo p, string message);

        public static cmdlist all = new cmdlist();

        public virtual CmdSettings.CmdSetting[] settings { get; } = new CmdSettings.CmdSetting[0];
        public CmdSettings.CmdSetting getSetting(string FileId)
        {
            foreach (var setting in settings)
            {
                if (setting.FileId == FileId)
                {
                    return setting;
                }
            }
            return null;
        }
    }
}
