using System;

namespace Spectrum.Plugins.SplitTracks
{
    public struct SplitTrack
    {
        private TimeSpan _old;
        private TimeSpan _new;
        public string TrackName { get; private set; }

        public TimeSpan Total
        {
            get
            {
                return _new + _old;
            }
        }

        public TimeSpan Track
        {
            get
            {
                return _new;
            }
        }

        public SplitTrack(SplitTrack oldTime, TimeSpan newTime, string trackName)
        {
            _old = oldTime.Total;
            _new = newTime;
            TrackName = trackName;
        }

        private string Render(TimeSpan time, int decPlaces = 2, char milSep = '.', char minSep = ':')
        {
            return $"{time.Minutes:D2}{minSep}{time.Seconds:D2}{milSep}{time.Milliseconds.ToString("D3").Substring(0, decPlaces)}";
        }

        public string RenderHud()
        {
            return $"<size=25>{Render(Track)}   <color=#00000000>(+00:00.00)</color>   {TrackName}</size>";
        }

        public string RenderHud(TimeSpan previousBest)
        {
            var output = new System.Text.StringBuilder();

            output.Append("<size=25>");
            output.Append(Render(Track));

            if (previousBest < Track)
                output.Append($"   <color=#de6262ff>(+{Render(Track - previousBest)})</color>");
            else if (previousBest > Track)
                output.Append($"   <color=#6be584ff>(-{Render(previousBest - Track)})</color>");
            else
                output.Append("   <color=#00000000>(+00:00.00)</color>");

            output.Append($"   {TrackName}</size>");

            return output.ToString();
        }

        public string RenderTotal()
        {
            return Render(Total);
        }

        public string RenderTotal(TimeSpan elapsed)
        {
            return Render(Total + elapsed);
        }
    }
}
