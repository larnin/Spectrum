
using System;
using System.Collections.Generic;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    enum PermType
    {
        ALL,
        HOST,
        LOCAL
    };

    abstract class Cmd
    {
        public abstract string name { get; }
        public abstract PermType perm { get; }
        public abstract bool canUseAsClient { get; }

        public abstract void help(ClientPlayerInfo p);
        public abstract void use(ClientPlayerInfo p, string message);

        public static CmdList all = new CmdList();

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
