namespace Spectrum.API.Game
{
    public static class Audio
    {
        public static bool CurrentSongLoops => G.Sys.AudioManager_.CurrentSongLoops();

        public static bool HighPassFilterEnabled
        {
            set { G.Sys.AudioManager_.SetQuarantineFilter(value); }
        }

        public static float MusicVolume
        {
            get { return G.Sys.AudioManager_.GetMusicVolume(); }
            set { G.Sys.AudioManager_.SetMusicVolume(value); }
        }

        public static float VehicleVolume
        {
            get { return G.Sys.AudioManager_.GetCarVolume(); }
            set { G.Sys.AudioManager_.SetCarVolume(value); }
        }

        public static float AmbientVolume
        {
            get { return G.Sys.AudioManager_.GetAmbientVolume(); }
            set { G.Sys.AudioManager_.SetAmbientVolume(value); }
        }

        public static float ObstacleVolume
        {
            get { return G.Sys.AudioManager_.GetObstacleVolume(); }
            set { G.Sys.AudioManager_.SetObstacleVolume(value); }
        }

        public static float MenuVolume
        {
            get { return G.Sys.AudioManager_.GetMenuVolume(); }
            set { G.Sys.AudioManager_.SetMenuVolume(value); }
        }
    }
}
