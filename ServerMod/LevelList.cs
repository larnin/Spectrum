using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerMod
{
    public class Modifiers
    {
        public GameModeID mode = GameModeID.None;
        public string name = "";
        public string author = "";
        public List<int> index = new List<int>();
        public bool all = false;
    }
    
    static class LevelList
    {
        static private List<GameModeID> validModes = new List<GameModeID>
            { GameModeID.Sprint
            , GameModeID.Challenge
            , GameModeID.ReverseTag
            , GameModeID.Soccer
            , GameModeID.SpeedAndStyle
            , GameModeID.Stunt };

        static private List<string> validModesName = new List<string>
            { "sprint"
            ,"challenge"
            , "tag"
            , "soccer"
            , "style"
            , "stunt" };

        public static Modifiers extractModifiers(string filter)
        {
            Modifiers m = new Modifiers();
            if (!filter.StartsWith("-"))
                m.name = filter.ToLower().Trim();
            else
            {
                string nameText = "";
                string authorText = "";

                var list = filter.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string value in list)
                {
                    var words = value.ToLower().Trim().Split(' ');
                    if (words.Count() == 0)
                        continue;
                    string key = words[0];
                    if (key == "m" || key == "mode")
                    {
                        if (words.Count() < 2)
                            continue;
                        var id = 0;
                        for (int i = 0; i < validModesName.Count(); i++)
                            if (validModesName[i].ToLower() == words[1].ToLower())
                                id = i;

                        var mode = G.Sys.GameManager_.GetModeID(words[1]);
                        if (id < 0)
                        {
                            Utilities.sendMessage("Valid modes :");
                            string modes = "";
                            foreach (var modeName in validModes)
                                modes += modeName + ", ";
                            Utilities.sendMessage(modes.Substring(0, modes.Count() - 2));
                        }
                        else mode = validModes[id];
                        m.mode = mode;
                    }
                    else if (key == "n" || key == "name")
                    {
                        if (words.Count() < 2)
                            continue;
                        for (int i = 1; i < words.Count(); i++)
                            nameText += " " + words[i];
                    }
                    else if (key == "a" || key == "author")
                    {
                        if (BuildType.type_ != BuildPackage.STEAM)
                        {
                            Utilities.sendMessage("You can't use the filter '-author' without steam");
                            continue;
                        }

                        if (words.Count() < 2)
                            continue;
                        for (int i = 1; i < words.Count(); i++)
                            authorText += words[i] + " ";
                    }
                    else if (key == "i" || key == "index")
                    {
                        if (words.Count() < 2)
                            continue;
                        int index = 0;
                        if (int.TryParse(words[1], out index))
                            m.index.Add(index);
                    }
                    else if (key == "all")
                        m.all = true;
                }
                m.author = authorText.ToLower().Trim();
                m.name = nameText.ToLower().Trim();
            }

            if (validModes.IndexOf(m.mode) < 0)
                m.mode = validModes[0];
            
            return m;
        }

        public static List<LevelPlaylist.ModeAndLevelInfo> levels(Modifiers filter)
        {
            var m = filter;

            if (m.mode == GameModeID.Trackmogrify)
                return levelsInTrackmod(m.name);

            var set = G.Sys.LevelSets_.GetSet(m.mode);
            var lvls = set.GetAllLevelNameAndPathExceptMyLevelsPairs();
            List<LevelNameAndPathPair> nameFilteredList = new List<LevelNameAndPathPair>();
            if (m.name.Count() == 0)
                nameFilteredList = lvls;
            else
            {
                foreach (var lvl in lvls)
                    if (lvl.levelName_.ToLower().Contains(m.name.ToLower()))
                        nameFilteredList.Add(lvl);
            }

            List<LevelNameAndPathPair> authorFilteredList = new List<LevelNameAndPathPair>();
            if (m.author.Count() == 0)
                authorFilteredList = nameFilteredList;
            else
            {
                foreach(var lvl in nameFilteredList)
                {
                    var author = getAuthorOfLevel(lvl);
                    if (author.Count() == 0)
                        continue;
                    if (author.ToLower().Trim() == m.author.ToLower().Trim())
                        authorFilteredList.Add(lvl);
                }
            }
            
            if (m.name.Count() > 0 && authorFilteredList.Exists(item => item.levelName_ == m.name))
            {
                List<LevelNameAndPathPair> exactFilteredList = new List<LevelNameAndPathPair>();
                foreach (var lvl in authorFilteredList)
                {
                    if (lvl.levelName_.Trim().ToLower() == m.name.Trim().ToLower())
                        exactFilteredList.Add(lvl);
                }
                authorFilteredList = exactFilteredList;
            }

            if(m.index.Count() > 0 && !m.all)
            {
                List<LevelNameAndPathPair> indexFilteredList = new List<LevelNameAndPathPair>();
                foreach (var index in m.index)
                {
                    var i = index - 1;
                    if (i >= authorFilteredList.Count())
                    {
                        Utilities.sendMessage("Index " + i + " is too hight !");
                        continue;
                    }
                    if (i < 0)
                        continue;
                    indexFilteredList.Add(authorFilteredList[i]);
                }
                authorFilteredList = indexFilteredList;
            }

            List<LevelPlaylist.ModeAndLevelInfo> finalList = new List<LevelPlaylist.ModeAndLevelInfo>();
            foreach (var lvl in authorFilteredList)
                finalList.Add(new LevelPlaylist.ModeAndLevelInfo(m.mode, lvl));

            return finalList;
        }

        public static string getAuthorOfLevel(LevelNameAndPathPair lvl)
        {
            var lvlInfo = G.Sys.LevelSets_.GetLevelInfo(lvl.levelPath_);
            if (lvlInfo.levelType_ == LevelType.Official)
                return "Refract";

            if(lvlInfo.levelType_ == LevelType.Workshop)
                return G.Sys.SteamworksManager_.GetSteamName(lvlInfo.workshopCreatorID_);

            return "";
        }

        private static List<LevelPlaylist.ModeAndLevelInfo> levelsInTrackmod(string name)
        {
            return new List<LevelPlaylist.ModeAndLevelInfo>(){ new LevelPlaylist.ModeAndLevelInfo(GameModeID.Trackmogrify, new LevelNameAndPathPair(name, ""))};
        }

        public static void printLevels(List<LevelPlaylist.ModeAndLevelInfo> lvls, int maxLvl, bool showIndexs)
        {
            if (lvls.Count > 0)
            {
                Utilities.sendMessage("Levels found :");
                for (int i = 0; i < Math.Min(lvls.Count(), maxLvl); i++)
                {
                    if(showIndexs)
                        Utilities.sendMessage("(" + (i+1) + ") " + lvls[i].levelNameAndPath_.levelName_);
                    else Utilities.sendMessage(lvls[i].levelNameAndPath_.levelName_);
                }
                if (lvls.Count > maxLvl)
                    Utilities.sendMessage("And " + (lvls.Count - maxLvl) + " more ...");
            }
            else Utilities.sendMessage("No levels found");
        }
    }
}
