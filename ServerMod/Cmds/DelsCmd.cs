using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class DelsCmd : Cmd
    {
        public override string name { get { return "dels"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseLocal { get { return false; } }

        private string cmdPattern = @"^\s*(\d+)\s+(\d+)\s*$";

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!dels <indexStart> <indexEnd>") + ": remove the maps between indexStart and indexEnd from the playlist");
            MessageUtilities.sendMessage(p, "The next map has an index of 0");
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
                MessageUtilities.sendMessage(p, "You can't manage the playlist in trackmogrify");
                return;
            }

            Match match = Regex.Match(message, cmdPattern);

            if (!match.Success)
            {
                help(p);
                MessageUtilities.sendMessage(p, "For example, !dels 0 5");
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
                MessageUtilities.sendMessage(p, "The indices must be positive numbers or 0.");
                return;
            }
            else if (id1 > id2)
            {
                MessageUtilities.sendMessage(p, "indexStart must be <= indexEnd.");
            }
            
            int index = G.Sys.GameManager_.LevelPlaylist_.Index_;

            int playListSize = G.Sys.GameManager_.LevelPlaylist_.Playlist_.Count - index - 1;
            if(id2 >= playListSize)
            {
                MessageUtilities.sendMessage(p, "The playlist has only " + playListSize + " maps.");
                return;
            }

            LevelPlaylist playlist = new LevelPlaylist();
            playlist.CopyFrom(G.Sys.GameManager_.LevelPlaylist_);
            var currentPlaylist = playlist.Playlist_;
            for (int id = id1; id <= id2; id++)
            {
                currentPlaylist.RemoveAt(index + id1 + 1);
            }

            G.Sys.GameManager_.LevelPlaylist_.Clear();

            foreach (var lvl in currentPlaylist)
                G.Sys.GameManager_.LevelPlaylist_.Add(lvl);
            G.Sys.GameManager_.LevelPlaylist_.SetIndex(index);

            MessageUtilities.sendMessage(p, $"{id2 - id1 + 1} maps removed !");
        }
    }
}
