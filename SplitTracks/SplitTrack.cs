using System;

namespace SplitTracks
{
    public struct SplitTrack
    {
        private TimeSpan _old;
        private TimeSpan _new;
        private string _track;

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

        public string TrackName
        {
            get
            {
                return _track;
            }
        }

        public SplitTrack(SplitTrack oldTime, TimeSpan newTime, string trackName)
        {
            _old = oldTime.Total;
            _new = newTime;
            _track = trackName;
        }

        private string Render(TimeSpan time, int decPlaces = 3, char milSep = '.', char minSep = ':')
        {
            return $"{time.Minutes:D2}{minSep}{time.Seconds:D2}{milSep}{time.Milliseconds.ToString("D3").Substring(0, decPlaces)}";
        }

        public string RenderHud()
        {
            return $"<size=25>{Render(Track)}   {TrackName}</size>";
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
                output.Append("             ");

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
