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
        public override string name { get { return "level"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return true; } }

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingLevelFormat()
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
}
