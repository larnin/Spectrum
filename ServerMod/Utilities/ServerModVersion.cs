using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.Utilities
{
    public class ServerModParseException : Exception
    {
        public ServerModParseException(string message) : base(message) { }
    }
    public class ServerModVersion
    {
        public static ServerModVersion Empty = new ServerModVersion();

        public bool empty
        {
            get
            {
                return string.IsNullOrEmpty(forkCode)
                    && major == 0
                    && minor == 0
                    && patch == 0;
            }
        }
        public readonly string forkCode;
        public readonly int major;
        public readonly int minor;
        public readonly int patch;

        public ServerModVersion()
        {
            forkCode = "";
            major = 0;
            minor = 0;
            patch = 0;
        }

        public ServerModVersion(string forkCode, int major, int minor, int patch)
        {
            this.forkCode = forkCode;
            this.major = major;
            this.minor = minor;
            this.patch = patch;
        }
        public ServerModVersion(string versionString)
        {
            var match = Regex.Match(versionString, @"(.+)\.(\d+)\.(\d+)\.(\d+)");
            if (!match.Success)
                throw new ServerModParseException("Bad version string format");
            forkCode = match.Groups[1].Value;
            if (!int.TryParse(match.Groups[2].Value, out major))
                throw new ServerModParseException("Bad int format for major");
            if (!int.TryParse(match.Groups[3].Value, out minor))
                throw new ServerModParseException("Bad int format for minor");
            if (!int.TryParse(match.Groups[4].Value, out patch))
                throw new ServerModParseException("Bad int format for patch");
        }

        public static bool TryParse(string input, out ServerModVersion version)
        {
            try
            {
                version = new ServerModVersion(input);
                return true;
            }
            catch(ServerModParseException)
            {
                version = ServerModVersion.Empty;
                return false;
            }
        }

        public override string ToString()
        {
            return $"{forkCode}.{major}.{minor}.{patch}";
        }

        public static bool operator <(ServerModVersion left, ServerModVersion right)
        {
            if (left.forkCode != right.forkCode)
                return false;
            if (left.major < right.major)
                return true;
            else if (left.major == right.major)
            {
                if (left.minor < right.minor)
                    return true;
                else if (left.minor == right.minor)
                    return left.patch < right.patch;
            }
            return false;
        }
        public static bool operator >=(ServerModVersion left, ServerModVersion right)
        {
            if (left.forkCode != right.forkCode)
                return false;
            return !(left < right);
        }
        public static bool operator >(ServerModVersion left, ServerModVersion right)
        {
            if (left.forkCode != right.forkCode)
                return false;
            if (left.major > right.major)
                return true;
            else if (left.major == right.major)
            {
                if (left.minor > right.minor)
                    return true;
                else if (left.minor == right.minor)
                    return left.patch > right.patch;
            }
            return false;
        }
        public static bool operator <=(ServerModVersion left, ServerModVersion right)
        {
            if (left.forkCode != right.forkCode)
                return false;
            return !(left > right);
        }
        public static bool operator ==(ServerModVersion left, ServerModVersion right)
        {
            return left.forkCode == right.forkCode
                && left.major == right.major
                && left.minor == right.minor
                && left.patch == right.patch;
        }
        public static bool operator !=(ServerModVersion left, ServerModVersion right)
        {
            return !(left == right);
        }
        public override bool Equals(object o)
        {
            if (!(o is ServerModVersion))
                return false;
            return (ServerModVersion)o == this;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public static int Comparison(ServerModVersion left, ServerModVersion right)
        {
            if (left.forkCode != right.forkCode)
            {
                return string.Compare(left.forkCode, right.forkCode);
            }
            return left < right ? -1 : left > right ? 1 : 0;
        }
    }
}
