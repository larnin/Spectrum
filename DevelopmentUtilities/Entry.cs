using System;
using DevelopmentUtilities.Utilities;
using Spectrum.API;
using Spectrum.API.Configuration;
using Spectrum.API.Game.Network;
using Spectrum.API.Game.Vehicle;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.API.Interfaces.Systems;

namespace DevelopmentUtilities
{
    public class Entry : IPlugin
    {
        public string FriendlyName => "Development Utilities";
        public string Author => "Ciastex";
        public string Contact => "ciastexx@live.com";
        public  APILevel CompatibleAPILevel => APILevel.InfraRed;

        private FileSystem FileSystem { get; set; }
        private Settings Settings { get; set; }

        private SceneDumper SceneDumper { get; set; }

        public void Initialize(IManager manager)
        {
            Settings = new Settings(typeof(Entry));
            ValidateSettings();

            FileSystem = new FileSystem(typeof(Entry));

            SceneDumper = new SceneDumper(FileSystem);

            manager.Hotkeys.Bind(Settings["SceneDumperBriefHotkey"] as string, () =>
            {
                SceneDumper.DumpCurrentScene(false);
            });

            manager.Hotkeys.Bind(Settings["SceneDumperDetailedHotkey"] as string, () =>
            {
                SceneDumper.DumpCurrentScene(true);
            });

            manager.Hotkeys.Bind("LeftControl+Alpha7", () =>
            {
                LocalVehicle.SetJetFlamesColor("#0077DD");
            });

            manager.Hotkeys.Bind("LeftControl+Alpha6", () =>
            {
                LocalVehicle.SetBoostFlameColor("#00DD44");
            });

            manager.Hotkeys.Bind("LeftControl+Alpha5", () =>
            {
                LocalVehicle.SetWingTrailsColor("#FF00FF");
            });

            manager.Hotkeys.Bind("LeftControl+Alpha4", () =>
            {
                Console.WriteLine("PRESSED, NOT ONE-TIME");
            }, false);

            LocalVehicle.BeforeExploded += (sender, args) => Console.WriteLine("BeforeExploded");
            LocalVehicle.BeforeSplit += (sender, args) => Console.WriteLine("BeforeSplit");
            LocalVehicle.CheckpointPassed += (sender, args) => Console.WriteLine($"CheckpointPassed: {args.CheckpointIndex}, {args.TrackT}");
            LocalVehicle.Destroyed += (sender, args) => Console.WriteLine("Destroyed because of: {args.Cause.ToString()}");
            LocalVehicle.Exploded += (sender, args) => Console.WriteLine($"Exploded because of: {args.Cause.ToString()}");
            LocalVehicle.Honked += (sender, args) => Console.WriteLine($"Honked with {args.HornPower} of maximum horn power at {args.Position.X}, {args.Position.Y}, {args.Position.Z}.");
            LocalVehicle.Collided += (sender, args) => Console.WriteLine($"Collided with {args.ImpactedObjectName} at {args.Position.X}, {args.Position.Y}, {args.Position.Z} with speed of {args.Speed} units.");
            LocalVehicle.Jumped += (sender, args) => Console.WriteLine("Jumped");
            LocalVehicle.SpecialModeEvent += (sender, args) => Console.WriteLine("SpecialModeEvent");
            LocalVehicle.Split += (sender, args) => Console.WriteLine($"Split with a penetration factor of {args.Penetration} and split speed of {args.SeparationSpeed} units.");
            LocalVehicle.TrickCompleted += (sender, args) =>
            {
                Console.WriteLine($"Trick completed for a total of {args.PointsEarned} points with {args.CeilingRideMeters} ceiling, {args.WallRideMeters} wall and {args.GrindMeters} grind ride meters for {args.CooldownAmount * 100} cooldown percent");
            };
            LocalVehicle.WingsEnabled += (sender, args) => Console.WriteLine("WingsEnabled");
            LocalVehicle.WingsDisabled += (sender, args) => Console.WriteLine("WingsDisabled");
            LocalVehicle.WingsClosed += (sender, args) => Console.WriteLine("WingsClosed");
            LocalVehicle.WingsOpened += (sender, args) => Console.WriteLine("WingsOpened");
            Server.LobbyInitialized += (sender, args) => Console.WriteLine("Lobby initialized.");
            Server.ServerCreated += (sender, args) => Console.WriteLine($"Server '{args.Name}' created with password: '{args.Password}' and capacity of '{args.Capacity}' players");
            Chat.MessageReceived += (sender, args) => Console.WriteLine($"Message received from '{args.Author}': '{args.Message}'");
            Chat.MessageSent += (sender, args) => Console.WriteLine($"Message sent: '{args.Message}'");
            Chat.ActionReceived += (sender, args) => Console.WriteLine($"Action received from '{args.Nickname}' who has an index of '{args.PlayerIndex}': '{args.ActionMessage}'");

            Server.PlayerJoined += (sender, args) =>
            {
                Console.WriteLine($"Player '{args.Nickname}' joined with ready status as '{args.IsReady}' and level compatibility '{args.LevelCompatibility}'");   
            };

            Server.PlayerLeft += (sender, args) =>
            {
                Console.WriteLine($"Player '{args.Nickname}' left with ready status as '{args.IsReady}' and level compatibility '{args.LevelCompatibility}");
            };

            Server.GameModeChanged += (sender, args) =>
            {
                Console.WriteLine($"Game mode changed to {args.Name}");
            };

            LocalVehicle.Finished += (sender, args) =>
            {
                Console.WriteLine($"Vehicle finished race: {args.Type}");
            };

            manager.Hotkeys.Bind("LeftAlt+Alpha9", () =>
            {
                Chat.AddLocalMessage("This is a local message. It shouldn't be seen by others.");
            });

            manager.Hotkeys.Bind("LeftAlt+Alpha8", () =>
            {
                Chat.SendAction("sends a test action so people can see it.");
            });

            manager.Hotkeys.Bind("LeftAlt+Alpha7", () =>
            {
                Chat.SendMessage("I've sent an automatic message.");
            });
        }

        public void Shutdown()
        {

        }

        private void ValidateSettings()
        {
            if ((string)Settings["SceneDumperBriefHotkey"] == string.Empty)
            {
                Settings["SceneDumperBriefHotkey"] = "LeftControl+Alpha0";
            }

            if ((string)Settings["SceneDumperDetailedHotkey"] == string.Empty)
            {
                Settings["SceneDumperDetailedHotkey"] = "LeftControl+Alpha9";
            }
            Settings.Save();
        }
    }
}
