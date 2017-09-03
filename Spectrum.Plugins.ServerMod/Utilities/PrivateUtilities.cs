using Events;
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

        public static T getComponent<T>() where T : MonoBehaviour
        {
            GameObject[] objs = UnityEngine.Object.FindObjectsOfType<GameObject>();
            foreach (GameObject tObj in objs)
            {
                T component = tObj.GetComponent<T>();
                if (component != null)
                    return component;
            }
            return null;
        }

        public static StaticEvent<T>.Delegate removeParticularSubscriber<T>(MonoBehaviour component)
        {
            SubscriberList list = (SubscriberList)PrivateUtilities.getPrivateField(component, "subscriberList_");
            StaticEvent<T>.Delegate func = null;
            var index = 0;
            foreach (var subscriber in list)
            {
                if (subscriber is StaticEvent<T>.Subscriber)
                {
                    func = (StaticEvent<T>.Delegate)PrivateUtilities.getPrivateField(subscriber, "func_");
                    subscriber.Unsubscribe();
                    break;
                }
                index++;
            }
            if (func != null)
            {
                list.RemoveAt(index);
            }
            return func;
        }
    }
}
