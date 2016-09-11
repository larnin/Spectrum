using Events;
using Events.GameMode;
using System.Collections;
using UnityEngine;

namespace ServerMod.cmds
{
    class AutoCMD : cmd
    {
        const int maxRunTime = 15*60;
        bool autoMode = false;
        int index = 0;

        public override string name { get { return "auto"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        public AutoCMD()
        {
            Events.ServerToClient.ModeFinished.Subscribe(data =>
            {
                onModeFinish();
            });

            Events.GameMode.Go.Subscribe(data =>
            {
                onModeStart();
            });
        }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("!auto: Toggle the server auto mode.");
            Utilities.sendMessage("You must have a playlist to active the auto server");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(!autoMode)
            {
                autoMode = true;
                Utilities.sendMessage("Automode started !");
                if(Utilities.isOnLobby())
                    G.Sys.GameManager_.StartCoroutine(startFromLobby());
                else if(Utilities.isModeFinished())
                    G.Sys.GameManager_.StartCoroutine(waitAndGoNext());
                else onModeStart();
            }
            else
            {
                autoMode = false;
                Utilities.sendMessage("Automode stoped !");
            }
        }

        private void onModeFinish()
        {
            if(autoMode)
                G.Sys.GameManager_.StartCoroutine(waitAndGoNext());
        }

        private void onModeStart()
        {
            index++;
            if (autoMode)
                G.Sys.GameManager_.StartCoroutine(waitUtilEnd());
        }

        IEnumerator waitAndGoNext()
        {
            if (!Utilities.isOnLobby())
            {
                Utilities.sendMessage("Go to next level in 10 seconds ...");
                Utilities.sendMessage("Next level is : " + Utilities.getNextLevelName());
                yield return new WaitForSeconds(10.0f);
                if (autoMode && !Utilities.isOnLobby())
                {
                    if (Utilities.isCurrentLastLevel())
                    {
                        G.Sys.GameManager_.GoToLobby();
                        Utilities.sendMessage("No more level in the playlist, automode stoped !");
                        autoMode = false;
                    }
                    else G.Sys.GameManager_.GoToNextLevel();
                }
                else autoMode = false;
            }
            else autoMode = false;
            yield return null;
        }

        IEnumerator startFromLobby()
        {
            Utilities.sendMessage("Start game in 10 seconds ...");
            yield return new WaitForSeconds(10.0f);
            if (Utilities.isOnLobby())
                G.Sys.GameManager_.GoToCurrentLevel();
            yield return null;
        }

        IEnumerator waitUtilEnd()
        {
            int currentIndex = index;
            yield return new WaitForSeconds(maxRunTime);
            if (currentIndex == index && autoMode)
            {
                Utilities.sendMessage("This map had run for 15min ...");
                Utilities.sendMessage("Finishing in 30 sec ...");
                yield return new WaitForSeconds(30);

                if (currentIndex == index && autoMode)
                {
                    StaticTargetedEvent<Finished.Data>.Broadcast(RPCMode.All, default(Finished.Data));
                }
            }
            yield return null;
        }
    }
}
