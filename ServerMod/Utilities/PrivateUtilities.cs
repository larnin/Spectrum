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
            try
            {
                return obj
                    .GetType()
                    .GetField(
                        fieldName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
                    )
                    .GetValue(obj);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error accessing private field {fieldName}. Has it been removed?");
                Console.WriteLine($"Error: {e}");
                if (GeneralUtilities.isHost())
                {
                    MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(GeneralUtilities.localClient()));
                    MessageUtilities.sendMessage("[FF1010]ServerMod encountered an error and could not complete a task.[-]");
                    MessageUtilities.sendMessage("[FF1010]ServerMod might not work properly from this point onwards.[-]");
                    MessageUtilities.sendMessage("[FF1010]Check the console for information. You can turn on the console with the -console launch parameter.[-]");
                    MessageUtilities.sendMessage($"[FF1010]Error accessing private field {fieldName}. Has it been removed?[-]");
                    MessageUtilities.popMessageOptions();
                }
                throw e;
            }
        }
        public static void setPrivateField(object obj, string fieldName, object value)
        {
            try
            {
                obj
                    .GetType()
                    .GetField(
                        fieldName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
                    )
                    .SetValue(obj, value);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error setting private field {fieldName}. Has it been removed?");
                Console.WriteLine($"Error: {e}");
                if (GeneralUtilities.isHost())
                {
                    MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(GeneralUtilities.localClient()));
                    MessageUtilities.sendMessage("[FF1010]ServerMod encountered an error and could not complete a task.[-]");
                    MessageUtilities.sendMessage("[FF1010]ServerMod might not work properly from this point onwards.[-]");
                    MessageUtilities.sendMessage("[FF1010]Check the console for information. You can turn on the console with the -console launch parameter.[-]");
                    MessageUtilities.sendMessage($"[FF1010]Error setting private field {fieldName}. Has it been removed?[-]");
                    MessageUtilities.popMessageOptions();
                }
                throw e;
            }
        }
        public static object getPrivateProperty(object obj, string propertyName)
        {
            try
            {
                return obj
                    .GetType()
                    .GetProperty(
                        propertyName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
                    )
                    .GetGetMethod().Invoke(obj, null);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error accessing private property {propertyName}. Has it been removed?");
                Console.WriteLine($"Error: {e}");
                if (GeneralUtilities.isHost())
                {
                    MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(GeneralUtilities.localClient()));
                    MessageUtilities.sendMessage("[FF1010]ServerMod encountered an error and could not complete a task.[-]");
                    MessageUtilities.sendMessage("[FF1010]ServerMod might not work properly from this point onwards.[-]");
                    MessageUtilities.sendMessage("[FF1010]Check the console for information. You can turn on the console with the -console launch parameter.[-]");
                    MessageUtilities.sendMessage($"[FF1010]Error setting private property {propertyName}. Has it been removed?[-]");
                    MessageUtilities.popMessageOptions();
                }
                throw e;
            }
        }
        public static void setPrivateProperty(object obj, string propertyName, object value)
        {
            try
            {
                obj
                    .GetType()
                    .GetProperty(
                        propertyName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
                    )
                    .GetSetMethod().Invoke(obj, new object[] { value });
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error setting private property {propertyName}. Has it been removed?");
                Console.WriteLine($"Error: {e}");
                if (GeneralUtilities.isHost())
                {
                    MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(GeneralUtilities.localClient()));
                    MessageUtilities.sendMessage("[FF1010]ServerMod encountered an error and could not complete a task.[-]");
                    MessageUtilities.sendMessage("[FF1010]ServerMod might not work properly from this point onwards.[-]");
                    MessageUtilities.sendMessage("[FF1010]Check the console for information. You can turn on the console with the -console launch parameter.[-]");
                    MessageUtilities.sendMessage($"[FF1010]Error accessing private property {propertyName}. Has it been removed?[-]");
                    MessageUtilities.popMessageOptions();
                }
                throw e;
            }
        }

        public static object callPrivateMethod(Type tp, object obj, string methodName, params object[] args)
        {
            try
            {
                return tp.GetMethod(
                    methodName,
                    BindingFlags.NonPublic | BindingFlags.Instance
                ).Invoke(obj, args);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error calling private method {methodName}. Has it been removed?");
                Console.WriteLine($"Error: {e}");
                if (GeneralUtilities.isHost())
                {
                    MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(GeneralUtilities.localClient()));
                    MessageUtilities.sendMessage("[FF1010]ServerMod encountered an error and could not complete a task.[-]");
                    MessageUtilities.sendMessage("[FF1010]ServerMod might not work properly from this point onwards.[-]");
                    MessageUtilities.sendMessage("[FF1010]Check the console for information. You can turn on the console with the -console launch parameter.[-]");
                    MessageUtilities.sendMessage($"[FF1010]Error calling private method {methodName}. Has it been removed?[-]");
                    MessageUtilities.popMessageOptions();
                }
                throw e;
            }
        }

        public static object callPrivateMethod(object obj, string methodName, params object[] args)
        {
            try
            {
                return obj.GetType().GetMethod(
                    methodName,
                    BindingFlags.NonPublic | BindingFlags.Instance
                ).Invoke(obj, args);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error calling private method {methodName}. Has it been removed?");
                Console.WriteLine($"Error: {e}");
                if (GeneralUtilities.isHost())
                {
                    MessageUtilities.pushMessageOption(new MessageStateOptionPlayer(GeneralUtilities.localClient()));
                    MessageUtilities.sendMessage("[FF1010]ServerMod encountered an error and could not complete a task.[-]");
                    MessageUtilities.sendMessage("[FF1010]ServerMod might not work properly from this point onwards.[-]");
                    MessageUtilities.sendMessage("[FF1010]Check the console for information. You can turn on the console with the -console launch parameter.[-]");
                    MessageUtilities.sendMessage($"[FF1010]Error calling private method {methodName}. Has it been removed?[-]");
                    MessageUtilities.popMessageOptions();
                }
                throw e;
            }
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
