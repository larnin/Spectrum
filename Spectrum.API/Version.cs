namespace Spectrum.API
{
    public class Version
    {
        public readonly static APILevel APILevel = APILevel.RadioWave;

        public static int DistanceBuild
        {
            get { return SVNRevision.number_; }
            set { SVNRevision.number_ = value; }
        }
    }
}
