using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod
{
    class ReleaseNotices
    {
        public static List<ReleaseNotice> Notices = new List<ReleaseNotice>
        {
            new ReleaseNotice("C.8.1.0",
                "• !scores now shows times for finished players\n" +
                "• Added !stuck to help debug what's going on when the server gets stuck on the loading screen\n" +
                "• Added AutospecIdleTimeout: after the timeout, idle players will go into autospec automatically\n" +
                "• Autospec now tells players how to leave autospec mode at the beginning of every level"
            ),
            new ReleaseNotice("C.8.0.2",
                "• Fixed ServerMod<->ServerMod chat log replication"
            ),
            new ReleaseNotice("C.8.0.1",
                "• Added [FFFFFF]!server mod <setting>[-] which makes it easier to modify settings\n" +
                "• Various bugfixes"
            ),
            new ReleaseNotice("C.8.0.0",
                "• Commands, and their results, now show up only to you or the player that used it.\n" +
                "• [70AAAA]Blue colored text[-] is local text.\n" +
                "• Commands from clients will still show to all players.\n" +
                "• Use " + GeneralUtilities.formatCmd("!!command") + " or " + GeneralUtilities.formatCmd("%%command") + " to show the command and its results to everyone.\n" +
                "• Some commands, such as " + GeneralUtilities.formatCmd("!rip") + " will always show to everyone.\n" +
                "• !log shows command logs."
                )
        };
    }
    public class ReleaseNotice
    {
        public ServerModVersion version;
        public string notes;
        public ReleaseNotice(string version, string notes)
        {
            this.version = new ServerModVersion(version);
            this.notes = notes;
        }
    }
}
