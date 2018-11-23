using Spectrum.Plugins.ServerMod.Cmds;
using System.Collections.Generic;

namespace Spectrum.Plugins.ServerMod
{
    class CmdList
    {
        private List<Cmd> cmds = new List<Cmd>();

        public CmdList()
        {
            Cmd[] addCmds = new Cmd[] {
                new AutoCmd(this),
                new AutoSpecCmd(),
                new ClearCmd(),
                new CountdownCmd(),
                new DateCmd(),
                new DelCmd(),
                new DelsCmd(),
                new FilterCmd(),
                new HelpCmd(),
                new InfoCmd(),
                new KickCmd(),
                new LevelCmd(),
                new ListCmd(),
                new LoadCmd(),
                new LogCmd(),
                //new NameCmd(), // not supported
                new PlayCmd(),
                new PlaylistCmd(),
                new PlayersCmd(),
                new PluginCmd(),
                //new RestartCmd(),
                new RipCmd(),
                new SaveCmd(),
                new SayCmd(),
                new ScoresCmd(),
                new ServerCmd(),
                new SettingsCmd(),
                new ShuffleCmd(),
                new SpecCmd(),
                new StuckCmd(),
                new TimelimitCmd(),
                new UnstuckCmd(),
                new UpdateCmd(),
                new WelcomeCmd(),
                new WinCmd(),
            };
            foreach (Cmd addCmd in addCmds)
                cmds.Add(addCmd);

            VoteHandler voteHandler = new VoteHandler(this);
            cmds.Add(voteHandler.voteCommand);
            cmds.Add(voteHandler.voteControlCommand);
            
        }

        public T getCommand<T>() where T : Cmd
        {
            foreach (Cmd c in cmds)
                if (c is T)
                    return (T) c;
            return null;
        }

        public T getCommand<T>(string name) where T : Cmd
        {
            foreach (Cmd c in cmds)
                if (c is T && c.name == name)
                    return (T)c;
            return null;
        }

        public Cmd getCommand(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            name = name.ToLower();

            foreach(Cmd c in cmds)
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
        public List<Cmd> list()
        {
            List<Cmd> l = new List<Cmd>();
            foreach (var c in cmds)
                l.Add(c);
            return l;
        }
    }
}
