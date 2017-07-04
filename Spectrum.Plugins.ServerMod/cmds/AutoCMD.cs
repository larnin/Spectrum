using Events;
using Events.GameMode;
using Spectrum.Plugins.ServerMod.CmdSettings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class AutoCMD : cmd
    {
        public bool voteNext
        {
            get { return (bool)getSetting("voteNext").Value; }
            set { getSetting("voteNext").Value = value; }
        }
        public bool shuffleAtEnd
        {
            get { return (bool)getSetting("autoShuffleAtEnd").Value; }
            set { getSetting("autoShuffleAtEnd").Value = value; }
        }
        public bool uniqueEndVotes
        {
            get { return (bool)getSetting("autoUniqueVotes").Value; }
            set { getSetting("autoUniqueVotes").Value = value; }
        }

        public string advanceMessage
        {
            get { return (string)getSetting("autoAdvanceMsg").Value; }
            set { getSetting("autoAdvanceMsg").Value = value; }
        }
        public int maxRunTime
        {
            get { return (int)getSetting("autoMaxTime").Value; }
            set { getSetting("autoMaxTime").Value = value; }
        }
        public int minPlayers
        {
            get { return (int)getSetting("autoMinPlayers").Value; }
            set { getSetting("autoMinPlayers").Value = value; }
        }

        const int maxtVoteValue = 3;

        public bool autoMode = false;
        int index = 0;  // Tracks when new levels load. Some code needs to stop running if a new level loads.
        // when index changes, it invalidates any running code after is does WaitForSeconds
        //  this means that loading a new map through the gui or restarting auto will invalidate the old auto routine
        //  which would otherwise interfere by selecting a level when the host or new auto routine already did.
        bool voting = false;
        Dictionary<string, int> votes = new Dictionary<string, int>();

        public override string name { get { return "auto"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        bool didFinish = false;

        cmdlist list;

        public override CmdSetting[] settings { get; } = {
            new CmdSettingAutoVote(),
            new CmdSettingAutoShuffle(),
            new CmdSettingAutoUniqueVotes(),
            new CmdSettingAutoMessage(),
            new CmdSettingAutoMinPlayers(),
            new CmdSettingAutoMaxTime()
        };

        public AutoCMD(cmdlist list)
        {
            this.list = list;

            Events.ServerToClient.ModeFinished.Subscribe(data =>
            {
                onModeFinish();
            });

            Events.GameMode.ModeStarted.Subscribe(data =>
            {
                onModeStart();
            });

            Events.GameMode.Go.Subscribe(data =>
            {
                onGo();
            });

            Events.ClientToAllClients.ChatMessage.Subscribe(data =>
            {
                onChatEvent(data.message_);
            });

            Events.RaceMode.FinalCountdownActivate.Subscribe(data =>
            {
                try {
                    AutoSpecCMD autoSpecCommand = (AutoSpecCMD)list.getCommand("autospec");
                    CountdownCMD countdownCommand = (CountdownCMD)list.getCommand("countdown");
                    if (G.Sys.PlayerManager_.PlayerList_.Count == 2 && autoSpecCommand.autoSpecMode)
                    {
                        countdownCommand.stopCountdown();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to stop countdown: {e}");
                }
            });
            
        }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage(Utilities.formatCmd("!auto") + ": Toggle the server auto mode.");
            Utilities.sendMessage("You must have a playlist to activate the auto server");
            Utilities.sendMessage("You can change auto mode settings with the !settings command or in the settings file.");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            index++;  // Tracks when new levels load. Some code needs to stop running if a new level loads.
            if (!autoMode)
            {
                autoMode = true;
                Utilities.sendMessage("Auto mode started!");
                if (Utilities.isOnLobby())
                    G.Sys.GameManager_.StartCoroutine(startFromLobby());
                else if (Utilities.isModeFinished())
                    G.Sys.GameManager_.StartCoroutine(waitAndGoNext());
                else onModeStart();
            }
            else
            {
                autoMode = false;
                Utilities.sendMessage("Auto mode stopped!");
            }
        }

        private void onChatEvent(string message)
        {
            if (!voting)
                return;

            var author = Utilities.ExtractMessageAuthor(message);
            var text = Utilities.ExtractMessageBody(message);
            var value = 0;
            if (!int.TryParse(text, out value))
                return;
            if (value < 0 || value > maxtVoteValue)
                return;
            votes[author] = value;
        }

        private void onModeFinish()
        {
            if(autoMode && !didFinish)
            {
                nextLevel();
            }
        }

        public void nextLevel()
        {
            didFinish = true;
            if (voteNext && G.Sys.GameManager_.LevelPlaylist_.Playlist_.Count >= maxtVoteValue)
                G.Sys.GameManager_.StartCoroutine(voteAndGoNext());
            else G.Sys.GameManager_.StartCoroutine(waitAndGoNext());
        }

        private void onModeStart()
        {
            if (!Utilities.isOnline())
                autoMode = false;
            index++;
        }

        private void onGo()
        {
            if (autoMode)
                G.Sys.GameManager_.StartCoroutine(waitUtilEnd());
        }

        public int getMinPlayers()
        {
            AutoSpecCMD autoSpecCommand = (AutoSpecCMD)list.getCommand("autospec");
            return minPlayers + autoSpecCommand.getAutoSpecPlayers().Count;
        }

        IEnumerable<float> waitForMinPlayers()
        {
            int myIndex;
            if (G.Sys.PlayerManager_.PlayerList_.Count < getMinPlayers() && autoMode)
            {
                Utilities.sendMessage($"Waiting for there to be {getMinPlayers()} players.");
                while (G.Sys.PlayerManager_.PlayerList_.Count < getMinPlayers() && autoMode)
                {
                    myIndex = index;
                    yield return 5.0f;
                    if (index != myIndex)
                        yield break;
                }
            }
        }

        IEnumerator waitAndGoNext()
        {
            foreach (float f in waitForMinPlayers())
            {
                yield return new WaitForSeconds(f);
            }
            int myIndex; // index and myIndex are used to check if the level advances before auto does it.
            if (!Utilities.isOnLobby())
            {
                Utilities.sendMessage("Going to the next level in 10 seconds...");
                Utilities.sendMessage("Next level is: " + Utilities.getNextLevelName());
                myIndex = index;
                yield return new WaitForSeconds(10.0f);
                if (index != myIndex)
                    yield break;
                if (autoMode && !Utilities.isOnLobby())
                {
                    if (Utilities.isCurrentLastLevel())
                    {
                        if (shuffleAtEnd)
                            cmd.all.getCommand("shuffle").use(null, "");
                        else
                        {
                            if (G.Sys.GameManager_.LevelPlaylist_.Playlist_.Count != 0)
                            {
                                G.Sys.GameManager_.LevelPlaylist_.SetIndex(0);
                                G.Sys.GameManager_.NextLevelName_ = G.Sys.GameManager_.LevelPlaylist_.Playlist_[0].levelNameAndPath_.levelName_;
                                G.Sys.GameManager_.NextLevelPath_ = G.Sys.GameManager_.LevelPlaylist_.Playlist_[0].levelNameAndPath_.levelPath_;
                            }
                        }
                    }
                    G.Sys.GameManager_.GoToNextLevel(true);
                }
                else autoMode = false;
            }
            else autoMode = false;
            yield return null;
        }

        IEnumerator voteAndGoNext()
        {
            foreach (float f in waitForMinPlayers())
            {
                yield return new WaitForSeconds(f);
            }
            int myIndex;
            if (!Utilities.isOnLobby())
            {
                voting = true;
                votes.Clear();
                if(G.Sys.GameManager_.LevelPlaylist_.Playlist_.Count - G.Sys.GameManager_.LevelPlaylist_.Index_ < maxtVoteValue)
                {
                    if (shuffleAtEnd)
                        cmd.all.getCommand("shuffle").use(null, "");
                    else
                    {
                        if (G.Sys.GameManager_.LevelPlaylist_.Playlist_.Count != 0)
                        {
                            G.Sys.GameManager_.LevelPlaylist_.SetIndex(0);
                            G.Sys.GameManager_.NextLevelName_ = G.Sys.GameManager_.LevelPlaylist_.Playlist_[0].levelNameAndPath_.levelName_;
                            G.Sys.GameManager_.NextLevelPath_ = G.Sys.GameManager_.LevelPlaylist_.Playlist_[0].levelNameAndPath_.levelPath_;
                        }
                    }
                }
                Utilities.sendMessage("Vote for the next map (write [FF0000]1[-], [00FF00]2[-], [0088FF]3[-], or [FFFFFF]0[-] to restart) ! Votes end in 15 sec !");
                Utilities.sendMessage("[b][FF0000]1[-] : [FFFFFF]" + G.Sys.GameManager_.LevelPlaylist_.Playlist_[G.Sys.GameManager_.LevelPlaylist_.Index_ + 1].levelNameAndPath_.levelName_ + "[-][/b]");
                Utilities.sendMessage("[b][00FF00]2[-] : [FFFFFF]" + G.Sys.GameManager_.LevelPlaylist_.Playlist_[G.Sys.GameManager_.LevelPlaylist_.Index_ + 2].levelNameAndPath_.levelName_ + "[-][/b]");
                Utilities.sendMessage("[b][0088FF]3[-] : [FFFFFF]" + G.Sys.GameManager_.LevelPlaylist_.Playlist_[G.Sys.GameManager_.LevelPlaylist_.Index_ + 3].levelNameAndPath_.levelName_ + "[-][/b]");

                myIndex = index;
                yield return new WaitForSeconds(15);
                if (index != myIndex)
                    yield break;

                if (autoMode && !Utilities.isOnLobby())
                {
                    int index = bestVote();
                    if(index == 0)
                        Utilities.sendMessage("Restart the current level !");
                    else Utilities.sendMessage("Level [b][FFFFFF]" + G.Sys.GameManager_.LevelPlaylist_.Playlist_[G.Sys.GameManager_.LevelPlaylist_.Index_ + index].levelNameAndPath_.levelName_ + "[-][/b] selected !");
                    voting = false;

                    myIndex = this.index;
                    yield return new WaitForSeconds(5);
                    if (this.index != myIndex)
                        yield break;
                        
                    if (autoMode && !Utilities.isOnLobby())
                    {
                        setToNextMap(index);
                        G.Sys.GameManager_.GoToNextLevel(true);
                    }
                    else autoMode = false;
                }
                else autoMode = false;
            }
            else autoMode = false;
            yield return null;
        }

        IEnumerator startFromLobby()
        {
            bool hasRanOnce = false;
            int myIndex;
            int total = 0;
            while (total < 2)
            {
                if (hasRanOnce)
                {
                    Utilities.sendMessage("Starting the game in 10 seconds...");

                    myIndex = index;
                    yield return new WaitForSeconds(10.0f);
                    if (index != myIndex)
                        yield break;
                }
                else hasRanOnce = true;

                foreach (float f in waitForMinPlayers())
                {
                    yield return new WaitForSeconds(f);
                }
                total = 1;

                bool canContinue = false;
                do
                {
                    canContinue = true;
                    string players = "";
                    foreach (ClientPlayerInfo current in G.Sys.PlayerManager_.PlayerList_)
                    {
                        if (!current.Ready_)
                        {
                            canContinue = false;
                            players += current.Username_ + ", ";
                        }
                    }
                    if (!canContinue)
                    {
                        Utilities.sendMessage($"Waiting for all players to be ready. ({players}is not ready.)");
                        myIndex = index;
                        yield return new WaitForSeconds(5.0f);
                        if (index != myIndex)
                            yield break;
                        total = 0;  // since we had to wait for 5 seconds, some players might have left. We need to run the loop again to make sure.
                        // by making this 0, total will be < 2 and the loop will repeat.
                    }
                } while (!canContinue);
                total = total + 1;
            }
            if (Utilities.isOnLobby())
                G.Sys.GameManager_.GoToCurrentLevel();
            yield return null;
        }

        IEnumerator waitUtilEnd()
        {
            if (advanceMessage != "")
            {
                Utilities.sendMessage(advanceMessage);
            }
            didFinish = false;
            int currentIndex = index;
            if (maxRunTime > 60)
            {
                yield return new WaitForSeconds(maxRunTime - 60);
                if (currentIndex == index && autoMode)
                {
                    Utilities.sendMessage("This map has run for the maximum run time.");

                    // start countdown for 60 seconds. Everyone is marked DNF at 60 seconds.
                    CountdownCMD countdownCommand = (CountdownCMD)list.getCommand("countdown");
                    countdownCommand.startCountdown(60);
                }
            }
            else
            {
                CountdownCMD countdownCommand = (CountdownCMD)list.getCommand("countdown");
                countdownCommand.startCountdown(maxRunTime);
            }
            yield return null;
        }

        int bestVote()
        {
            var choice = 1;
            Utilities.testFunc(() =>
            {
                List<int> values = new List<int>();
                for (int i = 0; i <= maxtVoteValue; i++)
                    values.Add(0);

                foreach (var v in votes)
                {
                    if (v.Value <= maxtVoteValue && v.Value >= 0)
                        values[v.Value]++;
                }

                if (uniqueEndVotes)
                {
                    // return a random map out of the tied maps
                    System.Random r = new System.Random();
                    List<int> ties = new List<int>();  // list of indexes
                    int maxValue = values.Max();
                    if (maxValue == 0)  // if no one voted, choose randomly between choices 1 and 3. never choose 0.
                    {
                        choice = r.Next(1, maxtVoteValue);
                        return;
                    }
                    for (int i = 0; i < values.Count; i++)
                        if (values[i] == maxValue)
                            ties.Add(i);
                    int rValue = r.Next(0, ties.Count);
                    choice = ties[rValue];  // get & return a random index from the list of tied indexes
                }
                else
                {
                    // return the first map out of the tied maps
                    // the other tied maps will be playable after the first is done
                    int maxValue = values.Max();
                    if (maxValue == 0)  // if no one voted, choose the first non-restart map so that all maps are played
                    {
                        choice = 1;
                        return;
                    }
                    for (int i = 0; i < values.Count; i++)
                        if (values[i] == maxValue)
                        {
                            choice = i;
                            return;
                        }
                    choice = 1;
                }
            });
            return choice;
        }

        void setToNextMap(int nextIndex)
        {
            if (!uniqueEndVotes)
            {
                G.Sys.GameManager_.LevelPlaylist_.SetIndex(G.Sys.GameManager_.LevelPlaylist_.Index_ + nextIndex - 1);
            }
            else
            {
                for (int i = 1; i <= maxtVoteValue; i++)
                {
                    int offset = i >= nextIndex && nextIndex != 0 ? 2 : 1;
                    var item = G.Sys.GameManager_.LevelPlaylist_.Playlist_[G.Sys.GameManager_.LevelPlaylist_.Index_ + offset];
                    G.Sys.GameManager_.LevelPlaylist_.Playlist_.RemoveAt(G.Sys.GameManager_.LevelPlaylist_.Index_ + offset);
                    G.Sys.GameManager_.LevelPlaylist_.Playlist_.Insert(G.Sys.GameManager_.LevelPlaylist_.Index_, item);
                    G.Sys.GameManager_.LevelPlaylist_.SetIndex(G.Sys.GameManager_.LevelPlaylist_.Index_ + 1);
                }
                if (nextIndex == 0)
                    G.Sys.GameManager_.LevelPlaylist_.SetIndex(G.Sys.GameManager_.LevelPlaylist_.Index_ - 1);
            }
        }
    }
    class CmdSettingAutoVote : CmdSettingBool
    {
        public override string FileId { get; } = "voteNext";
        public override string SettingsId { get; } = "autoVote";

        public override string DisplayName { get; } = "!auto Vote";
        public override string HelpShort { get; } = "!auto: Level-end votes";
        public override string HelpLong { get; } = "Whether or not players can vote for the next map at the end of a level in auto mode";

        public override object Default { get; } = false;
    }
    class CmdSettingAutoShuffle : CmdSettingBool
    {
        public override string FileId { get; } = "autoShuffleAtEnd";
        public override string SettingsId { get; } = "autoShuffle";

        public override string DisplayName { get; } = "!auto Shuffle";
        public override string HelpShort { get; } = "!auto: Shuffle at end of playlist";
        public override string HelpLong { get; } = "Whether or not the playlist should be shuffled when it finishes in auto mode";

        public override object Default { get; } = true;
    }
    class CmdSettingAutoUniqueVotes : CmdSettingBool
    {
        public override string FileId { get; } = "autoUniqueVotes";
        public override string SettingsId { get; } = "autoUniqueEndVotes";

        public override string DisplayName { get; } = "!auto Unique Votes";
        public override string HelpShort { get; } = "!auto: Level-end voting choices are unique";
        public override string HelpLong { get; } = "Whether or not levels should be re-ordered after votes so the next vote has all-new options";

        public override object Default { get; } = true;
    }
    class CmdSettingAutoMessage : CmdSettingString
    {
        public override string FileId { get; } = "autoAdvanceMsg";
        public override string SettingsId { get; } = "autoMsg";

        public override string DisplayName { get; } = "!auto Message";
        public override string HelpShort { get; } = "!auto: Level advance message";
        public override string HelpLong { get; } = "The message to display when the level advances. `clear` to turn off.";

        public override object Default { get; } = "";
    }
    class CmdSettingAutoMinPlayers : CmdSettingInt
    {
        public override string FileId { get; } = "autoMinPlayers";

        public override string DisplayName { get; } = "!auto Minimum Players";
        public override string HelpShort { get; } = "!auto: Min players for auto mode to adv. level";
        public override string HelpLong { get; } = "How many players auto mode needs before it will advance to the next level";

        public override object Default { get; } = 1;
        public override int LowerBound { get; } = 0;
    }
    class CmdSettingAutoMaxTime : CmdSettingSeconds
    {
        public override string FileId { get; } = "autoMaxTime";

        public override string DisplayName { get; } = "!auto Maximum Time";
        public override string HelpShort { get; } = "!auto: Max time before level adv.";
        public override string HelpLong { get; } = "Maximum amount of time a level can run for in auto mode before it advances to the next";

        public override object Default { get; } = 900;
    }
}