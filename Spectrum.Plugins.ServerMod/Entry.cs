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

namespace Spectrum.Plugins.ServerMod
{
    public class Entry : IPlugin
    {
        public string FriendlyName => "Server commands Mod";
        public string Author => "Corecii";
        public string Contact => "SteamID: Corecii; Discord: Corecii#3019";
        public APILevel CompatibleAPILevel => APILevel.XRay;

        public static string PluginVersion = "Version C.7.4.0";

        private static Settings Settings = new Settings(typeof(Entry));

        public void Initialize(IManager manager)
        {
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
                    var steamName = SteamworksManager.GetUserName().ToLower().Trim();
                    var profileName = G.Sys.PlayerManager_.Current_.profile_.Name_.ToLower().Trim();

                    if (!GeneralUtilities.IsSystemMessage(data.message_) && (author.ToLower().Trim() != steamName && author.ToLower().Trim() != profileName))
                        Chat_MessageReceived(author, GeneralUtilities.ExtractMessageBody(data.message_));
                });
            });

            Events.Network.ServerInitialized.Subscribe(data =>
            {
                G.Sys.GameManager_.StartCoroutine(serverInit());
            });

            replicateChatFunc = removeClientLogicChatSubmitSubscriber();
        }
        StaticEvent<ChatSubmitMessage.Data>.Delegate replicateChatFunc = null;

        StaticEvent<ChatSubmitMessage.Data>.Delegate removeClientLogicChatSubmitSubscriber()
        {
            // this disconnects and returns the default function that replicates chat to the server and other players
            // by disconnecting it, we can keep commands that we run private so the player doesn't have to worry about
            // clogging up the chat
            var clientLogic = PrivateUtilities.getClientLogic();
            SubscriberList list = (SubscriberList) PrivateUtilities.getPrivateField(clientLogic, "subscriberList_");
            StaticEvent<ChatSubmitMessage.Data>.Delegate func = null;
            var index = 0;
            foreach (var subscriber in list)
            {
                if (subscriber is StaticEvent<ChatSubmitMessage.Data>.Subscriber)
                {
                    func = (StaticEvent<ChatSubmitMessage.Data>.Delegate) PrivateUtilities.getPrivateField(subscriber, "func_");
                    subscriber.Unsubscribe();
                    break;
                }
                index++;
            }
            if (func != null)
            {
                list.RemoveAt(index);
            }
            return func;
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
                replicateChatFunc?.Invoke(messageData);
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

        private void Chat_MessageReceived(string author, string message)
        {
            if (!GeneralUtilities.isHost())
                return;

            var commandInfo = MessageUtilities.getCommandInfo(message);
            if (!commandInfo.matches || commandInfo.local)
                return;

            if (commandInfo.commandName.ToLower() == "plugin")
            {
                printClient();
                return;
            }

            var client = GeneralUtilities.clientFromName(author);
            if (client == null)
            {
                Console.WriteLine("Error: client can't be found");
                return;
            }

            Cmd cmd = Cmd.all.getCommand(commandInfo.commandName);
            if (cmd == null)
            {
                MessageUtilities.sendMessage(client, "The command '" + commandInfo.commandName + "' doesn't exist.");
                return;
            }

            if (cmd.perm != PermType.ALL)
            {
                MessageUtilities.sendMessage(client, "You don't have the permission to do that!");
                return;
            }

            if (commandInfo.forceVisible)
                MessageUtilities.pushMessageOption(new MessageStateOptionPlayer());
            exec(cmd, client, commandInfo.commandParams);
            if (commandInfo.forceVisible)
                MessageUtilities.popMessageOptions();
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
            MessageUtilities.sendMessage(GeneralUtilities.localClient().GetChatName() + " " + PluginVersion);
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
                foreach (Cmd Command in Cmd.all.list())
                {
                    foreach (CmdSetting Setting in Command.settings)
                    {
                        if (Setting.FileId != "")
                        {
                            var value = Settings[Setting.FileId];
                            if (value != null)
                            {
                                UpdateResult result = Setting.UpdateFromObject(value);
                                if (!result.Valid)
                                    Console.WriteLine($"Invalid value for {Setting.FileId}: {result.Message}");
                                else if (result.Message != "")
                                    Console.WriteLine(result.Message);
                                Setting.Value = result.NewValue;
                            }
                        }
                    }
                }
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
                            Settings[Setting.FileId] = Setting.Value;
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
