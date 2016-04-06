using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spectrum.API
{
    internal static class Utilities
    {
        internal static GameObject FindLocalCar()
        {
            return GameObject.Find("LocalCar");
        }

        internal static CarScreenLogic FindLocalVehicleScreen()
        {
            foreach (var gameObject in Object.FindObjectsOfType<GameObject>())
            {
                if (gameObject.name == "CarScreenGroup")
                {
                    var screenComponent = gameObject.GetComponent<CarScreenLogic>();
                    if (screenComponent.CarLogic_.IsLocalCar_)
                    {
                        return screenComponent;
                    }
                    Console.WriteLine("API: Found CarScreenGroup but it is not local.");
                }
            }
            return null;
        }

        internal static T GetPrivate<T>(object o, string fieldname)
        {
            var field = o.GetType().GetField(fieldname, BindingFlags.Instance | BindingFlags.NonPublic);
            return (T) field?.GetValue(o);
        }

        internal static void SetPrivate<T>(object o, string fieldname, T value)
        {
            var field = o.GetType().GetField(fieldname, BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(o, value);
        }
    }
}
