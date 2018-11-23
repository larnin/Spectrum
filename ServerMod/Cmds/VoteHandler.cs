using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.PlaylistTools;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class VoteHandler
    {

        private string voteCmdPattern = @"^\s*(\S+)\s+(\S+)\s*(.*)$";
        private string voteCmdShortcutPattern = @"^\s*(\S+)\s*(.*)$";
        private string voteCtrlPattern = @"^\s*(\S+)\s+(\d+)$";
        
        private Dictionary<string, List<string>> votes;

        private CmdList list;

        public VoteCMD voteCommand;
        public VoteCtrlCMD voteControlCommand;

        public VoteHandler(CmdList list)
        {
            this.list = list;

            votes = new Dictionary<string, List<string>>();
            votes.Add("skip", new List<string>());
            votes.Add("stop", new List<string>());
            votes.Add("play", new List<string>());
            votes.Add("kick", new List<string>());
            votes.Add("count", new List<string>());

            voteCommand = new VoteCMD(this);
            voteControlCommand = new VoteCtrlCMD(this);
        }

       public class VoteCtrlCMD : Cmd
        {
            public override string name { get { return "votectrl"; } }
            public override PermType perm { get { return PermType.HOST; } }
            public override bool canUseLocal { get { return false; } }

            public VoteHandler parent;

            public VoteCtrlCMD(VoteHandler parent)
            {
                this.parent = parent;
            }

            public override void help(ClientPlayerInfo p)
            {
                MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!votectrl <voteType> <percent>") + ": Set <percent> as the amount of players needed for a <voteType> vote to succeed.");
                MessageUtilities.sendMessage(p, "<percent> should be an integer 0 - 100. Set above 100 to disable.");
            }

            public override void use(ClientPlayerInfo p, string message)
            {
                var match = Regex.Match(message, parent.voteCtrlPattern);

                if (!match.Success)
                {
                    help(p);
                    return;
                }

                string voteType = match.Groups[1].Value.ToLower();
                string percentS = match.Groups[2].Value;
                int percent = int.Parse(percentS);

                if (!parent.voteCommand.voteThresholds.ContainsKey(voteType))
                {
                    MessageUtilities.sendMessage(p, $"Invalid <voteType>. ({voteType})");
                    help(p);
                    return;
                }

                parent.voteCommand.voteThresholds[voteType] = Convert.ToDouble(percent) / 100.0;
                Entry.save();
                MessageUtilities.sendMessage(p, $"Pass threshold for {voteType} changed to {percent}%");
            }
        }

        class CmdSettingVotesEnabled : CmdSettingBool
        {
            public override string FileId { get; } = "allowVoteSystem";
            public override string SettingsId { get; } = "voteSystem";

            public override string DisplayName { get; } = "!vote On/Off";
            public override string HelpShort { get; } = "!vote enable/disable";
            public override string HelpLong { get; } = "Whether or not players can use votes with !vote";

            public override bool Default { get; } = false;
        }
        class CmdSettingVoteThreshold : CmdSettingInt
        {
            private string ThresholdName;
            private int DefaultThreshold;

            public override string FileId { get; } = ""; // disabled
            public override string SettingsId { get { return "vote" + ThresholdName; } }

            public override string DisplayName { get { return "Vote Threshold " + ThresholdName; } }
            public override string HelpShort { get { return "The percent where " + ThresholdName + " vote passes"; } }
            public override string HelpLong { get { return HelpShort; } }

            public override int Default { get { return DefaultThreshold; } }

            public override int LowerBound { get; } = 0;
            
            public CmdSettingVoteThreshold(string ThresholdName, int DefaultThreshold)
            {
                this.ThresholdName = ThresholdName;
                this.DefaultThreshold = DefaultThreshold;
            }

            public CmdSettingVoteThreshold(string ThresholdName, int DefaultThreshold, int Value)
            {
                this.ThresholdName = ThresholdName;
                this.DefaultThreshold = DefaultThreshold;
                this.Value = Value;
            }
        }
        class CmdSettingVoteThresholds : CmdSetting<Dictionary<string, double>>
        {
            public override string FileId { get; } = "voteSystemThresholds";
            public override string SettingsId { get; } = "";  // disabled

            public override string DisplayName { get; } = "Vote System Thresholds";
            public override string HelpShort { get; } = "The thresholds at which each vote passes";
            public override string HelpLong { get { return HelpShort; } }

            public override Dictionary<string, double> Default
            {
                get
                {
                    var thresholds = new Dictionary<string, double>();
                    thresholds.Add("skip", 0.55);
                    thresholds.Add("stop", 0.55);
                    thresholds.Add("play", 0.55);
                    thresholds.Add("kick", 0.7);
                    thresholds.Add("count", 0.6);
                    return thresholds;
                }
            }

            public override UpdateResult<Dictionary<string, double>> UpdateFromString(string input)
            {
                throw new NotImplementedException();
            }

            public override UpdateResult<Dictionary<string, double>> UpdateFromObject(object input)
            {
                if (input.GetType() != typeof(Dictionary<string, object>))
                {
                    return new UpdateResult<Dictionary<string, double>>(false, Default, "Invalid dictionary. Resetting to default.");
                }
                try
                {
                    var thresholds = new Dictionary<string, double>();
                    foreach (KeyValuePair<string, object> entry in (Dictionary<string, object>) input)
                    {
                        thresholds[entry.Key] = (double)entry.Value;
                    }
                    return new UpdateResult<Dictionary<string, double>>(true, thresholds);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error reading dictionary: {e}");
                    return new UpdateResult<Dictionary<string, double>>(false, Default, "Error reading dictionary. Resetting to default.");
                }
            }
        }

        public class VoteCMD : Cmd
        {
            public override string name { get { return "vote"; } }
            public override PermType perm { get { return PermType.ALL; } }
            public override bool canUseLocal { get { return false; } }

            public override bool showChatPublic(ClientPlayerInfo p)
            {
                return true;
            }

            public VoteHandler parent;

            public bool votesAllowed
            {
                get { return getSetting<CmdSettingVotesEnabled>().Value; }
                set { getSetting<CmdSettingVotesEnabled>().Value = value; }
            }
            public Dictionary<string, double> voteThresholds
            {
                get { return getSetting<CmdSettingVoteThresholds>().Value; }
                set { getSetting<CmdSettingVoteThresholds>().Value = value; }
            }

            private bool doForceNextUse = false;
            private bool votedSkip = false;

            public override CmdSetting[] settings { get; } = {
                new CmdSettingVotesEnabled(),
                new CmdSettingVoteThresholds()
            };



            public VoteCMD(VoteHandler parent)
            {
                this.parent = parent;

                Events.GameMode.ModeStarted.Subscribe(data =>
                {
                    GeneralUtilities.logExceptions(() =>
                    {
                        parent.votes["skip"].Clear();
                        parent.votes["stop"].Clear();

                        votedSkip = false;
                    });
                });
            }

            public override void help(ClientPlayerInfo p)
            {
                MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(p));
                MessageUtilities.sendMessage(GeneralUtilities.formatCmd("!vote <voteType> <parameters>") + ": Vote for <voteType>");
                MessageUtilities.sendMessage(GeneralUtilities.formatCmd("!vote i <voteType>") + ": View information about <voteType>");
                MessageUtilities.sendMessage("voteTypes:");
                MessageUtilities.sendMessage(" " + GeneralUtilities.formatCmd("!vote skip") + ": skip the current map.");
                MessageUtilities.sendMessage(" " + GeneralUtilities.formatCmd("!vote stop") + ": stop the countdown.");
                MessageUtilities.sendMessage(" " + GeneralUtilities.formatCmd("!vote play <mapName>") + ": vote to play <mapName>. Use !level to find maps.");
                MessageUtilities.sendMessage("Examples:");
                MessageUtilities.sendMessage(" " + GeneralUtilities.formatCmd("!vote play inferno") + ": vote to play inferno");
                MessageUtilities.sendMessage(" " + GeneralUtilities.formatCmd("!vote i skip") + ": view info about the skip vote");
                MessageUtilities.sendMessage(" " + GeneralUtilities.formatCmd("!vote stop") + ": vote to stop the countdown");
                /*
                MessageUtilities.sendMessage(" !vote <y/n> kick <player>: vote to kick <player> from the game.");
                MessageUtilities.sendMessage(" !vote <y/n> count <time>: vote for <time> to be the new max time.");
                MessageUtilities.sendMessage("  if <time> is 'off' then the vote is to disable max time.");
                MessageUtilities.sendMessage("  if <time> is 'bronze'/'silver'/'gold'/'diamond' it is the relevant time for the map.");
                */
                MessageUtilities.popMessageOptions();
            }

            public void forceNextUse()
            {
                // ignore votes on/off settings for next use
                // used by other commands when calling vote
                doForceNextUse = true;
            }

            public override void use(ClientPlayerInfo p, string message)
            {
                if (!doForceNextUse)
                {
                    if (!votesAllowed)
                    {
                        MessageUtilities.sendMessage(p, "Votes are disabled.");
                        return;
                    }
                }
                else
                    doForceNextUse = false;

                bool voteValue;
                bool isInfo;

                string voteType;
                string parameter;

                var matchShortcut = Regex.Match(message, parent.voteCmdShortcutPattern);
                var shortcutVoteType = matchShortcut.Groups[1].Value.ToLower();
                if (matchShortcut.Success && voteThresholds.ContainsKey(shortcutVoteType))
                {
                    voteValue = true;
                    isInfo = false;
                    voteType = shortcutVoteType;
                    parameter = matchShortcut.Groups[2].Value;
                }
                else
                {
                    var match = Regex.Match(message, parent.voteCmdPattern);

                    if (!match.Success)
                    {
                        help(p);
                        return;
                    }

                    string voteValueS = match.Groups[1].Value.ToLower();
                    voteValue = false;
                    isInfo = false;
                    if (voteValueS == "yes" || voteValueS == "y" || voteValueS == "1" || voteValueS == "true" || voteValueS == "t")
                    {
                        voteValue = true;
                    }
                    else if (voteValueS == "no" || voteValueS == "n" || voteValueS == "0" || voteValueS == "false" || voteValueS == "f")
                    {
                        voteValue = false;
                    }
                    else if (voteValueS == "info" || voteValueS == "i")
                    {
                        isInfo = true;
                    }
                    else
                    {
                        MessageUtilities.sendMessage(p, "Invalid <voteType>, or invalid [yes/no] You can use yes/no, y/n, 1/0, true/false, and t/f.");
                        MessageUtilities.sendMessage(p, "You can use [info/i] to get info.");
                        MessageUtilities.sendMessage(p, "<voteType>s: skip, stop, play");
                        return;
                    }

                    voteType = match.Groups[2].Value.ToLower();
                    parameter = match.Groups[3].Value;
                }


                if (!voteThresholds.ContainsKey(voteType))
                {
                    MessageUtilities.sendMessage(p, $"Invalid <voteType>. ({voteType})");
                    help(p);
                    return;
                }

                AutoCmd autoCommand = parent.list.getCommand<AutoCmd>("auto");
                AutoSpecCmd autoSpecCommand = parent.list.getCommand<AutoSpecCmd>("autospec");
                int playerOffset = (autoCommand.autoMode && autoSpecCommand.autoSpecMode) ? -1 : 0;

                int numPlayers = G.Sys.PlayerManager_.PlayerList_.Count + playerOffset;

                if (isInfo)
                {
                    MessageUtilities.sendMessage(p, $"Info for {voteType}:");
                    MessageUtilities.sendMessage(p, $"Pass threshold: {Convert.ToInt32(Math.Floor(voteThresholds[voteType]*100))}%");
                    int value = parent.votes[voteType].Count;
                    MessageUtilities.sendMessage(p, $"Votes: {value}/{Convert.ToInt32(Math.Ceiling(voteThresholds[voteType] * Convert.ToDouble(numPlayers)))}");
                    return;
                }


                if (voteType == "skip")
                {
                    if (GeneralUtilities.isOnLobby())
                    {
                        MessageUtilities.sendMessage(p, "You can't vote to skip in the lobby!");
                        return;
                    }

                    if (votedSkip)
                    {
                        MessageUtilities.sendMessage(p, "Vote skip already succeeded.");
                        return;
                    }

                    List<string> votes = parent.votes[voteType];

                    int value = votes.Count;
                    

                    string playerVoteId = GeneralUtilities.getUniquePlayerString(p);
                    if (voteValue)
                    {
                        if (votes.Contains(playerVoteId))
                        {
                            MessageUtilities.sendMessage($"{p.Username_} has already voted to skip {G.Sys.GameManager_.LevelName_}.");
                            return;
                        }
                        votes.Add(playerVoteId);
                        value = votes.Count;
                        MessageUtilities.sendMessage($"{p.Username_} voted to skip {G.Sys.GameManager_.LevelName_}.");
                        if (Convert.ToDouble(value) / Convert.ToDouble(numPlayers) < voteThresholds[voteType])
                            MessageUtilities.sendMessage($"Votes: {value}/{Convert.ToInt32(Math.Ceiling(voteThresholds[voteType] * Convert.ToDouble(numPlayers)))}");
                        else
                        {
                            votedSkip = true;
                            MessageUtilities.sendMessage("Vote skip succeeded! Skipping map...");
                            parent.advanceLevel();
                        }
                    }
                    else
                    {
                        votes.Remove(playerVoteId);
                        value = votes.Count;
                        MessageUtilities.sendMessage($"{p.Username_} unvoted to skip {G.Sys.GameManager_.LevelName_}.");
                        MessageUtilities.sendMessage($"Votes: {value}/{Convert.ToInt32(Math.Ceiling(voteThresholds[voteType] * Convert.ToDouble(numPlayers)))}");
                        return;
                    }
                }
                else if (voteType == "stop")
                {
                    if (GeneralUtilities.isOnLobby())
                    {
                        MessageUtilities.sendMessage(p, "You can't vote to stop the countdown in the lobby!");
                        return;
                    }

                    List<string> votes = parent.votes[voteType];
                    
                    int value = votes.Count;

                    string playerVoteId = GeneralUtilities.getUniquePlayerString(p);
                    if (voteValue)
                    {
                        if (votes.Contains(playerVoteId))
                        {
                            MessageUtilities.sendMessage($"{p.Username_} has already voted to stop the countdown.");
                            return;
                        }
                        votes.Add(playerVoteId);
                        value = votes.Count;
                        MessageUtilities.sendMessage($"{p.Username_} voted to stop the countdown.");
                        if (Convert.ToDouble(value) / Convert.ToDouble(numPlayers) < voteThresholds[voteType])
                            MessageUtilities.sendMessage($"Votes: {value}/{Convert.ToInt32(Math.Ceiling(voteThresholds[voteType] * Convert.ToDouble(numPlayers)))}");
                        else
                        {
                            parent.votes[voteType].Clear();
                            CountdownCmd command = parent.list.getCommand<CountdownCmd>("countdown");
                            command.stopCountdown();
                            MessageUtilities.sendMessage("Vote stop succeeded! Stopping countdown...");
                        }
                    }
                    else
                    {
                        votes.Remove(playerVoteId);
                        value = votes.Count;
                        MessageUtilities.sendMessage($"{p.Username_} unvoted to stop the countdown.");
                        MessageUtilities.sendMessage($"Votes: {value}/{Convert.ToInt32(Math.Ceiling(voteThresholds[voteType] * Convert.ToDouble(numPlayers)))}");
                        return;
                    }
                }
                else if (voteType == "play")
                {

                    if (GeneralUtilities.isOnLobby())
                    {
                        MessageUtilities.sendMessage(p, "You can't vote for the next level in the lobby.");
                        return;
                    }

                    if (G.Sys.GameManager_.ModeID_ == GameModeID.Trackmogrify)
                    {
                        MessageUtilities.sendMessage(p, "You can't vote for levels in trackmogrify.");
                        return;
                    }

                    string levelName = parameter;

                    FilteredPlaylist filterer = new FilteredPlaylist(GeneralUtilities.getAllLevelsAndModes());
                    if (!p.IsLocal_)
                    {
                        PlayCmd playCmd = Cmd.all.getCommand<PlayCmd>("play");
                        filterer.AddFiltersFromString(playCmd.playFilter);
                    }
                    MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(p));
                    GeneralUtilities.sendFailures(GeneralUtilities.addFiltersToPlaylist(filterer, p, levelName, true), 4);
                    MessageUtilities.popMessageOptions();

                    List<LevelPlaylist.ModeAndLevelInfo> lvls = filterer.Calculate().levelList;
                    List<LevelResultsSortInfo> levelResults = new List<LevelResultsSortInfo>();
                    
                    double threshold = voteThresholds["play"];

                    LevelPlaylist playlist = new LevelPlaylist();
                    playlist.CopyFrom(G.Sys.GameManager_.LevelPlaylist_);
                    var currentPlaylist = playlist.Playlist_;
                    AutoCmd autoCmd = Cmd.all.getCommand<AutoCmd>("auto");
                    int origIndex = G.Sys.GameManager_.LevelPlaylist_.Index_;
                    int index = autoCmd.getInsertIndex();

                    int addCount = 0;
                    foreach (LevelPlaylist.ModeAndLevelInfo lvl in lvls)
                    {

                        string id = $"{lvl.mode_}:{lvl.levelNameAndPath_.levelName_}:{lvl.levelNameAndPath_.levelPath_}";
                        List<string> votes;
                        if (!parent.votes.TryGetValue(id, out votes))
                        {
                            if (voteValue)
                            {
                                votes = new List<string>();
                                parent.votes[id] = votes;
                            }
                            else
                            {
                                //MessageUtilities.sendMessage($"{p.Username_} unvoted for these maps.");
                                levelResults.Add(new LevelResultsSortInfo(lvl, 0, numPlayers, threshold, -1));
                                continue;
                            }
                        }
                        string playerVoteId = GeneralUtilities.getUniquePlayerString(p);
                        if (!voteValue)
                        {
                            votes.Remove(playerVoteId);
                            if (votes.Count == 0)
                            {
                                parent.votes.Remove(id);
                            }
                            //MessageUtilities.sendMessage($"{p.Username_} unvoted for these maps.");
                            levelResults.Add(new LevelResultsSortInfo(lvl, votes.Count, numPlayers, threshold, -1));
                            continue;
                        }
                        else
                        {
                            if (votes.Contains(playerVoteId))
                            {
                                //MessageUtilities.sendMessage($"{p.Username_} has already voted for these maps.");
                                levelResults.Add(new LevelResultsSortInfo(lvl, votes.Count, numPlayers, threshold, 0));
                                //return;
                                continue;
                            }
                            else
                            {
                                votes.Add(playerVoteId);
                                levelResults.Add(new LevelResultsSortInfo(lvl, votes.Count, numPlayers, threshold, 1));
                                if (Convert.ToDouble(votes.Count) / Convert.ToDouble(numPlayers) >= threshold)
                                {
                                    currentPlaylist.Insert(index + addCount, lvl);
                                    parent.votes.Remove(id);
                                    addCount++;
                                }
                            }
                        }
                    }

                    G.Sys.GameManager_.LevelPlaylist_.Clear();
                    foreach (var lvl in currentPlaylist)
                        G.Sys.GameManager_.LevelPlaylist_.Add(lvl);
                    G.Sys.GameManager_.LevelPlaylist_.SetIndex(origIndex);

                    var playersThreshold = Convert.ToInt32(Math.Ceiling(voteThresholds[voteType] * Convert.ToDouble(numPlayers)));

                    if (lvls.Count == 0)
                    {
                        MessageUtilities.sendMessage(p, "Can't find any levels with the filter '" + levelName + "'.");
                    }

                    string newMessage;
                    int count;

                    newMessage = "";
                    count = 0;
                    foreach (LevelResultsSortInfo level in levelResults)
                    {
                        if (level.voteState == -1)
                        {
                            if (++count <= 10)
                            {
                                newMessage = newMessage + level.level.levelNameAndPath_.levelName_ + $"({level.votes}/{playersThreshold}), ";
                            }
                        }
                    }
                    if (count != 0)
                    {
                        if (count > 10)
                        {
                            MessageUtilities.sendMessage($"{p.Username_} unvoted for {newMessage} and {count - 10} more.");
                        }
                        else
                        {
                            MessageUtilities.sendMessage($"{p.Username_} unvoted for {newMessage}.");
                        }
                    }

                    newMessage = "";
                    count = 0;
                    foreach (LevelResultsSortInfo level in levelResults)
                    {
                        if (level.voteState >= 0)
                        {
                            if (++count <= 10)
                            {
                                newMessage = newMessage + level.level.levelNameAndPath_.levelName_ + $"({level.votes}/{playersThreshold}), ";
                            }
                        }
                    }
                    if (count != 0)
                    {
                        if (count > 10)
                        {
                            MessageUtilities.sendMessage($"{p.Username_} voted for {newMessage} and {count - 10} more.");
                        }
                        else
                        {
                            MessageUtilities.sendMessage($"{p.Username_} voted for {newMessage}.");
                        }
                    }

                    newMessage = "";
                    count = 0;
                    foreach (LevelResultsSortInfo level in levelResults)
                    {
                        if (level.success)
                        {
                            if (++count <= 10)
                            {
                                newMessage = newMessage + level.level.levelNameAndPath_.levelName_ + ", ";
                            }
                        }
                    }
                    if (count != 0)
                    {
                        if (count > 10)
                        {
                            MessageUtilities.sendMessage(newMessage + $"and {count - 10} more added.");
                        }
                        else
                        {
                            MessageUtilities.sendMessage(newMessage + "added.");
                        }
                    }
                }
            }
        }

        class LevelResultsSortInfo
        {
            public LevelPlaylist.ModeAndLevelInfo level;
            public int votes;
            public bool success;
            public int voteState;
            public LevelResultsSortInfo(LevelPlaylist.ModeAndLevelInfo level, int votes, int numPlayers, double threshold, int voteState)
            {
                this.level = level;
                this.votes = votes;
                success = Convert.ToDouble(votes) / Convert.ToDouble(numPlayers) >= threshold;
                this.voteState = voteState;
            }
        }

        void advanceLevel()
        {
            AutoCmd autoCommand = list.getCommand<AutoCmd>("auto");
            if (autoCommand.autoMode)
                autoCommand.nextLevel();
            else G.Sys.GameManager_.StartCoroutine(waitAndGoNext());
        }

        IEnumerator waitAndGoNext()
        {
            if (!GeneralUtilities.isOnLobby())
            {
                MessageUtilities.sendMessage("Going to the next level in 10 seconds...");
                MessageUtilities.sendMessage("Next level is : " + GeneralUtilities.getNextLevelName());
                yield return new WaitForSeconds(10.0f);
                if (!GeneralUtilities.isOnLobby())
                {
                    if (GeneralUtilities.isCurrentLastLevel())
                    {
                        G.Sys.GameManager_.GoToLobby();
                        MessageUtilities.sendMessage("No more levels in the playlist.");
                    }
                    else G.Sys.GameManager_.GoToNextLevel();
                }
            }
            yield return null;
        }
    }
}
