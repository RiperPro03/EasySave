using System.Collections.Concurrent;
using System.Globalization;
using System.Xml;
using EasySave.EasyLog.Interfaces;

namespace EasySave.EasyLog.Serialization
{
    /// <summary>
    /// Serializes log entries to XML.
    /// </summary>
    internal sealed class XmlSerializer : ILogSerializer
    {
        private static readonly ConcurrentDictionary<Type, System.Xml.Serialization.XmlSerializer> Cache =
            new ();

        private static readonly System.Xml.Serialization.XmlSerializerNamespaces Namespaces =
            new (new[]
            {
                XmlQualifiedName.Empty
            });

        /// <summary>
        /// Gets the XML file extension.
        /// </summary>
        public string FileExtension => "xml";

        /// <summary>
        /// Serializes an entry to XML.
        /// </summary>
        /// <param name="entry">The entry to serialize.</param>
        /// <returns>The serialized XML line.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entry"/> is null.</exception>
        public string Serialize(object entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            // Cache le serializer par type pour eviter des allocations repetitives.
            System.Xml.Serialization.XmlSerializer serializer = Cache.GetOrAdd(
                entry.GetType(),
                type => new System.Xml.Serialization.XmlSerializer(type));

            // Supprime la declaration XML pour ecrire des fragments dans un fichier daily.
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
