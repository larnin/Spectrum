using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class VoteHandler
    {

        private string voteCmdPattern = @"^\s*(\S+)\s+(\S+)\s*(.*)$";
        private string voteCmdShortcutPattern = @"^\s*(\S+)\s*(.*)$";
        private string voteCtrlPattern = @"^\s*(\S+)\s+(\d+)$";

        public static Dictionary<string, double> thresholds = null;
        private Dictionary<string, List<string>> votes;

        private cmdlist list;

        public VoteCMD voteCommand;
        public VoteCtrlCMD voteControlCommand;

        public VoteHandler(cmdlist list)
        {
            this.list = list;
            thresholds = new Dictionary<string, double>();
            thresholds.Add("skip", 0.55);
            thresholds.Add("stop", 0.55);
            thresholds.Add("play", 0.55);
            thresholds.Add("kick", 0.7);
            thresholds.Add("count", 0.6);

            Entry.load();  // load any existing thresholds
            Entry.save();  // save any thresholds that were not loaded

            votes = new Dictionary<string, List<string>>();
            votes.Add("skip", new List<string>());
            votes.Add("stop", new List<string>());
            votes.Add("play", new List<string>());
            votes.Add("kick", new List<string>());
            votes.Add("count", new List<string>());

            voteCommand = new VoteCMD(this);
            voteControlCommand = new VoteCtrlCMD(this);
        }

       public class VoteCtrlCMD : cmd
        {
            public override string name { get { return "votectrl"; } }
            public override PermType perm { get { return PermType.HOST; } }
            public override bool canUseAsClient { get { return false; } }

            public VoteHandler parent;

            public VoteCtrlCMD(VoteHandler parent)
            {
                this.parent = parent;
            }

            public override void help(ClientPlayerInfo p)
            {
                Utilities.sendMessage("!votectrl <voteType> <percent>: Set <percent> as the amount of players needed for a <voteType> vote to succeed.");
                Utilities.sendMessage("<percent> should be an integer 0 - 100. Set above 100 to disable.");
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

                if (!VoteHandler.thresholds.ContainsKey(voteType))
                {
                    Utilities.sendMessage($"Invalid <voteType>. ({voteType})");
                    help(p);
                    return;
                }

                VoteHandler.thresholds[voteType] = Convert.ToDouble(percent) / 100.0;
                Utilities.sendMessage($"Pass threshold for {voteType} changed to {percent}%");
            }
        }

        public class VoteCMD : cmd
        {
            public override string name { get { return "vote"; } }
            public override PermType perm { get { return PermType.ALL; } }
            public override bool canUseAsClient { get { return false; } }

            public VoteHandler parent;

            public static bool votesAllowed = false;

            private bool doForceNextUse = false;
            private bool votedSkip = false;

            public VoteCMD(VoteHandler parent)
            {
                this.parent = parent;
                
                Events.ServerToClient.ModeFinished.Subscribe(data =>
                {
                    parent.votes["skip"].Clear();
                    parent.votes["stop"].Clear();

                    votedSkip = false;
                });
            }

            public override void help(ClientPlayerInfo p)
            {
                Utilities.sendMessage("!vote <voteType> <parameters>: Vote yes on the given voteType.");
                Utilities.sendMessage("!vote <yes/no> <voteType> <parameters>: Vote yes/no on the given voteType. y/n also work.");
                Utilities.sendMessage("!vote <info/i> <voteType>: View information about the voteType.");
                Utilities.sendMessage("voteTypes:");
                Utilities.sendMessage(" !vote [y/n] skip: skip the current map.");
                Utilities.sendMessage(" !vote [y/n] stop: stop the countdown.");
                Utilities.sendMessage(" !vote [y/n] play <mapName>: vote to play map matching mapName. Use !level to find maps. Uses the same syntax as !level.");
                Utilities.sendMessage("Examples:");
                Utilities.sendMessage(" !vote play inferno: vote yes on playing inferno");
                Utilities.sendMessage(" !vote n stop: cancel your vote to stop the countdown");
                Utilities.sendMessage(" !vote i skip: view info about the skip vote ");
                /*
                Utilities.sendMessage(" !vote <y/n> kick <player>: vote to kick <player> from the game.");
                Utilities.sendMessage(" !vote <y/n> count <time>: vote for <time> to be the new max time.");
                Utilities.sendMessage("  if <time> is 'off' then the vote is to disable max time.");
                Utilities.sendMessage("  if <time> is 'bronze'/'silver'/'gold'/'diamond' it is the relevant time for the map.");
                */
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
                        Utilities.sendMessage("Votes are disabled.");
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
                if (matchShortcut.Success && thresholds.ContainsKey(shortcutVoteType))
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
                        Utilities.sendMessage("Invalid <voteType>, or invalid [yes/no] You can use yes/no, y/n, 1/0, true/false, and t/f.");
                        Utilities.sendMessage("You can use [info/i] to get info.");
                        Utilities.sendMessage("<voteType>s: skip, stop, play");
                        return;
                    }

                    voteType = match.Groups[2].Value.ToLower();
                    parameter = match.Groups[3].Value;
                }


                if (!VoteHandler.thresholds.ContainsKey(voteType))
                {
                    Utilities.sendMessage($"Invalid <voteType>. ({voteType})");
                    help(p);
                    return;
                }

                AutoCMD autoCommand = (AutoCMD)parent.list.getCommand("auto");
                AutoSpecCMD autoSpecCommand = (AutoSpecCMD)parent.list.getCommand("autospec");
                int playerOffset = (autoCommand.autoMode && autoSpecCommand.autoSpecMode) ? -1 : 0;

                int numPlayers = G.Sys.PlayerManager_.PlayerList_.Count + playerOffset;

                if (isInfo)
                {
                    Utilities.sendMessage($"Info for {voteType}:");
                    Utilities.sendMessage($"Pass threshold: {Convert.ToInt32(Math.Floor(VoteHandler.thresholds[voteType]*100))}%");
                    int value = parent.votes[voteType].Count;
                    Utilities.sendMessage($"Votes: {value}/{Convert.ToInt32(Math.Ceiling(VoteHandler.thresholds[voteType] * Convert.ToDouble(numPlayers)))}");
                    return;
                }


                if (voteType == "skip")
                {
                    if (Utilities.isOnLobby())
                    {
                        Utilities.sendMessage("You can't vote to skip in the lobby!");
                        return;
                    }

                    if (votedSkip)
                    {
                        Utilities.sendMessage("Vote skip already succeeded.");
                        return;
                    }

                    List<string> votes = parent.votes[voteType];

                    int value = votes.Count;

                    string playerVoteId = $"{p.Username_}:{p.NetworkPlayer_.externalIP}:{p.NetworkPlayer_.externalPort}";
                    if (voteValue)
                    {
                        if (votes.Contains(playerVoteId))
                        {
                            Utilities.sendMessage($"{p.Username_} has already voted to skip {G.Sys.GameManager_.LevelName_}.");
                            return;
                        }
                        votes.Add(playerVoteId);
                        value = votes.Count;
                        Utilities.sendMessage($"{p.Username_} voted to skip {G.Sys.GameManager_.LevelName_}.");
                        Utilities.sendMessage($"Votes: {value}/{Convert.ToInt32(Math.Ceiling(VoteHandler.thresholds[voteType] * Convert.ToDouble(numPlayers)))}");
                    }
                    else
                    {
                        votes.Remove(playerVoteId);
                        value = votes.Count;
                        Utilities.sendMessage($"{p.Username_} unvoted to skip {G.Sys.GameManager_.LevelName_}.");
                        Utilities.sendMessage($"Votes: {value}/{Convert.ToInt32(Math.Ceiling(VoteHandler.thresholds[voteType] * Convert.ToDouble(numPlayers)))}");
                        return;
                    }



                    if (Convert.ToDouble(value) / Convert.ToDouble(numPlayers) >= VoteHandler.thresholds[voteType])
                    {
                        votedSkip = true;
                        Utilities.sendMessage("Vote skip succeeded! Skipping map...");
                        parent.advanceLevel();
                    }
                }
                else if (voteType == "stop")
                {
                    if (Utilities.isOnLobby())
                    {
                        Utilities.sendMessage("You can't vote to stop the countdown in the lobby!");
                        return;
                    }

                    List<string> votes = parent.votes[voteType];
                    
                    int value = votes.Count;

                    string playerVoteId = $"{p.Username_}:{p.NetworkPlayer_.externalIP}:{p.NetworkPlayer_.externalPort}";
                    if (voteValue)
                    {
                        if (votes.Contains(playerVoteId))
                        {
                            Utilities.sendMessage($"{p.Username_} has already voted to stop the countdown.");
                            return;
                        }
                        votes.Add(playerVoteId);
                        value = votes.Count;
                        Utilities.sendMessage($"{p.Username_} voted to stop the countdown.");
                        Utilities.sendMessage($"Votes: {value}/{Convert.ToInt32(Math.Ceiling(VoteHandler.thresholds[voteType] * Convert.ToDouble(numPlayers)))}");
                    }
                    else
                    {
                        votes.Remove(playerVoteId);
                        value = votes.Count;
                        Utilities.sendMessage($"{p.Username_} unvoted to stop the countdown.");
                        Utilities.sendMessage($"Votes: {value}/{Convert.ToInt32(Math.Ceiling(VoteHandler.thresholds[voteType] * Convert.ToDouble(numPlayers)))}");
                        return;
                    }

                    if (Convert.ToDouble(value) / Convert.ToDouble(numPlayers) >= VoteHandler.thresholds[voteType])
                    {
                        parent.votes[voteType].Clear();
                        CountdownCMD command = (CountdownCMD)parent.list.getCommand("countdown");
                        command.stopCountdown();
                        Utilities.sendMessage("Vote stop succeeded! Stopping countdown...");
                    }
                }
                else if (voteType == "play")
                {

                    if (Utilities.isOnLobby())
                    {
                        Utilities.sendMessage("You can't vote for the next level in the lobby.");
                        return;
                    }

                    if (G.Sys.GameManager_.ModeID_ == GameModeID.Trackmogrify)
                    {
                        Utilities.sendMessage("You can't vote for levels in trackmogrify.");
                        return;
                    }

                    string levelName = parameter;
                    var m = LevelList.extractModifiers(levelName);
                    List<LevelPlaylist.ModeAndLevelInfo> lvls = LevelList.levels(m);
                    List<LevelResultsSortInfo> levelResults = new List<LevelResultsSortInfo>();
                    
                    double threshold = VoteHandler.thresholds["play"];

                    LevelPlaylist playlist = new LevelPlaylist();
                    playlist.Copy(G.Sys.GameManager_.LevelPlaylist_);
                    var currentPlaylist = playlist.Playlist_;
                    int index = G.Sys.GameManager_.LevelPlaylist_.Index_;

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
                                //Utilities.sendMessage($"{p.Username_} unvoted for these maps.");
                                levelResults.Add(new LevelResultsSortInfo(lvl, 0, numPlayers, threshold, -1));
                                continue;
                            }
                        }
                        string playerVoteId = $"{p.Username_}:{p.NetworkPlayer_.externalIP}:{p.NetworkPlayer_.externalPort}";
                        if (!voteValue)
                        {
                            votes.Remove(playerVoteId);
                            if (votes.Count == 0)
                            {
                                parent.votes.Remove(id);
                            }
                            //Utilities.sendMessage($"{p.Username_} unvoted for these maps.");
                            levelResults.Add(new LevelResultsSortInfo(lvl, votes.Count, numPlayers, threshold, -1));
                            continue;
                        }
                        else
                        {
                            if (votes.Contains(playerVoteId))
                            {
                                //Utilities.sendMessage($"{p.Username_} has already voted for these maps.");
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
                                    currentPlaylist.Insert(index + 1, lvl);
                                    parent.votes.Remove(id);
                                }
                            }
                        }
                    }

                    G.Sys.GameManager_.LevelPlaylist_.Clear();
                    foreach (var lvl in currentPlaylist)
                        G.Sys.GameManager_.LevelPlaylist_.Add(lvl);
                    G.Sys.GameManager_.LevelPlaylist_.SetIndex(index);

                    var playersThreshold = Convert.ToInt32(Math.Ceiling(thresholds[voteType] * Convert.ToDouble(numPlayers)));

                    if (lvls.Count == 0)
                    {
                        Utilities.sendMessage("Can't find any levels with the filter '" + levelName + "'.");
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
                            Utilities.sendMessage(newMessage + $"and {count - 10} more unvoted.");
                        }
                        else
                        {
                            Utilities.sendMessage(newMessage + "unvoted.");
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
                            Utilities.sendMessage(newMessage + $"and {count - 10} more voted.");
                        }
                        else
                        {
                            Utilities.sendMessage(newMessage + "voted.");
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
                            Utilities.sendMessage(newMessage + $"and {count - 10} more added.");
                        }
                        else
                        {
                            Utilities.sendMessage(newMessage + "added.");
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
            AutoCMD autoCommand = (AutoCMD) list.getCommand("auto");
            if (autoCommand.autoMode)
                autoCommand.nextLevel();
            else G.Sys.GameManager_.StartCoroutine(waitAndGoNext());
        }

        IEnumerator waitAndGoNext()
        {
            if (!Utilities.isOnLobby())
            {
                Utilities.sendMessage("Going to the next level in 10 seconds...");
                Utilities.sendMessage("Next level is : " + Utilities.getNextLevelName());
                yield return new WaitForSeconds(10.0f);
                if (!Utilities.isOnLobby())
                {
                    if (Utilities.isCurrentLastLevel())
                    {
                        G.Sys.GameManager_.GoToLobby();
                        Utilities.sendMessage("No more levels in the playlist.");
                    }
                    else G.Sys.GameManager_.GoToNextLevel(true);
                }
            }
            yield return null;
        }
    }
}
