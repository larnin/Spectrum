using Spectrum.API;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.API.Interfaces.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spectrum.Plugins.ServerMod.Cmds;
using System;
using Spectrum.API.Configuration;
using System.Linq;
using System.IO;
using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.PlaylistTools;
using Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters;
using Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters.Sorts;
using Spectrum.Plugins.ServerMod.Utilities;
using System.Reflection;
using Events;
using Events.Local;
using Events.Server;
using Events.ServerToClient;
using System.Text;
using Events.ChatLog;
using Events.ClientToAllClients;

namespace Spectrum.Plugins.ServerMod
{
    public class PlayerInfo
    {
        public PlayerDataBase playerData;
        public long lastMove = 0;
        public PlayerInfo(PlayerDataBase playerData)
        {
            this.playerData = playerData;
            lastMove = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
        }
        public long millisSinceLastMove { get
            {
                return lastMove <= 0 ? 0 : (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - lastMove;
            }
        }
        public float timeSinceLastMove {  get
            {
                return millisSinceLastMove / 1000f;
            }
        }
    }
    public class Entry : IPlugin, IUpdatable
    {
        public static ServerModVersion PluginVersion = new ServerModVersion("C.8.2.1");
        private static Settings Settings = new Settings(typeof(Entry));
        public static bool IsFirstRun = false;
        public static Entry Instance = null;

        public string FriendlyName => "Server commands Mod";
        public string Author => "Corecii";
        public string Contact => "SteamID: Corecii; Discord: Corecii#3019";
        public APILevel CompatibleAPILevel => APILevel.XRay;

        public List<PlayerInfo> playerInfos = new List<PlayerInfo>();

        StaticEvent<ChatSubmitMessage.Data>.Delegate replicateLocalChatFunc = null;
        StaticEvent<ChatMessage.Data>.Delegate addMessageFromRemote = null;
        public ChatReplicationManager chatReplicationManager = null;
        bool sendingLocalChat = false;

        public void Initialize(IManager manager)
        {
            if (Instance != null)
            {
                Console.WriteLine("Attempt to create a second Entry");
                throw new Exception("There should only be one Entry");
            }
            Instance = this;
            GeneralUtilities.logExceptions(() =>
            {
                var levelFilters = new LevelFilter[]
                {
                    new LevelFilterAll(),
                    new LevelFilterAuthor(),
                    new LevelFilterCreated(),
                    new LevelFilterDifficulty(),
                    new LevelFilterIndex(),
                    new LevelFilterIsNew(),
                    new LevelFilterIsUpdated(),
                    new LevelFilterLast(),
                    new LevelFilterMode(),
                    new LevelFilterName(),
                    new LevelFilterPage(),
                    new LevelFilterPlaylist(),
                    new LevelFilterRegex(),
                    new LevelFilterSaved(),
                    new LevelFilterStars(),
                    new LevelFilterTime(),
                    new LevelFilterUpdated(),

                    new LevelSortFilterAuthor(),
                    new LevelSortFilterCreated(),
                    new LevelSortFilterDifficulty(),
                    new LevelSortFilterIndex(),
                    new LevelSortFilterMode(),
                    new LevelSortFilterName(),
                    new LevelSortFilterShuffle(),
                    new LevelSortFilterStars(),
                    new LevelSortFilterTime(),
                    new LevelSortFilterUpdated()
                };
                foreach (var filter in levelFilters)
                    FilteredPlaylist.AddFilterType(filter);
            });

            load();  // load existing data
            save();  // save defaults that were not loaded

            // player data list stuff

            Events.Player.AddRemovePlayerData.Subscribe((data) =>
            {
                if (data.added_)
                    playerInfos.Add(new PlayerInfo(data.player_));
                else
                    playerInfos.RemoveAll((info) => info.playerData == data.player_);
            });

            // chat stuff

            Events.Local.ChatSubmitMessage.Subscribe(data =>
            {
                GeneralUtilities.logExceptions(() =>
                {
                    Chat_MessageSent(data);
                });
            });

            var sendingClientToAllClientsMessage = false;
            AddMessage.Subscribe(data =>
            {
                if (!sendingClientToAllClientsMessage
                && (MessageUtilities.currentState == null || !MessageUtilities.currentState.forPlayer))
                    chatReplicationManager.AddPublic(data.message_);
            });

            ChatMessage.Subscribe(data =>
            {
                GeneralUtilities.logExceptions(() =>
                {
                    sendingClientToAllClientsMessage = true;
                    var author = MessageUtilities.ExtractMessageAuthor(data.message_);
                    
                    if (!MessageUtilities.IsSystemMessage(data.message_) && !sendingLocalChat && !string.IsNullOrEmpty(author))
                        Chat_MessageReceived(author, MessageUtilities.ExtractMessageBody(data.message_), data);
                    else
                    {
                        addMessageFromRemote(data);
                        chatReplicationManager.AddPublic(data.message_);
                    }

                    sendingLocalChat = false;
                    sendingClientToAllClientsMessage = false;
                });
            });

            Events.Network.ServerInitialized.Subscribe(data =>
            {
                chatReplicationManager.Clear();
                G.Sys.GameManager_.StartCoroutine(serverInit());
            });

            replicateLocalChatFunc = PrivateUtilities.removeParticularSubscriber<ChatSubmitMessage.Data>(PrivateUtilities.getComponent<ClientLogic>());
            addMessageFromRemote = PrivateUtilities.removeParticularSubscriber<ChatMessage.Data>(G.Sys.NetworkingManager_);

            chatReplicationManager = new ChatReplicationManager();
            chatReplicationManager.Setup();
        }

