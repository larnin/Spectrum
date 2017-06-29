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
            Utilities.sendMessage("[b][D00000]General Settings[-][/b]");
            Utilities.sendMessage(Utilities.formatCmd("!settings reload") + ": reload the settings from the file.");
            Utilities.sendMessage(Utilities.formatCmd("!settings autoSpecReturnToLobby") + ": return to lobby if autospectating and no one else is in the server. default: false");
            Utilities.sendMessage(Utilities.formatCmd("!settings updateCheck [true/false]") + ": whether or not an update check should be performed when the server is started.");
            Utilities.sendMessage(Utilities.formatCmd("!settings playVote [true/false]") + ": set play command to act as '!vote y play'");
            Utilities.sendMessage("This setting overrides the 'play' setting.");
            Utilities.sendMessage(Utilities.formatCmd("!settings play [true/false]") + ": allow player to add maps on the playlist.");
            Utilities.sendMessage(Utilities.formatCmd("!settings addOne [true/false]") + ": if enabled, allow the players to add only one map at a time.");
            Utilities.sendMessage(Utilities.formatCmd("!settings welcome [message]") + ": Set the welcome message.");
            Utilities.sendMessage("\"off\" to disable.");
            Utilities.sendMessage("%USERNAME% is substituted for the player's name.");
            Utilities.sendMessage(Utilities.formatCmd("!settings voteSystem [true/false]") + ": Turn the voting system off/on.");
            Utilities.sendMessage("This is separate from autoVote!");
            Utilities.sendMessage("voteSystem thresholds are changed with !votectrl");
            Utilities.sendMessage("[b][D00000]!auto Settings[-][/b]");
            Utilities.sendMessage(Utilities.formatCmd("!settings autoVote [true/false]") + ": allow players to vote on auto mode.");
            Utilities.sendMessage(Utilities.formatCmd("!settings autoShuffle [true/false]") + ": shuffle the playlist at the end in auto mode.");
            Utilities.sendMessage(Utilities.formatCmd("!settings autoUniqueVotes [true/false]") + ": whether or not 0-3 votes at the end of a level should remove vote entries that showed up in previous votes.");
            Utilities.sendMessage(Utilities.formatCmd("!settings autoMsg [message]") + ": the message to display when advancing the map in auto mode. \"off\" to disable.");
            Utilities.sendMessage(Utilities.formatCmd("!settings autoMinPlayers [amount]") + ": the minimum amount of players needed for a map to start in auto mode.");
            Utilities.sendMessage(Utilities.formatCmd("!settings autoMaxTime [seconds]") + ": the maximum amount of time that auto mode will spend on one map.");
            Utilities.sendMessage(Utilities.formatCmd("!settings autoSpecHostIgnored [true/false]") + ": turn on/off whether the host is ignored as a player when running auto mode.");
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
            else if (strs[0] == "autospecreturntolobby")
            {
                if (strs.Length == 1)
                    help(p);
                else autoSpecReturnToLobby(p, strs[1]);
            }
            else if (strs[0] == "updatecheck")
            {
                if (strs.Length == 1)
                    help(p);
                else updateCheck(p, strs[1]);
            }
            else if (strs[0] == "playvote")
            {
                if (strs.Length == 1)
                    help(p);
                else playVote(p, strs[1]);
            }
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
                else autoVote(p, strs[1]);
            }
            else if (strs[0] == "autoshuffle")
            {
                if (strs.Length == 1)
                    help(p);
                else autoShuffle(p, strs[1]);
            }
            else if (strs[0] == "autouniquevotes")
            {
                if (strs.Length == 1)
                    help(p);
                else autoUniqueVotes(p, strs[1]);
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
            else if (strs[0] == "autospechostignored")
            {
                if (strs.Length == 1)
                    help(p);
                else autoSpecPlayer(p, strs[1]);
            }
            else help(p);
        }

        void reload(ClientPlayerInfo p)
        {
            Entry.reload();
            Utilities.sendMessage("Settings reloaded from file!");
        }

        void autoSpecReturnToLobby(ClientPlayerInfo p, string value)
        {
            if (value == "0" || value == "false")
            {
                AutoSpecCMD.autoSpecReturnToLobby = false;
                Utilities.sendMessage("'!autospec' no longer returns to lobby when no one else is in the server.");
            }
            else if (value == "1" || value == "true")
            {
                AutoSpecCMD.autoSpecReturnToLobby = true;
                Utilities.sendMessage("'!autospec' now returns to lobby when no one else is in the server.");
            }
            else
            {
                help(p);
                return;
            }

            Entry.save();
        }

        void updateCheck(ClientPlayerInfo p, string value)
        {
            if (value == "0" || value == "false")
            {
                UpdateCMD.updateCheck = false;
                Utilities.sendMessage("No longer checking for updates when the server is started.");
            }
            else if (value == "1" || value == "true")
            {
                UpdateCMD.updateCheck = true;
                Utilities.sendMessage("Will check for updates when a server is started.");
            }
            else
            {
                help(p);
                return;
            }

            Entry.save();
        }

        void playVote(ClientPlayerInfo p, string value)
        {
            if (value == "0" || value == "false")
            {
                PlayCMD.useVote = false;
                Utilities.sendMessage("'!play' no longer acts as '!vote y play' for non-hosts.");
            }
            else if (value == "1" || value == "true")
            {
                PlayCMD.useVote = true;
                Utilities.sendMessage("'!play' now acts as '!vote y play' for non-hosts.");
            }
            else
            {
                help(p);
                return;
            }

            Entry.save();
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

        void autoVote(ClientPlayerInfo p, string value)
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

        void autoShuffle(ClientPlayerInfo p, string value)
        {
            if (value == "0" || value == "false")
            {
                AutoCMD.shuffleAtEnd = false;
                Utilities.sendMessage("'!auto' no longer shuffles at the end of the playlist");
            }
            else if (value == "1" || value == "true")
            {
                AutoCMD.shuffleAtEnd = true;
                Utilities.sendMessage("'!auto' will shuffle at the end of the playlist.");
            }
            else
            {
                help(p);
                return;
            }

            Entry.save();
        }

        void autoUniqueVotes(ClientPlayerInfo p, string value)
        {
            if (value == "0" || value == "false")
            {
                AutoCMD.uniqueEndVotes = false;
                Utilities.sendMessage("'!auto' level-end votes are no longer unique.");
            }
            else if (value == "1" || value == "true")
            {
                AutoCMD.uniqueEndVotes = true;
                Utilities.sendMessage("'!auto' level-end votes are now unique.");
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
                AutoCMD.autoSpecHostIgnored = false;
                Utilities.sendMessage("AutoSpec host is no longer ignored in auto mode!");
            }
            else if (value == "1" || value == "true")
            {
                AutoCMD.autoSpecHostIgnored = true;
                Utilities.sendMessage("AutoSpec host is now ignored in auto mode!");
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
