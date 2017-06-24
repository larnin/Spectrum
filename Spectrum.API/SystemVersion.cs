namespace Spectrum.API
{
    public class SystemVersion
    {
        public static readonly APILevel APILevel = APILevel.XRay;

        public static int DistanceBuild
        {
            get { return SVNRevision.number_; }
            set { SVNRevision.number_ = value; }
        }

        public static string VersionString => $"DISTANCE {DistanceBuild} (SPECTRUM [00AA77]{APILevel.ToString().ToUpperInvariant()}[-])";
    }
}
