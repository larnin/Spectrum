using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class SettingsCMD : cmd
    {
        public override string name { get { return "settings"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        private const string msgRegex = @"^\s*\w+ (.*)[\r\n]*$";

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("!settings reload: reload the settings for file.");
            Utilities.sendMessage("!settings play [true/false]: allow player to add maps on the playlist.");
            Utilities.sendMessage("!settings addOne [true/false]: if enabled, allow the players to add only one map at a time.");
            Utilities.sendMessage("!settings welcome [message]: Set the welcome message. \"off\" to disable. %USERNAME% is substituted for the player's name.");
            Utilities.sendMessage("  \"off\" to disable.");
            Utilities.sendMessage("  %USERNAME% is substituted for the player's name.");
            Utilities.sendMessage("!settings voteSystem [true/false]: Turn the voting system off/on. This is separate from autoVote!");
            Utilities.sendMessage("  This is separate from autoVote!");
            Utilities.sendMessage("voteSystem thresholds are changed with !votectrl");
            Utilities.sendMessage("-- Setting for !auto: --");
            Utilities.sendMessage("!settings autoVote [true/false]: allow players to vote on auto mode.");
            Utilities.sendMessage("!settings autoMsg [message]: the message to display when advancing the map in auto mode. \"off\" to disable.");
            Utilities.sendMessage("!settings autoMinPlayers [amount]: the minimum amount of players needed for a map to start in auto mode.");
            Utilities.sendMessage("!settings autoMaxTime [seconds]: the maximum amount of time that auto mode will spend on one map.");
            Utilities.sendMessage("!settings autoSpecPlayer [true/false]: turn on/off whether the host counts as a player for autoMinPlayers when using autospec.");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(message == "")
            {
                help(p);
                return;
            }

            var strs = message.ToLower().Trim().Split(' ');

            if (strs[0] == "reload")
                reload(p);
            else if (strs[0] == "play")
            {
                if (strs.Length == 1)
                    help(p);
                else play(p, strs[1]);
            }
            else if (strs[0] == "addone")
            {
                if (strs.Length == 1)
                    help(p);
                else addOne(p, strs[1]);
            }
            else if (strs[0] == "welcome")
            {
                if (strs.Length == 1)
                    help(p);
                else
                {
                    Match msgMatch = Regex.Match(message, msgRegex);
                    if (msgMatch.Success)
                    {
                        welcome(p, msgMatch.Groups[1].Value);
                    }
                }
            }
            else if (strs[0] == "votesystem")
            {
                if (strs.Length == 1)
                    help(p);
                else voteSystem(p, strs[1]);
            }
            else if (strs[0] == "autovote")
            {
                if (strs.Length == 1)
                    help(p);
                else vote(p, strs[1]);
            }
            else if (strs[0] == "automsg")
            {
                if (strs.Length == 1)
                    help(p);
                else
                {
                    Match msgMatch = Regex.Match(message, msgRegex);
                    if (msgMatch.Success)
                    {
                        autoMsg(p, msgMatch.Groups[1].Value);
                    }
                }
            }
            else if (strs[0] == "autominplayers")
            {
                if (strs.Length == 1)
                    help(p);
                else autoMinPlayers(p, strs[1]);
            }
            else if (strs[0] == "automaxtime")
            {
                if (strs.Length == 1)
                    help(p);
                else autoMaxTime(p, strs[1]);
            }
            else if (strs[0] == "autospecplayer")
            {
                if (strs.Length == 1)
                    help(p);
                else autoSpecPlayer(p, strs[1]);
            }
            else help(p);
        }

        void reload(ClientPlayerInfo p)
        {
            Entry.load();
            Utilities.sendMessage("Settings reloaded !");
        }

        void play(ClientPlayerInfo p, string value)
        {
            if (value == "0" || value == "false")
            {
                PlayCMD.playersCanAddMap = false;
                Utilities.sendMessage("Players can't add maps now !");
            }
            else if (value == "1" || value == "true")
            {
                PlayCMD.playersCanAddMap = true;
                Utilities.sendMessage("Players are now allowed to add maps !");
            }
            else
            {
                help(p);
                return;
            }

            Entry.save();
        }

        void addOne(ClientPlayerInfo p, string value)
        {
            if (value == "0" || value == "false")
            {
                PlayCMD.addOneMapOnly = false;
                Utilities.sendMessage("Multiple add allowed to players !");
            }
            else if (value == "1" || value == "true")
            {
                PlayCMD.addOneMapOnly = true;
                Utilities.sendMessage("Players can only add one map now !");
            }
            else
            {
                help(p);
                return;
            }

            Entry.save();
        }

        void welcome(ClientPlayerInfo p, string value)
        {
            if (value == "off")
            {
                WelcomeCMD.welcomeMessage = "";
                Utilities.sendMessage("Welcome message turned off.");
            }
            else
            {
                WelcomeCMD.welcomeMessage = value;
                Utilities.sendMessage("Welcome message set.");
            }

            Entry.save();
        }

        void voteSystem(ClientPlayerInfo p, string value)
        {
            if (value == "0" || value == "false")
            {
                VoteHandler.VoteCMD.votesAllowed = false;
                Utilities.sendMessage("Disabled the voteSystem!");
            }
            else if (value == "1" || value == "true")
            {
                VoteHandler.VoteCMD.votesAllowed = true;
                Utilities.sendMessage("Enabled the voteSystem!");
            }
            else
            {
                help(p);
                return;
            }

            Entry.save();
        }

        void vote(ClientPlayerInfo p, string value)
        {
            if (value == "0" || value == "false")
            {
                AutoCMD.voteNext = false;
                Utilities.sendMessage("Votes disabled !");
            }
            else if (value == "1" || value == "true")
            {
                AutoCMD.voteNext = true;
                Utilities.sendMessage("Votes enabled !");
            }
            else
            {
                help(p);
                return;
            }

            Entry.save();
        }

        void autoMsg(ClientPlayerInfo p, string value)
        {
            if (value == "off")
            {
                AutoCMD.advanceMessage = "";
                Utilities.sendMessage("Advance message turned off.");
            }
            else
            {
                AutoCMD.advanceMessage = value;
                Utilities.sendMessage("Advance message set.");
            }

            Entry.save();
        }

        void autoMinPlayers(ClientPlayerInfo p, string value)
        {
            int num;
            if (int.TryParse(value, out num)) {
                AutoCMD.minPlayers = num;
                Utilities.sendMessage($"Min players set to {num} !");
            }
            else
            {
                Utilities.sendMessage("Invalid number!");
                help(p);
                return;
            }

            Entry.save();
        }

        void autoMaxTime(ClientPlayerInfo p, string value)
        {
            int num;
            if (int.TryParse(value, out num))
            {
                AutoCMD.maxRunTime = num;
                Utilities.sendMessage($"Max time set to {num} !");
            }
            else
            {
                Utilities.sendMessage("Invalid number!");
                help(p);
                return;
            }

            Entry.save();
        }

        void autoSpecPlayer(ClientPlayerInfo p, string value)
        {
            if (value == "0" || value == "false")
            {
                AutoCMD.autoSpecCountsAsPlayer = false;
                Utilities.sendMessage("AutoSpec host no longer counts as a player!");
            }
            else if (value == "1" || value == "true")
            {
                AutoCMD.autoSpecCountsAsPlayer = true;
                Utilities.sendMessage("AutoSpec host counts as a player!");
            }
            else
            {
                help(p);
                return;
            }

            Entry.save();
        }
    }
}
