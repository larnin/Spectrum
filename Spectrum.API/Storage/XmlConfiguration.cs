using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace Spectrum.API.Storage
{
    public class XmlConfiguration
    {
        internal Dictionary<string, XmlSection> Sections { get; }
        internal Dictionary<string, string> Entries { get; }
        private XDocument Document { get; set; }
        private string FilePath { get; set; }

        public string Name { get; private set; }

        private XmlConfiguration()
        {
            Sections = new Dictionary<string, XmlSection>();
            Entries = new Dictionary<string, string>();
        }

        public XmlSection Section(string name)
        {
            if (!Sections.ContainsKey(name))
                throw new Exception($"No section with specified name '{name}' exists in root section.");

            return Sections[name];
        }

        public string Entry(string key)
        {
            if (!Entries.ContainsKey(key))
                throw new Exception($"No entry with sepcified '{key}' exists in root section.");

            return Entries[key];
        }

        public T Entry<T>(string key)
        {
            if (!Entries.ContainsKey(key))
                throw new Exception($"No entry with given key '{key}' exists.");

            var typeConverter = TypeDescriptor.GetConverter(typeof(T));
            return (T)typeConverter.ConvertFromString(Entries[key]);
        }

        public void Add(string name)
        {
            if (Sections.ContainsKey(name))
                throw new Exception($"Section with name '{name}' already exists in root section.");

            Sections.Add(name, new XmlSection(name));
        }

        public void Add(string key, string value)
        {
            if (Entries.ContainsKey(key))
                throw new Exception($"Entry with key '{key}' already exists.");

            Entries.Add(key, value);
        }

        public void RemoveSection(string name)
        {
            if(!Sections.ContainsKey(name))
                throw new Exception($"Section with name '{name}' does not exist in root section.");

            Sections.Remove(name);
        }

        public void RemoveEntry(string key)
        {
            if (!Entries.ContainsKey(key))
                throw new Exception($"No entry with key '{key}' exists.");

            Entries.Remove(key);
        }

        public void SetEntryValue(string key, string newValue)
        {
            if (!Entries.ContainsKey(key))
                throw new Exception($"No entry with key '{key}' exists.");

            Entries[key] = newValue;
        }

        public void Save()
        {
            var writer = new ConfigurationWriter(this, FilePath);
            writer.WriteConfiguration();
        }

        public static XmlConfiguration AppData(string fileName)
        {
            var cfg = new XmlConfiguration();

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var asmName = Assembly.GetEntryAssembly().GetName().Name;
            var combined = Path.Combine(appData, asmName);

            if (!Directory.Exists(combined))
                Directory.CreateDirectory(combined);

            cfg.FilePath = Path.Combine(combined, fileName);
            if (!File.Exists(cfg.FilePath))
                File.Create(cfg.FilePath).Dispose();

            cfg.Document = XDocument.Load(cfg.FilePath);
            cfg.Parse();

            return cfg;
        }

        public static XmlConfiguration AbsolutePath(string filePath)
        {
            var cfg = new XmlConfiguration
            {
                FilePath = filePath
            };
            cfg.Document = XDocument.Load(cfg.FilePath);
            cfg.Parse();

            return cfg;
        }

        private void Parse()
        {
            if (Document.Root == null)
                throw new Exception("Root element does not exist.");

            if (Document.Root.Name != "Configuration")
                throw new Exception("Root element not of Configuration name.");

            if (Document.Root.Attribute("Name") == null)
                throw new Exception("Configuration element has no name.");

            Name = Document.Root.Attribute("Name").Value;

            foreach (var element in Document.Root.Elements())
            {
                if (element.Name == "Section")
                {
                    if (element.Attribute("Name") == null)
                        throw new Exception("Section element without name.");

                    if (Sections.ContainsKey(element.Attribute("Name").Value))
                        throw new Exception($"Section with name '{element.Attribute("Name").Value}' already exists in root section.");

                    var xmlSection = new XmlSection(element.Attribute("Name").Value);
                    ParseSection(element, xmlSection);

                    Sections.Add(xmlSection.Name, xmlSection);
                }
                else if (element.Name == "Entry")
                {
                    if (element.Attribute("Key") == null)
                        throw new Exception("Entry element without key.");

                    if (element.Attribute("Value") == null)
                        throw new Exception("Entry element without value.");

                    if (Entries.ContainsKey(element.Attribute("Key").Value))
                        throw new Exception($"Entry with key '{element.Attribute("Key").Value}' already exists in root section.");

                    Entries.Add(element.Attribute("Key").Value, element.Attribute("Value").Value);
                }
                else
                {
                    throw new Exception($"Unexpected element '{element.Name}'.");
                }
            }
        }

        private static void ParseSection(XElement xElement, XmlSection xmlSection)
        {
            foreach (var element in xElement.Elements())
            {
                if (element.Name == "Section")
                {
                    if (element.Attribute("Name") == null)
                        throw new Exception("Section element without name.");

                    if (xmlSection.SectionExists(element.Attribute("Name").Value))
                        throw new Exception($"Section with key '{element.Attribute("Name").Value}' already exists in '{xmlSection.Name}' section.");

                    var newSection = new XmlSection(element.Attribute("Name").Value);
                    ParseSection(element, newSection);

                    xmlSection.Add(newSection.Name, newSection);
                    continue;
                }

                if (element.Name == "Entry")
                {
                    if (element.Attribute("Key") == null)
                        throw new Exception("Entry without a key.");

                    if (element.Attribute("Value") == null)
                        throw new Exception("Entry without a value.");

                    if (xmlSection.EntryExists(element.Attribute("Key").Value))
                        throw new Exception($"Entry with key '{element.Attribute("Key").Value}' already exists in '{xmlSection.Name}' section.");

                    xmlSection.Add(element.Attribute("Key").Value, element.Attribute("Value").Value);
                    continue;
                }
                throw new Exception($"Unexpected element {element.Name}");
            }
        }
    }
}
