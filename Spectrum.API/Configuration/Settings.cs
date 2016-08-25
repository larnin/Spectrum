using System;
using System.IO;
using JsonFx.Serialization;

namespace Spectrum.API.Configuration
{
    public class Settings : Section
    {
        private string FileName { get; }
        private string FilePath => Path.Combine(Defaults.SettingsDirectory, FileName);

        public Settings(Type type, string postfix = "") : base("ROOT")
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

                    Settings settings = null;
                    try
                    {
                        settings = reader.Read<Settings>(json);
                    }
                    catch
                    {
                        saveLater = true;
                    }

                    if (settings != null)
                    {
                        Values = settings.Values;
                        Sections = settings.Sections;
                    }
                }

                if (saveLater)
                {
                    Save();
                }
            }
        }

        public void Save(bool formatJson = false)
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
