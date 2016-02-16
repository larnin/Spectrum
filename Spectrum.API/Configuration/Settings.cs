﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Spectrum.API.Configuration
{
    public class Settings
    {
        private string FilePath => Path.Combine(Defaults.SettingsDirectory, FileName);
        private Dictionary<string, string> Entries { get; set; }

        public string FileName { get; }
        public int SettingsVersion { get; private set; }

        public string this[string key]
        {
            get
            {
                if (key.StartsWith("#"))
                    return string.Empty;

                if (Entries.ContainsKey(key))
                {
                    return Entries[key];
                }
                return string.Empty;
            }
            set
            {
                if (key.StartsWith("#"))
                    return;

                if (Entries.ContainsKey(key))
                {
                    Entries[key] = value;
                }
                else
                {
                    Entries.Add(key, value);
                }
            }
        }

        public Settings(Type type)
        {
            FileName = $"{type.Assembly.GetName().Name}.scx";

            if (!SettingsExist())
            {
                CreateSettings();
                WriteSettingsFileMarker();
            }
            Parse();
        }

        public T GetValue<T>(string key)
        {
            try
            {
                return (T)Convert.ChangeType(Entries[key], typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        public void Save()
        {
            try
            {
                File.Delete(FilePath);

                using (var sw = new StreamWriter(FilePath))
                {
                    foreach (var pair in Entries)
                    {
                        if (pair.Key.StartsWith("#"))
                        {
                            sw.WriteLine(pair.Key);
                            continue;
                        }
                        sw.WriteLine($"{pair.Key} = {pair.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] <private> Settings.Save(): exception occured:\n{ex}");
            }
        }

        private void CreateSettings()
        {
            try
            {
                if (!Directory.Exists(Defaults.SettingsDirectory))
                {
                    Directory.CreateDirectory(Defaults.SettingsDirectory);
                }
                File.Create(FilePath).Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] <private> Settings.CreateSettings(): exception occured:\n{ex}");
            }
        }

        private bool SettingsExist()
        {
            return File.Exists(FilePath);
        }

        private void WriteSettingsFileMarker()
        {
            try
            {
                using (var sw = new StreamWriter(FilePath))
                {
                    sw.WriteLine($"#!SpectrumConfig Version {Version.APILevel}");
                    sw.WriteLine("# Autogenerated by Spectrum. Change it only if you know what you're doing.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] <private> WriteSettingsFileMarker(): exception occured:\n{ex}");
            }
        }

        private void Parse()
        {
            Entries = new Dictionary<string, string>();

            string fileContents;
            using (var sr = new StreamReader(FilePath))
            {
                fileContents = sr.ReadToEnd();
            }

            if (fileContents == string.Empty)
                return;

            var individualLines = fileContents.Split('\n');

            try
            {
                foreach (var line in individualLines)
                {
                    if (string.IsNullOrEmpty(line.Trim()))
                        continue;

                    if (line.StartsWith("#!"))
                    {
                        SettingsVersion = GetSettingsVersionNumber(line);
                        Entries.Add(line.Trim(), string.Empty);
                        continue;
                    }
                    if (line.StartsWith("#"))
                    {
                        Entries.Add(line.Trim(), string.Empty);
                        continue;
                    }

                    // Can't use LINQ here. Mono Runtime throws exceptions.
                    if (line.Contains("="))
                    {
                        var chunks = line.Split('=');
                        var key = chunks[0].Trim();
                        var value = string.Empty;

                        if (!line.EndsWith("="))
                        {
                            var valueChunks = new List<string>();
                            if (chunks.Length > 1)
                            {
                                foreach (var c in chunks)
                                {
                                    valueChunks.Add(c);
                                }
                                valueChunks.RemoveAt(0);

                                foreach (var s in valueChunks)
                                {
                                    value += s.Trim();
                                }
                            }
                        }
                        Entries.Add(key, value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] <private> Settings.Parse(): exception occured:\n{ex}");
            }
        }

        private static int GetSettingsVersionNumber(string fileMarker)
        {
            var identifierRemoved = fileMarker.Substring(2, fileMarker.Length - 2);
            var individualKeywords = identifierRemoved.Split(' ');

            if (individualKeywords.Length != 3)
            {
                return -1;
            }

            if (individualKeywords[0] != "SpectrumConfig")
            {
                return -1;
            }

            if (individualKeywords[1] != "Version")
            {
                return -1;
            }

            int version;
            if (!int.TryParse(individualKeywords[2], out version))
            {
                return -1;
            }

            return version;
        }
    }
}
