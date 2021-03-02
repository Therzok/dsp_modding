using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace SaveTheWindows
{
    static class XmlWriterExtensions
    {
        public static void WriteAttributeString(this XmlWriter writer, string key, float value)
        {
            writer.WriteAttributeString(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public static void WriteAttributeString(this XmlWriter writer, string key, int value)
        {
            writer.WriteAttributeString(key, value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
