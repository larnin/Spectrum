using System;
using System.Collections.Generic;
using Spectrum.API.Exceptions;

namespace Spectrum.API.Configuration
{
    public class Section
    {
        public Dictionary<string, Section> Sections { get; protected set; }
        public Dictionary<string, object> Values { get; protected set; }

        public string Name { get; private set; }

        public object this[string key]
        {
            get
            {
                if (Values.ContainsKey(key))
                {
                    return Values[key];
                }
                throw new SettingsException("The value you want to retrieve does not exist.", key);
            }

            set
            {
                if (Values.ContainsKey(key))
                {
                    Values[key] = value;
                }
                else
                {
                    Values.Add(key, value);
                }
            }
        }

        public Section(string name)
        {
            Name = name;

            Sections = new Dictionary<string, Section>();
            Values = new Dictionary<string, object>();
        }

        public void AddValue(string key, object value)
        {
            if (!Sections.ContainsKey(key) && !Values.ContainsKey(key))
            {
                Values.Add(key, value);
            }
            else
            {
                throw new SettingsException("A setting under the provided key already exists.", key);
            }
        }

        public void RemoveValue(string key)
        {
            if (Sections.ContainsKey(key))
            {
                throw new SettingsException("The key you want to remove is a section.", key);
            }

            if (Values.ContainsKey(key))
            {
                Values.Remove(key);
            }
        }

        public T GetValue<T>(string key)
        {
            if (Values.ContainsKey(key))
            {
                try
                {
                    return (T) Convert.ChangeType(Values[key], typeof(T));
                }
                catch
                {
                    throw new SettingsException("Couldn't load a value for provided type.", key);
                }
            }
            throw new SettingsException("The requested key does not exist.", key);
        }

        public bool ValueExists(string key)
        {
            return Values.ContainsKey(key);
        }

        public Section AddSection(string key)
        {
            if (!Values.ContainsKey(key) && !Sections.ContainsKey(key))
            {
                var sec = new Section(key);
                Sections.Add(key, sec);

                return sec;
            }
            throw new SettingsException("The key you want to use with the section already exists.", key);
        }

        public void RemoveSection(string key)
        {
            if (Sections.ContainsKey(key))
            {
                Sections.Remove(key);
            }
            else
            {
                throw new SettingsException("The section you want to remove does not exist.", key);
            }
        }

        public Section GetSection(string key)
        {
            if (Sections.ContainsKey(key))
            {
                return Sections[key];
            }
            throw new SettingsException("The section you want to retrieve does not exist.", key);
        }

        public bool SectionExists(string key)
        {
            return Sections.ContainsKey(key);
        }
    }
}
