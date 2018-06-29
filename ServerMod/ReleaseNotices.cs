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
            new ReleaseNotice("C.8.3.1",
                "• Update for Spectrum Gamma"
            ),
            new ReleaseNotice("C.8.3.0",
                "• Add InfiniteLoadingPatch on level load\n" +
                "• Add InfiniteLoadingTimeoutPatch at 45 seconds (configurable)"
            ),
            new ReleaseNotice("C.8.2.3",
                "• Properly strip symbols/tags for usernames\n" +
                "• Recognize strikethrough symbols/tags"
            ),
            new ReleaseNotice("C.8.2.2",
                "• Allow clients that use the chat name colorizer to use commands\n" +
                "• Added debug info for when level text fails to format\n" +
                "• Added `!level CHECK` for hosts which will check if the level text formatter works on all levels."
            ),
            new ReleaseNotice("C.8.2.1",
                "• Performance bug fix: stop checking for chat windows when offline."
            ),
            new ReleaseNotice("C.8.2.0",
                "• Add level format regex replacements."
            ),
            new ReleaseNotice("C.8.1.5",
                "• Fix !auto from end of match and isModeFinished\n" +
                "• Add more error detection\n" +
                "• Add AutoSpecDebug setting. AutoSpec debug info is on by default, turn it off with AutoSpecDebug."
            ),
            new ReleaseNotice("C.8.1.4",
                "• Added AutoSpecIdleTimeout debug to console"
            ),
            new ReleaseNotice("C.8.1.3",
                "• Fixed AutoSpecIdleTimeout and !scores compatibility with most recent update"
            ),
            new ReleaseNotice("C.8.1.2",
                "• Added " + GeneralUtilities.formatCmd("!unstuck [mode] [param]") + " to try to fix stuck loading screens.\n" +
                "• Added more debugging info to !stuck"
            ),
            new ReleaseNotice("C.8.1.1",
                "• Servers now ignore when players use % commands\n" +
                "• Added AutospecIdleSingle: choose whether or not to enable regular spectate mode or auto spectate mode on idle timeout"
            ),
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
