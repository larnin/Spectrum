using Spectrum.API;
using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class SettingsCmd : Cmd
    {
        public override string name { get { return "settings"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseLocal { get { return true; } }

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingRecentVersion()
        };

        private const string settingRegex = @"^\s*(\w+) (.*)[\r\n]*$";
        private const string baseRegex = @"^\s*(\w+)[\r\n]*$";

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, $"{GeneralUtilities.formatCmd("!settings reload")}: reload the settings from the file.");
            MessageUtilities.sendMessage(p, $"{GeneralUtilities.formatCmd("!settings summary")}: view the value of all settings.");
            MessageUtilities.sendMessage(p, $"{GeneralUtilities.formatCmd("!settings defaults")}: view the default of all settings.");
            MessageUtilities.sendMessage(p, $"{GeneralUtilities.formatCmd("!settings reset <setting>")}: reset <setting> to its default value");
            MessageUtilities.sendMessage(p, $"{GeneralUtilities.formatCmd("!settings default <setting>")}: view default for <setting>");
            MessageUtilities.sendMessage(p, $"{GeneralUtilities.formatCmd("!settings help <setting>")}: view a more detailed help message for <setting>");
            foreach (Cmd Command in Cmd.all.list())
            {
                string txt = "";
                foreach (CmdSetting Setting in Command.settings)
                {
                    if (Setting.SettingsId != "")
                       txt += $"\n {GeneralUtilities.formatCmd($"!settings {Setting.SettingsId} {Setting.UsageParameters}")}: {Setting.HelpShort}";
                }
                if (txt != "")
                {
                    MessageUtilities.sendMessage(p, $"[b][D00000]!{Command.name} Settings[-][/b]{txt}");
                }
            }
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(message == "")
            {
                help(p);
                return;
            }


            Match msgMatch = Regex.Match(message, settingRegex);
            if (msgMatch.Success)
            {
                string setting = msgMatch.Groups[1].Value.ToLower();
                if (setting == "reload")
                {
                    reload(p);
                    return;
                }
                /*  // TODO: fix this to use new FileSystem stuff
                else if (setting == "writemarkdown")
                {  // command used to generate markdown help for settings. not shown to user.
                    string style = msgMatch.Groups[2].Value.ToLower();
                    if (style == "command" || style == "both")
                    {
                        string txt = "";
                        foreach (Cmd Command in Cmd.all.list())
                        {
                            string txt2 = "";
                            foreach (CmdSetting Setting in Command.settings)
                            {
                                if (Setting.SettingsId != "")
                                    txt2 += $"  * `!settings {Setting.SettingsId} {Setting.UsageParameters}`  \n{Setting.HelpMarkdown}  \nDefault: {Setting.DefaultTypeless}  \n";
                            }
                            if (txt2 != "")
                                txt += $"* `!{Command.name}` Settings\n" + txt2;
                        }
                        string FilePath = Path.Combine(Defaults.SettingsDirectory, "servermod-settings.command.md");
                        using (var sw = new StreamWriter(FilePath, false))
                        {
                            sw.Write(txt);
                        }
                    }

                    if (style == "file" || style == "both")
                    {
                        string txt = "";
                        foreach (Cmd Command in Cmd.all.list())
                        {
                            string txt2 = "";
                            foreach (CmdSetting Setting in Command.settings)
                            {
                                if (Setting.FileId != "")
                                 txt2 += $"* `\"{Setting.FileId}\" :  {Setting.UsageParameters},`  \n{Setting.HelpMarkdown}  \nDefault: {Setting.DefaultTypeless}  \n";
                            }
                            if (txt2 != "")
                                txt += txt2;
                        }
                        string FilePath = Path.Combine(Defaults.SettingsDirectory, "servermod-settings.file.md");
                        using (var sw = new StreamWriter(FilePath, false))
                        {
                            sw.Write(txt);
                        }
                    }

                    if (style != "file" && style != "command" && style != "both")
                        Console.WriteLine("Unknown style type. Styles: command, file, both");
                    return;
                }
                */
                else if (setting == "help")
                {
                    string settingHelp = msgMatch.Groups[2].Value.ToLower();
                    if (settingHelp.Length == 0)
                    {
                        help(p);
                        return;
                    }
                    foreach (Cmd Command in Cmd.all.list())
                    {
                        foreach (CmdSetting Setting in Command.settings)
                        {
                            if (Setting.SettingsId.ToLower() == settingHelp.ToLower())
                            {
                                MessageUtilities.sendMessage(p, $"{GeneralUtilities.formatCmd($"!settings {Setting.SettingsId} {Setting.UsageParameters}")}");
                                MessageUtilities.sendMessage(p, Setting.HelpLong);
                                return;
                            }
                        }
                    }
                    MessageUtilities.sendMessage(p, $"Could not find setting by the name of `{msgMatch.Groups[2].Value}`");
                }
                else if (setting == "mod")
                {
                    UIExInputGeneric<string> inputBox = UIExInputGeneric<string>.current_;
                    string settingHelp = msgMatch.Groups[2].Value.ToLower();
                    if (settingHelp.Length == 0)
                    {
                        help(p);
                        return;
                    }
                    foreach (Cmd Command in Cmd.all.list())
                    {
                        foreach (CmdSetting Setting in Command.settings)
                        {
                            if (Setting.SettingsId.ToLower() == settingHelp.ToLower())
                            {
                                MessageUtilities.sendMessage(p, $"Editing settings {Setting.SettingsId}");
                                inputBox.Value_ = $"!settings {Setting.SettingsId} {Setting.ValueTypeless}";
                                PrivateUtilities.callPrivateMethod(inputBox, "StartEdit");
                                return;
                            }
                        }
                    }
                    MessageUtilities.sendMessage(p, $"Could not find setting by the name of `{msgMatch.Groups[2].Value}`");
                }
                else if (setting == "reset")
                {
                    string settingHelp = msgMatch.Groups[2].Value.ToLower();
                    if (settingHelp.Length == 0)
                    {
                        help(p);
                        return;
                    }
                    foreach (Cmd Command in Cmd.all.list())
                    {
                        foreach (CmdSetting Setting in Command.settings)
                        {
                            if (Setting.SettingsId.ToLower() == settingHelp.ToLower())
                            {
                                Setting.ValueTypeless = Setting.DefaultTypeless;
                                MessageUtilities.sendMessage(p, $"Setting {Setting.SettingsId} reset to default:");
                                MessageUtilities.sendMessage(p, $"{Setting.DefaultTypeless}");
                                Entry.save();
                                return;
                            }
                        }
                    }
                    MessageUtilities.sendMessage(p, $"Could not find setting by the name of `{msgMatch.Groups[2].Value}`");
                }
                else if (setting == "default")
                {
                    string settingHelp = msgMatch.Groups[2].Value.ToLower();
                    if (settingHelp.Length == 0)
                    {
                        help(p);
                        return;
                    }
                    foreach (Cmd Command in Cmd.all.list())
                    {
                        foreach (CmdSetting Setting in Command.settings)
                        {
                            if (Setting.SettingsId.ToLower() == settingHelp.ToLower())
                            {
                                MessageUtilities.sendMessage(p, $"{Setting.SettingsId} default:");
                                MessageUtilities.sendMessage(p, $"{Setting.DefaultTypeless}");
                                return;
                            }
                        }
                    }
                    MessageUtilities.sendMessage(p, $"Could not find setting by the name of `{msgMatch.Groups[2].Value}`");
                }
                else
                {
                    foreach (Cmd Command in Cmd.all.list())
                    {
                        foreach (CmdSetting Setting in Command.settings)
                        {
                            if (Setting.SettingsId.ToLower() == setting.ToLower())
                            {
                                UpdateResult result = Setting.UpdateFromStringTypeless(msgMatch.Groups[2].Value);
                                if (!result.Valid)
                                    MessageUtilities.sendMessage(p, $"Failed to set setting: {result.Message}");
                                else
                                {
                                    MessageUtilities.sendMessage(p, $"Set {Setting.SettingsId} successfully.");
                                    if (result.Message != "")
                                        MessageUtilities.sendMessage(p, result.Message);
                                }
                                Setting.ValueTypeless = result.NewValueTypeless;
                                Entry.save();
                                return;
                            }
                        }
                    }
                    MessageUtilities.sendMessage(p, $"Could not find setting by the name of `{msgMatch.Groups[1].Value}`");
                    return;
                }

            }
            else
            {
                Match baseMatch = Regex.Match(message, baseRegex);
                if (baseMatch.Success)
                {
                    string setting = baseMatch.Groups[1].Value.ToLower();
                    if (setting == "reload")
                    {
                        reload(p);
                        return;
                    }
                    else if (setting == "summary")
                    {
                        foreach (Cmd Command in Cmd.all.list())
                        {
                            string txt = "";
                            foreach (CmdSetting Setting in Command.settings)
                            {
                                if (Setting.SettingsId != "")
                                    txt += $"\n {GeneralUtilities.formatCmd($"{Setting.SettingsId}")}: {Setting.ValueTypeless}";
                            }
                            if (txt != "")
                            {
                                MessageUtilities.sendMessage(p, $"[b][D00000]!{Command.name} Settings[-][/b]{txt}");
                            }
                        }
                        return;
                    }
                    else if (setting == "defaults")
                    {
                        foreach (Cmd Command in Cmd.all.list())
                        {
                            string txt = "";
                            foreach (CmdSetting Setting in Command.settings)
                            {
                                if (Setting.SettingsId != "")
                                    txt += $"\n {GeneralUtilities.formatCmd($"{Setting.SettingsId}")}: {Setting.DefaultTypeless}";
                            }
                            if (txt != "")
                            {
                                MessageUtilities.sendMessage(p, $"[b][D00000]!{Command.name} Default Settings[-][/b]{txt}");
                            }
                        }
                        return;
                    }
                    else
                    {
                        foreach (Cmd Command in Cmd.all.list())
                        {
                            foreach (CmdSetting Setting in Command.settings)
                            {
                                if (Setting.SettingsId.ToLower() == setting.ToLower())
                                {
                                    MessageUtilities.sendMessage(p, $"Value of {Setting.SettingsId}: {Setting.ValueTypeless}");
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            help(p);
        }

        void reload(ClientPlayerInfo p)
        {
            Entry.reload();
            MessageUtilities.sendMessage(p, "Settings reloaded from file!");
        }

        public void showNewSettings()
        {
            var currentVersion = Entry.PluginVersion;
            if (Entry.IsFirstRun)  // if player is new, don't show info for everything up from C.7.3.1, which would happen otherwise.
            {
                getSetting<CmdSettingRecentVersion>().Value = currentVersion;
                Entry.save();
            }
            var previousVersion = getSetting<CmdSettingRecentVersion>().Value;
            if (currentVersion == previousVersion)
                return;
            getSetting<CmdSettingRecentVersion>().Value = currentVersion;
            Entry.save();
            MessageUtilities.sendMessage($"[b][D00000]New Settings Defaults for {Entry.PluginVersion}[-][/b]");
            var count = 0;
            foreach (Cmd Command in Cmd.all.list())
            {
                string txt = "";
                foreach (CmdSetting Setting in Command.settings)
                {
                    if (Setting.SettingsId != "" && Setting.UpdatedOnVersion > previousVersion && (Setting.New || !Setting.ValueTypeless.Equals(Setting.DefaultTypeless)))
                        if (Setting.New)
                            txt += $"\n {GeneralUtilities.formatCmd($"{Setting.SettingsId}")}: New Setting\n  [FFFFFF]Default:[-] {Setting.DefaultTypeless}";
                        else
                            txt += $"\n {GeneralUtilities.formatCmd($"{Setting.SettingsId}")}:\n  [FFFFFF]Default:[-] {Setting.DefaultTypeless}\n  [FFFFFF]  Yours:[-] {Setting.ValueTypeless}";
                }
                if (txt != "")
                {
                    count++;
                    MessageUtilities.sendMessage($"[D00000]!{Command.name} Settings[-]{txt}");
                }
            }
            if (count == 0)
                MessageUtilities.sendMessage("None");
            else
                MessageUtilities.sendMessage($"[FFFFFF]Use {GeneralUtilities.formatCmd("!settings reset <setting>")} to reset a setting to its default.");
            MessageUtilities.sendMessage("");

            foreach (var notice in ReleaseNotices.Notices)
            {
                if (notice.version > previousVersion)
                {
                    MessageUtilities.sendMessage($"[b][00D000]Notices for version {notice.version}[-][/b]");
                    MessageUtilities.sendMessage(notice.notes);
                    MessageUtilities.sendMessage("");
                }
                else
                    break;
            }
        }
    }
    class CmdSettingRecentVersion : CmdSetting<ServerModVersion>
    {
        public override string FileId { get; } = "recentVersion";
        public override string SettingsId { get; } = "";

        public override string DisplayName { get; } = "Recent ServerMod Version";
        public override string HelpShort { get; } = "Stores the most recent version of ServerMod to detect when ServerMod has been updated.";
        public override string HelpLong { get { return HelpShort; } }

        public override object SaveValue { get { return Value.ToString(); } }
        public override ServerModVersion Default { get { return new ServerModVersion("C.7.3.1"); } }  // the version before version info was added
        public override ServerModVersion UpdatedOnVersion { get; } = new ServerModVersion("C.8.0.0");

        public override UpdateResult<ServerModVersion> UpdateFromString(string input)
        {
            ServerModVersion version;
            if (ServerModVersion.TryParse(input, out version))
                return new UpdateResult<ServerModVersion>(true, version);
            else
                return new UpdateResult<ServerModVersion>(false, Value, "Failed to parse ServerModVersion");
        }

        public override UpdateResult<ServerModVersion> UpdateFromObject(object input)
        {
            if (input is string)
                return UpdateFromString((string)input);
            else
                return new UpdateResult<ServerModVersion>(true, (ServerModVersion)input);
        }
    }
}
