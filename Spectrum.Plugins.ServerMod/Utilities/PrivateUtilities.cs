using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Spectrum.Plugins.ServerMod.Utilities
{
    static class PrivateUtilities
    {
        public static object getPrivateField(object obj, string fieldName)
        {
            return obj
                .GetType()
                .GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
                )
                .GetValue(obj);
        }

        public static ClientLogic getClientLogic()
        {
            GameObject[] objs = UnityEngine.Object.FindObjectsOfType<GameObject>();
            foreach (GameObject tObj in objs)
            {
                ClientLogic logic = tObj.GetComponent<ClientLogic>();
                if (logic != null)
                {
                    Console.WriteLine(tObj.name);
                    return logic;
                }
            }
            return null;
        }
    }
}
