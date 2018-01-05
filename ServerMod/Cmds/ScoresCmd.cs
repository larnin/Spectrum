using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class ScoresCmd : Cmd
    {
        public override string name { get { return "scores"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseLocal { get { return true; } }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!scores") + ": depending to the gamemode, it will show the current distances, times or points of players.");
        }

        public static FinishType[] finishTypeValues = new FinishType[]
        {
            FinishType.Normal,
            FinishType.None,
            FinishType.DNF,
            FinishType.Spectate,
            FinishType.JoinedLate,
            FinishType.LeavingLevel,
            FinishType.ViewingReplay,
        };
        public override void use(ClientPlayerInfo p, string message)
        {
            if(GeneralUtilities.isOnLobby())
            {
                MessageUtilities.sendMessage(p, "You can't do that on the lobby !");
                return;
            }

            try
            {
                var playersInfos = (List<ModePlayerInfoBase>)PrivateUtilities.getPrivateField(G.Sys.GameManager_.Mode_, "modePlayerInfos_");
                playersInfos.Sort((a, b) =>
                {
                    if (a.finishType_ == FinishType.Normal && b.finishType_ == FinishType.Normal)
                    {
                        double diff = a.modeData_ - b.modeData_;
                        return diff < 0 ? -1 : diff > 0 ? 1 : 0;
                    }
                    else if (a.finishType_ == FinishType.None && b.finishType_ == FinishType.None)
                    {
                        if (a is TimeBasedModePlayerInfo)
                        {
                            double diff = ((TimeBasedModePlayerInfo)a).distanceToFinish_ - ((TimeBasedModePlayerInfo)b).distanceToFinish_;
                            return diff < 0 ? -1 : diff > 0 ? 1 : 0;
                        }
                        else
                        {
                            double diff = a.modeData_ - b.modeData_;
                            return diff < 0 ? -1 : diff > 0 ? 1 : 0;
                        }
                    }
                    else
                        return Array.IndexOf(finishTypeValues, a.finishType_) - Array.IndexOf(finishTypeValues, b.finishType_);
                });
                foreach(var pI in playersInfos)
                {
                    string playerStr = pI.Name_ + " : ";
                    switch(pI.finishType_)
                    {
                        case FinishType.None:
                            playerStr += $"[FFFFFF]{textInfoOf(pI, false)}[-]";
                            break;
                        case FinishType.DNF:
                            playerStr += "[FF2222]DNF[-]";
                            break;
                        case FinishType.JoinedLate:
                            playerStr += "[FFFF22]Joined late[-]";
                            break;
                        case FinishType.Normal:
                            playerStr += $"[22FF22]Finished {textInfoOf(pI, true)}[-]";
                            break;
                        case FinishType.Spectate:
                            playerStr += "[88FF88]Spectator[-]";
                            break;
                        default:
                            playerStr += "None";
                            break;
                    }
                    MessageUtilities.sendMessage(p, playerStr);
                }
            }
            catch(Exception e)
            {
                MessageUtilities.sendMessage(p, "Error !");
                Console.WriteLine(e);
            }
        }

        public string textInfoOf(ModePlayerInfoBase playerInfo, bool isFinished)
        {
            if (playerInfo is TimeBasedModePlayerInfo)
                if (isFinished)
                    return GUtils.GetFormattedMS((double)playerInfo.modeData_);
                else
                    return ((int)((TimeBasedModePlayerInfo)playerInfo).distanceToFinish_).ToString() + "m";
            if(playerInfo is SoccerMode.SoccerModePlayerInfo)
                return "Team " + ((SoccerMode.SoccerModePlayerInfo)playerInfo).team_.ID_ + " - score " + ((SoccerMode.SoccerModePlayerInfo)playerInfo).team_.points_;
            if(G.Sys.GameManager_.ModeID_ == GameModeID.ReverseTag)
                return GUtils.GetFormattedTime((float)playerInfo.modeData_, true, 3);
            return playerInfo.modeData_.ToString() + " eV";
        }
    }
}
