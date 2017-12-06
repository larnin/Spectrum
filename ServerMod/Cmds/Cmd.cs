
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
        public abstract bool canUseLocal { get; }

        public virtual bool showChatPublic(ClientPlayerInfo p)
        {
            return false;
        }

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
        public T getSetting<T>() where T : CmdSettings.CmdSetting
        {
            foreach (var setting in settings)
            {
                if (setting is T)
                {
                    return (T)setting;
                }
            }
            return null;
        }
        public T getSetting<T>(string FileId) where T : CmdSettings.CmdSetting
        {
            foreach (var setting in settings)
            {
                if (setting is T && setting.FileId == FileId)
                {
                    return (T)setting;
                }
            }
            return null;
        }
    }
}
