using System;
using System.Collections.Generic;
using System.Reflection;

namespace ServerMod.cmds
{
    class ScoresCMD : cmd
    {
        public override string name { get { return "scores"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return true; } }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("!scores: depending to the gamemode, it will show the current distances, times or points of players.");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(Utilities.isOnLobby())
            {
                Utilities.sendMessage("You can't do that on the lobby !");
                return;
            }

            try
            {
                var methode = G.Sys.GameManager_.Mode_.GetType().GetMethod("GetSortedListOfModeInfos", BindingFlags.Instance | BindingFlags.NonPublic);
                List<ModePlayerInfoBase> playersInfos = (List<ModePlayerInfoBase>)methode.Invoke(G.Sys.GameManager_.Mode_, new object[] { });
                foreach(var pI in playersInfos)
                {
                    string playerStr = pI.Name_ + " : ";
                    switch(pI.finishType_)
                    {
                        case FinishType.None:
                            playerStr += textInfoOf(pI);
                            break;
                        case FinishType.DNF:
                            playerStr += "DNF";
                            break;
                        case FinishType.JoinedLate:
                            playerStr += "Joined late";
                            break;
                        case FinishType.Normal:
                            playerStr += "Finished";
                            break;
                        case FinishType.Spectate:
                            playerStr += "Spectator";
                            break;
                        default:
                            playerStr += "None";
                            break;
                    }
                    Utilities.sendMessage(playerStr);
                }
            }
            catch(Exception e)
            {
                Utilities.sendMessage("Error !");
                Console.WriteLine(e);
            }
        }

        public string textInfoOf(ModePlayerInfoBase playerInfo)
        {
            if(playerInfo is TimeBasedModePlayerInfo)
                return ((int)((TimeBasedModePlayerInfo)playerInfo).distanceToFinish_).ToString() + "m";
            if(playerInfo is SoccerMode.SoccerModePlayerInfo)
                return "Team " + ((SoccerMode.SoccerModePlayerInfo)playerInfo).team_.ID_ + " - score " + ((SoccerMode.SoccerModePlayerInfo)playerInfo).team_.points_;
            if(G.Sys.GameManager_.ModeID_ == GameModeID.ReverseTag)
                return GUtils.GetFormattedTime((float)playerInfo.modeData_, true, 3);
            return playerInfo.modeData_.ToString() + " eV";
        }
    }
}
