using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.PlaylistTools;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class CmdSettingPlayPlayersAddMaps : CmdSettingBool
    {
        public override string FileId { get; } = "playersCanAddMap";
        public override string SettingsId { get; } = "play";

        public override string DisplayName { get; } = "Player !play";
        public override string HelpShort { get; } = "!play: Allow players to add maps";
        public override string HelpLong { get; } = "Whether or not players can add maps using !play";

        public override bool Default { get; } = false;
    }
    class CmdSettingPlayMaxPerCmd : CmdSettingInt
    {
        public override string FileId { get; } = "playMaxPerCmd";
        public override string SettingsId { get; } = "playMaxPerCmd";

        public override string DisplayName { get; } = "Player !play max per command use";
        public override string HelpShort { get; } = "!play: allow players to add only a certain number of maps each time they use `!play`";
        public override string HelpLong { get; } = "Limit players to adding a certain number of maps with !play. A value of 0 allows any number of maps.";

        public override int Default { get; } = 0;
    }
    class CmdSettingPlayMaxPerRound : CmdSettingInt
    {
        public override string FileId { get; } = "playMaxPerRound";
        public override string SettingsId { get; } = "playMaxPerRound";

        public override string DisplayName { get; } = "Player !play max per round use";
        public override string HelpShort { get; } = "!play: allow players to add only a certain number of maps per round";
        public override string HelpLong { get; } = "Limit players to adding a certain number of maps with !play. A value of 0 allows any number of maps.";

        public override int Default { get; } = 1;
    }
    class CmdSettingPlayIsVote : CmdSettingBool
    {
        public override string FileId { get; } = "playIsVote";
        public override string SettingsId { get; } = "playVote";

        public override string DisplayName { get; } = "Play is Vote";
        public override string HelpShort { get; } = "!play acts as !vote play for players.";
        public override string HelpLong { get; } = "For non-hosts, !play will use !vote play instead of adding maps directly. The host still adds maps directly.";

        public override bool Default { get; } = false;
    }
    class CmdSettingPlayFilter : CmdSettingString
    {
        public override string FileId { get; } = "playFilter";
        public override string SettingsId { get; } = "playFilter";

        public override string DisplayName { get; } = "Play Filter";
        public override string HelpShort { get; } = "!play: limit addable maps to a filter. Also affects !level and !vote play";
        public override string HelpLong { get; } = "For non-hosts, !play will only allow maps to be added if they match a filter. Use `!settings playFilter clear` to clear.";

        public override string Default { get; } = "";
    }
    class PlayCmd : Cmd
    {
        public bool playersCanAddMap
        {
            get { return getSetting<CmdSettingPlayPlayersAddMaps>().Value; }
            set { getSetting<CmdSettingPlayPlayersAddMaps>().Value = value; }
        }
        public int maxPerCmd
        {
            get { return getSetting<CmdSettingPlayMaxPerCmd>().Value; }
            set { getSetting<CmdSettingPlayMaxPerCmd>().Value = value; }
        }
        public int maxPerRound
        {
            get { return getSetting<CmdSettingPlayMaxPerRound>().Value; }
            set { getSetting<CmdSettingPlayMaxPerRound>().Value = value; }
        }
        public bool useVote
        {
            get { return getSetting<CmdSettingPlayIsVote>().Value; }
            set { getSetting<CmdSettingPlayIsVote>().Value = value; }
        }
        public string playFilter
        {
            get { return getSetting<CmdSettingPlayFilter>().Value; }
            set { getSetting<CmdSettingPlayFilter>().Value = value; }
        }

        public override string name { get { return "play"; } }
        public override PermType perm { get { return (playersCanAddMap || useVote) ? PermType.ALL : PermType.HOST; } }
        public override bool canUseLocal { get { return false; } }

        public override bool showChatPublic(ClientPlayerInfo p)
        {
            return !p.IsLocal_;
        }

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingPlayPlayersAddMaps(),
            new CmdSettingPlayMaxPerCmd(),
            new CmdSettingPlayMaxPerRound(),
            new CmdSettingPlayIsVote(),
            new CmdSettingPlayFilter()
        };

        Dictionary<string, int> playerVotesThisRound = new Dictionary<string, int>();

        public PlayCmd()
        {
            Events.GameMode.ModeStarted.Subscribe(data =>
            {
                GeneralUtilities.logExceptions(() =>
                {
                    playerVotesThisRound.Clear();
                });
            });
        }

        public override void help(ClientPlayerInfo p)
        {
            if (useVote)
                MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!play [lvl name]") + ": Vote to add a level to the playlist.");
            else
                MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!play [lvl name]") + ": Adds a level to the playlist as the next to be played.");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!play [filter]") + ": Use filters to find a level");
            MessageUtilities.sendMessage(p, "Valid filters: -mode -m -name -n -author -a -index -i -last -l -all");
            MessageUtilities.sendMessage(p, "The level must be known by the server to be shown");
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
                Cmd.all.getCommand<VoteHandler.VoteCMD>("vote").forceNextUse();
                Cmd.all.getCommand<VoteHandler.VoteCMD>("vote").use(p, "y play " + message);
                return;
            }

            if (G.Sys.GameManager_.ModeID_ == GameModeID.Trackmogrify)
            {
                MessageUtilities.sendMessage(p, "You can't manage the playlist in trackmogrify.");
                return;
            }

            FilteredPlaylist filterer = new FilteredPlaylist(GeneralUtilities.getAllLevelsAndModes());

            if (!p.IsLocal_)
                filterer.AddFiltersFromString(playFilter);

            MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(p));
            GeneralUtilities.sendFailures(GeneralUtilities.addFiltersToPlaylist(filterer, p, message, true), 4);
            MessageUtilities.popMessageOptions();

            var list = filterer.Calculate().levelList;

            if (list.Count == 0)
            {
                MessageUtilities.sendMessage(p, "Can't find a level with the filter '" + message + "'.");
                return;
            }

            LevelPlaylist playlist = new LevelPlaylist();
            playlist.CopyFrom(G.Sys.GameManager_.LevelPlaylist_);

            var currentPlaylist = playlist.Playlist_;
            AutoCmd autoCmd = Cmd.all.getCommand<AutoCmd>("auto");
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
            playerVotesThisRound.TryGetValue(GeneralUtilities.getUniquePlayerString(p), out countRound);

            string lvlsStr = "";
            foreach (var lvl in list)
            {
                if (countRound + 1 > maxPerRoundLocal)
                {
                    MessageUtilities.sendMessage(p, $"You have reached the maximum amount of maps you can add per round ({maxPerRound})");
                    break;
                }
                if (countCmd + 1 > maxPerCmdLocal)
                {
                    MessageUtilities.sendMessage(p, "You have reached the maximum amount of maps you can add per " + GeneralUtilities.formatCmd("!play") + $" ({maxPerCmd})");
                    break;
                }
                countRound++;
                countCmd++;
                if (countCmd <= 10)
                    lvlsStr += lvl.levelNameAndPath_.levelName_ + ", ";
                currentPlaylist.Insert(index + countCmd - 1, lvl);
            }
            playerVotesThisRound[GeneralUtilities.getUniquePlayerString(p)] = countRound;

            if (countCmd > 0)
            {
                G.Sys.GameManager_.LevelPlaylist_.Clear();

                foreach (var lvl in currentPlaylist)
                    G.Sys.GameManager_.LevelPlaylist_.Add(lvl);
                G.Sys.GameManager_.LevelPlaylist_.SetIndex(origIndex);

                lvlsStr = lvlsStr.Substring(0, lvlsStr.Count() - 2);
                if (countCmd > 10)
                    lvlsStr = lvlsStr + $" and {countCmd - 10} more";

                if (p.IsLocal_)
                    MessageUtilities.sendMessage(p, "Level(s) " + lvlsStr + " added to the playlist !");
                else
                    MessageUtilities.sendMessage("Level(s) " + lvlsStr + " added to the playlist by " + p.GetChatName() + "!");
            }
        }
    }
}
