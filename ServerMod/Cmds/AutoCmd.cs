using Events;
using Events.GameMode;
using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class AutoCmd : Cmd
    {
        public bool voteNext
        {
            get { return getSetting<CmdSettingAutoVote>().Value; }
            set { getSetting<CmdSettingAutoVote>().Value = value; }
        }
        public bool shuffleAtEnd
        {
            get { return getSetting<CmdSettingAutoShuffle>().Value; }
            set { getSetting<CmdSettingAutoShuffle>().Value = value; }
        }
        public bool uniqueEndVotes
        {
            get { return getSetting<CmdSettingAutoUniqueVotes>().Value; }
            set { getSetting<CmdSettingAutoUniqueVotes>().Value = value; }
        }
        public string advanceMessage
        {
            get { return getSetting<CmdSettingAutoMessage>().Value; }
            set { getSetting<CmdSettingAutoMessage>().Value = value; }
        }
        public int maxRunTime
        {
            get { return getSetting<CmdSettingAutoMaxTime>().Value; }
            set { getSetting<CmdSettingAutoMaxTime>().Value = value; }
        }
        public int minPlayers
        {
            get { return getSetting<CmdSettingAutoMinPlayers>().Value; }
            set { getSetting<CmdSettingAutoMinPlayers>().Value = value; }
        }
        public bool skipOffline
        {
            get { return getSetting<CmdSettingSkipOfflineTracks>().Value; }
            set { getSetting<CmdSettingSkipOfflineTracks>().Value = value; }
        }
        public string voteText
        {
            get { return getSetting<CmdSettingVoteText>().Value; }
            set { getSetting<CmdSettingVoteText>().Value = value; }
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
        public override bool canUseLocal { get { return false; } }

        bool didFinish = false;

        CmdList list;

        public override CmdSetting[] settings { get; } = {
            new CmdSettingAutoVote(),
            new CmdSettingAutoShuffle(),
            new CmdSettingAutoUniqueVotes(),
            new CmdSettingAutoMessage(),
            new CmdSettingAutoMinPlayers(),
            new CmdSettingAutoMaxTime(),
            new CmdSettingSkipOfflineTracks(),
            new CmdSettingVoteText()
        };

        public int currentMapInsertIndex = -1;

        public AutoCmd(CmdList list)
        {
            this.list = list;

            Events.Network.ConnectedToServer.Subscribe(data =>
            {
                autoMode = false;
            });

            Events.Network.ServerInitialized.Subscribe(data =>
            {
                autoMode = false;
            });

            Events.ServerToClient.ModeFinished.Subscribe(data =>
            {
                GeneralUtilities.logExceptions(() =>
                {
                    onModeFinish();
                });
            });

            Events.GameMode.ModeStarted.Subscribe(data =>
            {
                GeneralUtilities.logExceptions(() =>
                {
                    onModeStart();
                });
            });

            Events.GameMode.Go.Subscribe(data =>
            {
                GeneralUtilities.logExceptions(() =>
                {
                    onGo();
                });
            });

            Events.ClientToAllClients.ChatMessage.Subscribe(data =>
            {
                GeneralUtilities.logExceptions(() =>
                {
                    onChatEvent(data.message_);
                });
            });

            Events.RaceMode.FinalCountdownActivate.Subscribe(data =>
            {
                try {
                    AutoSpecCmd autoSpecCommand = list.getCommand<AutoSpecCmd>("autospec");
                    CountdownCmd countdownCommand = list.getCommand<CountdownCmd>("countdown");
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
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!auto") + ": Toggle the server auto mode.");
            MessageUtilities.sendMessage(p, "You must have a playlist to activate the auto server");
            MessageUtilities.sendMessage(p, "You can change auto mode settings with the !settings command or in the settings file.");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            index++;  // Tracks when new levels load. Some code needs to stop running if a new level loads.
            if (!autoMode)
            {
                autoMode = true;
                MessageUtilities.sendMessage("Auto mode started!");
                if (GeneralUtilities.isOnLobby())
                    G.Sys.GameManager_.StartCoroutine(startFromLobby());
                else if (GeneralUtilities.isModeFinished())
                {
                    G.Sys.GameManager_.StartCoroutine(waitAndGoNext());
                }
                else onModeStart();
            }
            else
            {
                autoMode = false;
                MessageUtilities.sendMessage("Auto mode stopped!");
            }
        }

        private void onChatEvent(string message)
        {
            if (!voting)
                return;

            var author = MessageUtilities.ExtractMessageAuthor(message);
            var text = MessageUtilities.ExtractMessageBody(message);
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
            if (!GeneralUtilities.isOnline())
                autoMode = false;
            index++;
            currentMapInsertIndex = -1;
        }

        private void onGo()
        {
            if (autoMode)
                G.Sys.GameManager_.StartCoroutine(waitUtilEnd());
        }

        public int getMinPlayers()
        {
            AutoSpecCmd autoSpecCommand = list.getCommand<AutoSpecCmd>("autospec");
            return minPlayers + autoSpecCommand.getAutoSpecPlayers().Count;
        }

        IEnumerable<float> waitForMinPlayers()
        {
            int myIndex;
            if (G.Sys.PlayerManager_.PlayerList_.Count < getMinPlayers() && autoMode)
            {
                AutoSpecCmd autoSpecCommand = list.getCommand<AutoSpecCmd>("autospec");
                var specCount = autoSpecCommand.getAutoSpecPlayers().Count;
                if (specCount != 0) {
                    string specWord = specCount == 1 ? "is" : "are";
                    MessageUtilities.sendMessage($"Waiting for there to be {minPlayers} players. ({specCount} {specWord} auto-spectating)");
                }
                else
                {
                    MessageUtilities.sendMessage($"Waiting for there to be {minPlayers} players.");
                }
                while (G.Sys.PlayerManager_.PlayerList_.Count < getMinPlayers() && autoMode)
                {
                    myIndex = index;
                    yield return 5.0f;
                    if (index != myIndex)
                        yield break;
                }
            }
        }

        private List<int> getOnlineLevels(int start, int limit)
        {
            var list = new List<int>();
            if (G.Sys.GameManager_.LevelPlaylist_.Playlist_.Count == 0)
                return list;
            var startIndex = start;
            var firstOnlineIndex = startIndex;
            var NextLevel = GeneralUtilities.getLevel(firstOnlineIndex);
            while (true)
            {
                if (NextLevel == null)
                    firstOnlineIndex = 0;
                else
                {
                    if (GeneralUtilities.isLevelOnline(NextLevel))
                    {
                        list.Add(firstOnlineIndex);
                        if (list.Count == limit)
                        {
                            return list;
                        }
                    }
                    firstOnlineIndex++;
                }
                NextLevel = GeneralUtilities.getLevel(firstOnlineIndex);
                if (firstOnlineIndex == startIndex)
                    return list;
            }
        }
        private List<int> getOnlineLevels(int limit)
        {
            return getOnlineLevels(G.Sys.GameManager_.LevelPlaylist_.Index_, limit);
        }

        private int getFirstOnlineLevelIndex(int start)
        {
            var list = getOnlineLevels(start, 1);
            if (list.Count < 1)
                return -1;
            return list[0];
        }

        private int getFirstOnlineLevelIndex()
        {
            var list = getOnlineLevels(1);
            if (list.Count < 1)
                return -1;
            return list[0];
        }

        private bool getOnlineLevelsWrapsAround(int start, int limit)
        {
            int last = 0;
            foreach (int index in getOnlineLevels(start, limit))
            {
                if (index < last)
                    return true;
                last = index;
            }
            return false;
        }

        public int getInsertIndex()
        {
            if (currentMapInsertIndex == -1)
                return G.Sys.GameManager_.LevelPlaylist_.Count_ == 0 ? 0 : G.Sys.GameManager_.LevelPlaylist_.Index_ + 1;
            else
                return currentMapInsertIndex;
        }

        IEnumerator waitAndGoNext()
        {
            if (GeneralUtilities.isOnLobby())
            {
                autoMode = false;
                yield break;
            }

            int myIndex = index; // index and myIndex are used to check if the level advances before auto does it.
            foreach (float f in waitForMinPlayers())
            {
                yield return new WaitForSeconds(f);
                if (index != myIndex)
                    yield break;
            }

            int nextLevelIndex = 0;
            GeneralUtilities.logExceptionsThrow(() =>
            {
                if (GeneralUtilities.isCurrentLastLevel())
                {
                    if (shuffleAtEnd)
                        Cmd.all.getCommand<ShuffleCmd>("shuffle").use(null, "");
                    else
                    {
                        if (G.Sys.GameManager_.LevelPlaylist_.Playlist_.Count != 0)
                        {
                            G.Sys.GameManager_.LevelPlaylist_.SetIndex(0);
                            G.Sys.GameManager_.NextLevelName_ = G.Sys.GameManager_.LevelPlaylist_.Playlist_[0].levelNameAndPath_.levelName_;
                            G.Sys.GameManager_.NextLevelPath_ = G.Sys.GameManager_.LevelPlaylist_.Playlist_[0].levelNameAndPath_.levelPath_;
                        }
                    }
                    nextLevelIndex = 0;
                }
                else
                {
                    nextLevelIndex = G.Sys.GameManager_.LevelPlaylist_.Index_ + 1;
                }
            });

            if (skipOffline)
            {
                nextLevelIndex = getFirstOnlineLevelIndex(nextLevelIndex);
                if (nextLevelIndex == -1)
                {
                    autoMode = false;
                    MessageUtilities.sendMessage("The only levels available are offline levels (not official and not on the workshop).");
                    MessageUtilities.sendMessage("Qutting auto mode...");
                    MessageUtilities.sendMessage("You can disable this behavior with " + GeneralUtilities.formatCmd("!settings autoSkipOffline false"));
                    yield break;
                }
            }

            currentMapInsertIndex = nextLevelIndex + 1;

            var level = GeneralUtilities.getLevel(nextLevelIndex);
            MessageUtilities.sendMessage("Going to the next level in 10 seconds...");
            MessageUtilities.sendMessage("Next level is: " + level.levelName_);
            myIndex = index;
            yield return new WaitForSeconds(10.0f);
            if (index != myIndex)
                yield break;
            if (autoMode && !GeneralUtilities.isOnLobby())
            {
                GeneralUtilities.logExceptions(() =>
                {
                    G.Sys.GameManager_.LevelPlaylist_.SetIndex(nextLevelIndex - 1);
                    G.Sys.GameManager_.GoToNextLevel();
                });
            }
            else autoMode = false;

            yield break;
        }

        IEnumerator voteAndGoNext()
        {
            if (!GeneralUtilities.isOnLobby())
            {
                int myIndex = index;
                foreach (float f in waitForMinPlayers())
                {
                    yield return new WaitForSeconds(f);
                }
                if (index != myIndex)
                    yield break;

                voting = true;
                votes.Clear();

                int nextLevelIndex;
                if(
                    G.Sys.GameManager_.LevelPlaylist_.Playlist_.Count - G.Sys.GameManager_.LevelPlaylist_.Index_ < maxtVoteValue
                    || (skipOffline && getOnlineLevelsWrapsAround(G.Sys.GameManager_.LevelPlaylist_.Index_ + 1, 3))
                )
                {
                    if (shuffleAtEnd)
                        Cmd.all.getCommand<ShuffleCmd>("shuffle").use(null, "");
                    else
                    {
                        if (G.Sys.GameManager_.LevelPlaylist_.Playlist_.Count != 0)
                        {
                            G.Sys.GameManager_.LevelPlaylist_.SetIndex(0);
                            G.Sys.GameManager_.NextLevelName_ = G.Sys.GameManager_.LevelPlaylist_.Playlist_[0].levelNameAndPath_.levelName_;
                            G.Sys.GameManager_.NextLevelPath_ = G.Sys.GameManager_.LevelPlaylist_.Playlist_[0].levelNameAndPath_.levelPath_;
                        }
                    }
                    nextLevelIndex = 0;
                }
                else
                {
                    nextLevelIndex = G.Sys.GameManager_.LevelPlaylist_.Index_ + 1;
                }

                List<int> voteLevels;
                if (skipOffline)
                {
                    voteLevels = getOnlineLevels(nextLevelIndex, 3);
                    if (voteLevels.Count < 3)
                    {
                        voting = false;
                        autoMode = false;
                        MessageUtilities.sendMessage("The playlist does not have at least 3 online levels. Online levels are official levels and workshop levels only.");
                        MessageUtilities.sendMessage("Qutting auto mode...");
                        MessageUtilities.sendMessage("You can disable this behavior with " + GeneralUtilities.formatCmd("!settings autoSkipOffline false"));
                        yield break;
                    }
                }
                else
                {
                    voteLevels = new List<int>();
                    for (int i = 0; i < 3; i++)
                    {
                        voteLevels.Add(nextLevelIndex + i);
                    }
                }
                voteLevels.Insert(0, G.Sys.GameManager_.LevelPlaylist_.Index_);

                currentMapInsertIndex = voteLevels[voteLevels.Count - 1] + 1;

                MessageUtilities.sendMessage("Vote for the next map (write [FF0000]1[-], [00FF00]2[-], [0088FF]3[-], or [FFFFFF]0[-] to restart)! Votes end in 15 sec!");
                MessageUtilities.sendMessage("[b][FF0000]1[-] :[/b] [FFFFFF]" + GeneralUtilities.formatLevelInfoText(G.Sys.GameManager_.LevelPlaylist_.Playlist_[voteLevels[1]], 1, voteText) + "[-]");
                MessageUtilities.sendMessage("[b][00FF00]2[-] :[/b] [FFFFFF]" + GeneralUtilities.formatLevelInfoText(G.Sys.GameManager_.LevelPlaylist_.Playlist_[voteLevels[2]], 2, voteText) + "[-]");
                MessageUtilities.sendMessage("[b][0088FF]3[-] :[/b] [FFFFFF]" + GeneralUtilities.formatLevelInfoText(G.Sys.GameManager_.LevelPlaylist_.Playlist_[voteLevels[3]], 3, voteText) + "[-]");

                myIndex = index;
                yield return new WaitForSeconds(15);
                if (index != myIndex)
                {
                    voting = false;
                    yield break;
                }

                if (autoMode && !GeneralUtilities.isOnLobby())
                {
                    int index = bestVote();
                    if(index == 0)
                        MessageUtilities.sendMessage("Restarting the current level!");
                    else MessageUtilities.sendMessage("Level [b][FFFFFF]" + G.Sys.GameManager_.LevelPlaylist_.Playlist_[voteLevels[index]].levelNameAndPath_.levelName_ + "[-][/b] selected !");
                    voting = false;

                    myIndex = this.index;
                    yield return new WaitForSeconds(5);
                    if (this.index != myIndex)
                        yield break;
                        
                    if (autoMode && !GeneralUtilities.isOnLobby())
                    {
                        GeneralUtilities.logExceptions(() =>
                        {
                            setToNextMap(voteLevels[index], voteLevels[voteLevels.Count - 1]);
                            G.Sys.GameManager_.GoToNextLevel();
                        });
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
            int myIndex = index;
            int total = 0;
            while (total < 2)
            {
                if (hasRanOnce)
                {
                    MessageUtilities.sendMessage("Starting the game in 10 seconds...");

                    myIndex = index;
                    yield return new WaitForSeconds(10.0f);
                    if (index != myIndex)
                        yield break;
                }
                else hasRanOnce = true;

                myIndex = index;
                foreach (float f in waitForMinPlayers())
                {
                    yield return new WaitForSeconds(f);
                }
                if (index != myIndex)
                    yield break;
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
                        MessageUtilities.sendMessage($"Waiting for all players to be ready. ({players}is not ready.)");
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
            if (GeneralUtilities.isOnLobby())
            {
                if (skipOffline)
                {
                    int firstOnlineLevelIndex = getFirstOnlineLevelIndex();
                    if (firstOnlineLevelIndex == -1)
                    {
                        autoMode = false;
                        MessageUtilities.sendMessage("The only levels available are offline levels (not official and not on the workshop).");
                        MessageUtilities.sendMessage("Qutting auto mode...");
                        MessageUtilities.sendMessage("You can disable this behavior with " + GeneralUtilities.formatCmd("!settings autoSkipOffline false"));
                        yield break;
                    }
                    G.Sys.GameManager_.LevelPlaylist_.SetIndex(firstOnlineLevelIndex);
                }
                G.Sys.GameManager_.GoToCurrentLevel(GameManager.OpenOnMainMenuInit.UsePrevious, false);
            }
            yield return null;
        }

        IEnumerator waitUtilEnd()
        {
            if (advanceMessage != "")
            {
                MessageUtilities.sendMessage(advanceMessage);
            }
            didFinish = false;
            int currentIndex = index;
            if (maxRunTime > 60)
            {
                yield return new WaitForSeconds(maxRunTime - 60);
                CountdownCmd countdownCommand = list.getCommand<CountdownCmd>("countdown");
                if (currentIndex == index && autoMode && !countdownCommand.countdownStarted)
                {
                    MessageUtilities.sendMessage("This map has run for the maximum time.");

                    // start countdown for 60 seconds. Everyone is marked DNF at 60 seconds.
                    countdownCommand.startCountdown(60);
                }
            }
            else
            {
                CountdownCmd countdownCommand = list.getCommand<CountdownCmd>("countdown");
                countdownCommand.startCountdown(maxRunTime);
            }
            yield return null;
        }

        int bestVote()
        {
            var choice = 1;
            GeneralUtilities.logExceptions(() =>
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

        void setToNextMap(int nextIndexAbsolute, int maxIndex)
        {
            if (!uniqueEndVotes)
            {
                G.Sys.GameManager_.LevelPlaylist_.SetIndex(nextIndexAbsolute - 1);
            }
            else
            {
                // just move the next level past all of the vote-able levels
                var level = G.Sys.GameManager_.LevelPlaylist_.Playlist_[nextIndexAbsolute];
                G.Sys.GameManager_.LevelPlaylist_.Playlist_.RemoveAt(nextIndexAbsolute);
                G.Sys.GameManager_.LevelPlaylist_.Playlist_.Insert(maxIndex, level);
                G.Sys.GameManager_.LevelPlaylist_.SetIndex(maxIndex - 1);
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

        public override bool Default { get; } = false;
    }
    class CmdSettingAutoShuffle : CmdSettingBool
    {
        public override string FileId { get; } = "autoShuffleAtEnd";
        public override string SettingsId { get; } = "autoShuffle";

        public override string DisplayName { get; } = "!auto Shuffle";
        public override string HelpShort { get; } = "!auto: Shuffle at end of playlist";
        public override string HelpLong { get; } = "Whether or not the playlist should be shuffled when it finishes in auto mode";

        public override bool Default { get; } = true;
    }
    class CmdSettingAutoUniqueVotes : CmdSettingBool
    {
        public override string FileId { get; } = "autoUniqueVotes";
        public override string SettingsId { get; } = "autoUniqueEndVotes";

        public override string DisplayName { get; } = "!auto Unique Votes";
        public override string HelpShort { get; } = "!auto: Level-end voting choices are unique";
        public override string HelpLong { get; } = "Whether or not levels should be re-ordered after votes so the next vote has all-new options";

        public override bool Default { get; } = true;
    }
    class CmdSettingAutoMessage : CmdSettingString
    {
        public override string FileId { get; } = "autoAdvanceMsg";
        public override string SettingsId { get; } = "autoMsg";

        public override string DisplayName { get; } = "!auto Message";
        public override string HelpShort { get; } = "!auto: Level advance message";
        public override string HelpLong { get; } = "The message to display when the level advances. `clear` to turn off.";

        public override string Default { get; } = "";
    }
    class CmdSettingAutoMinPlayers : CmdSettingInt
    {
        public override string FileId { get; } = "autoMinPlayers";

        public override string DisplayName { get; } = "!auto Minimum Players";
        public override string HelpShort { get; } = "!auto: Min players for auto mode to adv. level";
        public override string HelpLong { get; } = "How many players auto mode needs before it will advance to the next level";

        public override int Default { get; } = 1;
        public override int LowerBound { get; } = 0;
    }
    class CmdSettingAutoMaxTime : CmdSettingSeconds
    {
        public override string FileId { get; } = "autoMaxTime";

        public override string DisplayName { get; } = "!auto Maximum Time";
        public override string HelpShort { get; } = "!auto: Max time before level adv.";
        public override string HelpLong { get; } = "Maximum amount of time a level can run for in auto mode before it advances to the next";

        public override int Default { get; } = 900;
    }
    class CmdSettingSkipOfflineTracks : CmdSettingBool
    {
        public override string FileId { get; } = "autoSkipOffline";
        public override string SettingsId { get; } = "autoSkipOffline";

        public override string DisplayName { get; } = "!auto Skip Offline Tracks";
        public override string HelpShort { get; } = "!auto: Skip tracks that can't be downloaded from the workshop";
        public override string HelpLong { get; } = "Whether or not levels that are not official levels and are not workshop levels should be skipped over in auto mode";

        public override bool Default { get; } = true;
    }
    class CmdSettingVoteText : CmdSettingString
    {
        public override string FileId { get; } = "autoVoteText";
        public override string SettingsId { get; } = "autoVoteText";

        public override string DisplayName { get; } = "!auto Vote Text";
        public override string HelpShort { get; } = "!auto: Formatted text to display for level-end votes";
        public override string HelpLong { get; } = "The text to display for level-end votes. Formatting options: "
            + "%NAME%, %DIFFICULTY%, %MODE%, %MBRONZE%, %MSILVER%, %MGOLD%, %MDIAMOND%, %AUTHOR%, %STARS%, %STARSINT%, %STARSDEC%, %CREATED%, %UPDATED%";

        public override string Default { get; } = "[b]%NAME% [A0A0A0]by %AUTHOR%[-][/b]";
    }
}
 