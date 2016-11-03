using System;
using System.IO;
using System.Xml.Serialization;

namespace SendCorrespondenceService.Model
{
    /// <summary>
    /// We created a simple object to hold the received data.
    /// </summary>
    [XmlRoot]
    [Serializable]
    public class Payload
    {
        [XmlElement]
        public string Username { get; set; }
        [XmlElement]
        public string Password { get; set; }
        [XmlElement]
        public string ReceiverReference { get; set; }
        [XmlElement]
        public long SequenceNumber { get; set; }
        [XmlElement]
        public string Batch { get; set; }
        [XmlElement]
        public byte[] Attachments { get; set; }

        public override int GetHashCode()
        {
            string str = $"{Username}{ReceiverReference}{SequenceNumber}{Batch}";
            return str.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == ((Payload)obj).GetHashCode();
        }

        /// <summary>
        /// Generates an XML string of the Payload object and saves it to the specifid path
        /// </summary>
        /// <param name="payload">The object to serialize</param>
        /// <param name="path">The path to store the XML</param>
        public static void SerializeAsXml(Payload payload, string path)
        {
            XmlSerializer ser = new XmlSerializer(typeof(Payload));
            TextWriter writer = new StreamWriter(path);
            ser.Serialize(writer, payload);
            writer.Close();
        }

    }
}
