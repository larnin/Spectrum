using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spectrum.API.TypeWrappers
{
    public class Position
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Position(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Position(float x, float y) : this(x, y, 0) { }

        public float DistanceTo(Position position)
        {
            var xPow = Math.Pow(position.X - X, 2);
            var yPow = Math.Pow(position.Y - Y, 2);
            var zPow = Math.Pow(position.Z - Z, 2);

            var distance = Math.Sqrt(xPow + yPow + zPow);

            return (float)distance;
        }

        public static Position GetNearest(Position position, string name)
        {
            var gameObjects = Object.FindObjectsOfType<GameObject>();
            var validGameObjects = new List<GameObject>();

            foreach (var gameObject in gameObjects)
            {
                if (gameObject.name == name)
                    validGameObjects.Add(gameObject);
            }

            var nearestObjectDistance = float.MaxValue;
            var nearestObjectPosition = new Position(float.MaxValue, float.MaxValue, float.MaxValue);
            foreach (var validGameObject in validGameObjects)
            {
                var gameObjectPosition = new Position(validGameObject.transform.position.x, validGameObject.transform.position.y, validGameObject.transform.position.z);
                var distance = position.DistanceTo(gameObjectPosition);

                if (distance < nearestObjectDistance)
                {
                    nearestObjectDistance = distance;
                    nearestObjectPosition = gameObjectPosition;
                }
            }
            return nearestObjectPosition;
        }

        public static float GetNearestDistance(Position position, string name)
        {
            var nearestPosition = GetNearest(position, name);
            return position.DistanceTo(nearestPosition);
        }

        public static object GetNearestGameObjectOfName(Position position, string name)
        {
            var validObjects = new List<GameObject>();

            foreach (var gameObject in Object.FindObjectsOfType<GameObject>())
            {
                if (gameObject.name == name)
                {
                    validObjects.Add(gameObject);
                }
            }

            var distance = float.MaxValue;
            GameObject returnObject = null;
            foreach (var gameObject in validObjects)
            {
                var currentObjectDistance = GetNearestDistance(position, name);
                if (currentObjectDistance < distance)
                {
                    distance = currentObjectDistance;
                    returnObject = gameObject;
                }
            }

            return returnObject;
        }
    }
}
