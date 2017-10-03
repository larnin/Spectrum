﻿using System;
using System.Collections.Generic;
using Spectrum.API.Exceptions;

namespace Spectrum.API.Configuration
{
    public class Section : Dictionary<string, object>
    {
        public new object this[string key]
        {
            get
            {
                if (!ContainsKey(key))
                    return null;

                return base[key];
            }

            set
            {
                if (!ContainsKey(key))
                    Add(key, value);
                else
                    base[key] = value;
            }
        }
        public T GetItem<T>(string key)
        {
            if (!ContainsKey(key))
            {
                throw new SettingsException("The value you want to retrieve does not exist.", key);
            }

            try
            {
                return (T)Convert.ChangeType(this[key], typeof(T));
            }
            catch
            {
                throw new SettingsException("Couldn't load value in requested type", key);
            }
        }

        public bool ContainsKey<T>(string key)
        {
            try
            {
                GetItem<T>(key);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
