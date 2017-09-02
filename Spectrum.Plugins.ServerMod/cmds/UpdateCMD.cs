using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class CmdSettingUpdateCheck : CmdSettingBool
    {
        public override string FileId { get; } = "updateCheck";
        public override string SettingsId { get; } = "updateCheck";

        public override string DisplayName { get; } = "Automatic Update Check";
        public override string HelpShort { get; } = "Show updates on server start";
        public override string HelpLong { get; } = "Whether or not to show updates to ServerMod when a server is started";

        public override object Default { get; } = true;
    }
    class UpdateCmd : Cmd
    {
        public override string name { get { return "update"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        public bool updateCheck
        {
            get { return (bool)getSetting("updateCheck").Value; }
            set { getSetting("updateCheck").Value = value; }
        }
        private static string updateCheckURL = "https://api.github.com/repos/corecii/spectrum/releases";
        private static string updateCheckRemoteRegex = @"ServerMod\.(.\.\d+\.\d+\.\d+)";
        public static string updateCheckLocalRegex = @"Version (.\.\d+\.\d+\.\d+)";

        private const string tagNameRegex = "\"tag_name\": ?\"(.+?)\",";

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingUpdateCheck()
        };

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(GeneralUtilities.formatCmd("!update") + ": Check for updates to ServerMod");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            checkForUpdates(true);
        }

        public static void checkForUpdates(bool sendMessageIfNone)
        {
            G.Sys.GameManager_.StartCoroutine(getUpdates(updates =>
            {
                try
                {
                    if (updates.Count == 0)
                    {
                        if (sendMessageIfNone)
                        {
                            MessageUtilities.sendMessage("No updates to ServerMod available.");
                        }
                    }
                    else
                    {
                        MessageUtilities.sendMessage("[A0D0A0]There are updates for ServerMod available.[-]");
                        MessageUtilities.sendMessage("[00D000]You are on " + Entry.PluginVersion + "[-]");
                        MessageUtilities.sendMessage("[A0D0A0]Newer versions:[-]");
                        int count = 0;
                        foreach (string update in updates)
                        {
                            if (count == 0)
                                MessageUtilities.sendMessage("[00F000]" + update + "[-]");
                            else if (count == 3)
                            {
                                MessageUtilities.sendMessage($"[A0D0A0]And {updates.Count - count} more...[-]");
                                break;
                            }
                            else
                                MessageUtilities.sendMessage("[A0D0A0]" + update + "[-]");
                            count++;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }));
        }

        public delegate void updateCallback(List<string> versions);
        
        public static IEnumerator getUpdates(updateCallback callback)
        {
            // The async http request code here is a mess cobbled together from various posts
            //  on the internet. Non-async requests would freeze up the client.
            var webRequest = (HttpWebRequest)WebRequest.Create(updateCheckURL);
            webRequest.UserAgent = "Corecii-Spectrum-ServerMod";
            webRequest.Method = "GET";
            webRequest.Accept = "application/vnd.github.v3+json";
            // NOTE: THIS IS NOT SECURE. IT DOES NOT CHECK THE SSL CERTIFICATE
            // I was getting SSL errors making the web request. After looking it up, it appeared
            //  that the cause was an old version of whatever provides the network stuff. I
            //  would assume that can't be updated unless Distance is updated, but I don't know.
            // Anyways, this is not too bad just for one request. If it's MitM'd, the worst case
            //  scenario is that the player checks github for an update and finds nothing new.
            // After making the request, the original certificate checker is put back in place.
            HttpWebResponse response = null;
            Action wrapperAction = () =>
            {
                var previousCallback = ServicePointManager.ServerCertificateValidationCallback;
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                webRequest.BeginGetResponse(new AsyncCallback((iar) =>
                {
                    response = (HttpWebResponse)((HttpWebRequest)iar.AsyncState).EndGetResponse(iar);
                    ServicePointManager.ServerCertificateValidationCallback = previousCallback;

                }), webRequest);
            };
            IAsyncResult asyncResult = wrapperAction.BeginInvoke(new AsyncCallback((iar) =>
            {
                var action = (Action)iar.AsyncState;
                action.EndInvoke(iar);
            }), wrapperAction);
            while (!asyncResult.IsCompleted || response == null) { yield return null; }
            ///
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("There was an error checking for a newer version of ServerMod");
                yield break;
            }
            var stream = response.GetResponseStream();
            var reader = new StreamReader(stream);

            var result = reader.ReadToEnd();
            response.Close();

            var localVersion = Regex.Match(Entry.PluginVersion, updateCheckLocalRegex).Groups[1].Value;

            List<string> versions = new List<string>();

            foreach (Match match in Regex.Matches(result, tagNameRegex))
            {
                var remoteVersion = Regex.Match(match.Groups[1].Value, updateCheckRemoteRegex).Groups[1].Value;
                if (remoteVersion == localVersion)
                {
                    break;
                }
                versions.Add(remoteVersion);
            }

            callback(versions);
            ///
            yield break;
        }
    }
}
