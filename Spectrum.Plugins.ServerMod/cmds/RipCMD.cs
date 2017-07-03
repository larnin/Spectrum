using Spectrum.Plugins.ServerMod.CmdSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class RipCMD : cmd
    {
        public override string name { get { return "rip"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return true; } }

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingRipList()
        };

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage(Utilities.formatCmd("!rip") + ": Rip the game !");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            Random r = new Random();
            float color = (float)r.NextDouble() * 360;
            Utilities.sendMessage("[b][" + ColorEx.ColorToHexNGUI(new ColorHSB(color, 0.5f, 0.5f, 1f).ToColor()) + "]" + ripList[r.Next(ripList.Count)] + "[-][/b]");
        }

        public List<string> ripList
        {
            get { return (List<string>)getSetting("rip").Value; }
            set { getSetting("rip").Value = value; }
        }
    }
    class CmdSettingRipList : CmdSetting
    {
        public override string FileId { get; } = "rip";
        public override string SettingsId { get; } = "";  // disabled

        public override string DisplayName { get; } = "Rip List";
        public override string HelpShort { get; } = "Possible !rip phrases";
        public override string HelpLong { get { return HelpShort; } }

        public override UpdateResult UpdateFromString(string input)
        {
            throw new NotImplementedException();
        }

        public override UpdateResult UpdateFromObject(object input)
        {
            try
            {
                return new UpdateResult(true, ((string[])input).ToList());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading list: {e}");
                return new UpdateResult(false, Default, "Error reading list. Resetting to default.");
            }
        }

        public override object Default
        {
            get
            {
                return new List<string>
                {
                    "ACCESS VIOLATION!",
                    "AW, THAT'S TOO BAD!",
                    "BOGUS!",
                    "BUCKETS OF TEARS!",
                    "BUMMER!",
                    "CAR BOOM!",
                    "CASUAL!",
                    "CRIKEY!",
                    "CRITICAL ERROR!",
                    "DENIED!",
                    "DESTROYED!",
                    "DIVISION BY ZERO!",
                    "DRIVING MISS DAISY!",
                    "ERROR 404: SUCCESS NOT FOUND!",
                    "FAREWELL!",
                    "FRAGGED!",
                    "FRAGMENTED!",
                    "GAME OVER!",
                    "HOLY TOLEDO!",
                    "INERTIA SUCKS!",
                    "IT KEEPS HAPPENING!",
                    "KNOCKOUT!",
                    "LAMESAUCE!",
                    "MAYDAY!",
                    "MONDO DISASTER!",
                    "NICE TRY!",
                    "NITRAGIC!",
                    "NITRONICRASHED!",
                    "NO CIGAR!",
                    "NUCLEAR!",
                    "NULL TERMINATED!",
                    "REJECTED!",
                    "ROADKILL!",
                    "SAD DAY!",
                    "SAYONARA!",
                    "SHORT CIRCUIT!",
                    "SHUT DOWN!",
                    "SO SAD!",
                    "STEP ON IT!",
                    "STUDENT DRIVER!",
                    "TAKE DOWN!",
                    "THAT'S GOTTA HURT!",
                    "THAT'S WHACK!",
                    "TOO BAD!",
                    "UNCOOL!",
                    "UNFORTUNATE!",
                    "VAPORIZED!",
                    "WEAKSAUCE!",
                    "WRECKED!",
                    "WRONG TURN!",
                    "YOU'D BETTER UPGRADE THAT GRAPHICS CARD--YOUR COMPUTER IS TOO SLOW!",
                    "YOU'RE OUT OF CONTROL!",
                };
            }
        }
    }
}
