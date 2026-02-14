using System.Xml.Serialization;

namespace EasySave.Core.Enums;

/// <summary>
/// Log severity level (RFC 5424).
/// </summary>
public enum LogLevel
{
    [XmlEnum("Emergency")]
    Emergency,

    [XmlEnum("Alert")]
    Alert,

    [XmlEnum("Critical")]
    Critical,

    [XmlEnum("Error")]
    Error,

    [XmlEnum("Warning")]
    Warning,

    [XmlEnum("Notice")]
    Notice,

    [XmlEnum("Info")]
    Info,

    [XmlEnum("Debug")]
    Debug
}
