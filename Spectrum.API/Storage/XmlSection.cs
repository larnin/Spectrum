using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Spectrum.API.Storage
{
    public class XmlSection
    {
        private Dictionary<string, XmlSection> SectionDictionary { get; }
        private Dictionary<string, string> EntryDictionary { get; }

        public string Name { get; }

        public XmlSection(string name)
        {
            SectionDictionary = new Dictionary<string, XmlSection>();
            EntryDictionary = new Dictionary<string, string>();

            Name = name;
        }

        public void Add(string key, XmlSection section)
        {
            if (KeyExists(key))
                throw new Exception($"Section with key '{key}' already exists.");

            SectionDictionary.Add(key, section);
        }

        public void Add(string key, string entry)
        {
            if (KeyExists(key))
                throw new Exception($"Entry with key '{key}' already exists.");

            EntryDictionary.Add(key, entry);
        }

        public void RemoveSection(string key)
        {
            if (!SectionExists(key))
                throw new Exception($"No section with key '{key}' exists.");

            SectionDictionary.Remove(key);
        }

        public void RemoveEntry(string key)
        {
            if (!EntryExists(key))
                throw new Exception($"No entry with key '{key}' exists.");

            EntryDictionary.Remove(key);
        }

        public void SetEntryValue(string key, string newValue)
        {
            if (!EntryExists(key))
                throw new Exception($"No entry with key '{key}' exists.");

            EntryDictionary[key] = newValue;
        }

        public XmlSection Section(string key)
        {
            if (!SectionExists(key))
                throw new Exception($"No section with given key '{key}' exists.");

            return SectionDictionary[key];
        }

        public string Entry(string key)
        {
            if (!EntryExists(key))
                throw new Exception($"No entry with given key '{key}' exists.");

            return EntryDictionary[key];
        }

        public T Entry<T>(string key)
        {
            if (!EntryExists(key))
                throw new Exception($"No entry with given key '{key}' exists.");

            var typeConverter = TypeDescriptor.GetConverter(typeof(T));
            return (T)typeConverter.ConvertFromString(EntryDictionary[key]);
        }

        public List<KeyValuePair<string, string>> Entries()
        {
            return EntryDictionary.ToList();
        }

        public List<XmlSection> Sections()
        {
            return SectionDictionary.Values.ToList();
        } 

        public bool KeyExists(string key)
        {
            return SectionExists(key) || EntryExists(key);
        }

        public bool SectionExists(string key)
        {
            return SectionDictionary.ContainsKey(key);
        }

        public bool EntryExists(string key)
        {
            return EntryDictionary.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return EntryDictionary.GetEnumerator();
        }
    }
}
