namespace Spectrum.API
{
    public class Version
    {
        public const int APILevel = 1;
        public static int DistanceBuild => SVNRevision.number_;
    }
}
