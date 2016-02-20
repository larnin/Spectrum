using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Spectrum.API.Game
{
    public class Vehicle
    {
        private static GameObject Car { get; set;  }
        private static CarLogic Logic { get; set; }
        private static CarStats Stats { get; set; }

        public static float VelocityKPH => Stats.GetKilometersPerHour();
        public static float VelocityMPH => Stats.GetMilesPerHour();

        static Vehicle()
        {
            DetectLocalCar();
        }

        private static void DetectLocalCar()
        {
            Car = GameObject.Find("LocalCar");
            Logic = Car.GetComponent<CarLogic>();
            Stats = Car.GetComponent<CarStats>();
        }
    }
}
