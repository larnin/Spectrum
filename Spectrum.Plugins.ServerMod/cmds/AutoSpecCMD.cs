using Events;
using Events.GameMode;
using Spectrum.Plugins.ServerMod.CmdSettings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class CmdSettingAutoSpecLobby : CmdSettingBool
    {
        public override string FileId { get; } = "autoSpecReturnToLobby";

        public override string DisplayName { get; } = "Auto-Spec Return to Lobby";
        public override string HelpShort { get; } = "!autospec: return to lobby if no one is playing";
        public override string HelpLong { get; } = "Whether or not to return to the lobby if eveyone leaves while auto-spectate is running.";

        public override object Default { get; } = false;
    }
    class AutoSpecCMD : cmd
    {
        public bool autoSpecMode = false;

        public bool autoSpecReturnToLobby
        {
            get { return (bool)getSetting("autoSpecReturnRoLobby").Value; }
            set { getSetting("autoSpecReturnRoLobby").Value = value; }
        }

        public override string name { get { return "autospec"; } }
        public override PermType perm { get { return PermType.LOCAL; } }
        public override bool canUseAsClient { get { return true; } }

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingAutoSpecLobby()
        };

        public AutoSpecCMD()
        {
            Events.GameMode.Go.Subscribe(data =>
            {
                onModeStart();
            });
        }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage(Utilities.formatCmd("!autospec") + ": Toggle automatic spectating for you.");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(autoSpecMode)
            {
                autoSpecMode = false;
                Utilities.sendMessage("Auto spectator mode turned off");
                return;
            }

            autoSpecMode = true;
            Utilities.sendMessage("Auto spectator mode turned on");
            onModeStart();
        }

        private void onModeStart()
        {
            if (!Utilities.isOnline())
                autoSpecMode = false;
            if (autoSpecMode)
                G.Sys.GameManager_.StartCoroutine(spectate());
        }

        IEnumerator spectate()
        {
            yield return new WaitForSeconds(1.0f);
            var players = G.Sys.PlayerManager_.PlayerList_;
            if (players.Count != 0)
            {
                if (players.Count == 1 && Utilities.isHost() && autoSpecReturnToLobby)
                    G.Sys.GameManager_.GoToLobby();
                else
                {
                    var localPlayer = G.Sys.PlayerManager_.Current_.playerData_;
                    localPlayer.Spectate();
                    /*var p = players[0];
                    if (p.IsLocal_)
                        StaticTargetedEvent<Finished.Data>.Broadcast(p.NetworkPlayer_, default(Finished.Data));*/
                }
            }
            yield return null;
        }
    }
}
