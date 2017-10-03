using System.Reflection;
using UnityEngine;

namespace Spectrum.API.Utilities
{
    internal static class Utilities
    {
        internal static GameObject FindLocalCar()
        {
            return G.Sys.PlayerManager_?.Current_?.playerData_?.Car_;
        }

        internal static CarLogic FindLocalCarLogic()
        {
            return G.Sys.PlayerManager_?.Current_?.playerData_?.CarLogic_;
        }

        internal static CarScreenLogic FindLocalVehicleScreen()
        {
            var carScreenLogic = G.Sys.PlayerManager_?.Current_?.playerData_?.CarScreenLogic_;
            if (carScreenLogic?.CarLogic_.IsLocalCar_ ?? false)
            {
                return carScreenLogic;
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
