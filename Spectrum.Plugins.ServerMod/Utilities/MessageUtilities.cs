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
        public bool shown = true;
        public bool forPlayer = false;
        public ClientPlayerInfo player = null;
        public MessageState(bool shown, ClientPlayerInfo player)
        {
            this.shown = shown;
            forPlayer = player != null;
            this.player = player;
        }
        public MessageState(bool shown)
        {
            this.shown = shown;
        }
        public MessageState(ClientPlayerInfo player)
        {
            forPlayer = player != null;
            this.player = player;
        }
        public MessageState() { }
    }
    abstract class MessageStateOption
    {
        public abstract void Apply(MessageState messageState);
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
        public static MessageStateOption[] popMessageOptions()
        {
            return messageOptionStack.Pop();
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
        public static void sendMessage(string message)
        {
            var currentState = computeMessageState();
            if (!currentState.shown)
                return;
            if (currentState.forPlayer)
            {
                // slightly blue text for local-only messages
                if (currentState.player.IsLocal_)
                    StaticEvent<AddMessage.Data>.Broadcast(new AddMessage.Data((message).Colorize("[70AAAA]")));
                else
                {
                    Entry.Instance.chatReplicationManager.AddPersonal(currentState.player.NetworkPlayer_, (message).Colorize("[70AAAA]"));
                    Entry.Instance.chatReplicationManager.MarkForReplication(currentState.player.NetworkPlayer_);
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
            }
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

        public static CommandInfo getCommandInfo(string input)
        {
            var match = Regex.Match(input, @"^(([!%])\2?)(\S+)\s*(.*)\s*$");
            if (!match.Success)
                return new CommandInfo();
            else
                return new CommandInfo(match.Groups[2].Value == "%", match.Groups[1].Value.Length == 2,
                    match.Groups[3].Value, match.Groups[4].Value);
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
