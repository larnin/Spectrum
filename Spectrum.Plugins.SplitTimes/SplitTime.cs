using System;
using System.IO;
using System.Linq;
using Spectrum.API;
using Spectrum.API.Game.Vehicle;

namespace Spectrum.Plugins.SplitTimes
{
    public struct SplitTime
    {
        private TimeSpan _old;
        private TimeSpan _new;
        public int CheckpointId { get; private set; }
        public int LastCheckpointId { get; private set; }

        public TimeSpan Total
        {
            get
            {
                return _new;
            }
        }

        public TimeSpan Split
        {
            get
            {
                return _new - _old;
            }
        }

        private SplitTime Rounded
        {
            get
            {
                return new SplitTime(_old, TimeSpan.FromMilliseconds(Math.Round (_new.TotalMilliseconds / 10f) * 10f), CheckpointId, LastCheckpointId);
            }
        }

        public SplitTime(SplitTime oldTime, TimeSpan newTime, int checkpointId)
        {
            _old = oldTime.Total;
            _new = newTime;
            CheckpointId = checkpointId;

            if (_old == TimeSpan.Zero)
                LastCheckpointId = -1;
            else
                LastCheckpointId = oldTime.CheckpointId;
        }

        public SplitTime(TimeSpan oldTime, TimeSpan newTime, int checkpointId, int lastCheckpointId)
        {
            _old = oldTime;
            _new = newTime;
            CheckpointId = checkpointId;
            LastCheckpointId = lastCheckpointId;
        }

        private string Render(TimeSpan time, int decPlaces = 2, char milSep = '.', char minSep = ':')
        {
            return $"{time.Minutes:D2}{minSep}{time.Seconds:D2}{milSep}{time.Milliseconds.ToString("D3").Substring(0, decPlaces)}";
        }

        public void SetTimeBarText(float delay)
        {
            LocalVehicle.Screen.SetTimeBarText(Render(Total, 2, ':'), "#0FA6D9", delay);
        }

        public void SetTimeBarText(SplitTime previousBest, float delay)
        {
            if (previousBest.Rounded.Total < Rounded.Total)
                LocalVehicle.Screen.SetTimeBarText("+" + Render(Rounded.Total - previousBest.Rounded.Total, 2, ':') + "  ", "#FF0000", delay);
            else if (previousBest.Rounded.Total > Rounded.Total)
                LocalVehicle.Screen.SetTimeBarText("-" + Render(previousBest.Rounded.Total - Rounded.Total, 2, ':') + " ", "#00FF00", delay);
            else
                SetTimeBarText(delay);
        }

        public string RenderHud()
        {
            return $"<size=25>{Render(Total)}</size>  {Render(Split)}";
        }

        public string RenderHud(SplitTime previousBest)
        {
            var output = new System.Text.StringBuilder();
            output.Append("<size=25>");

            if (previousBest.Rounded.Total < Rounded.Total)
                output.Append("<color=#de6262ff>");
            else if (previousBest.Rounded.Total > Rounded.Total)
                output.Append("<color=#6be584ff>");

            output.Append(Render(Total));

            if (previousBest.Rounded.Total != Rounded.Total)
                output.Append("</color>");

            output.Append("</size>  ");

            if (LastCheckpointId == previousBest.LastCheckpointId)
            {
                if (previousBest.Rounded.Split < Rounded.Split)
                    output.Append("<color=#de6262ff>");
                else if (previousBest.Rounded.Split > Rounded.Split)
                    output.Append("<color=#6be584ff>");
            }

            output.Append(Render(Split));

            if (LastCheckpointId == previousBest.LastCheckpointId && previousBest.Rounded.Split != Rounded.Split)
                output.Append("</color>");

            return output.ToString();
        }

        public string RenderSave()
        {
            if (CheckpointId == -1)
                return $"{Render(Total)}\t{Render(Rounded.Split)}";
            else
                return $"{Render(Rounded.Total, 2)}\t{Render(Rounded.Split, 2)}\t{CheckpointId}";
        }

        public string RenderFilename()
        {
            if (CheckpointId != -1)
                throw new Exception("Spectrum.Plugins.SplitTimes: Could not generate filename. Attempted to generate filename from checkpoint time.");

            return $"{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}_{Render(Total, 3, '-', '-')}.txt";
        }

        public static string GetSavePath(Level l, GameMode m, Profile p)
        {
            var folder = FileSystem.GetValidFileNameToLower(p.Name_, "_");
            folder = Path.Combine(folder, FileSystem.GetValidFileNameToLower(m.GameModeID_.ToString(), "_"));
            folder = Path.Combine(folder, FileSystem.GetValidFileNameToLower(l.Name_, "_"));
            folder = Path.Combine(folder, String.Join("_", l.Path_.Split(Path.DirectorySeparatorChar).Reverse().Take(3).Reverse().ToArray()));

            return folder;
        }
    }
}
