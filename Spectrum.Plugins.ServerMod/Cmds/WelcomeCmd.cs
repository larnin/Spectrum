using Events;
using Events.RaceMode;
using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using UnityEngine;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class CmdSettingWelcomeMessage : CmdSettingString
    {
        public override string FileId { get; } = "welcome";
        public override string SettingsId { get; } = "welcome";

        public override string DisplayName { get; } = "Welcome Message";
        public override string HelpShort { get; } = "Show a welcome message";
        public override string HelpLong { get; } = "The welcome message to show to players. `clear` to turn off. `%USERNAME%` is replaced with the player's name.";

        public override string Default { get; } = "";
    }
    class WelcomeCmd : Cmd
    {
        public string welcomeMessage
        {
            get { return getSetting<CmdSettingWelcomeMessage>().Value; }
            set { getSetting<CmdSettingWelcomeMessage>().Value = value; }
        }

        public override string name { get { return "welcome"; } }
        public override PermType perm { get { return PermType.ALL; } }
        public override bool canUseAsClient { get { return false; } }

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingWelcomeMessage()
        };

        public WelcomeCmd()
        {

            Events.Server.StartClientLate.Subscribe(data =>
            {
                GeneralUtilities.testFunc(() =>
                {
                    if (GeneralUtilities.isOnline() && GeneralUtilities.isHost())
                        onClientJoin(data.client_);
                });
            });
        }

        public override void help(ClientPlayerInfo p)
        {
            MessageUtilities.sendMessage(p, GeneralUtilities.formatCmd("!welcome") + ": Hear the welcome message.");
            if (GeneralUtilities.isHost())
            {
                MessageUtilities.sendMessage(p, "You can set the welcome message with !settings");
            }
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            MessageUtilities.sendMessage(p, welcomeMessage.Replace("%USERNAME%", p.Username_));
        }

        

        private void onClientJoin(NetworkPlayer client)
        {
            if (welcomeMessage != "")
            {
                foreach (ClientPlayerInfo current in G.Sys.PlayerManager_.PlayerList_)
                {
                    if (current.NetworkPlayer_ == client)
                    {
                        MessageUtilities.sendMessage(current, welcomeMessage.Replace("%USERNAME%", current.Username_));
                        return;
                    }
                }
                MessageUtilities.sendMessage(welcomeMessage.Replace("%USERNAME%", "Player"));
            }
        }

       
    }
}
