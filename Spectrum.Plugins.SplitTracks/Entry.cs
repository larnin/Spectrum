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

namespace Spectrum.Plugins.SplitTracks
{
    public class Entry : IPlugin
    {
        public string FriendlyName => "Split tracks";
        public string Author => "Jonathan Vollebregt";
        public string Contact => "jnvsor@gmail.com";
        public APILevel CompatibleAPILevel => APILevel.UltraViolet;

        private readonly List<SplitTrack> _previousTrackTimes = new List<SplitTrack>();
        private readonly List<TimeSpan> _bestTrackTimes = new List<TimeSpan>();
        private TimeSpan _thisTrackBest;
        private bool _active = false;
        private bool _started = false;
        private bool _finished = true;

        private Settings Settings;

        public void Initialize(IManager manager)
        {
            LocalVehicle.Finished += Finished;
            Race.Started += Started;

            Settings = new Settings(typeof(Entry));
            ValidateSettings();

            manager.Hotkeys.Bind(Settings["ShowTimesHotkey"] as string, ShowPressed, false);
            manager.Hotkeys.Bind(Settings["StartListHotkey"] as string, StartList);
            manager.Hotkeys.Bind(Settings["EndListHotkey"] as string, EndList);
        }

        private void Started(object sender, EventArgs e)
        {
            if (!_active)
                return;

            _started = false;
            _finished = false;

            _thisTrackBest = GetBestTime();

            if (_previousTrackTimes.Count > 0)
            {
                ShowPressed(8f);
            }

            _started = true;
        }

        private void Finished(object sender, FinishedEventArgs e)
        {
            if (e.Type != RaceEndType.Finished)
                return;

            if (!_active || !_started)
                return;

            _bestTrackTimes.Add(_thisTrackBest);
            var finished = new SplitTrack(_previousTrackTimes.LastOrDefault(), TimeSpan.FromMilliseconds(e.FinalTime), G.Sys.GameManager_.Level_.Name_);
            _previousTrackTimes.Add(finished);

            _started = false;
            ShowPressed(8f);
            _finished = true;
        }

        private void ShowPressed()
        {
            ShowPressed(2.5f);
        }

        private void ShowPressed(float duration)
        {
            if (G.Sys.GameManager_.IsModeGo_ && !G.Sys.GameManager_.PauseMenuOpen_ && !_finished && !G.Sys.GameManager_.Mode_.IsChatWindowOpen_)
            {
                Show(duration);
            }
        }

        private void Show(float duration)
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

        private void StartList()
        {
            _active = true;
            _started = false;
            _previousTrackTimes.Clear();
            _bestTrackTimes.Clear();
        }

        private void EndList()
        {
            _active = false;
            _started = false;
            _finished = false;

            ShowPressed(10f);
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
            LevelInfo levelInfo = G.Sys.LevelSets_.GetLevelInfo(G.Sys.GameManager_.LevelPath_);
            ProfileProgress progress = G.Sys.ProfileManager_.CurrentProfile_.Progress_;
            int pb = progress.GetTopResultWithRelativePath(levelInfo.relativePath_, G.Sys.GameManager_.ModeID_);

            if (pb != -1)
            {
                return TimeSpan.FromMilliseconds(pb);
            }
            else
            {
                return TimeSpan.Zero;
            }
        }

        private void ValidateSettings()
        {
            if (!Settings.ContainsKey("ShowTimesHotkey"))
                Settings["ShowTimesHotkey"] = "LeftControl+X";
            if (!Settings.ContainsKey("StartListHotkey"))
                Settings["StartListHotkey"] = "LeftBracket";
            if (!Settings.ContainsKey("EndListHotkey"))
                Settings["EndListHotkey"] = "RightBracket";

            Settings.Save();
        }

        public void Shutdown()
        {
        }
    }
}
