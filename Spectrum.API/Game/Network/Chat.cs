using System;
using Spectrum.API.Game.EventArgs.Network;

namespace Spectrum.API.Game.Network
{
    public class Chat
    {
        public static event EventHandler<ChatMessageEventArgs> MessageReceived;
        public static event EventHandler<ChatMessageEventArgs> MessageSent;

        static Chat()
        {
            Events.Local.ChatSubmitMessage.Subscribe(data =>
            {
                var eventArgs = new ChatMessageEventArgs(G.Sys.PlayerManager_.Current_.profile_.Name_, data.message_);
                MessageSent?.Invoke(null, eventArgs);
            });

            Events.ClientToAllClients.ChatMessage.Subscribe(data =>
            {
                var author = ExtractMessageAuthor(data.message_);

                if (author != G.Sys.PlayerManager_.Current_.profile_.Name_ && !IsSystemMessage(data.message_))
                {
                    var eventArgs = new ChatMessageEventArgs(author, ExtractMessageBody(data.message_));
                    MessageReceived?.Invoke(null, eventArgs);
                }
            });
        }

        private static string ExtractMessageAuthor(string message)
        {
            // 1. [xxxxxx]user[xxxxxx]: adfsafasf
            var withoutFirstColorTag = message.Substring(message.IndexOf(']') + 1, message.Length - message.IndexOf(']') - 1);
            // 2. user[xxxxxx]: adfsafasf
            var withoutSecondColorTag = withoutFirstColorTag.Substring(0, withoutFirstColorTag.IndexOf('['));
            // 3. user

            return withoutSecondColorTag;
        }

        private static string ExtractMessageBody(string message)
        {
            // 1. [xxxxxx]user[xxxxxx]: body
            return message.Substring(message.IndexOf(':') + 1).Trim();
        }

        private static bool IsSystemMessage(string message)
        {
            return message.Contains("[c]") && message.Contains("[/c]");
        }
    }
}
