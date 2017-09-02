using Events;
using Events.ChatLog;
using Events.ClientToAllClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.Utilities
{
    static class MessageUtilities
    {
        public static void sendMessage(string message)
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
