using System;

namespace Spectrum.API.TypeWrappers
{
    public class Rotation
    {
        public float Roll { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }

        public Rotation(float roll, float pitch, float yaw)
        {
            Roll = roll;
            Pitch = pitch;
            Yaw = yaw;
        }

        public Rotation ToRadians()
        {
            return new Rotation(Roll * (180 / (float)Math.PI), Pitch * (180 / (float)Math.PI), Yaw * (180 / (float)Math.PI));
        }

        public Rotation ToDegrees()
        {
            return new Rotation(Roll * ((float)Math.PI / 180), Pitch * ((float)Math.PI / 180), Yaw * ((float)Math.PI / 180));
        }
    }
}
