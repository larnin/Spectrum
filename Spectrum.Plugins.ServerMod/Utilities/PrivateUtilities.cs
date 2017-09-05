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

        public static object callPrivateMethod(Type tp, object obj, string methodName, params object[] args)
        {
            return tp.GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Instance
            ).Invoke(obj, args);
        }

        public static object callPrivateMethod(object obj, string methodName, params object[] args)
        {
            return obj.GetType().GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Instance
            ).Invoke(obj, args);
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

        public static List<T> getComponents<T>() where T : MonoBehaviour
        {
            List<T> results = new List<T>();
            GameObject[] objs = UnityEngine.Object.FindObjectsOfType<GameObject>();
            foreach (GameObject tObj in objs)
            {
                T component = tObj.GetComponent<T>();
                if (component != null)
                    results.Add(component);
            }
            return results;
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

        public static List<StaticEvent<T>.Delegate> removeParticularSubscribers<T, T2>(List<T2> components) where T2 : MonoBehaviour
        {
            var results = new List<StaticEvent<T>.Delegate>();
            foreach (var component in components)
            {
                var result = removeParticularSubscriber<T>(component);
                if (result != null)
                    results.Add(result);
            }
            return results;
        }
    }
}
