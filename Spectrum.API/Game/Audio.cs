using System.Reflection;

namespace Spectrum.API.Game
{
    public class Audio
    {
        public static bool CurrentSongLoops => G.Sys.AudioManager_.CurrentSongLoops();

        public static string CurrentCustomSongName => G.Sys.AudioManager_.CurrentCustomSong_;

        public static string CurrentCustomSongPath => G.Sys.AudioManager_.CurrentCustomSongPath_;

        public static bool CustomMusicEnabled
        {
            get { return G.Sys.AudioManager_.CurrentMusicState_ == AudioManager.MusicState.CustomMusic; }
            set { G.Sys.AudioManager_.EnableCustomMusic(value); }
        }

        public static bool RepeatCustomMusic
        {
            get
            {
                var loopCustomMusicEnabled = G.Sys.AudioManager_?.GetType()
                    .GetField("loopCustomTrack_", BindingFlags.NonPublic | BindingFlags.Instance)?
                    .GetValue(G.Sys.AudioManager_);

                return loopCustomMusicEnabled != null && (bool)loopCustomMusicEnabled;
            }
            set { G.Sys.AudioManager_.SetLoopCustomTrack(value); }
        }

        public static bool ShuffleCustomMusic
        {
            get
            {
                var shuffleEnabled = G.Sys.AudioManager_?.GetType()
                    .GetField("randomizeTracks_", BindingFlags.NonPublic | BindingFlags.Instance)?
                    .GetValue(G.Sys.AudioManager_);

                return shuffleEnabled != null && (bool)shuffleEnabled;
            }
            set { G.Sys.AudioManager_.SetRandomizeTracks(value); }
        }

        public static bool HighPassFilterEnabled
        {
            set { G.Sys.AudioManager_.SetQuarantineFilter(value); }
        }

        public static bool LowPassFilterEnabled
        {
            set { G.Sys.AudioManager_.ExplodeLowPass(value); }
        }

        // All volume meters are values from 0.0 to 1.0 (0% - 100%)
        public static float AmbientVolume
        {
            get { return G.Sys.AudioManager_.GetAmbientVolume(); }
            set { G.Sys.AudioManager_.SetAmbientVolume(value); }
        }

        public static float MasterVolume
        {
            get { return G.Sys.AudioManager_.GetMasterVolume(); }
            set { G.Sys.AudioManager_.SetMasterVolume(value); }
        }

        public static float MenuVolume
        {
            get { return G.Sys.AudioManager_.GetMenuVolume(); }
            set { G.Sys.AudioManager_.SetMenuVolume(value); }
        }

        public static float MusicVolume
        {
            get { return G.Sys.AudioManager_.GetMusicVolume(); }
            set { G.Sys.AudioManager_.SetMusicVolume(value); }
        }

        public static float ObstacleVolume
        {
            get { return G.Sys.AudioManager_.GetObstacleVolume(); }
            set { G.Sys.AudioManager_.SetObstacleVolume(value); }
        }

        public static float VehicleVolume
        {
            get { return G.Sys.AudioManager_.GetCarVolume(); }
            set { G.Sys.AudioManager_.SetCarVolume(value); }
        }

        public static string CustomMusicDirectory
        {
            get { return G.Sys.AudioManager_.CurrentCustomMusicDirectory_; }
            set { G.Sys.AudioManager_.SetCustomMusicDirectory(value); }
        }

        public static void NextCustomSong()
        {
            G.Sys.AudioManager_.IncrementCustomMusic(1);
        }

        public static void PreviousCustomSong()
        {
            G.Sys.AudioManager_.IncrementCustomMusic(-1);
        }

        public static void PlayCustomMusic()
        {
            G.Sys.AudioManager_.PlayCustomMusic();
        }

        public static void PlayCustomMusic(string path)
        {
            G.Sys.AudioManager_.PlayCustomMusic(path);
        }

        public static void PauseAllNonMusicAudio()
        {
            G.Sys.AudioManager_.PauseAudio(true);
        }

        public static void ResumeAllNonMusicAudio()
        {
            G.Sys.AudioManager_.PauseAudio(false);
        }

        public static void StopCustomMusic(bool fadeOut)
        {
            G.Sys.AudioManager_.StopAllMusic(!fadeOut);
        }
    }
}
