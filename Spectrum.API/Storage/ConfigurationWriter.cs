using System.Xml.Linq;

namespace Spectrum.API.Storage
{
    internal class ConfigurationWriter
    {
        private XmlConfiguration XmlConfiguration { get; }
        private string FilePath { get; }

        internal ConfigurationWriter(XmlConfiguration xmlConfiguration, string filePath)
        {
            XmlConfiguration = xmlConfiguration;
            FilePath = filePath;
        }

        internal void WriteConfiguration()
        {
            var xDocument = new XDocument();
            WriteConfigurationRoot(xDocument);

            foreach (var section in XmlConfiguration.Sections.Values)
            {
                WriteSection(xDocument.Root, section);
            }

            foreach (var entry in XmlConfiguration.Entries)
            {
                WriteEntry(xDocument.Root, entry.Key, entry.Value);
            }
            xDocument.Save(FilePath);
        }

        private void WriteConfigurationRoot(XDocument xDocument)
        {
            var xElement = new XElement("Configuration");
            xElement.SetAttributeValue("Name", XmlConfiguration.Name);

            xDocument.Add(xElement);
        }

        private void WriteSection(XElement element, XmlSection section)
        {
            var xElement = new XElement("Section");
            xElement.SetAttributeValue("Name", section.Name);

            foreach (var childSection in section.Sections())
            {
                WriteSection(xElement, childSection);
            }

            foreach (var entry in section.Entries())
            {
                WriteEntry(xElement, entry.Key, entry.Value);
            }
            element.Add(xElement);
        }

        private void WriteEntry(XElement element, string key, string value)
        {
            var xElement = new XElement("Entry");
            xElement.SetAttributeValue("Key", key);
            xElement.SetAttributeValue("Value", value);

            element.Add(xElement);
        }
    }
}
