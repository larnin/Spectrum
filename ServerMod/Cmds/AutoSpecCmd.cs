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
    class CmdSettingAutoSpecIdleTimeout : CmdSettingInt
    {
        public override string FileId { get; } = "autoSpecIdleTimeout";

        public override string DisplayName { get; } = "Auto-Spec Idle Timeout";
        public override string HelpShort { get; } = "!autospec: turn on autospec for idle players after this time. -1 to disable.";
        public override string HelpLong { get; } = "The amount of time after which idle (non-moving) players will have autospec turned on automatically. Set to -1 to disable.";

        public override int Default { get; } = -1;
        public override ServerModVersion UpdatedOnVersion { get; } = new ServerModVersion("C.8.1.0");
    }
    class CmdSettingAutoSpecIdleSingle : CmdSettingBool
    {
        public override string FileId { get; } = "autoSpecIdleSingle";

        public override string DisplayName { get; } = "Auto-Spec Idle Auto";
        public override string HelpShort { get; } = "!autospec: if idle should be a single spec or autospec";
        public override string HelpLong { get; } = "Whether not the autospec idle timeout should apply spectate only once.";

        public override bool Default { get; } = true;
        public override ServerModVersion UpdatedOnVersion { get; } = new ServerModVersion("C.8.1.1");
    }
    class CmdSettingAutoSpecDebug : CmdSettingBool
    {
        public override string FileId { get; } = "autoSpecDebug";

        public override string DisplayName { get; } = "Auto-Spec Debug";
        public override string HelpShort { get; } = "!autospec: if debug info should be sent to console output";
        public override string HelpLong { get; } = "!autospec: if debug info should be sent to console output";

        public override bool Default { get; } = true;
        public override ServerModVersion UpdatedOnVersion { get; } = new ServerModVersion("C.8.1.5");
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
        public int autoSpecIdleTimeout
        {
            get { return getSetting<CmdSettingAutoSpecIdleTimeout>().Value; }
            set { getSetting<CmdSettingAutoSpecIdleTimeout>().Value = value; }
        }
        public bool autoSpecIdleSingle
        {
            get { return getSetting<CmdSettingAutoSpecIdleSingle>().Value; }
            set { getSetting<CmdSettingAutoSpecIdleSingle>().Value = value; }
        }
        public bool autoSpecDebug
        {
            get { return getSetting<CmdSettingAutoSpecDebug>().Value; }
            set { getSetting<CmdSettingAutoSpecDebug>().Value = value; }
        }

        public override string name { get { return "autospec"; } }
        public override PermType perm { get { return autoSpecAllowPlayers ? PermType.ALL : PermType.LOCAL; } }
        public override bool canUseLocal { get { return true; } }

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingAutoSpecLobby(),
            new CmdSettingAutoSpecAllowPlayers(),
            new CmdSettingAutoSpecIdleTimeout(),
            new CmdSettingAutoSpecIdleSingle(),
            new CmdSettingAutoSpecDebug(),
        };

        List<string> spectators = new List<string>();

        public AutoSpecCmd()
        {
            Events.GameMode.Go.Subscribe(data =>
            {
                GeneralUtilities.logExceptions(() =>
                {
                    onModeStart();
                });
            });
            G.Sys.GameManager_.StartCoroutine(autoAutoSpectate());
        }

        IEnumerator autoAutoSpectate()
        {
            while (true)
            {
                try
                {
                    if (autoSpecIdleTimeout > 0 && GeneralUtilities.isHost() && GeneralUtilities.isOnline() && !GeneralUtilities.isOnLobby())
                    {
                        var debug_playerInfo = "";
                        var debug_didSpectate = false;
                        var unfinished = new List<PlayerDataBase>();
                        G.Sys.GameManager_.Mode_.GetSortedListOfUnfinishedPlayers(unfinished);
                        debug_playerInfo += $"Unfinished list size: {unfinished.Count}\n";
                        debug_playerInfo += $"Spectators list size: {spectators.Count}\n";
                        debug_playerInfo += $"autoSpecIdleTimeout: {autoSpecIdleTimeout}\n";
                        debug_playerInfo += $"Players:\n";
                        foreach (var info in Entry.Instance.playerInfos)
                        {
                            var client = info.playerData;
                            var timeSinceLastMove = info.timeSinceLastMove;
                            var uniq = GeneralUtilities.getUniquePlayerString(client.PlayerInfo_);
                            debug_playerInfo += $"\t{client.Name_}:\n";
                            debug_playerInfo += $"\t\ttimeSinceLastMove: {timeSinceLastMove}\n";
                            debug_playerInfo += $"\t\tguid: {uniq}\n";
                            debug_playerInfo += $"\t\tIn auto-spectators list: {spectators.Contains(uniq)}\n";
                            debug_playerInfo += $"\t\tIn unfinished list: {unfinished.Contains(client)}\n";
                            if (timeSinceLastMove > 0f && timeSinceLastMove > autoSpecIdleTimeout && !spectators.Contains(uniq) && unfinished.Contains(client))
                            {
                                debug_didSpectate = true;
                                if (!autoSpecIdleSingle)
                                {
                                    debug_playerInfo += "\t\tSet to auto-spectate\n";
                                    spectators.Add(uniq);
                                    MessageUtilities.sendMessage($"{MessageUtilities.closeTags(client.PlayerInfo_.GetChatName())} has been set to auto-spec for being idle.");
                                }
                                else
                                {
                                    debug_playerInfo += "\t\tSet to regular spectate\n";
                                    MessageUtilities.sendMessage($"{MessageUtilities.closeTags(client.PlayerInfo_.GetChatName())} has been set to spectate for being idle.");
                                }
                                spectatePlayer(client.PlayerInfo_, force: true);
                            }
                            else
                            {
                                debug_playerInfo += "\t\tNo action\n";
                            }
                        }
                        if (debug_didSpectate && autoSpecDebug)
                            Console.WriteLine(debug_playerInfo);
                    }
                } catch (Exception e)
                {
                    Console.WriteLine($"Error auto-specing idle players: {e}");
                }
                yield return new WaitForSeconds(1f);
            }
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

        private void spectatePlayer(ClientPlayerInfo player, bool force = false)
        {
            if (!GeneralUtilities.isOnGamemode())
                return;
            if (!autoSpecAllowPlayers && !player.IsLocal_ && !force)
                return;
            if (spectators.Contains(GeneralUtilities.getUniquePlayerString(player)))
                MessageUtilities.sendMessage(player, "You are in auto-spectate mode. Say " + GeneralUtilities.formatCmd("!autospec") + " to turn it off.");
            Entry.Instance.chatReplicationManager.ReplicateNeeded();
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
