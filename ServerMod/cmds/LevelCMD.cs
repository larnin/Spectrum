using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerMod.cmds
{
    class LevelCMD : cmd
    {
        public override string name { get { return "level"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return false; } }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("!level [name]: Find a level who have that keyword on his name");
            Utilities.sendMessage("The level must be know by the server to be show");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(message == "")
            {
                help(p);
                return;
            }

            if (Utilities.isOnLobby())
            {
                Utilities.sendMessage("You can't search on levels here");
                return;
            }

            var set = G.Sys.LevelSets_.GetSet(G.Sys.GameManager_.ModeID_);
            if (set == null)
                Utilities.sendMessage("Can't find a level with the current gamemode");
            var list = set.GetAllLevelNameAndPathPairs();

            List<LevelNameAndPathPair> validLevels = new List<LevelNameAndPathPair>();
            int exactLVLName = 0;

            foreach (var lvl in list)
            {
                if (lvl.levelName_.ToLower() == message.ToLower())
                    exactLVLName++;
                else if (lvl.levelName_.ToLower().Contains(message.ToLower()))
                    validLevels.Add(lvl);
            }

            if (validLevels.Count == 0)
            {
                Utilities.sendMessage("Can't find a level with the name '" + message + "'.");
                return;
            }

            if (exactLVLName == 1)
                Utilities.sendMessage("Found a level with the name '" + message + "'");
            if (exactLVLName > 1)
                Utilities.sendMessage("Found " + exactLVLName + " levels with the name '" + message + "'");

            if (validLevels.Count > 1)
            {
                Utilities.sendMessage(validLevels.Count + " levels found :");

                const int maxLvl = 10;
                for (int i = 0; i < Math.Min(validLevels.Count, maxLvl); i++)
                    Utilities.sendMessage(validLevels[i].levelName_);
                if (validLevels.Count > maxLvl)
                    Utilities.sendMessage("And " + (validLevels.Count - maxLvl) + " more ...");
                return;
            }
        }
    }
}
