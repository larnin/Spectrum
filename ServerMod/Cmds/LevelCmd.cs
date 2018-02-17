using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.PlaylistTools;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class LevelCmd : Cmd
    {
        public string levelFormat
        {
            get { return getSetting<CmdSettingLevelFormat>().Value; }
            set { getSetting<CmdSettingLevelFormat>().Value = value; }
        }
        public Dictionary<string, string> levelFormatReplacements
        {
            get { return getSetting<CmdSettingLevelFormatReplacements>().Value; }
            set { getSetting<CmdSettingLevelFormatReplacements>().Value = value; }
        }
        public override string name { get { return "level"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseLocal { get { return true; } }

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingLevelFormat(),
            new CmdSettingLevelFormatReplacements(),
        };

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!level [name]") + ": Find a level who have that keyword on his name");
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!level [filter] ") + ": Use filters to find a level");
            List<LevelFilter> filters = new List<LevelFilter>();
            foreach (KeyValuePair<string, LevelFilter> filter in FilteredPlaylist.filterTypes)
            {
                if (!filters.Contains(filter.Value))
                    filters.Add(filter.Value);
            }
            string filtersStr = "";
            foreach (LevelFilter filter in filters)
                foreach (string option in filter.options)
                    filtersStr += "-" + option + " ";
            MessageUtilities.sendMessage(p, "Valid filters: " + filtersStr);
            MessageUtilities.sendMessage(p, "Filter types:  default (and): -filter; not default: !filter; and: &filter; or: |filter; and not: &!filter; or not: |!filter");
            MessageUtilities.sendMessage(p, "The level must be known by the server to be shown");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(message == "")
            {
                help(p);
                return;
            }

            if (message == "CHECK" && p == GeneralUtilities.localClient())
            {
                var index = 0;
                foreach (var a in GeneralUtilities.getAllLevelsAndModes())
                {
                    GeneralUtilities.formatLevelInfoText(a, index++, "%NAME% %DIFFICULTY% %MODE% %AUTHOR% %INDEX% %MBRONZE% %MSILVER% %MGOLD% %MDIAMOND% %STARS% %STARSINT% %STARSDEC% %STARSPCT% %CREATED% %UPDATED%");
                }
                Console.WriteLine($"Tried to format {index} levels. Errors are in the console. If you see no errors, it means all levels formatted successfully.");
                MessageUtilities.sendMessage(GeneralUtilities.localClient(), $"Tried to format {index} levels. Errors are in the console. If you see no errors, it means all levels formatted successfully.");
                return;
            }

            FilteredPlaylist filterer = new FilteredPlaylist(GeneralUtilities.getAllLevelsAndModes());

            if (!p.IsLocal_)
            {
                PlayCmd playCmd = Cmd.all.getCommand<PlayCmd>("play");
                filterer.AddFiltersFromString(playCmd.playFilter);
            }

            MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(p));
            GeneralUtilities.sendFailures(GeneralUtilities.addFiltersToPlaylist(filterer, p, message, true), 4);
            MessageUtilities.popMessageOptions();
            
            MessageUtilities.sendMessage(p, GeneralUtilities.getPlaylistText(filterer, GeneralUtilities.IndexMode.Final, levelFormat));
        }
    }
    class CmdSettingLevelFormat : CmdSettingString
    {
        public override string FileId { get; } = "levelFormat";
        public override string SettingsId { get; } = "levelFormat";

        public override string DisplayName { get; } = "!level Level Format";
        public override string HelpShort { get; } = "!level: Formatted text to display for each level, also for !list";
        public override string HelpLong { get; } = "The text to display for each level, also for !list. Formatting options: "
            + "%NAME%, %DIFFICULTY%, %MODE%, %MBRONZE%, %MSILVER%, %MGOLD%, %MDIAMOND%, %AUTHOR%, %STARS%, %STARSINT%, %STARSDEC%, %CREATED%, %UPDATED%, %INDEX%";

        public override string Default { get; } = "%INDEX% %MODE%: [FFFFFF]%NAME%[-] by %AUTHOR%";

        public override ServerModVersion UpdatedOnVersion { get; } = new ServerModVersion("C.7.4.0");
    }
    class CmdSettingLevelFormatReplacements : CmdSetting<Dictionary<string, string>>
    {
        public override string FileId { get; } = "levelFormatReplacements";
        public override string SettingsId { get; } = "";

        public override string DisplayName { get; } = "Level format string replacements";
        public override string HelpShort { get; } = "Level format string replacements";
        public override string HelpLong { get { return HelpShort; } }

        public override ServerModVersion UpdatedOnVersion { get; } = new ServerModVersion("C.8.2.0");

        public override UpdateResult<Dictionary<string, string>> UpdateFromString(string input)
        {
            throw new NotImplementedException();
        }

        public override UpdateResult<Dictionary<string, string>> UpdateFromObject(object input)
        {
            if (input.GetType() != typeof(Dictionary<string, object>))
            {
                return new UpdateResult<Dictionary<string, string>>(false, Default, "Invalid dictionary. Resetting to default.");
            }
            try
            {
                var data = new Dictionary<string, string>();
                foreach (KeyValuePair<string, object> entry in (Dictionary<string, object>)input)
                {
                    data[entry.Key] = (string)entry.Value;
                }
                return new UpdateResult<Dictionary<string, string>>(true, data);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading dictionary: {e}");
                return new UpdateResult<Dictionary<string, string>>(false, Default, "Error reading dictionary. Resetting to default.");
            }
        }

        public override Dictionary<string, string> Default
        {
            get
            {
                return new Dictionary<string, string>();
            }
        }
    }
}
