using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class DelsCMD : cmd
    {
        public override string name { get { return "dels"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        private string cmdPattern = @"^\s*(\d+)\s+(\d+)\s*$";

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage(Utilities.formatCmd("!dels <indexStart> <indexEnd>") + ": remove the maps between indexStart and indexEnd from the playlist");
            Utilities.sendMessage("The next map has an index of 0");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(message == "")
            {
                help(p);
                return;
            }

            if (G.Sys.GameManager_.ModeID_ == GameModeID.Trackmogrify)
            {
                Utilities.sendMessage("You can't manage the playlist in trackmogrify");
                return;
            }

            Match match = Regex.Match(message, cmdPattern);

            if (!match.Success)
            {
                help(p);
                Utilities.sendMessage("For example, !dels 0 5");
            }

            string id1s = match.Groups[1].Value;
            string id2s = match.Groups[2].Value;

            Console.WriteLine($"{id1s}");
            Console.WriteLine($"{id2s}");

            int id1 = 0;
            int id2 = 0;

            int.TryParse(id1s, out id1);
            int.TryParse(id2s, out id2);

            if (id1 < 0 || id2 < 0)
            {
                Utilities.sendMessage("The indices must be positive numbers or 0.");
                return;
            }
            else if (id1 > id2)
            {
                Utilities.sendMessage("indexStart must be <= indexEnd.");
            }
            
            int index = G.Sys.GameManager_.LevelPlaylist_.Index_;

            int playListSize = G.Sys.GameManager_.LevelPlaylist_.Playlist_.Count - index - 1;
            if(id2 > playListSize)
            {
                Utilities.sendMessage("The playlist has only " + playListSize + " maps.");
                return;
            }

            LevelPlaylist playlist = new LevelPlaylist();
            playlist.Copy(G.Sys.GameManager_.LevelPlaylist_);
            var currentPlaylist = playlist.Playlist_;
            for (int id = id1; id <= id2; id++)
            {
                currentPlaylist.RemoveAt(index + id + 1);
            }

            G.Sys.GameManager_.LevelPlaylist_.Clear();

            foreach (var lvl in currentPlaylist)
                G.Sys.GameManager_.LevelPlaylist_.Add(lvl);
            G.Sys.GameManager_.LevelPlaylist_.SetIndex(index);

            Utilities.sendMessage($"{id2 - id1 + 1} maps removed !");
        }
    }
}
