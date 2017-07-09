using Events;
using Events.RaceMode;
using Spectrum.Plugins.ServerMod.CmdSettings;
using System;
using UnityEngine;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class CmdSettingWelcomeMessage : CmdSettingString
    {
        public override string FileId { get; } = "welcome";
        public override string SettingsId { get; } = "welcome";

        public override string DisplayName { get; } = "Welcome Message";
        public override string HelpShort { get; } = "Show a welcome message";
        public override string HelpLong { get; } = "The welcome message to show to players. `clear` to turn off. `%USERNAME%` is replaced with the player's name.";

        public override object Default { get; } = "";
    }
    class WelcomeCMD : cmd
    {
        public string welcomeMessage
        {
            get { return (string)getSetting("welcome").Value; }
            set { getSetting("welcome").Value = value; }
        }

        public override string name { get { return "welcome"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return false; } }

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingWelcomeMessage()
        };

        public WelcomeCMD()
        {

            Events.Server.StartClientLate.Subscribe(data =>
            {
                Utilities.testFunc(() =>
                {
                    if (Utilities.isOnline() && Utilities.isHost())
                        onClientJoin(data.client_);
                });
            });
        }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage(Utilities.formatCmd("!welcome") + ": Hear the welcome message.");
            if (Utilities.isHost())
            {
                Utilities.sendMessage("You can set the welcome message with !settings");
            }
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            Utilities.sendMessage(welcomeMessage.Replace("%USERNAME%", p.Username_));
        }

        

        private void onClientJoin(NetworkPlayer client)
        {
            if (welcomeMessage != "")
            {
                foreach (ClientPlayerInfo current in G.Sys.PlayerManager_.PlayerList_)
                {
                    if (current.NetworkPlayer_ == client)
                    {
                        Utilities.sendMessage(welcomeMessage.Replace("%USERNAME%", current.Username_));
                        return;
                    }
                }
                Utilities.sendMessage(welcomeMessage.Replace("%USERNAME%", "Player"));
            }
        }

       
    }
}
