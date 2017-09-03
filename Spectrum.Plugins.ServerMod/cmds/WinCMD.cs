using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class WinCmd : Cmd
    {
        public override string name { get { return "win"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return true; } }

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingWinList()
        };

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!win") + ": Win the game !");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            Random r = new Random();
            float color = (float)r.NextDouble() * 360;
            MessageUtilities.sendMessage("[b][" + ColorEx.ColorToHexNGUI(new ColorHSB(color, 1.0f, 1.0f, 1f).ToColor()) + "]" + winList[r.Next(winList.Count)] + "[-][/b]");
        }

        public List<string> winList
        {
            get { return (List<string>)getSetting("win").Value; }
            set { getSetting("win").Value = value; }
        }
    }
    class CmdSettingWinList : CmdSetting
    {
        public override string FileId { get; } = "win";
        public override string SettingsId { get; } = "";  // disabled

        public override string DisplayName { get; } = "Win List";
        public override string HelpShort { get; } = "Possible !win phrases";
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
                    "ALL RIGHT!",
                    "ALRIGHT!",
                    "AMAZING!",
                    "ASTOUNDING!",
                    "AWESOME!",
                    "AWESOMESAUCE!",
                    "BANGIN'!",
                    "BETTER THAN SLICED BREAD!",
                    "CHA-CHING!",
                    "COOL!",
                    "COWABUNGA!",
                    "CRAZY!",
                    "CUNNING!",
                    "DOUBLE RAINBOW!",
                    "DUDE!",
                    "DYN-O-MITE!",
                    "EXCELLENT!",
                    "EXTREME!",
                    "FABULOUS!",
                    "FANTABULOUS!",
                    "GOIN' THE DISTANCE!",
                    "GOING THE DISTANCE!",
                    "GOOD SHOW!",
                    "GREAT JOB!",
                    "GROOVY!",
                    "HARDCORE!",
                    "HOLY TOLEDO!",
                    "HOT DOG!",
                    "INSANE!",
                    "IT'S BUSINESS TIME!",
                    "JAMMIN'!",
                    "KA-BAM SON!",
                    "LEGENDARY!",
                    "LIKE A BOSS!",
                    "NICE!",
                    "NIFTY!",
                    "NO HANDLEBARS!",
                    "NO WAY!",
                    "OH SNAP!",
                    "OUTTA THE PARK!",
                    "PEAS AND CARROTS!",
                    "PIZZA!",
                    "PROFESSIONAL!",
                    "RIDICULOUS!",
                    "RIGHT ON!",
                    "ROCK N' ROLL!",
                    "SCORE!",
                    "SIDEWHEELIE!",
                    "SPIFFY!",
                    "STUNNING!",
                    "SUPER!",
                    "SUPERFLY!",
                    "SUPREME!",
                    "TOO GOOD!",
                    "TOTALLY AWESOME!",
                    "TOTALLY TUBULAR!",
                    "TRICKSTER!",
                    "TWIST AND SHOUT!",
                    "TWISTER CITY!",
                    "ULTRA COMBO!",
                    "ULTRACOMBO!",
                    "UNREAL!",
                    "WELL DONE!",
                    "WHOA!",
                    "WICKED!",
                    "WINNING!",
                    "WONDERFUL!",
                    "YEAH!",
                };
            }
        }
    }
}
