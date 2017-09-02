using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.PlaylistTools;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class InfoCmd : Cmd
    {
        public override string name { get { return "info"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return true; } }

        public string infoFormat =
              "[b][FFFFFF]%NAME%[-][/b] by %AUTHOR%\n"
            + "%DIFFICULTY% %MODE%\n"
            + "[994700]%MBRONZE%[-] [CCCCCC]%MSILVER%[-] [FFD900]%MGOLD%[-] [00EFFF]%MDIAMOND%[-]\n"
            + "%STARSDEC% / 5 %STARS%\n"
            + "%CREATED% Created\n"
            + "%UPDATED% Updated";

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(GeneralUtilities.formatCmd("!info [name]") + ": Find a level who have that keyword on his name");
            MessageUtilities.sendMessage(GeneralUtilities.formatCmd("!info [filter] ") + ": Use filters to find a level");
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
            MessageUtilities.sendMessage("Valid filters: " + filtersStr);
            MessageUtilities.sendMessage("Filter types:  default (and): -filter; not default: !filter; and: &filter; or: |filter; and not: &!filter; or not: |!filter");
            MessageUtilities.sendMessage("The level must be known by the server to be shown");
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

            GeneralUtilities.sendFailures(GeneralUtilities.addFiltersToPlaylist(filterer, p, message, true), 4);

            var levels = filterer.Calculate();
            if (levels.levelList.Count == 0)
            {
                MessageUtilities.sendMessage("No levels found.");
                return;
            }
            var level = levels.allowedList[0];

            MessageUtilities.sendMessage(GeneralUtilities.formatLevelInfoText(level.level, level.index, infoFormat));

            if (levels.levelList.Count > 1)
                MessageUtilities.sendMessage($"Found {levels.levelList.Count - 1} more levels.");
        }
    }
}
