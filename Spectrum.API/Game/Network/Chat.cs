using System;
using Events;
using Events.ClientToAllClients;
using Events.Local;
using Spectrum.API.Game.EventArgs.Network;

namespace Spectrum.API.Game.Network
{
    public class Chat
    {
        public static event EventHandler<ChatMessageEventArgs> MessageReceived;
        public static event EventHandler<ChatMessageEventArgs> MessageSent;
        public static event EventHandler<ChatActionEventArgs> ActionReceived;

        static Chat()
        {
            ChatSubmitMessage.Subscribe(data =>
            {
                var eventArgs = new ChatMessageEventArgs(G.Sys.PlayerManager_.Current_.profile_.Name_, data.message_);
                MessageSent?.Invoke(null, eventArgs);
            });

            ChatMessage.Subscribe(data =>
            {
                Console.WriteLine(data.message_);

                var author = ExtractMessageAuthor(data.message_);

                if (author != G.Sys.PlayerManager_.Current_.profile_.Name_ && !IsSystemMessage(data.message_))
                {
                    var eventArgs = new ChatMessageEventArgs(author, ExtractMessageBody(data.message_));
                    MessageReceived?.Invoke(null, eventArgs);
                }
            });

            ToAllClientsRemotePlayerActionMessage.Subscribe(data =>
            {
                if (G.Sys.NetworkingManager_.IsOnline_)
                {
                    var nickname = G.Sys.PlayerManager_.PlayerList_[data.index_].Username_;
                    var eventArgs = new ChatActionEventArgs(data.index_, nickname, data.message_);
                    ActionReceived?.Invoke(null, eventArgs);
                }
            });
        }

        public static void SendMessage(string message)
        {
            StaticEvent<ChatSubmitMessage.Data>.Broadcast(new ChatSubmitMessage.Data(message));
        }

        public static void SendAction(string actionMessage)
        {
            StaticEvent<PlayerActionMessage.Data>.Broadcast(new PlayerActionMessage.Data(" " + actionMessage));
        }

        public static void AddLocalMessage(string message)
        {
            BroadcastAllEvent<ChatMessage.Data>.BroadcastLocal(new ChatMessage.Data(message));
        }

        public static void ClearLog()
        {
            ChatLog.ClearLog();
        }

        private static string ExtractMessageAuthor(string message)
        {
            try
            {
                // 1. [xxxxxx]user[xxxxxx]: adfsafasf
                var withoutFirstColorTag = message.Substring(message.IndexOf(']') + 1, message.Length - message.IndexOf(']') - 1);
                // 2. user[xxxxxx]: adfsafasf
                var withoutSecondColorTag = withoutFirstColorTag.Substring(0, withoutFirstColorTag.IndexOf('['));
                // 3. user

                return withoutSecondColorTag;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string ExtractMessageBody(string message)
        {
            try
            {
                // 1. [xxxxxx]user[xxxxxx]: body
                return message.Substring(message.IndexOf(':') + 1).Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static bool IsSystemMessage(string message)
        {
            return message.Contains("[c]") && message.Contains("[/c]");
        }
    }
}
