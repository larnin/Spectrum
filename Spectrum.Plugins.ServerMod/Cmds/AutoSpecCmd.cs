using Events;
using Events.GameMode;
using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class CmdSettingAutoSpecLobby : CmdSettingBool
    {
        public override string FileId { get; } = "autoSpecReturnToLobby";

        public override string DisplayName { get; } = "Auto-Spec Return to Lobby";
        public override string HelpShort { get; } = "!autospec: return to lobby if no one is playing";
        public override string HelpLong { get; } = "Whether or not to return to the lobby if everyone leaves while auto-spectate is running.";

        public override bool Default { get; } = false;
    }
    class CmdSettingAutoSpecAllowPlayers : CmdSettingBool
    {
        public override string FileId { get; } = "autoSpecAllowPlayers";

        public override string DisplayName { get; } = "Auto-Spec Allow Players";
        public override string HelpShort { get; } = "!autospec: allow players to use autospec when hosting";
        public override string HelpLong { get; } = "Whether or not players/clients can use the !autospec command";

        public override bool Default { get; } = true;
    }
    class AutoSpecCmd : Cmd
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
            get { return getSetting<CmdSettingAutoSpecLobby>().Value; }
            set { getSetting<CmdSettingAutoSpecLobby>().Value = value; }
        }
        public bool autoSpecAllowPlayers
        {
            get { return getSetting<CmdSettingAutoSpecAllowPlayers>().Value; }
            set { getSetting<CmdSettingAutoSpecAllowPlayers>().Value = value; }
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

        public AutoSpecCmd()
        {
            Events.GameMode.Go.Subscribe(data =>
            {
                GeneralUtilities.testFunc(() =>
                {
                    onModeStart();
                });
            });
        }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!autospec") + ": Toggle automatic spectating");
        }

        public bool playerIsAutoSpec(ClientPlayerInfo p)
        {
            if (!GeneralUtilities.isHost() && !p.IsLocal_)
                return false;
            return spectators.Contains(GeneralUtilities.getUniquePlayerString(p));
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
            if (!autoSpecAllowPlayers && !p.IsLocal_)
                return;
            if (message != "")
            {
                if (!p.IsLocal_)
                {
                    MessageUtilities.sendMessage(p, "You are not allowed to autospec other players.");
                    MessageUtilities.sendMessage(p, "You can use " + GeneralUtilities.formatCmd("!autospec") + " alone to toggle autospec for yourself.");
                }
                else if (!GeneralUtilities.isHost())
                {
                    MessageUtilities.sendMessage(p, "You can only autospec other players while in your own server.");
                    MessageUtilities.sendMessage(p, "You can use " + GeneralUtilities.formatCmd("!autospec") + " alone to toggle autospec for yourself.");
                }
                else
                {
                    var off = "";
                    var on = "";
                    foreach (ClientPlayerInfo client in GeneralUtilities.getClientsBySearch(message))
                    {
                        string uniq = GeneralUtilities.getUniquePlayerString(client);
                        if (spectators.Contains(uniq))
                        {
                            spectators.Remove(uniq);
                            if (off == "")
                                off = client.Username_;
                            else
                               off += $", { client.Username_}";
                        }
                        else
                        {
                            spectators.Add(uniq);
                            spectatePlayer(client);
                            if (on == "")
                                on = client.Username_;
                            else
                                on += $", { client.Username_}";
                        }
                    }
                    if (off != "")
                        MessageUtilities.sendMessage($"Auto spectator mode turned off for " + off);
                    if (on != "")
                        MessageUtilities.sendMessage($"Auto spectator mode turned on for " + on);
                    if (on == "" && off == "")
                        MessageUtilities.sendMessage(p, "Could not find any players with that name or index");
                }
            }
            else
            {
                string uniq = GeneralUtilities.getUniquePlayerString(p);
                if (spectators.Contains(uniq))
                {
                    spectators.Remove(uniq);
                    MessageUtilities.sendMessage($"Auto spectator mode turned off for {p.Username_}");
                }
                else
                {
                    spectators.Add(uniq);
                    MessageUtilities.sendMessage($"Auto spectator mode turned on for {p.Username_}");
                    spectatePlayer(p);
                }
            }
        }

        private void onModeStart()
        {
            if (GeneralUtilities.isOnline())
                G.Sys.GameManager_.StartCoroutine(spectate());

        }

        private void spectatePlayer(ClientPlayerInfo player)
        {
            if (!GeneralUtilities.isOnGamemode())
                return;
            if (!autoSpecAllowPlayers && !player.IsLocal_)
                return;
            if (player.IsLocal_)
            {
                if (G.Sys.PlayerManager_.PlayerList_.Count == 1 && GeneralUtilities.isHost() && autoSpecReturnToLobby)
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
                    if (playerIsAutoSpec(player))
                    {
                        spectatePlayer(player);
                    }
                }
            }
            yield return null;
        }
    }
}
