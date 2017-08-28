using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.PlaylistTools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class LevelCMD : cmd
    {
        public string levelFormat
        {
            get { return (string)getSetting("levelFormat").Value; }
            set { getSetting("levelFormat").Value = value; }
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
            Utilities.sendMessage(Utilities.formatCmd("!level [name]") + ": Find a level who have that keyword on his name");
            Utilities.sendMessage(Utilities.formatCmd("!level [filter] ") + ": Use filters to find a level");
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
            Utilities.sendMessage("Valid filters: " + filtersStr);
            Utilities.sendMessage("Filter types:  default: -filter; not default: !filter; and: &filter; or: |filter; and not: &!filter; or not: |!filter");
            Utilities.sendMessage("The level must be known by the server to be shown");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(message == "")
            {
                help(p);
                return;
            }

            FilteredPlaylist filterer = new FilteredPlaylist(Utilities.getAllLevelsAndModes());

            if (!p.IsLocal_)
            {
                PlayCMD playCmd = cmd.all.getCommand<PlayCMD>("play");
                filterer.AddFiltersFromString(playCmd.playFilter);
            }

            Utilities.sendFailures(Utilities.addFiltersToPlaylist(filterer, p, message, true), 4);
            
            Utilities.sendMessage(Utilities.getPlaylistText(filterer, Utilities.IndexMode.Final, levelFormat));
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

        public override object Default { get; } = "%INDEX% %MODE%: %NAME%";

        public override string UpdatedOnVersion { get; } = "C.7.4.0";
    }
}