        public void Update()
        {
            long now = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
            foreach (var info in playerInfos)
                if (info.playerData.CarLogic_ != null && info.playerData.CarLogic_.CarDirectives_ != null && !info.playerData.CarLogic_.CarDirectives_.IsZero())
                    info.lastMove = now;
        }

        IEnumerator serverInit()
        {
            yield return new WaitForSeconds(1.0f);  // wait for the server to load
            if (Cmd.all.getCommand<UpdateCmd>().updateCheck)
                UpdateCmd.checkForUpdates(false);  // check for ServerMod updates
            Cmd.all.getCommand<SettingsCmd>().showNewSettings();  // show any new settings
            yield break;
        }

        private void Chat_MessageSent(ChatSubmitMessage.Data messageData)
        {
            // by doing the below instead, we preserver formatting symbols.
            string message = UIExInputGeneric<string>.current_.Value_; //messageData.message_;

            var commandInfo = MessageUtilities.getCommandInfo(message);
            Cmd cmd = commandInfo.matches ? Cmd.all.getCommand(commandInfo.commandName) : null;

            string logMessage = "";

            var client = GeneralUtilities.localClient();

            var showRegularChat = (!commandInfo.local && !GeneralUtilities.isHost()) || !LogCmd.localHostCommands
                || !commandInfo.matches || commandInfo.forceVisible || (cmd != null && client != null && cmd.showChatPublic(client));
            if (showRegularChat)
            {
                sendingLocalChat = true;
                replicateLocalChatFunc?.Invoke(messageData);
                if (!commandInfo.matches)
                    return;
                logMessage = message;
            }

            if (client == null)
            {
                Console.WriteLine("Error: Local client can't be found !");
                return;
            }

            if (!showRegularChat)
            {
                MessageUtilities.sendMessage(client, $"[00FFFF]{message}[-]");
                logMessage = $"[00FFFF]{message}[-]";
            }
            
            MessageStateOptionLog cmdLog = new MessageStateOptionLog(new List<string>());
            MessageUtilities.pushMessageOption(cmdLog);

            if (LogCmd.localHostResults)
                MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(client));
            else
                MessageUtilities.pushMessageOption(new MessageStateOptionPlayer());

            if (!commandInfo.local && commandInfo.commandName.ToLower() == "plugin")
            {
                printClient();
                LogCmd.AddLog(client, logMessage, cmdLog.GetLogString());
                MessageUtilities.popMessageOptions(2);
                return;
            }

