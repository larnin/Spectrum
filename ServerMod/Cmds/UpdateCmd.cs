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

        public override bool Default { get; } = true;
    }
    class CmdSettingShowPrerelease : CmdSettingBool
    {
        public override string FileId { get; } = "updateShowPrerelease";
        public override string SettingsId { get; } = "updateShowPrerelease";

        public override string DisplayName { get; } = "Show pre-releases";
        public override string HelpShort { get; } = "Show pre-releases in updates list";
        public override string HelpLong { get; } = "Whether or not to show pre-release versions in the updates list. Note that at the time of writing this, all versions are pre-release versions.";

        public override bool Default { get; } = true;
        public override ServerModVersion UpdatedOnVersion { get; } = new ServerModVersion("C.8.0.0");
    }
    class UpdateCmd : Cmd
    {
        public override string name { get { return "update"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseLocal { get { return false; } }

        public bool updateCheck
        {
            get { return getSetting<CmdSettingUpdateCheck>().Value; }
            set { getSetting<CmdSettingUpdateCheck>().Value = value; }
        }
        public bool showPrerelease
        {
            get { return getSetting<CmdSettingShowPrerelease>().Value; }
            set { getSetting<CmdSettingShowPrerelease>().Value = value; }
        }

        public static bool showPrereleaseStatic
        {
            get
            {
                return Cmd.all.getCommand<UpdateCmd>("update").showPrerelease;
            }
        }

        private static string updateCheckURL = "https://api.github.com/repos/corecii/spectrum/releases";
        private static string updateCheckRemoteRegex = @"ServerMod\.(.\.\d+\.\d+\.\d+)";

        public static string changesRegexOuter = @"Changes\:[\r\n]+((?:(?:\* [^\r\n]+[\r\n]*)(?:\s+\* [^\r\n]+[\r\n]*)*)+)";
        public static string changesRegexInner = @"(?:\* ([^\r\n]+)[\r\n]*)(?:\s+\* [^\r\n]+[\r\n]*)*";

        private const string tagNameRegex = "\"tag_name\": ?\"(.+?)\",";

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingUpdateCheck(),
            new CmdSettingShowPrerelease()
        };

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!update") + ": Check for updates to ServerMod");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(p));
            checkForUpdates(true);
            MessageUtilities.popMessageOptions();
        }

        public static void checkForUpdates(bool sendMessageIfNone)
        {
            G.Sys.GameManager_.StartCoroutine(getUpdates(releases =>
            {
                try
                {
                    var updates = new List<Dictionary<string, object>>();
                    foreach (var release in releases)
                    {
                        if (!(bool)release["draft"])
                        {
                            string releaseTag = (string)release["tag_name"];
                            var releaseTagMatch = Regex.Match(releaseTag, updateCheckRemoteRegex);
                            if (releaseTagMatch.Success)
                            {
                                var version = new ServerModVersion(releaseTagMatch.Groups[1].Value);
                                if (version <= Entry.PluginVersion)
                                    break;
                                updates.Add(release);
                            }
                        }
                    }
                    ///
                    if (updates.Count == 0)
                    {
                        if (sendMessageIfNone)
                        {
                            MessageUtilities.sendMessage("No updates to ServerMod available.");
                        }
                    }
                    else
                    {
                        string printTxt = "";
                        printTxt += ("[A0D0A0]There are updates for ServerMod available.[-]\n");
                        printTxt += ("[00D000]You are on " + Entry.PluginVersion + "[-]\n");
                        printTxt += ("[A0D0A0]Newer versions:[-]\n");
                        int preCount = 0;
                        int count = 0;
                        foreach (var release in updates)
                        {
                            string releaseTag = (string)release["tag_name"];
                            var releaseTagMatch = Regex.Match(releaseTag, updateCheckRemoteRegex);
                            var version = new ServerModVersion(releaseTagMatch.Groups[1].Value);
                            if (count == 3)
                            {
                                printTxt += ($"  [A0D0A0]and {updates.Count - count} more...[-]\n");
                                break;
                            }
                            bool prerelease = (bool)release["prerelease"];
                            if (prerelease && !showPrereleaseStatic)
                            {
                                preCount++;
                                continue;
                            }
                            string prereleaseText = (prerelease ? " [707070](pre-release)[-]" : "");
                            if (count == 0)
                            {
                                if (preCount > 0)
                                    printTxt += ($"  [508050]({preCount} pre-releases)[-]\n");
                                printTxt += ("  [00F000]" + version + "[-]" + prereleaseText + "\n");
                                string body = (string)release["body"];
                                var outerMatch = Regex.Match(body, changesRegexOuter);
                                if (outerMatch.Success)
                                {
                                    var innerMatches = Regex.Matches(outerMatch.Groups[1].Value, changesRegexInner);
                                    var releaseNoteCount = 0;
                                    foreach (Match innerMatch in innerMatches)
                                    {
                                        if (releaseNoteCount == 4)
                                        {
                                            if (innerMatches.Count > 4)
                                                printTxt += ($"  and {innerMatches.Count - 4} more.\n");
                                            break;
                                        }
                                        printTxt += ("  • " + innerMatch.Groups[1].Value + "\n");
                                        releaseNoteCount++;
                                    }
                                }
                            }
                            else
                                printTxt += ("  [A0D0A0]" + version + "[-]" + prereleaseText + "\n");
                            count++;
                        }
                        if (preCount > 0 && count == 0)
                            MessageUtilities.sendMessage($"[508050]ServerMod: {preCount} pre-release updates available[-]");
                        else
                            MessageUtilities.sendMessage(printTxt.Substring(0, printTxt.Length - 1));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }));
        }

        public delegate void updateCallback(List<Dictionary<string, object>> releases);
        
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

            var jsonReader = new JsonFx.Json.JsonReader();

            var releases = jsonReader.Read<List<Dictionary<string, object>>>(result);

            callback(releases);
            ///
            yield break;
        }
    }
}
