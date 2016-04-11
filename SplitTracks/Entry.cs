using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Spectrum.API;
using Spectrum.API.Game;
using Spectrum.API.Game.Vehicle;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.API.Interfaces.Systems;
using Spectrum.API.Configuration;
using Spectrum.API.Game.EventArgs.Vehicle;

namespace SplitTracks
{
    public class Entry : IPlugin
    {
        public string FriendlyName => "Split tracks";
        public string Author => "Jonathan Vollebregt";
        public string Contact => "jnvsor@gmail.com";
        public APILevel CompatibleAPILevel => APILevel.MicroWave;

        private readonly List<SplitTrack> _previousTrackTimes = new List<SplitTrack>();
        private readonly List<TimeSpan> _bestTrackTimes = new List<TimeSpan>();
        private TimeSpan _thisTrackBest;
        private bool _active = false;
        private bool _started = false;

        private Settings Settings { get; set; }

        public void Initialize(IManager manager)
        {
            LocalVehicle.Finished += LocalVehicle_Finished;
            Race.Started += Race_Started;

            Settings = new Settings(typeof(Entry));
            ValidateSettings();

            manager.Hotkeys.Bind(Settings["ShowTimesHotkey"], SplitTracks_ShowPressed, false);
            manager.Hotkeys.Bind(Settings["StartListHotkey"], SplitTracks_StartList);
            manager.Hotkeys.Bind(Settings["EndListHotkey"], SplitTracks_EndList);
        }

        private void Race_Started(object sender, EventArgs e)
        {
            if (!_active)
                return;

            _started = false;

            _thisTrackBest = GetBestTime();

            if (_previousTrackTimes.Count > 0)
            {
                SplitTracks_ShowPressed(8f);
            }

            _started = true;
        }

        private void LocalVehicle_Finished(object sender, FinishedEventArgs e)
        {
            if (e.Type != RaceEndType.Finished)
                return;

            if (!_active || !_started)
                return;

            _bestTrackTimes.Add(_thisTrackBest);
            var finished = new SplitTrack(_previousTrackTimes.LastOrDefault(), TimeSpan.FromMilliseconds(e.FinalTime), G.Sys.GameManager_.Level_.Name_);
            _previousTrackTimes.Add(finished);

            _started = false;
            SplitTracks_ShowPressed(8f);
        }

        private void SplitTracks_ShowPressed()
        {
            SplitTracks_ShowPressed(2.5f);
        }

        private void SplitTracks_ShowPressed(float duration)
        {
            if (G.Sys.GameManager_.IsModeGo_ && !G.Sys.GameManager_.Paused_ && !G.Sys.PlayerManager_.Current_.inGameData_.Finished_)
            {
                var times = GetTimeStrings();
                if (_started)
                {
                    times.Insert(0, new SplitTrack(_previousTrackTimes.LastOrDefault(), Race.ElapsedTime, G.Sys.GameManager_.Level_.Name_).RenderHud());
                    times.Insert(0, "Total time: " + _previousTrackTimes.LastOrDefault().RenderTotal(Race.ElapsedTime));
                }
                else if (_previousTrackTimes.Count != 0)
                {
                    times.Insert(0, "Total time: " + _previousTrackTimes.LastOrDefault().RenderTotal());
                }
                HudLinesDownward(duration, times);
            }
        }

        private void SplitTracks_StartList()
        {
            _active = true;
            _started = false;
            _previousTrackTimes.Clear();
            _bestTrackTimes.Clear();
        }

        private void SplitTracks_EndList()
        {
            _active = false;
            _started = false;

            SplitTracks_ShowPressed(10f);
        }

        private void HudLinesDownward(float delay, List<string> lines)
        {
            if (lines.Count == 0)
                return;

            var output = new StringBuilder();

            output.Append(string.Join(Environment.NewLine, lines.ToArray()));
            for (int i = 0; i < lines.Count; i++)
                output.Insert(0, Environment.NewLine);

            LocalVehicle.HUD.Clear();
            LocalVehicle.HUD.SetHUDText(output.ToString(), delay);
        }

        private List<string> GetTimeStrings()
        {
            var l = new List<string>();

            for (int i = _previousTrackTimes.Count; i > 0; i--)
            {
                if (_bestTrackTimes[i - 1] == TimeSpan.Zero)
                    l.Add(_previousTrackTimes[i - 1].RenderHud());
                else
                    l.Add(_previousTrackTimes[i - 1].RenderHud(_bestTrackTimes[i - 1]));
            }

            return l;
        }

        private TimeSpan GetBestTime()
        {
            var leaderboard = LocalLeaderboard.Load(G.Sys.GameManager_.Level_.Path_, G.Sys.GameManager_.ModeID_);
            TimeSpan previousBest = TimeSpan.Zero;

            if (leaderboard != null)
            {
                foreach (ResultInfo time in leaderboard.Results_)
                {
                    if (time.ProfileName_ == G.Sys.PlayerManager_.Current_.profile_.Name_ && (previousBest == TimeSpan.Zero || TimeSpan.FromMilliseconds(time.Time_) < previousBest))
                        previousBest = TimeSpan.FromMilliseconds(time.Time_);
                }
            }

            if (previousBest != TimeSpan.Zero)
                return previousBest;

            var path = Path.Combine(Defaults.PluginDataDirectory, "SplitTimes.plugin");
            path = Path.Combine(path, Resource.GetValidFileNameToLower(G.Sys.PlayerManager_.Current_.profile_.Name_, "_"));
            path = Path.Combine(path, Resource.GetValidFileNameToLower(G.Sys.GameManager_.Mode_.GameModeID_.ToString(), "_"));
            path = Path.Combine(path, Resource.GetValidFileNameToLower(G.Sys.GameManager_.Level_.Name_, "_"));
            path = Path.Combine(path, "pb.txt");

            if (File.Exists(path))
            {
                try
                {
                    using (var sr = new StreamReader(path))
                    {
                        string[] line;
                        while ((line = sr.ReadLine()?.Split('\t')) != null)
                        {
                            if (line.Length == 2)
                            {
                                return TimeSpan.Parse("00:" + line[0]);
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"SplitTracks: Tried to load time from SplitTimes and failed. Exception below:\n{ex}");
                }
            }

            return TimeSpan.Zero;
        }

        private void ValidateSettings()
        {
            if (Settings["ShowTimesHotkey"] == string.Empty)
                Settings["ShowTimesHotkey"] = "LeftControl+X";
            if (Settings["StartListHotkey"] == string.Empty)
                Settings["StartListHotkey"] = "LeftBracket";
            if (Settings["EndListHotkey"] == string.Empty)
                Settings["EndListHotkey"] = "RightBracket";

            Settings.Save();
        }

        public void Shutdown()
        {
        }
    }
}
