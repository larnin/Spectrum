using Spectrum.Plugins.ServerMod.cmds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.PlaylistTools.LevelFilters
{
    class LevelFilterPlaylist : LevelFilter
    {
        public override string[] options { get; } = new string[] {"pl", "playlist"};

        public List<LevelPlaylist.ModeAndLevelInfo> validLevels = new List<LevelPlaylist.ModeAndLevelInfo>();

        public LevelFilterPlaylist() { }

        public LevelFilterPlaylist(List<LevelPlaylist.ModeAndLevelInfo> validLevels)
        {
            this.validLevels = new List<LevelPlaylist.ModeAndLevelInfo>(validLevels);
        }

        public override void Apply(List<PlaylistLevel> levels)
        {
            foreach (PlaylistLevel level in levels)
            {
                level.Mode(mode, validLevels.Contains(validLevel => validLevel.levelNameAndPath_.levelPath_ == level.level.levelNameAndPath_.levelPath_ && validLevel.mode_ == level.level.mode_));
            }
        }

        public override LevelFilterResult FromChatString(string chatString, string option)
        {
            PlaylistCMD playlistCmd = cmd.all.getCommand<PlaylistCMD>("playlist");
            int count;
            LevelPlaylist list = playlistCmd.getPlaylistLevels(chatString, out count);
            if (count == 0)
                return new LevelFilterResult($"Could not find any matching playlist for `{chatString}`");
            else  // TODO: allow messages and results at the same time, for case count > 1
                return new LevelFilterResult(new LevelFilterPlaylist(list.Playlist_));
        }
    }
}
