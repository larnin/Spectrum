using Events;
using Events.ChatLog;
using Events.Network;
using Events.Server;
using Events.ServerToClient;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections;
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

        public int RemoveEventListeners()
        {
            DebugLog("Removing event listeners...");
            ChatLog chatLog = ChatLog.Instance_;
            PrivateUtilities.removeParticularSubscriber<WelcomeClient.Data>(chatLog);
            PrivateUtilities.removeParticularSubscriber<SetServerChat.Data>(chatLog);
            var chatInputV2s = PrivateUtilities.getComponents<ChatInputV2>();
            PrivateUtilities.removeParticularSubscribers<SetServerChat.Data, ChatInputV2>(chatInputV2s);
            return chatInputV2s.Count;
        }

        IEnumerator RemoveEventListenersCoroutine()
        {
            var count = 0;
            while (count == 0)
            {
                count = RemoveEventListeners();
                if (count == 0)
                {
                    yield return new WaitForSeconds(0.1f);
                    DebugLog("Cannot find ChatInputV2 to remove event listeners from...");
                }
            }
            DebugLog($"Removed event listeners from {count} ChatInputV2s.");
            yield break;
        }

        public void DebugLog(string txt)
        {
            if (Cmds.LogCmd.debugChatLogs)
                Console.WriteLine(txt);
        }

        public void Setup()
        {
            RemoveEventListeners();

            ServerInitialized.Subscribe(data =>
            {
                DebugLog("Started server, clearing logs and removing event listeners.");
                Clear();
                G.Sys.GameManager_.StartCoroutine(RemoveEventListenersCoroutine());
            });
            ConnectedToServer.Subscribe(data =>
            {
                DebugLog("Connected to server, clearing logs and removing event listeners.");
                Clear();
            });
            
            WelcomeClient.Subscribe(data =>
            {
                DebugLog($"Client {data.client_.guid} joined, replicating logs.");
                ReplicatePersonal(data.client_);
            });
            ClientDisconnected.Subscribe(data =>
            {
                DebugLog($"Client {data.networkPlayer_.guid} left, removing logs.");
                personalChatBuffers.Remove(GeneralUtilities.getUniquePlayerString(data.networkPlayer_));
                needsReplication.Remove(GeneralUtilities.getUniquePlayerString(data.networkPlayer_));
            });
            // fix remote logs
            SetServerChat.Subscribe(data =>
            {
                GeneralUtilities.testFunc(() =>
                {
                    DebugLog($"Replicating remote log from SetServerChat...");
                    RemoveEventListeners();

                    AddRemoteLog(data.chatText_);
                });
            });
        }

        public void AddRemoteLog(string remoteLog)
        {
            DebugLog("Adding remote log...");
            var localClient = GeneralUtilities.localClient();
            if (localClient == null)
            {
                DebugLog("No local client. Clearing logs and setting logs directly.");
                Clear();
                AddPublic(remoteLog);
                ChatLog.SetLog(remoteLog);
                G.Sys.GameManager_.StartCoroutine(RemoveEventListenersCoroutine());
                return;
            }
            DebugLog("Adding logs from the server:");

            string[] remoteLogArray = System.Text.RegularExpressions.Regex.Split(remoteLog, $"\r\n|\n|\r");

            var localNetworkPlayer = localClient.NetworkPlayer_;
            var personalBuffer = GetPersonalBuffer(localNetworkPlayer);

            List<DiffLine> publicDiff = DiffLine.GetDiffLines(publicChatBuffer);
            DiffLine.ExecuteDiff(publicDiff, remoteLogArray);
            string publicLog = DiffLine.DiffLinesToString(publicDiff);

            DebugLog($"\nPublic diff:\n{DiffLine.DiffLinesToStringInfo(publicDiff)}");

            List<DiffLine> personalDiff = publicDiff;
            DiffLine.ExecuteDiff(personalDiff, personalBuffer, true, true);  // true, true: only add lines, add lines before any other previously added lines
            string personalLog = DiffLine.DiffLinesToString(personalDiff);

            publicChatBuffer.Clear();
            AddPublicNoPersonal(publicLog);

            personalBuffer.Clear();
            AddPersonalNoAddLocal(localNetworkPlayer, personalLog);

            DebugLog($"\nFinal diff:\n{DiffLine.DiffLinesToStringInfo(personalDiff)}");

            ChatLog.SetLog(personalLog);
            foreach (var chatInput in PrivateUtilities.getComponents<ChatInputV2>())
                PrivateUtilities.callPrivateMethod(chatInput, "OnEventSetServerChat", new SetServerChat.Data(personalLog));
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
                    publicChatBuffer.Add(text);
                    foreach (var personalBuffer in personalChatBuffers.Values)
                    {
                        personalBuffer.Add(text);
                    }
                }
            }
        }

        public void AddPublicNoPersonal(string message)
        {
            string[] array = message.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None);
            foreach (string text in array)
            {
                if (text.Length > 0)
                {
                    publicChatBuffer.Add(text);
                }
            }
        }

        public void AddPersonal(NetworkPlayer p, string message)
        {
            if (p.IsLocal())
                StaticEvent<AddMessage.Data>.Broadcast(new AddMessage.Data(message));
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

        public void AddPersonalNoAddLocal(NetworkPlayer p, string message)
        {
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
    public class DiffLine
    {
        public string Original;
        public List<string> New;
        public bool Remove = false;
        public DiffLine()
        {
            Original = string.Empty;
            New = new List<string>();
        }
        public DiffLine(string original)
        {
            Original = original;
            New = new List<string>();
        }

        public static List<DiffLine> GetDiffLines(IEnumerable<string> lines)
        {
            List<DiffLine> personalDiff = new List<DiffLine>();
            foreach (string line in lines)
            {
                personalDiff.Add(new DiffLine(line));
            }
            var finalLine = new DiffLine();
            finalLine.Remove = true;
            personalDiff.Add(finalLine);
            return personalDiff;
        }

        public static void ExecuteDiff(List<DiffLine> personalDiff, IEnumerable<string> linesInput)
        {
            ExecuteDiff(personalDiff, linesInput, false, false);
        }

        public static void ExecuteDiff(List<DiffLine> personalDiff, IEnumerable<string> linesInput, bool addOnly, bool addFirst)
        {
            List<string> lines = new List<string>(linesInput);
            lines.Add(string.Empty);
            int currentLineAddIndex = 0;
            int currentDiffLineIndex = 0;
            foreach (string line in lines)
            {
                if (currentDiffLineIndex >= personalDiff.Count)
                    currentDiffLineIndex = personalDiff.Count - 1;
                DiffLine currentDiffLine = personalDiff[currentDiffLineIndex];
                if (currentDiffLine.Original != line)
                {
                    int foundInnerIndex = -1;
                    if (!addOnly)
                        for (int indexInner = currentDiffLineIndex + 1; indexInner < personalDiff.Count; indexInner++)
                        {
                            DiffLine currentDiffLineInner = personalDiff[indexInner];
                            if (currentDiffLineInner.Original.TrimEnd() == line.TrimEnd())
                            {
                                foundInnerIndex = indexInner;
                                break;
                            }
                        }
                    if (foundInnerIndex != -1)
                    {
                        for (int indexInner = foundInnerIndex - 1; indexInner >= currentDiffLineIndex; indexInner--)
                        {
                            personalDiff[indexInner].Remove = true;
                        }
                        currentDiffLineIndex = foundInnerIndex + 1;
                        currentLineAddIndex = 0;
                    }
                    else
                    {
                        if (addFirst)
                            currentDiffLine.New.Insert(currentLineAddIndex, line);
                        else
                            currentDiffLine.New.Add(line);
                        currentLineAddIndex++;
                    }
                }
                else
                {
                    currentDiffLineIndex++;
                    currentLineAddIndex = 0;
                }
            }
        }

        public static List<string> DiffLinesToList(List<DiffLine> lines)
        {
            List<string> newList = new List<string>();
            for (int i = 0; i < lines.Count; i++)
            {
                var diffLine = lines[i];
                foreach (string line in diffLine.New)
                    newList.Add(line);
                if (!diffLine.Remove)
                    newList.Add(diffLine.Original);
            }
            return newList;
        }

        public static string DiffLinesToString(List<DiffLine> lines)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string value in DiffLinesToList(lines))
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

        public static List<string> DiffLinesToListInfo(List<DiffLine> lines)
        {
            List<string> newList = new List<string>();
            for (int i = 0; i < lines.Count; i++)
            {
                var diffLine = lines[i];
                foreach (string line in diffLine.New)
                    newList.Add("+ " + line);
                if (!diffLine.Remove)
                    newList.Add("| " + diffLine.Original);
                else
                    newList.Add("- " + diffLine.Original);
            }
            return newList;
        }

        public static string DiffLinesToStringInfo(List<DiffLine> lines)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string value in DiffLinesToListInfo(lines))
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
    }
}
