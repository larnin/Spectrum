using Events;
using Events.GameMode;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class AutoCMD : cmd
    {
        public static bool voteNext = false;
        public static bool autoSpecCountsAsPlayer = false;

        public static string advanceMessage = "";
        public static int maxRunTime = 15*60;
        public static int minPlayers = 2;

        const int maxtVoteValue = 3;

        public bool autoMode = false;
        int index = 0;  // Tracks when new levels load. Some code needs to stop running if a new level loads.
        bool voting = false;
        Dictionary<string, int> votes = new Dictionary<string, int>();

        public override string name { get { return "auto"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        bool didFinish = false;

        cmdlist list;

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

            Events.ClientToAllClients.ChatMessage.Subscribe(data =>
            {
                onChatEvent(data.message_);
            });

            AutoSpecCMD autoSpecCommand = (AutoSpecCMD)list.getCommand("autospec");
            CountdownCMD countdownCommand = (CountdownCMD)list.getCommand("countdown");
            Events.RaceMode.FinalCountdownActivate.Subscribe(data =>
            {
                if (G.Sys.PlayerManager_.PlayerList_.Count == 2 && autoSpecCommand.autoSpecMode)
                {
                    countdownCommand.stopCountdown();
                }
            });
        }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("!auto: Toggle the server auto mode.");
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
            if (autoMode)
                G.Sys.GameManager_.StartCoroutine(waitUtilEnd());
        }

        public int getMinPlayers()
        {
            AutoSpecCMD autoSpecCommand = (AutoSpecCMD)list.getCommand("autospec");
            return minPlayers + ((!autoSpecCountsAsPlayer && autoSpecCommand.autoSpecMode) ? 1 : 0);
        }

        IEnumerator waitAndGoNext()
        {
            // index and myIndex are used to check if the level advances before auto does it.
            int myIndex;
            if (G.Sys.PlayerManager_.PlayerList_.Count < getMinPlayers() && autoMode)
            {
                Utilities.sendMessage($"Waiting for there to be {getMinPlayers()} players.");
                while (G.Sys.PlayerManager_.PlayerList_.Count < getMinPlayers() && autoMode)
                {
                    myIndex = index;
                    yield return new WaitForSeconds(5.0f);
                    if (index != myIndex)
                        yield break;
                }
            }
            if (!Utilities.isOnLobby())
            {
                Utilities.sendMessage("Going to the next level in 10 seconds...");
                Utilities.sendMessage("Next level is: " + Utilities.getNextLevelName());
                if (advanceMessage != "")
                {
                    Utilities.sendMessage(advanceMessage);
                }
                myIndex = index;
                yield return new WaitForSeconds(10.0f);
                if (index != myIndex)
                    yield break;
                if (autoMode && !Utilities.isOnLobby())
                {
                    if (Utilities.isCurrentLastLevel())
                    {
                        cmd.all.getCommand("shuffle").use(null, "");
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
            int myIndex;
            if (G.Sys.PlayerManager_.PlayerList_.Count < getMinPlayers() && autoMode)
            {
                Utilities.sendMessage($"Waiting for there to be {getMinPlayers()} players.");
                while (G.Sys.PlayerManager_.PlayerList_.Count < getMinPlayers() && autoMode)
                {
                    myIndex = index;
                    yield return new WaitForSeconds(5.0f);
                    if (index != myIndex)
                        yield break;
                }
            }
            if (!Utilities.isOnLobby())
            {
                voting = true;
                votes.Clear();
                if(G.Sys.GameManager_.LevelPlaylist_.Playlist_.Count - G.Sys.GameManager_.LevelPlaylist_.Index_ < maxtVoteValue)
                    cmd.all.getCommand("shuffle").use(null, "");
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

                    myIndex = index;
                    yield return new WaitForSeconds(5);
                    if (index != myIndex)
                        yield break;

                    if (advanceMessage != "")
                    {
                        Utilities.sendMessage(advanceMessage);
                    }
                        
                    if (autoMode && !Utilities.isOnLobby())
                    {
                        setToNextMap(index);
                        G.Sys.GameManager_.GoToNextLevel(true);
                    }
                    else autoMode = false;
                }
                else autoMode = false;
                yield return null;
            }
            else autoMode = false;
            yield return null;
        }

        IEnumerator startFromLobby()
        {
            int myIndex;
            int total = 0;
            while (total < 2)
            {
                if (G.Sys.PlayerManager_.PlayerList_.Count < getMinPlayers() && autoMode)
                {
                    Utilities.sendMessage($"Waiting for there to be {getMinPlayers()} players.");
                    while (G.Sys.PlayerManager_.PlayerList_.Count < getMinPlayers() && autoMode)
                    {
                        myIndex = index;
                        yield return new WaitForSeconds(5.0f);
                        if (index != myIndex)
                            yield break;
                    }
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
                        total = 0;
                    }
                } while (!canContinue);
                total = total + 1;
            }
            Utilities.sendMessage("Starting the game in 10 seconds...");
            myIndex = index;
            yield return new WaitForSeconds(10.0f);
            if (index != myIndex)
                yield break;
            if (Utilities.isOnLobby())
                G.Sys.GameManager_.GoToCurrentLevel();
            yield return null;
        }

        IEnumerator waitUtilEnd()
        {
            didFinish = false;
            int currentIndex = index;
            yield return new WaitForSeconds(maxRunTime);
            if (currentIndex == index && autoMode)
            {
                Utilities.sendMessage("This map has run for the maximum run time.");
                Utilities.sendMessage("Finishing in 30 sec...");
                int myIndex = index;
                yield return new WaitForSeconds(30);
                if (index != myIndex)
                    yield break;

                if (currentIndex == index && autoMode)
                {
                    StaticTargetedEvent<Finished.Data>.Broadcast(RPCMode.All, default(Finished.Data));
                }
            }
            yield return null;
        }

        int bestVote()
        {
            List<int> values = new List<int>();
            for (int i = 0 ; i <= maxtVoteValue; i++)
                values.Add(0);

            foreach(var v in votes)
            {
                if (v.Value <= maxtVoteValue && v.Value >= 0)
                    values[v.Value]++;
            }

            int maxValue = values.Max();
            for (int i = 0; i < values.Count; i++)
                if (values[i] < maxValue)
                    values[i] = -1;
            System.Random r = new System.Random();
            for(int i = 0; i < 20; i++)
            {
                int rValue = r.Next(0, values.Count);
                if (values[rValue] >= 0)
                    return rValue;
            }

            for (int i = 0; i < values.Count; i++)
                if (values[i] == maxValue)
                    return i;
            return 1;
        }

        void setToNextMap(int nextIndex)
        {
            for(int i = 1; i <= maxtVoteValue; i++)
            {
                int offset = i >= nextIndex && nextIndex != 0 ? 2 : 1;
                var item = G.Sys.GameManager_.LevelPlaylist_.Playlist_[G.Sys.GameManager_.LevelPlaylist_.Index_ + offset];
                G.Sys.GameManager_.LevelPlaylist_.Playlist_.RemoveAt(G.Sys.GameManager_.LevelPlaylist_.Index_ + offset);
                G.Sys.GameManager_.LevelPlaylist_.Playlist_.Insert(G.Sys.GameManager_.LevelPlaylist_.Index_, item);
                G.Sys.GameManager_.LevelPlaylist_.SetIndex(G.Sys.GameManager_.LevelPlaylist_.Index_ + 1);
            }
            if(nextIndex == 0)
                G.Sys.GameManager_.LevelPlaylist_.SetIndex(G.Sys.GameManager_.LevelPlaylist_.Index_ - 1);
        }
    }
}