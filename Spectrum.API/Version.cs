namespace Spectrum.API
{
    public class Version
    {
        public const int APILevel = 1;

        public static int DistanceBuild
        {
            get { return SVNRevision.number_; }
            set { SVNRevision.number_ = value; }
        }
    }
}
