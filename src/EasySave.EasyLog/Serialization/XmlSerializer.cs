using System.Collections.Concurrent;
using System.Globalization;
using System.Xml;
using EasySave.EasyLog.Interfaces;

namespace EasySave.EasyLog.Serialization
{
    internal sealed class XmlSerializer : ILogSerializer
    {
        private static readonly ConcurrentDictionary<Type, System.Xml.Serialization.XmlSerializer> Cache =
            new ();

        private static readonly System.Xml.Serialization.XmlSerializerNamespaces Namespaces =
            new (new[]
            {
                XmlQualifiedName.Empty
            });

        public string FileExtension => "xml";

        public string Serialize(object entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            System.Xml.Serialization.XmlSerializer serializer = Cache.GetOrAdd(
                entry.GetType(),
                type => new System.Xml.Serialization.XmlSerializer(type));

            XmlWriterSettings settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = false,
                NewLineHandling = NewLineHandling.None
            };

            using StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
            using XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings);
            serializer.Serialize(xmlWriter, entry, Namespaces);

            return stringWriter.ToString() + Environment.NewLine;
        }
    }
}
