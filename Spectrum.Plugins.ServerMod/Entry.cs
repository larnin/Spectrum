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
    public class Entry : IPlugin
    {
        public static ServerModVersion PluginVersion = new ServerModVersion("C.8.0.0");
        private static Settings Settings = new Settings(typeof(Entry));
        public static bool IsFirstRun = false;
        public static Entry Instance = null;

        public string FriendlyName => "Server commands Mod";
        public string Author => "Corecii";
        public string Contact => "SteamID: Corecii; Discord: Corecii#3019";
        public APILevel CompatibleAPILevel => APILevel.XRay;


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
            GeneralUtilities.testFunc(() =>
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

            Events.Local.ChatSubmitMessage.Subscribe(data =>
            {
                GeneralUtilities.testFunc(() =>
                {
                    Chat_MessageSent(data);
                });
            });

            Events.ClientToAllClients.ChatMessage.Subscribe(data =>
            {
                GeneralUtilities.testFunc(() =>
                {
                    var author = GeneralUtilities.ExtractMessageAuthor(data.message_);

                    if (!GeneralUtilities.IsSystemMessage(data.message_) && !sendingLocalChat)
                        Chat_MessageReceived(author, GeneralUtilities.ExtractMessageBody(data.message_), data);
                    else
                    {
                        addMessageFromRemote(data);
                        if (GeneralUtilities.isHost())
                            chatReplicationManager.AddPublic(data.message_);
                    }

                    sendingLocalChat = false;
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
            string message = messageData.message_;

            var commandInfo = MessageUtilities.getCommandInfo(message);
            Cmd cmd = commandInfo.matches ? Cmd.all.getCommand(commandInfo.commandName) : null;

            var showRegularChat = !commandInfo.matches || commandInfo.forceVisible || (cmd != null && cmd.alwaysShowChat);
            if (showRegularChat)
            {
                sendingLocalChat = true;
                replicateLocalChatFunc?.Invoke(messageData);
                if (!commandInfo.matches)
                    return;
            }

            if (GeneralUtilities.isHost() ? commandInfo.local : !commandInfo.local)
                return;

            var client = GeneralUtilities.localClient();
            if (client == null)
            {
                Console.WriteLine("Error: Local client can't be found !");
                return;
            }

            if (!showRegularChat)
            {
                MessageUtilities.sendMessage(client, $"[00CCCC]{message}[-]");
            }

            if (cmd == null)
            {
                MessageUtilities.sendMessage(client, "The command '" + commandInfo.commandName + "' doesn't exist.");
                return;
            }

            if (commandInfo.local && !cmd.canUseAsClient && cmd.perm != PermType.LOCAL)
            {
                MessageUtilities.sendMessage(client, "You can't use that command as client");
                return;
            }

            if (!commandInfo.local && commandInfo.commandName.ToLower() == "plugin")
            {
                printClient();
                return;
            }

            if (commandInfo.forceVisible)
                MessageUtilities.pushMessageOption(new MessageStateOptionPlayer());
            exec(cmd, client, commandInfo.commandParams);
            if (commandInfo.forceVisible)
                MessageUtilities.popMessageOptions();
        }

        private void Chat_MessageReceived(string author, string message, ChatMessage.Data original)
        {
            var commandInfo = MessageUtilities.getCommandInfo(message);

            if (commandInfo.matches && commandInfo.commandName.ToLower() == "plugin")
                printClient();

            if (!GeneralUtilities.isHost())
            {
                addMessageFromRemote(original);
                return;
            }

            Cmd cmd = commandInfo.matches ? Cmd.all.getCommand(commandInfo.commandName) : null;

            var showRegularChat = !commandInfo.matches || commandInfo.forceVisible || (cmd != null && cmd.alwaysShowChat) || commandInfo.local;
            if (showRegularChat)
            {
                chatReplicationManager.AddPublic(original.message_);
                addMessageFromRemote(original);
            }

            if (!commandInfo.matches || commandInfo.commandName.ToLower() == "plugin")
                return;

            var client = GeneralUtilities.clientFromName(author);
            if (client == null)
            {
                Console.WriteLine("Error: client can't be found");
                return;
            }

            if (!showRegularChat)
            {
                MessageUtilities.sendMessage(client, $"[00CCCC]{message}[-]");
                chatReplicationManager.MarkAllForReplication();
            }

            if (cmd == null)
            {
                MessageUtilities.sendMessage(client, "The command '" + commandInfo.commandName + "' doesn't exist.");
                chatReplicationManager.MarkForReplication(client.NetworkPlayer_);
                chatReplicationManager.ReplicateNeeded();
                return;
            }

            if (cmd.perm != PermType.ALL)
            {
                MessageUtilities.sendMessage(client, "You don't have permission to do that!");
                chatReplicationManager.MarkForReplication(client.NetworkPlayer_);
                chatReplicationManager.ReplicateNeeded();
                return;
            }

            if (commandInfo.forceVisible)
                MessageUtilities.pushMessageOption(new MessageStateOptionPlayer());
            exec(cmd, client, commandInfo.commandParams);
            if (commandInfo.forceVisible)
                MessageUtilities.popMessageOptions();
            chatReplicationManager.ReplicateNeeded();
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
            MessageUtilities.sendMessage(GeneralUtilities.localClient().GetChatName() + " Version " + PluginVersion);
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