            if (GeneralUtilities.isHost() && commandInfo.local)
            {
                MessageUtilities.sendMessage(client, "Cannot use local commands as host");
                LogCmd.AddLog(client, logMessage, cmdLog.GetLogString());
                MessageUtilities.popMessageOptions(2);
                return;
            }
            else if (!GeneralUtilities.isHost() && !commandInfo.local)
            {
                MessageUtilities.popMessageOptions(2);
                return;
            }

            if (cmd == null)
            {
                MessageUtilities.sendMessage(client, "The command '" + commandInfo.commandName + "' doesn't exist.");
                LogCmd.AddLog(client, logMessage, cmdLog.GetLogString());
                MessageUtilities.popMessageOptions(2);
                return;
            }

            if (commandInfo.local && !cmd.canUseLocal && cmd.perm != PermType.LOCAL)
            {
                MessageUtilities.sendMessage(client, "You can't use that command as client");
                LogCmd.AddLog(client, logMessage, cmdLog.GetLogString());
                MessageUtilities.popMessageOptions(2);
                return;
            }

            MessageUtilities.popMessageOptions();  // remove local/non-local only option

            bool renderToPublic = commandInfo.forceVisible || !LogCmd.localHostResults;  // log settings may change if cmd changes them

            if (renderToPublic)
                MessageUtilities.pushMessageOption(new MessageStateOptionPlayer());
            exec(cmd, client, commandInfo.commandParams);
            if (renderToPublic)
                MessageUtilities.popMessageOptions();
            LogCmd.AddLog(client, logMessage, cmdLog.GetLogString());
            MessageUtilities.popAllMessageOptions();
        }

        private void Chat_MessageReceived(string author, string message, ChatMessage.Data original)
        {
            var commandInfo = MessageUtilities.getCommandInfo(message);

            if (commandInfo.matches && commandInfo.commandName.ToLower() == "plugin")
                printClient();

            if (!GeneralUtilities.isHost())
            {
                addMessageFromRemote(original);
                chatReplicationManager.AddPublic(original.message_);
                return;
            }

            Cmd cmd = commandInfo.matches ? Cmd.all.getCommand(commandInfo.commandName) : null;

            string logMessage = "";
            
            var client = GeneralUtilities.clientFromName(author);

            var showRegularChat = !LogCmd.localClientCommands || !commandInfo.matches || commandInfo.forceVisible || (cmd != null && client != null && cmd.showChatPublic(client)) || commandInfo.local;
            if (showRegularChat)
            {
                chatReplicationManager.AddPublic(original.message_);
                addMessageFromRemote(original);
                logMessage = message;
            }

            if (client == null)
            {
                Console.WriteLine($"Error: client can't be found for name: {author}");
                return;
            }

            if (!commandInfo.matches || commandInfo.commandName.ToLower() == "plugin" || commandInfo.local)
                return;

            if (!showRegularChat)
            {
                logMessage = $"[00FFFF]{message}[-]";
                MessageUtilities.sendMessage(client, logMessage);
                chatReplicationManager.MarkAllForReplication();
                if (LogCmd.showHostAllCommands)
                {
                    var hostClient = GeneralUtilities.localClient();
                    if (hostClient == null)
                    {
                        Console.WriteLine("Error: Local client can't be found !");
                        return;
                    }
                    string usedCmd;
                    if (cmd == null || cmd.perm != PermType.ALL)
                        usedCmd = MessageUtilities.closeTags(client.GetChatName()) + " tried to use " + logMessage;
                    else
                        usedCmd = MessageUtilities.closeTags(client.GetChatName()) + " used " + logMessage;
                    MessageUtilities.sendMessage(hostClient, usedCmd);
                }
            }

            MessageStateOptionLog cmdLog = new MessageStateOptionLog(new List<string>());
            MessageUtilities.pushMessageOption(cmdLog);


            if (LogCmd.showHostAllResults)
                MessageUtilities.pushMessageOption(new MessageStateOptionShowToHost(true));

            if (LogCmd.localClientResults)
                MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(client));
            else
                MessageUtilities.pushMessageOption(new MessageStateOptionPlayer());

