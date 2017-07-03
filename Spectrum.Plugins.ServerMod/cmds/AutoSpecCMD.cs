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
    class CmdSettingAutoSpecAllowPlayers : CmdSettingBool
    {
        public override string FileId { get; } = "autoSpecAllowPlayers";

        public override string DisplayName { get; } = "Auto-Spec Allow Players";
        public override string HelpShort { get; } = "!autospec: allow players to use autospec when hosting";
        public override string HelpLong { get; } = "Whether or not to return players can use the !autospec command when hosting";

        public override object Default { get; } = true;
    }
    class AutoSpecCMD : cmd
    {
        public bool autoSpecMode
        {
            get
            {
                foreach (var player in G.Sys.PlayerManager_.PlayerList_)
                {
                    if (player.IsLocal_ && playerIsAutoSpec(player))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool autoSpecReturnToLobby
        {
            get { return (bool)getSetting("autoSpecReturnToLobby").Value; }
            set { getSetting("autoSpecReturnToLobby").Value = value; }
        }
        public bool autoSpecAllowPlayers
        {
            get { return (bool)getSetting("autoSpecAllowPlayers").Value; }
            set { getSetting("autoSpecAllowPlayers").Value = value; }
        }

        public override string name { get { return "autospec"; } }
        public override PermType perm { get { return autoSpecAllowPlayers ? PermType.ALL : PermType.LOCAL; } }
        public override bool canUseAsClient { get { return true; } }

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingAutoSpecLobby(),
            new CmdSettingAutoSpecAllowPlayers()
        };

        List<string> spectators = new List<string>();

        public AutoSpecCMD()
        {
            Events.GameMode.Go.Subscribe(data =>
            {
                onModeStart();
            });
        }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage(Utilities.formatCmd("!autospec") + ": Toggle automatic spectating");
        }

        public bool playerIsAutoSpec(ClientPlayerInfo p)
        {
            return spectators.Contains(Utilities.getUniquePlayerString(p));
        }

        public List<ClientPlayerInfo> getAutoSpecPlayers()
        {
            var players = new List<ClientPlayerInfo>();
            foreach (var player in G.Sys.PlayerManager_.PlayerList_)
            {
                if (playerIsAutoSpec(player))
                {
                    players.Add(player);
                }
            }
            return players;
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            string uniq = Utilities.getUniquePlayerString(p);
            if (spectators.Contains(uniq))
            {
                spectators.Remove(uniq);
                Utilities.sendMessage($"Auto spectator mode turned off for {p.Username_}");
            }
            else
            {
                spectators.Add(uniq);
                Utilities.sendMessage($"Auto spectator mode turned on for {p.Username_}");
                spectatePlayer(p);
            }
        }

        private void onModeStart()
        {
            if (Utilities.isOnline())
                G.Sys.GameManager_.StartCoroutine(spectate());

        }

        private void spectatePlayer(ClientPlayerInfo player)
        {
            if (!Utilities.isOnGamemode())
                return;
            if (player.IsLocal_)
            {
                if (G.Sys.PlayerManager_.PlayerList_.Count == 1 && Utilities.isHost() && autoSpecReturnToLobby)
                {
                    G.Sys.GameManager_.GoToLobby();
                }
                else
                {
                    G.Sys.PlayerManager_.Current_.playerData_.Spectate();
                }
            }
            else
            {
                StaticTargetedEvent<Finished.Data>.Broadcast(player.NetworkPlayer_, default(Finished.Data));
            }
        }

        IEnumerator spectate()
        {
            yield return new WaitForSeconds(1.0f);
            var players = G.Sys.PlayerManager_.PlayerList_;
            if (players.Count != 0)
            {
                foreach (var player in players)
                {
                    string uniq = Utilities.getUniquePlayerString(player);
                    if (spectators.Contains(uniq))
                    {
                        spectatePlayer(player);
                    }
                }
            }
            yield return null;
        }
    }
}
