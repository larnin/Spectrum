using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.PlaylistTools;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class CmdLog
    {
        public readonly bool IsSystemMessage;
        public readonly string FromPlayer;
        public readonly string Command;
        public readonly string Results;
        public CmdLog(string FromPlayer, string Command, string Results)
        {
            this.IsSystemMessage = false;
            this.FromPlayer = FromPlayer;
            this.Command = Command;
            this.Results = Results;
        }
        public CmdLog(string Command, string Results)
        {
            this.IsSystemMessage = true;
            this.FromPlayer = "System";
            this.Command = Command;
            this.Results = Results;
        }
    }
    class LogCmd : Cmd
    {
        public override string name { get { return "log"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return true; } }

        const int pageSize = 10;
        const string cmdRegex = @"^(?:(\d+)(?:\s+(\d+)(?:\s+(\d+))?)?|(.+?)(?:\s+(\d+)(?:\s+(\d+)(?:\s+(\d+))?)?)?)$";
        // Group 1: Int 1 (page)
        // Group 2: Int 2 (index | index start)
        // Group 3: Int 3 (index | index end)

        // Group 4: String 1 (Player name)
        // Group 5: Int 1 (page)
        // Group 6: Int 2 (index | index start)
        // Group 7: Int 3 (index | index end)

        public static List<CmdLog> Logs = new List<CmdLog>();

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingLogCount(),
            new CmdSettingLocalHostCommand(),
            new CmdSettingLocalHostResults(),
            new CmdSettingLocalClientCommand(),
            new CmdSettingLocalClientResults(),
        };

        static public int logCount
        {
            get { return Cmd.all.getCommand<LogCmd>().getSetting<CmdSettingLogCount>().Value; }
            set { Cmd.all.getCommand<LogCmd>().getSetting<CmdSettingLogCount>().Value = value; }
        }
        static public bool localHostCommands
        {
            get { return Cmd.all.getCommand<LogCmd>().getSetting<CmdSettingLocalHostCommand>().Value; }
            set { Cmd.all.getCommand<LogCmd>().getSetting<CmdSettingLocalHostCommand>().Value = value; }
        }
        static public bool localHostResults
        {
            get { return Cmd.all.getCommand<LogCmd>().getSetting<CmdSettingLocalHostResults>().Value; }
            set { Cmd.all.getCommand<LogCmd>().getSetting<CmdSettingLocalHostResults>().Value = value; }
        }
        static public bool showHostAllCommands
        {
            get { return Cmd.all.getCommand<LogCmd>().getSetting<CmdSettingShowHostCommand>().Value; }
            set { Cmd.all.getCommand<LogCmd>().getSetting<CmdSettingShowHostCommand>().Value = value; }
        }
        static public bool showHostAllResults
        {
            get { return Cmd.all.getCommand<LogCmd>().getSetting<CmdSettingShowHostResults>().Value; }
            set { Cmd.all.getCommand<LogCmd>().getSetting<CmdSettingShowHostResults>().Value = value; }
        }
        static public bool localClientCommands
        {
            get { return Cmd.all.getCommand<LogCmd>().getSetting<CmdSettingLocalClientCommand>().Value; }
            set { Cmd.all.getCommand<LogCmd>().getSetting<CmdSettingLocalClientCommand>().Value = value; }
        }
        static public bool localClientResults
        {
            get { return Cmd.all.getCommand<LogCmd>().getSetting<CmdSettingLocalClientResults>().Value; }
            set { Cmd.all.getCommand<LogCmd>().getSetting<CmdSettingLocalClientResults>().Value = value; }
        }

        public static void AddLog(ClientPlayerInfo FromPlayer, string Command, string Results)
        {
            if (logCount == 0)
                return;
            if (Regex.Match(Command, @"^(?:\[[^\]]*\])*([\!\%])\1?log").Success)  //checks for `!log` preceded by 0 or more tags.
                return;  // don't log the !log command. makes things too confusing and hard to use.

            // finds the number of color tags and closing tags, then adds to the beginning/end to make sure they match
            string logName = MessageUtilities.closeTags(FromPlayer.GetChatName());

            Logs.Insert(0, new CmdLog(logName, Command, Results));
            while (Logs.Count > logCount)
                Logs.RemoveAt(Logs.Count - 1);
        }
        public static void AddLog(string Command, string Results)
        {
            if (logCount == 0)
                return;
            Logs.Insert(0, new CmdLog(Command, Results));
            while (Logs.Count > logCount)
                Logs.RemoveAt(Logs.Count - 1);
        }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!log") + ": Show all logs");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!log [page]") + ": Show logs on [page]");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!log [page] [index]") + ": Show detailed logs for cmd on [page] at [index]");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!log [page] [index start] [index end]") + ": Show detailed logs for cmd on [page] from [index start] to [index end]");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!log [player name]") + ": Show logs for [player name]");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!log [player name] [page]") + ": Show logs [player name] on [page]");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!log [player name] [page] [index]") + ": Show detailed logs");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!log [player name] [page] [index start] [index end]") + ": Show multiple detailed logs");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            string originalPlayerName;
            string playerName;
            int page, indexStart, indexEnd;
            bool showDetailedLogs = false;

            var match = Regex.Match(message, cmdRegex);
            if (!match.Success) // No text, we should show all recent logs
            {
                originalPlayerName = "All";
                playerName = ".*";
                page = 0;
                indexStart = 0;
                indexEnd = pageSize - 1;
            }
            else if (!string.IsNullOrEmpty(match.Groups[1].Value)) // Matching all logs
            {
                originalPlayerName = "All";
                playerName = ".*";
                page = int.Parse(match.Groups[1].Value);
                showDetailedLogs = !string.IsNullOrEmpty(match.Groups[2].Value);
                indexStart = showDetailedLogs ? int.Parse(match.Groups[2].Value) : 0;
                if (!string.IsNullOrEmpty(match.Groups[3].Value))
                    indexEnd = int.Parse(match.Groups[3].Value);
                else if (showDetailedLogs)
                    indexEnd = indexStart;
                else
                    indexEnd = pageSize - 1;
            }
            else // Matching player-specific logs
            {
                originalPlayerName = match.Groups[4].Value;
                playerName = GeneralUtilities.getSearchRegex(match.Groups[4].Value);
                page = !string.IsNullOrEmpty(match.Groups[5].Value) ? int.Parse(match.Groups[5].Value) : 0;
                showDetailedLogs = !string.IsNullOrEmpty(match.Groups[6].Value);
                indexStart = showDetailedLogs ? int.Parse(match.Groups[6].Value) : 0;
                if (!string.IsNullOrEmpty(match.Groups[7].Value))
                    indexEnd = int.Parse(match.Groups[7].Value);
                else if (showDetailedLogs)
                    indexEnd = indexStart;
                else
                    indexEnd = pageSize - 1;
            }
            // Now that playerName, page, indexStart, and indexEnd are set, we just find matching logs.
            List<CmdLog> playerSpecificLogs = new List<CmdLog>();
            foreach (CmdLog log in Logs)
            {
                string name = Regex.Replace(log.FromPlayer, @"\[[^\]]*\]", "");
                if (Regex.Match(name, playerName, RegexOptions.IgnoreCase).Success)
                    playerSpecificLogs.Add(log);
            }
            indexStart += page * pageSize;
            indexEnd += page * pageSize;
            indexEnd = Math.Min(playerSpecificLogs.Count - 1, indexEnd);
            int foundLogsCount = Math.Max(0, indexEnd - indexStart);
            string response = "";
            // If indexStart is beyond the end of Logs, it will also be beyond indexEnd so no iteration of the loop will run
            for (int index = indexEnd; index >= indexStart; index--)
            {
                CmdLog log = playerSpecificLogs[index];
                response += $"[FFFFFF]{index - indexStart} {log.FromPlayer}:[-] {log.Command}\n";
                if (showDetailedLogs)
                    response += $"{log.Results}\n\n";
            }
            string pageTxt = $"[FFFFFF]Page {page}/{playerSpecificLogs.Count/pageSize} for {originalPlayerName}";
            if (showDetailedLogs)
                pageTxt += $", index {indexStart} to {indexEnd} (detailed)[-]\n";
            else if (playerName == ".*")
                pageTxt += " (!log <page> <index> for detailed logs)[-]";
            else
                pageTxt += " (!log [player] <page> <index> for detailed logs)[-]";
            MessageUtilities.sendMessage(p, pageTxt);
            if (response.Length == 0)
                MessageUtilities.sendMessage(p, "No logs found.");
            else
                MessageUtilities.sendMessage(p, response.Substring(0, response.Length - 1));
        }
    }
    class CmdSettingLogCount : CmdSettingInt
    {
        public override string FileId { get; } = "logCount";
        public override string SettingsId { get; } = "logCount";

        public override string DisplayName { get; } = "!log Log Count";
        public override string HelpShort { get; } = "!log: How many logs to keep";
        public override string HelpLong { get { return HelpShort; } }
        public override ServerModVersion UpdatedOnVersion { get; } = new ServerModVersion("C.8.0.0");

        public override int Default { get; } = 100;
    }
    class CmdSettingShowHostCommand : CmdSettingBool
    {
        public override string FileId { get; } = "showHostCommand";
        public override string SettingsId { get; } = "showHostCmd";

        public override string DisplayName { get; } = "Show All Commands to Host";
        public override string HelpShort { get; } = "Whether the host should see all commands used.";
        public override string HelpLong { get { return HelpShort; } }
        public override ServerModVersion UpdatedOnVersion { get; } = new ServerModVersion("C.8.0.0");

        public override bool Default { get; } = true;
    }
    class CmdSettingShowHostResults : CmdSettingBool
    {
        public override string FileId { get; } = "showHostCommand";
        public override string SettingsId { get; } = "showHostCmd";

        public override string DisplayName { get; } = "Show All Command Results to Host";
        public override string HelpShort { get; } = "Whether the host should see the results of all commands used.";
        public override string HelpLong { get { return HelpShort; } }
        public override ServerModVersion UpdatedOnVersion { get; } = new ServerModVersion("C.8.0.0");

        public override bool Default { get; } = false;
    }
    class CmdSettingLocalHostCommand : CmdSettingBool
    {
        public override string FileId { get; } = "localHostCommand";
        public override string SettingsId { get; } = "localHostCmd";

        public override string DisplayName { get; } = "Host Command Visibility";
        public override string HelpShort { get; } = "Whether host command use should only be displayed to the host.";
        public override string HelpLong { get { return HelpShort; } }
        public override ServerModVersion UpdatedOnVersion { get; } = new ServerModVersion("C.8.0.0");

        public override bool Default { get; } = true;
    }
    class CmdSettingLocalHostResults : CmdSettingBool
    {
        public override string FileId { get; } = "localHostResults";
        public override string SettingsId { get; } = "localHostRes";

        public override string DisplayName { get; } = "Host Results Visibility";
        public override string HelpShort { get; } = "Whether host command results should only be displayed to the host.";
        public override string HelpLong { get { return HelpShort; } }
        public override ServerModVersion UpdatedOnVersion { get; } = new ServerModVersion("C.8.0.0");

        public override bool Default { get; } = true;
    }
    class CmdSettingLocalClientCommand : CmdSettingBool
    {
        public override string FileId { get; } = "localClientCommand";
        public override string SettingsId { get; } = "localClientCmd";

        public override string DisplayName { get; } = "Client Command Visibility";
        public override string HelpShort { get; } = "Whether client command use should only be displayed to the client.";
        public override string HelpLong { get { return HelpShort + " If this is on, commands might flicker for other clients as the command is erased from their screen."; } }
        public override ServerModVersion UpdatedOnVersion { get; } = new ServerModVersion("C.8.0.0");

        public override bool Default { get; } = false;
    }
    class CmdSettingLocalClientResults : CmdSettingBool
    {
        public override string FileId { get; } = "localClientResults";
        public override string SettingsId { get; } = "localClientRes";

        public override string DisplayName { get; } = "Client Results Visibility";
        public override string HelpShort { get; } = "Whether client command results should only be displayed to the client.";
        public override string HelpLong { get { return HelpShort; } }
        public override ServerModVersion UpdatedOnVersion { get; } = new ServerModVersion("C.8.0.0");

        public override bool Default { get; } = true;
    }
}
