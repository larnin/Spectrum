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
                var eventArgs = new ChatMessageEventArgs(data.message_);
                MessageSent?.Invoke(null, eventArgs);
            });

            Events.ClientToAllClients.ChatMessage.Subscribe(data =>
            {
                var eventArgs = new ChatMessageEventArgs(data.message_);
                MessageReceived?.Invoke(null, eventArgs);
            });
        }
    }
}
