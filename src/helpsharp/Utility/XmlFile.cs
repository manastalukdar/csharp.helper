using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using helpsharp.Security;

namespace helpsharp.Utility
{
    public class XmlFile
    {
        #region Public Methods

        public void CreateNode(XmlTextWriter writer, string element, string data)
        {
            writer.WriteStartElement(element);
            writer.WriteString(data);
            writer.WriteEndElement();
        }

        public XmlDocument DecryptXmlDocument(XmlDocument xmlDocument)
        {
            var encryptionTask = new Encryption();
            encryptionTask.DecryptXmlElement(xmlDocument);
            return xmlDocument;
        }

        public void EncryptXmlElementToFile(XmlDocument xmlDocument, string elementName, string fileNameAndPath)
        {
            var encryptionTask = new Encryption();
            encryptionTask.EncrypyXmlElement(xmlDocument, elementName);
            xmlDocument.Save(fileNameAndPath);
        }

        // DecryptXml() has to be called before this method
        public string GetNodeData(XmlTextReader reader, string element)
        {
            reader.MoveToContent();
            while (reader.Read())
            {
                if ((reader.NodeType == XmlNodeType.Element) || (reader.NodeType == XmlNodeType.Text)
                   || (reader.NodeType == XmlNodeType.EndElement))
                {
                    if (reader.Name == element)
                    {
                        var el = XNode.ReadFrom(reader) as XElement;
                        if (el != null)
                        {
                            return el.Value;
                        }
                    }
                }
            }

            return null;
        }

        public string GetNodeData2(XmlDocument xmlDocument, string element)
        {
            XmlNode xmlNode = xmlDocument.DocumentElement;
            foreach (XmlNode subNode in xmlNode.ChildNodes)
            {
                if (subNode.Name == element)
                {
                    return subNode.InnerText;
                }
            }

            return null;
        }

        public XmlTextReader GetXmlTextReader(object fileOrStream = null, bool isFile = true, bool isStream = false)
        {
            XmlTextReader reader = null;
            if (isFile && isStream)
            {
                throw new ArgumentException("Both isFile and isStream cannot be true.");
            }

            if (fileOrStream != null)
            {
                if (isFile)
                {
                    reader = new XmlTextReader((string)fileOrStream);
                }
                else if (isStream)
                {
                    reader = new XmlTextReader((Stream)fileOrStream);
                }
            }

            return reader;
        }

        public XmlTextWriter GetXmlTextWriter(object fileOrStream = null, bool isFile = true, bool isStream = false)
        {
            XmlTextWriter writer = null;
            if (isFile && isStream)
            {
                throw new ArgumentException("Both isFile and isStream cannot be true.");
            }

            if (fileOrStream != null)
            {
                if (isFile)
                {
                    writer = new XmlTextWriter((string)fileOrStream, System.Text.Encoding.UTF8);
                }
                else if (isStream)
                {
                    writer = new XmlTextWriter((Stream)fileOrStream, System.Text.Encoding.UTF8);
                }
            }

            return writer;
        }

        // DecryptXml() has to be called before this method
        public XmlDocument UpdateNodeData(XmlDocument xmlDocument, string elementName, string data)
        {
            XmlNode xmlNode = xmlDocument.DocumentElement;
            if (xmlNode != null)
            {
                foreach (XmlNode subNode in xmlNode.ChildNodes)
                {
                    if (subNode.Name == elementName)
                    {
                        subNode.InnerText = data;
                        return xmlDocument;
                    }
                }
            }

            return xmlDocument;
        }

        #endregion Public Methods
    }
}