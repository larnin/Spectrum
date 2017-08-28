using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.PlaylistTools;
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
    class CmdSettingPlayMaxPerCmd : CmdSettingInt
    {
        public override string FileId { get; } = "playMaxPerCmd";
        public override string SettingsId { get; } = "playMaxPerCmd";

        public override string DisplayName { get; } = "Player !play max per command use";
        public override string HelpShort { get; } = "!play: allow players to add only a certain number of maps each time they use `!play`";
        public override string HelpLong { get; } = "Limit players to adding a certain number of maps with !play. A value of 0 allows any number of maps.";

        public override object Default { get; } = 0;
    }
    class CmdSettingPlayMaxPerRound : CmdSettingInt
    {
        public override string FileId { get; } = "playMaxPerRound";
        public override string SettingsId { get; } = "playMaxPerRound";

        public override string DisplayName { get; } = "Player !play max per round use";
        public override string HelpShort { get; } = "!play: allow players to add only a certain number of maps per round";
        public override string HelpLong { get; } = "Limit players to adding a certain number of maps with !play. A value of 0 allows any number of maps.";

        public override object Default { get; } = 1;
    }
    class CmdSettingPlayIsVote : CmdSettingBool
    {
        public override string FileId { get; } = "playIsVote";
        public override string SettingsId { get; } = "playVote";

        public override string DisplayName { get; } = "Play is Vote";
        public override string HelpShort { get; } = "!play acts as !vote play for players.";
        public override string HelpLong { get; } = "For non-hosts, !play will use !vote play instead of adding maps directly. The host still adds maps directly.";

        public override object Default { get; } = false;
    }
    class CmdSettingPlayFilter : CmdSettingString
    {
        public override string FileId { get; } = "playFilter";
        public override string SettingsId { get; } = "playFilter";

        public override string DisplayName { get; } = "Play Filter";
        public override string HelpShort { get; } = "!play: limit addable maps to a filter. Also affects !level and !vote play";
        public override string HelpLong { get; } = "For non-hosts, !play will only allow mpas to be added if they match a filter. Use `!settings playFilter clear` to clear.";

        public override object Default { get; } = "";
    }
    class PlayCMD : cmd
    {
        public bool playersCanAddMap
        {
            get { return (bool)getSetting("playersCanAddMap").Value; }
            set { getSetting("playersCanAddMap").Value = value; }
        }
        public int maxPerCmd
        {
            get { return (int)getSetting("playMaxPerCmd").Value; }
            set { getSetting("playMaxPerCmd").Value = value; }
        }
        public int maxPerRound
        {
            get { return (int)getSetting("playMaxPerRound").Value; }
            set { getSetting("playMaxPerRound").Value = value; }
        }
        public bool useVote
        {
            get { return (bool)getSetting("playIsVote").Value; }
            set { getSetting("playIsVote").Value = value; }
        }
        public string playFilter
        {
            get { return (string)getSetting("playFilter").Value; }
            set { getSetting("playFilter").Value = value; }
        }

        public override string name { get { return "play"; } }
        public override PermType perm { get { return (playersCanAddMap || useVote) ? PermType.ALL : PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingPlayPlayersAddMaps(),
            new CmdSettingPlayMaxPerCmd(),
            new CmdSettingPlayMaxPerRound(),
            new CmdSettingPlayIsVote(),
            new CmdSettingPlayFilter()
        };

        Dictionary<string, int> playerVotesThisRound = new Dictionary<string, int>();

        public PlayCMD()
        {
            Events.GameMode.ModeStarted.Subscribe(data =>
            {
                Utilities.testFunc(() =>
                {
                    playerVotesThisRound.Clear();
                });
            });
        }

        public override void help(ClientPlayerInfo p)
        {
            if (useVote)
                Utilities.sendMessage(Utilities.formatCmd("!play [lvl name]") + ": Vote to add a level to the playlist.");
            else
                Utilities.sendMessage(Utilities.formatCmd("!play [lvl name]") + ": Adds a level to the playlist as the next to be played.");
            Utilities.sendMessage(Utilities.formatCmd("!play [filter]") + ": Use filters to find a level");
            Utilities.sendMessage("Valid filters: -mode -m -name -n -author -a -index -i -last -l -all");
            Utilities.sendMessage("The level must be known by the server to be shown");
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
                cmd.all.getCommand<VoteHandler.VoteCMD>("vote").forceNextUse();
                cmd.all.getCommand<VoteHandler.VoteCMD>("vote").use(p, "y play " + message);
                return;
            }

            if (G.Sys.GameManager_.ModeID_ == GameModeID.Trackmogrify)
            {
                Utilities.sendMessage("You can't manage the playlist in trackmogrify.");
                return;
            }

            FilteredPlaylist filterer = new FilteredPlaylist(Utilities.getAllLevelsAndModes());

            if (!p.IsLocal_)
                filterer.AddFiltersFromString(playFilter);

            Utilities.sendFailures(Utilities.addFiltersToPlaylist(filterer, p, message, true), 4);

            var list = filterer.Calculate().levelList;

            if (list.Count == 0)
            {
                Utilities.sendMessage("Can't find a level with the filter '" + message + "'.");
                return;
            }

            LevelPlaylist playlist = new LevelPlaylist();
            playlist.Copy(G.Sys.GameManager_.LevelPlaylist_);

            var currentPlaylist = playlist.Playlist_;
            AutoCMD autoCmd = cmd.all.getCommand<AutoCMD>("auto");
            int origIndex = G.Sys.GameManager_.LevelPlaylist_.Index_;
            int index = autoCmd.getInsertIndex();

            var maxPerRoundLocal = 0;
            if (p.IsLocal_ || maxPerRound <= 0)
                maxPerRoundLocal = int.MaxValue;
            else
                maxPerRoundLocal = maxPerRound;

            var maxPerCmdLocal = 0;
            if (p.IsLocal_ || maxPerCmd <= 0)
                maxPerCmdLocal = int.MaxValue;
            else
                maxPerCmdLocal = maxPerCmd;

            var countCmd = 0;

            var countRound = 0;
            playerVotesThisRound.TryGetValue(Utilities.getUniquePlayerString(p), out countRound);

            string lvlsStr = "";
            foreach (var lvl in list)
            {
                if (countRound + 1 > maxPerRoundLocal)
                {
                    Utilities.sendMessage($"You have reached the maximum amount of maps you can add per round ({maxPerRound})");
                    break;
                }
                if (countCmd + 1 > maxPerCmdLocal)
                {
                    Utilities.sendMessage("You have reached the maximum amount of maps you can add per " + Utilities.formatCmd("!play") + $" ({maxPerCmd})");
                    break;
                }
                countRound++;
                countCmd++;
                if (countCmd <= 10)
                    lvlsStr += lvl.levelNameAndPath_.levelName_ + ", ";
                currentPlaylist.Insert(index + countCmd - 1, lvl);
            }
            playerVotesThisRound[Utilities.getUniquePlayerString(p)] = countRound;

            if (countCmd > 0)
            {
                G.Sys.GameManager_.LevelPlaylist_.Clear();

                foreach (var lvl in currentPlaylist)
                    G.Sys.GameManager_.LevelPlaylist_.Add(lvl);
                G.Sys.GameManager_.LevelPlaylist_.SetIndex(origIndex);

                lvlsStr = lvlsStr.Substring(0, lvlsStr.Count() - 2);
                if (countCmd > 10)
                    lvlsStr = lvlsStr + $" and {countCmd - 10} more";

                Utilities.sendMessage("Level(s) " + lvlsStr + " added to the playlist !");
            }
        }
    }
}
