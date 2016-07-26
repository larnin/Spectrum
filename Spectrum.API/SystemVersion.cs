namespace Spectrum.API
{
    public class SystemVersion
    {
        public static readonly APILevel APILevel = APILevel.InfraRed;

        public static int DistanceBuild
        {
            get { return SVNRevision.number_; }
            set { SVNRevision.number_ = value; }
        }
    }
}
