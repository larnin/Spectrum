using Spectrum.Plugins.ServerMod.CmdSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class CmdSettingPlayPlayersAddMaps : CmdSettingBool
    {
        public override string FileId { get; } = "playersCanAddMap";
        public override string SettingsId { get; } = "play";

        public override string DisplayName { get; } = "Player !play";
        public override string HelpShort { get; } = "!play: Allow players to add maps";
        public override string HelpLong { get; } = "Whether or not players can add maps using !play";

        public override object Default { get; } = false;
    }
    class CmdSettingPlayAddOne : CmdSettingBool
    {
        public override string FileId { get; } = "addOneMapOnly";
        public override string SettingsId { get; } = "addOne";

        public override string DisplayName { get; } = "Player !play one map only";
        public override string HelpShort { get; } = "!play: allow players to add only one map";
        public override string HelpLong { get; } = "Limit players to adding only one map with !play";

        public override object Default { get; } = true;
    }
    class CmdSettingPlayIsVote : CmdSettingBool
    {
        public override string FileId { get; } = "playIsVote";
        public override string SettingsId { get; } = "playVote";

        public override string DisplayName { get; } = "Play is Vote";
        public override string HelpShort { get; } = "!play acts as !vote play for players";
        public override string HelpLong { get; } = "For non-hosts, !play will use !vote play instead of adding maps directly. The host still adds maps directly.";

        public override object Default { get; } = false;
    }
    class PlayCMD : cmd
    {
        public bool playersCanAddMap
        {
            get { return (bool)getSetting("playersCanAddMap").Value; }
            set { getSetting("playersCanAddMap").Value = value; }
        }
        public bool addOneMapOnly
        {
            get { return (bool)getSetting("addOneMapOnly").Value; }
            set { getSetting("addOneMapOnly").Value = value; }
        }
        public bool useVote
        {
            get { return (bool)getSetting("playIsVote").Value; }
            set { getSetting("playIsVote").Value = value; }
        }

        public override string name { get { return "play"; } }
        public override PermType perm { get { return playersCanAddMap ? PermType.ALL : PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            if (useVote)
                Utilities.sendMessage(Utilities.formatCmd("!play [lvl name]") + ": Vote to add a level to the playlist.");
            else
                Utilities.sendMessage(Utilities.formatCmd("!play [lvl name]") + ": Adds a level to the playlist as the next to be played.");
            Utilities.sendMessage(Utilities.formatCmd("!play [filter]") + ": Use filters to find a level");
            Utilities.sendMessage("Valid filters: -mode -m -name -n -author -a -index -i -last -l -all");
            Utilities.sendMessage("The level must be known by the server to be show up");

            CmdSetting[] setting =
            {
                new CmdSettingPlayPlayersAddMaps(),
                new CmdSettingPlayAddOne(),
                new CmdSettingPlayIsVote()
            };
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if (message == "")
            {
                help(p);
                return;
            }

            if (useVote && !p.IsLocal_)  // host can always force play, never uses vote.
            {
                ((VoteHandler.VoteCMD)cmd.all.getCommand("vote")).forceNextUse();
                ((VoteHandler.VoteCMD)cmd.all.getCommand("vote")).use(p, "y play " + message);
                return;
            }

            if (Utilities.isOnLobby())
            {
                Utilities.sendMessage("You can't set the next level in the lobby.");
                return;
            }

            if (G.Sys.GameManager_.ModeID_ == GameModeID.Trackmogrify)
            {
                Utilities.sendMessage("You can't manage the playlist in trackmogrify.");
                return;
            }

            Modifiers m = LevelList.extractModifiers(message);
            var list = LevelList.levels(m);

            if(list.Count == 0)
            {
                Utilities.sendMessage("Can't find a level with the filter '" + message + "'.");
                return;
            }

            if(!m.all && list.Count() > 1 && m.index.Count == 0)
            {
                LevelList.printLevels(list, m.page, 10, true);
                //LevelList.printLevels(list, 10, true);
                return;
            }

            if(playersCanAddMap && !p.IsLocal_)
            {
                var value = list[0];
                list.Clear();
                list.Add(value);
            }

            LevelPlaylist playlist = new LevelPlaylist();
            playlist.Copy(G.Sys.GameManager_.LevelPlaylist_);

            var currentPlaylist = playlist.Playlist_;
            int index = G.Sys.GameManager_.LevelPlaylist_.Index_;
            Utilities.Shuffle(list, new Random());
            foreach (var lvl in list)
                currentPlaylist.Insert(index + 1, lvl);
            G.Sys.GameManager_.LevelPlaylist_.Clear();

            foreach (var lvl in currentPlaylist)
                G.Sys.GameManager_.LevelPlaylist_.Add(lvl);
            G.Sys.GameManager_.LevelPlaylist_.SetIndex(index);

            string lvlsStr = "";
            foreach (var lvl in list)
                lvlsStr += lvl.levelNameAndPath_.levelName_ + ", ";

            Utilities.sendMessage("Level(s) " + lvlsStr.Substring(0, lvlsStr.Count()-2) + " added to the playlist !");
        }
    }
}
