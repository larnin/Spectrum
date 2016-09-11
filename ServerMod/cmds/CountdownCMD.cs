using Events;
using Events.RaceMode;
using System;
using UnityEngine;

namespace ServerMod.cmds
{
    class CountdownCMD : cmd
    {
        bool countdownStarted = false;
        DateTime countdownEndTime = DateTime.Now;

        public override string name { get { return "countdown"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        public CountdownCMD()
        {
            Events.ServerToClient.ModeFinished.Subscribe(data =>
            {
                onModeFinish();
            });

            Events.RaceMode.FinalCountdownCancel.Subscribe(data =>
            {
                onCountdownStop();
            });
        }

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage("!countdown: Start the 60sec final countdown");
            Utilities.sendMessage("!countdown <time>: Start the final countdown with <time> seconds");
            Utilities.sendMessage("!countdown stop: Stop the final countdown");
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(! Utilities.isOnGamemode())
            {
                Utilities.sendMessage("You can't do that here !");
                return;
            }

            int time = 60;
            if(message.Length > 0)
            {
                if(message == "stop")
                {
                    stopCountdown();
                    return;
                }
                try
                {
                    time = int.Parse(message);
                }
                catch(Exception)
                {
                    Utilities.sendMessage("The time must be a number");
                    return;
                }
                if(time < 10 || time > 300)
                {
                    Utilities.sendMessage("The time must be between 10 and 300 seconds");
                    return;
                }
            }

            StaticTargetedEvent<FinalCountdownActivate.Data>.Broadcast(RPCMode.All, new FinalCountdownActivate.Data(Timex.ModeTime_ + time, time));
            Utilities.sendMessage("Final countdown started for " + time + " seconds !");

            countdownStarted = true;
            countdownEndTime = DateTime.Now.AddSeconds(time);
        }

        private void stopCountdown()
        {
            countdownStarted = false;
#pragma warning disable CS0618 // Type or member is obsolete
            StaticTransceivedEvent<FinalCountdownCancel.Data>.Broadcast(default(FinalCountdownCancel.Data));
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private void onModeFinish()
        {
            countdownStarted = false;
        }

        private void onCountdownStop()
        {
            if (!countdownStarted)
                return;

            int finalTime = (int)((countdownEndTime - DateTime.Now).TotalSeconds);
            if(finalTime > 0)
                StaticTargetedEvent<FinalCountdownActivate.Data>.Broadcast(RPCMode.All, new FinalCountdownActivate.Data(Timex.ModeTime_ + finalTime, finalTime));
            Utilities.sendMessage("Final countdown stoped");
        }
    }
}
