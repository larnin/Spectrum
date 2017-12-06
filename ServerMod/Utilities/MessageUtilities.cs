using Events;
using Events.ChatLog;
using Events.ClientToAllClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.Utilities
{
    class MessageState
    {
        public List<string> log = null;
        public bool closeTags = true;
        public bool shown = true;
        public bool showToHost = false;
        public bool forPlayer = false;
        public ClientPlayerInfo player = null;
        public MessageState() { }
    }
    abstract class MessageStateOption
    {
        public abstract void Apply(MessageState messageState);
    }
    class MessageStateOptionCloseTags : MessageStateOption
    {
        bool closeTags;
        public MessageStateOptionCloseTags(bool closeTags)
        {
            this.closeTags = closeTags;
        }
        public override void Apply(MessageState messageState)
        {
            messageState.closeTags = closeTags;
        }
    }
    class MessageStateOptionShowToHost : MessageStateOption
    {
        bool showToHost;
        public MessageStateOptionShowToHost(bool showToHost)
        {
            this.showToHost = showToHost;
        }
        public override void Apply(MessageState messageState)
        {
            messageState.showToHost = showToHost;
        }
    }
    class MessageStateOptionShown : MessageStateOption
    {
        bool shown;
        public MessageStateOptionShown(bool shown)
        {
            this.shown = shown;
        }
        public override void Apply(MessageState messageState)
        {
            messageState.shown = shown;
        }
    }
    class MessageStateOptionPlayer : MessageStateOption
    {
        ClientPlayerInfo player;
        public MessageStateOptionPlayer(ClientPlayerInfo player)
        {
            this.player = player;
        }
        public MessageStateOptionPlayer()
        {
            this.player = null;
        }
        public override void Apply(MessageState messageState)
        {
            if (player == null)
            {
                messageState.forPlayer = false;
            }
            else
            {
                messageState.forPlayer = true;
                messageState.player = player;
            }
        }
    }
    class MessageStateOptionLog : MessageStateOption
    {
        List<string> log;
        public MessageStateOptionLog(List<string> log)
        {
            this.log = log;
        }
        public MessageStateOptionLog()
        {
            this.log = null;
        }
        public override void Apply(MessageState messageState)
        {
            messageState.log = log;
        }
        public string GetLogString()
        {
            string logString = "";
            foreach (string logValue in log)
            {
                logString += logValue + "\n";
            }
            return logString.Length == 0 ? "" : logString.Substring(0, logString.Length - 1);
        }
    }
    static class MessageUtilities
    {
        static List<MessageStateOption[]> messageOptionStack = new List<MessageStateOption[]>();
        public static void pushMessageOption(MessageStateOption stateOption)
        {
            messageOptionStack.Push(new MessageStateOption[] { stateOption });
        }
        public static void pushMessageOptions(MessageStateOption[] stateOptions)
        {
            messageOptionStack.Push(stateOptions);
        }
        public static void pushMessageOptions(List<MessageStateOption> stateOptions)
        {
            messageOptionStack.Push(stateOptions.ToArray<MessageStateOption>());
        }
        public static void pushMessageOptionsList(List<MessageStateOption[]> stateOptionsList)
        {
            foreach (var stateOptions in stateOptionsList)
                messageOptionStack.Push(stateOptions.ToArray<MessageStateOption>());
        }
        public static MessageStateOption[] popMessageOptions()
        {
            return messageOptionStack.Pop();
        }
        public static List<MessageStateOption[]> popAllMessageOptions()
        {
            return popMessageOptions(messageOptionStack.Count);
        }
        public static List<MessageStateOption[]> popMessageOptions(int count)
        {
            List<MessageStateOption[]> options = new List<MessageStateOption[]>();
            for (int i = 0; i < count; i++)
                options.Insert(0, messageOptionStack.Pop());
            return options;
        }
        public static MessageState computeMessageState()
        {
            MessageState state = new MessageState();
            Dictionary<Type, MessageStateOption> firstOfType = new Dictionary<Type, MessageStateOption>();
            foreach (var optionList in messageOptionStack)
                foreach (var option in optionList)
                    if (!firstOfType.ContainsKey(option.GetType()))
                        firstOfType.Add(option.GetType(), option);
            foreach (var option in firstOfType.Values)
                option.Apply(state);
            return state;
        }
        public static MessageState currentState = null;
        public static void sendMessage(string message)
        {
            currentState = computeMessageState();
            if (!currentState.shown)
            {
                currentState = null;
                return;
            }
            if (currentState.closeTags)
                message = closeTags(message);
            if (currentState.forPlayer)
            {
                // slightly blue text for local-only messages
                Entry.Instance.chatReplicationManager.AddPersonal(currentState.player.NetworkPlayer_, (message).Colorize("[70AAAA]"));
                Entry.Instance.chatReplicationManager.MarkForReplication(currentState.player.NetworkPlayer_);
                if (currentState.log != null)
                    currentState.log.Add((message).Colorize("[70AAAA]"));
                if (currentState.showToHost && !currentState.player.IsLocal_)
                {
                    var client = GeneralUtilities.localClient();
                    if (client == null)
                    {
                        Console.WriteLine("Error: Local client can't be found (sendMessage) !");
                        return;
                    }
                    Entry.Instance.chatReplicationManager.AddPersonal(client.NetworkPlayer_, (message).Colorize("[70AAAA]"));
                }
            }
            else
            {
                //StaticEvent<ChatSubmitMessage.Data>.Broadcast(new ChatSubmitMessage.Data(message));
                //Chat.SendAction(message);
#pragma warning disable CS0618 // Type or member is obsolete
                StaticTransceivedEvent<ChatMessage.Data>.Broadcast(new ChatMessage.Data((message).Colorize("[AAAAAA]")));
#pragma warning restore CS0618 // Type or member is obsolete
                //Console.WriteLine("Log : " + message);
                if (currentState.log != null)
                    currentState.log.Add((message).Colorize("[AAAAAA]"));
            }
            currentState = null;
        }
        public static void sendMessage(ClientPlayerInfo p, string message)
        {
            pushMessageOption(new MessageStateOptionPlayer(p));
            sendMessage(message);
            popMessageOptions();
        }
        public static void sendMessage(MessageStateOption[] options, string message)
        {
            pushMessageOptions(options);
            sendMessage(message);
            popMessageOptions();
        }
        public static void sendMessage(List<MessageStateOption> options, string message)
        {
            pushMessageOptions(options);
            sendMessage(message);
            popMessageOptions();
        }

        static string[][] tagPairs = new string[][]
        {
            new string[] {@"\[[A-Za-z0-9]{6}\]", "[FFFFFF]", @"\[\-\]", "[-]"},
            new string[] {@"\[b\]", "[b]", @"\[\/b\]", "[/b]"},
            new string[] {@"\[i\]", "[i]", @"\[\/i\]", "[/i]"},
            new string[] {@"\[u\]", "[u]", @"\[\/u\]", "[/u]"},
        };
        public static string closeTags(string input)
        {
            string logName = input;
            foreach (string[] tagPair in tagPairs)
            {
                var openingTagMatches = Regex.Matches(input, tagPair[0]);
                var closingTagMatches = Regex.Matches(input, tagPair[2]);
                for (int i = 0; i < closingTagMatches.Count - openingTagMatches.Count; i++)
                    logName = tagPair[1] + logName;
                for (int i = 0; i < openingTagMatches.Count - closingTagMatches.Count; i++)
                    logName += tagPair[3];
            }
            return logName;
        }

        static string authorMessageRegex = @"^\[[A-Fa-f0-9]{6}\](.+?)\[FFFFFF\]: (.*)$";
        //take from newer spectrum version (stable can't use messages events)
        public static string ExtractMessageAuthor(string message)
        {
            try
            {
                Match msgMatch = Regex.Match(message, authorMessageRegex);
                return msgMatch.Success ? msgMatch.Groups[1].Value : string.Empty;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error getting player name: {e}");
                return string.Empty;
            }
        }

        public static bool IsSystemMessage(string message)
        {
            return message.Contains("[c]") && message.Contains("[/c]");
        }

        public static string ExtractMessageBody(string message)
        {
            try
            {
                Match msgMatch = Regex.Match(message, authorMessageRegex);
                return msgMatch.Success ? msgMatch.Groups[2].Value : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static CommandInfo getCommandInfo(string input)
        {
            var match = Regex.Match(input, @"^(([!%])\2?)(\S+)\s*(.*)\s*$");
            if (!match.Success)
                return new CommandInfo();
            else
                return new CommandInfo(match.Groups[2].Value == "%",
                    match.Groups[1].Value.Length == 2, match.Groups[3].Value, match.Groups[4].Value);
        }
    }
    public class CommandInfo
    {
        public bool matches = false;
        public bool local;
        public bool forceVisible;
        public string commandName;
        public string commandParams;

        public CommandInfo() { }
        public CommandInfo(bool local, bool forceVisible, string commandName, string commandParams)
        {
            matches = true;
            this.local = local;
            this.forceVisible = forceVisible;
            this.commandName = commandName;
            this.commandParams = commandParams;
        }
    }
}
