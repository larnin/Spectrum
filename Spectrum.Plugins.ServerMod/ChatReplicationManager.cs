using Events;
using Events.Server;
using Events.ServerToClient;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Spectrum.Plugins.ServerMod
{
    public class ChatReplicationManager
    {
        CircularBuffer<string> publicChatBuffer;
        Dictionary<string, CircularBuffer<string>> personalChatBuffers;
        List<string> needsReplication;
        int bufferSize;

        public ChatReplicationManager()
        {
            ChatLog chatLog = ChatLog.Instance_;
            bufferSize = (int)PrivateUtilities.getPrivateField(chatLog, "bufferSize_");

            publicChatBuffer = new CircularBuffer<string>(bufferSize);
            personalChatBuffers = new Dictionary<string, CircularBuffer<string>>();
            needsReplication = new List<string>();
        }

        public void Setup()
        {
            ChatLog chatLog = ChatLog.Instance_;
            PrivateUtilities.removeParticularSubscriber<WelcomeClient.Data>(chatLog);
            WelcomeClient.Subscribe(data =>
            {
                ReplicatePersonal(data.client_);
            });
            RemovePlayerFromClientList.Subscribe(data =>
            {
                personalChatBuffers.Remove(GeneralUtilities.getUniquePlayerString(data.player_));
                needsReplication.Remove(GeneralUtilities.getUniquePlayerString(data.player_));
            });
        }

        public CircularBuffer<string> GetPersonalBuffer(NetworkPlayer p)
        {
            var uniq = GeneralUtilities.getUniquePlayerString(p);
            CircularBuffer<string> personalBuffer;
            if (!personalChatBuffers.TryGetValue(uniq, out personalBuffer))
            {
                personalBuffer = new CircularBuffer<string>(bufferSize);
                foreach (string line in publicChatBuffer)
                    personalBuffer.Add(line);
                personalChatBuffers[uniq] = personalBuffer;
            }
            return personalBuffer;
        }

        public void AddPublic(string message)
        {
            string[] array = message.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None);
            foreach (string text in array)
            {
                if (text.Length > 0)
                {
                    publicChatBuffer.Add(message);
                    foreach (var personalBuffer in personalChatBuffers.Values)
                    {
                        personalBuffer.Add(text);
                    }
                }
            }
        }

        public void AddPersonal(NetworkPlayer p, string message)
        {
            if (p.IsLocal())
                return;
            var personalBuffer = GetPersonalBuffer(p);
            string[] array = message.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None);
            foreach (string text in array)
            {
                if (text.Length > 0)
                {
                    personalBuffer.Add(text);
                }
            }
        }

        public string GetBufferString(CircularBuffer<string> buffer)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string value in buffer)
            {
                stringBuilder.AppendLine(value);
            }
            int length = Environment.NewLine.Length;
            if (stringBuilder.Length > length)
            {
                stringBuilder.Length -= length;
            }
            return stringBuilder.ToString();
        }

        public void ReplicatePersonal(NetworkPlayer p)
        {
            var uniq = GeneralUtilities.getUniquePlayerString(p);
            if (needsReplication.Contains(uniq))
                needsReplication.Remove(uniq);
            if (p.IsLocal() || !GeneralUtilities.isHost())
                return;
            var personalBuffer = GetPersonalBuffer(p);
            StaticTargetedEvent<SetServerChat.Data>.Broadcast(p, new SetServerChat.Data(GetBufferString(personalBuffer)));
        }

        public void ReplicateAll()
        {
            foreach (ClientPlayerInfo current in G.Sys.PlayerManager_.PlayerList_)
                if (!current.IsLocal_)
                {
                    ReplicatePersonal(current.NetworkPlayer_);
                }
        }

        public void ReplicateNeeded()
        {
            if (needsReplication.Count == 0)
                return;
            foreach (ClientPlayerInfo current in G.Sys.PlayerManager_.PlayerList_)
            {
                var uniq = GeneralUtilities.getUniquePlayerString(current);
                if (needsReplication.Contains(uniq))
                {
                    ReplicatePersonal(current.NetworkPlayer_);
                }
            }
        }

        public void MarkForReplication(NetworkPlayer p)
        {
            if (p.IsLocal())
                return;
            var uniq = GeneralUtilities.getUniquePlayerString(p);
            if (!needsReplication.Contains(uniq))
                needsReplication.Add(uniq);
        }

        public void MarkAllForReplication()
        {
            foreach (ClientPlayerInfo current in G.Sys.PlayerManager_.PlayerList_)
                MarkForReplication(current.NetworkPlayer_);
        }

        public void Clear()
        {
            publicChatBuffer.Clear();
            personalChatBuffers.Clear();
            needsReplication.Clear();
        }
    }
}
