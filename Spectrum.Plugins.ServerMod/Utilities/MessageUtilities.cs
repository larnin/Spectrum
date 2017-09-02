using Events;
using Events.ChatLog;
using Events.ClientToAllClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public override void Apply(MessageState messageState)
        {
            messageState.forPlayer = true;
            messageState.player = player;
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
            if (currentState.forPlayer && currentState.player.IsLocal_)
            {
                StaticEvent<AddMessage.Data>.Broadcast(new AddMessage.Data(message));
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
    }
}
