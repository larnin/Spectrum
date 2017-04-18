using System;
using System.IO;
using JsonFx.Serialization;

namespace Spectrum.API.Configuration
{
    public class Settings : Section
    {
        private string FileName { get; }
        private string FilePath => Path.Combine(Defaults.SettingsDirectory, FileName);

        public Settings(Type type, string postfix = "")
        {
            if (string.IsNullOrEmpty(postfix))
            {
                FileName = $"{type.Assembly.GetName().Name}.json";
            }
            else
            {
                FileName = $"{type.Assembly.GetName().Name}.{postfix}.json";
            }

            if (File.Exists(FilePath))
            {
                var saveLater = false;
                using (var sr = new StreamReader(FilePath))
                {
                    var json = sr.ReadToEnd();
                    var reader = new JsonFx.Json.JsonReader();

                    Section sec = null;

                    try
                    {
                        sec = reader.Read<Section>(json);
                    }
                    catch
                    {
                        saveLater = true;
                    }

                    if (sec != null)
                    {
                        foreach (string k in sec.Keys)
                        {
                            Add(k, sec[k]);
                        }
                    }
                }

                if (saveLater)
                {
                    Save();
                }
            }
        }

        public void Save(bool formatJson = true)
        {
            DataWriterSettings st = new DataWriterSettings { PrettyPrint = formatJson };
            var writer = new JsonFx.Json.JsonWriter(st);

            using (var sw = new StreamWriter(FilePath, false))
            {
                sw.WriteLine(writer.Write(this));
            }
        }
    }
}
