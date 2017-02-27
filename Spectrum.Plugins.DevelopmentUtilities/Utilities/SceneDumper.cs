using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Spectrum.API;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Spectrum.Plugins.DevelopmentUtilities.Utilities
{
    internal class SceneDumper
    {
        private FileSystem FileSystem { get; }

        internal SceneDumper(FileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        public void DumpCurrentScene(bool detailed)
        {
            var currentSceneName = SceneManager.GetActiveScene().name;
            var gameObjects = Object.FindObjectsOfType<GameObject>();

            var validGameObjects = new List<GameObject>();
            foreach (var gameObject in gameObjects)
            {
                if (gameObject.transform.parent == null)
                    validGameObjects.Add(gameObject);
            }

            using (var sw = new StreamWriter(FileSystem.CreateFile(currentSceneName, true)))
            {
                foreach (var gameObject in validGameObjects)
                {
                    DumpGameObject(gameObject, sw, "", detailed);
                }
            }
        }

        private void DumpGameObject(GameObject gameObject, StreamWriter writer, string indent, bool detailed)
        {
            writer.WriteLine($"{indent}+{gameObject.name}");

            foreach (var component in gameObject.GetComponents<Component>())
            {
                DumpComponent(component, writer, indent + "  ", detailed);
            }

            foreach (Transform child in gameObject.transform)
            {
                DumpGameObject(child.gameObject, writer, indent + "  ", detailed);
            }
        }

        private void DumpComponent(Component component, StreamWriter writer, string indent, bool detailed)
        {
            writer.WriteLine($"{indent}{component?.GetType().Name ?? "(null)"}");

            if(detailed)
                DumpFields(component, writer, indent + "  ");
        }

        private void DumpFields(Component component, StreamWriter writer, string indent)
        {
            var publicFields = component.GetType().GetFields();
            var nonPublicFields = component.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var field in nonPublicFields)
            {
                writer.WriteLine($"{indent}{field.Name} = {field.GetValue(component)}");
            }

            foreach (var field in publicFields)
            {
                writer.WriteLine($"{indent}{field.Name} = {field.GetValue(component)}");
            }
        }
    }
}
