using System;
using System.Xml.Serialization;

namespace Octopus.Tentacle.Configuration
{
    public class XmlSetting
    {
        [XmlAttribute("key")]
        public string Key { get; set; } = string.Empty;

        [XmlText]
        public string? Value { get; set; }
    }
}