using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SendCorrespondenceService.Utils
{
    /// <summary>
    /// Utils for handling XML
    /// </summary>
    public static class XmlUtils
    {
        /// <summary>
        /// Creating XmlSerializer object 
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <returns>an object of type T</returns>
        public static XmlSerializer GetXmlSerializerOfType<T>()
        {
            var serializer = new XmlSerializer(typeof(T));

            return serializer;
        }

        /// <summary>
        /// Deserializing an XMLstring
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="serializer">The XmlSerializer object</param>
        /// <param name="batch">batch as string</param>
        /// <returns>T</returns>
        public static T DeserializeXmlString<T>(XmlSerializer serializer, string data)
        {
            T result;

            using (TextReader reader = new StringReader(data))
            {
                result = (T) serializer.Deserialize(reader);
            }

            return result;
        }

        public static T DeserializeXmlString<T>(XmlSerializer serializer, XElement data)
        {
            T result;

            using (TextReader reader = new StringReader(data.FirstNode.ToString()))
            {
                result = (T)serializer.Deserialize(reader);
            }

            return result;
        }
    }
}
