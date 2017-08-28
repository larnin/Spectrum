using Spectrum.Plugins.ServerMod.cmds;
using System.Collections.Generic;

namespace Spectrum.Plugins.ServerMod
{
    class cmdlist
    {
        private List<cmd> cmds = new List<cmd>();

        public cmdlist()
        {
            cmd[] addCmds = new cmd[] {
                new AutoCMD(this),
                new AutoSpecCMD(),
                new ClearCMD(),
                new CountdownCMD(),
                new DateCMD(),
                new DelCMD(),
                new DelsCMD(),
                new FilterCMD(),
                new ForceStartCMD(),
                new HelpCMD(),
                new InfoCMD(),
                new KickCMD(),
                new LevelCMD(),
                new ListCMD(),
                new LoadCMD(),
                //new NameCMD(), // not supported
                new PlayCMD(),
                new PlaylistCMD(),
                new PlayersCMD(),
                new PluginCMD(),
                new RestartCMD(),
                new RipCMD(),
                new SaveCMD(),
                new ScoresCMD(),
                new ServerCMD(),
                new SettingsCMD(),
                new ShuffleCMD(),
                new SpecCMD(),
                new TimelimitCMD(),
                new UpdateCMD(),
                new WelcomeCMD(),
                new WinCMD(),
            };
            foreach (cmd addCmd in addCmds)
                cmds.Add(addCmd);

            VoteHandler voteHandler = new VoteHandler(this);
            cmds.Add(voteHandler.voteCommand);
            cmds.Add(voteHandler.voteControlCommand);
            
        }

        public T getCommand<T>() where T : cmd
        {
            foreach (cmd c in cmds)
                if (c is T)
                    return (T) c;
            return null;
        }

        public T getCommand<T>(string name) where T : cmd
        {
            foreach (cmd c in cmds)
                if (c is T && c.name == name)
                    return (T)c;
            return null;
        }

        public cmd getCommand(string name)
        {
            if (name.Length == 0)
                return null;
            name = name.ToLower();

            foreach(cmd c in cmds)
                if (c.name.Equals(name))
                    return c;
            return null;
        }

        public List<string> commands()
        {
            List<string> l = new List<string>();
            foreach(var c in cmds)
                l.Add(c.name);
            return l;
        }
        public List<cmd> list()
        {
            List<cmd> l = new List<cmd>();
            foreach (var c in cmds)
                l.Add(c);
            return l;
        }
    }
}