            if (cmd == null)
            {
                MessageUtilities.sendMessage(client, "The command '" + commandInfo.commandName + "' doesn't exist.");
                chatReplicationManager.MarkForReplication(client.NetworkPlayer_);
                chatReplicationManager.ReplicateNeeded();
                LogCmd.AddLog(client, logMessage, cmdLog.GetLogString());
                MessageUtilities.popAllMessageOptions();
                return;
            }

            if (cmd.perm != PermType.ALL)
            {
                MessageUtilities.sendMessage(client, "You don't have permission to do that!");
                chatReplicationManager.MarkForReplication(client.NetworkPlayer_);
                chatReplicationManager.ReplicateNeeded();
                LogCmd.AddLog(client, logMessage, cmdLog.GetLogString());
                MessageUtilities.popAllMessageOptions();
                return;
            }

            MessageUtilities.popMessageOptions();
            
            if (commandInfo.forceVisible || !LogCmd.localClientResults)
                MessageUtilities.pushMessageOption(new MessageStateOptionPlayer());

            exec(cmd, client, commandInfo.commandParams);

            chatReplicationManager.ReplicateNeeded();
            LogCmd.AddLog(client, logMessage, cmdLog.GetLogString());
            MessageUtilities.popAllMessageOptions();
        }

        private void exec(Cmd c, ClientPlayerInfo p, string message)
        {
            try
            {
                c.use(p, message);
            }
            catch (Exception error)
            {
                MessageUtilities.sendMessage("Error");
                Console.WriteLine(error);
            }
        }

        public void Shutdown()
        {
            
        }

        private void printClient()
        {
            var optionsList = MessageUtilities.popAllMessageOptions();
            MessageUtilities.sendMessage(GeneralUtilities.localClient().GetChatName() + " Version " + PluginVersion);
            MessageUtilities.pushMessageOptionsList(optionsList);
        }

        private static void reloadSettingsFromFile()
        {
            // NOTE: Code from Spectrum's Settings. Used because there is no provided method to reload settings.
            Type type = typeof(Entry);
            string postfix = "";
            string FileName;
            if (string.IsNullOrEmpty(postfix))
            {
                FileName = $"{type.Assembly.GetName().Name}.json";
            }
            else
            {
                FileName = $"{type.Assembly.GetName().Name}.{postfix}.json";
            }
            string FilePath = Path.Combine(Defaults.SettingsDirectory, FileName);

            if (File.Exists(FilePath))
            {
                using (var sr = new StreamReader(FilePath))
                {
                    var json = sr.ReadToEnd();
                    var reader = new JsonFx.Json.JsonReader();

                    Section sec = null;

                    try
                    {
                        sec = reader.Read<Section>(json);
                    }
                    catch
                    {
                    }

                    if (sec != null)
                    {
                        foreach (string k in sec.Keys)
                        {
                            Settings[k] = sec[k];
                        }
                    }
                }
            }
        }

        public static void reload()
        {
            reloadSettingsFromFile();
            load();
        }

        public static void load()
        {
            try
            {
                var settingsCount = 0;
                foreach (Cmd Command in Cmd.all.list())
                {
                    foreach (CmdSetting Setting in Command.settings)
                    {
                        if (Setting.FileId != "")
                        {
                            var value = Settings[Setting.FileId];
                            if (value != null)
                            {
                                settingsCount++;
                                UpdateResult result = Setting.UpdateFromObjectTypeless(value);
                                if (!result.Valid)
                                    Console.WriteLine($"Invalid value for {Setting.FileId}: {result.Message}");
                                else if (result.Message != "")
                                    Console.WriteLine(result.Message);
                                Setting.ValueTypeless = result.NewValueTypeless;
                            }
                        }
                    }
                }
                if (settingsCount == 0)
                    IsFirstRun = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void save()
        {
            try
            {
                foreach (Cmd Command in Cmd.all.list())
                {
                    foreach (CmdSetting Setting in Command.settings)
                    {
                        if (Setting.FileId != "")
                        {
                            Settings[Setting.FileId] = Setting.SaveValue;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Settings.Save();
        }
    }
}
