using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using helpsharp.Utility;

namespace helpsharp.Security
{
    // Some of this code is from http://msdn.microsoft.com/en-us/library/sb7w85t6.aspx
    public class Encryption
    {
        #region Private Fields

        private static readonly string Password = "9^#yc&g@";
        private static readonly string Salt = "Salt01234";
        private static byte[] _key;
        private static RijndaelManaged _rijndaelManaged;

        #endregion Private Fields

        #region Public Constructors

        public Encryption()
        {
            _rijndaelManaged = new RijndaelManaged();
            _rijndaelManaged.BlockSize = 128;
            _rijndaelManaged.KeySize = 256;
            _rijndaelManaged.Padding = PaddingMode.Zeros;

            var utfe = new UTF8Encoding();
            _key = utfe.GetBytes(Password);
            ////var ue = new UnicodeEncoding();
            ////_key = ue.GetBytes(_password);

            var passwordBytes = Encoding.UTF8.GetBytes(Password); // password here
            var saltBytes = Encoding.UTF8.GetBytes(Salt); // salt here (another string)
            var p = new Rfc2898DeriveBytes(Password, saltBytes);

            // sizes are devided by 8 because [ 1 byte = 8 bits ]
            _rijndaelManaged.IV = p.GetBytes(_rijndaelManaged.BlockSize / 8);
            _rijndaelManaged.Key = p.GetBytes(_rijndaelManaged.KeySize / 8);
        }

        #endregion Public Constructors

        #region Private Destructors

        ~Encryption()
        {
            // Clear the key.
            if (_rijndaelManaged != null)
            {
                _rijndaelManaged.Clear();
            }
        }

        #endregion Private Destructors

        #region Public Methods

        /// <summary>
        /// Decrypts a file using Rijndael algorithm.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        public static void DecryptFile(string inputFile, string outputFile)
        {
            var fileStreamCrypt = new FileStream(inputFile, FileMode.Open);

            var rijndaelManagedCrypto = new RijndaelManaged();

            var cs = new CryptoStream(fileStreamCrypt,
                                      rijndaelManagedCrypto.CreateDecryptor(_key, _key),
                                      CryptoStreamMode.Read);

            var fileStreamOut = new FileStream(outputFile, FileMode.Create);

            int data;
            while ((data = cs.ReadByte()) != -1)
            {
                fileStreamOut.WriteByte((byte)data);
            }

            fileStreamOut.Close();
            cs.Close();
            fileStreamCrypt.Close();
        }

        /// <summary>
        /// Encrypts a file using Rijndael algorithm.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        public static void EncryptFile(string inputFile, string outputFile)
        {
            var cryptFile = outputFile;
            var fileStreamCrypt = new FileStream(cryptFile, FileMode.Create);

            var rijndaelManagedCrypto = new RijndaelManaged();

            var cs = new CryptoStream(fileStreamCrypt,
                rijndaelManagedCrypto.CreateEncryptor(_key, _key),
                CryptoStreamMode.Write);

            var fileStreamIn = new FileStream(inputFile, FileMode.Open);

            int data;
            while ((data = fileStreamIn.ReadByte()) != -1)
            {
                cs.WriteByte((byte)data);
                cs.FlushFinalBlock();
            }

            fileStreamIn.Close();
            cs.Close();
            fileStreamCrypt.Close();
        }

        public void DecryptXmlElement(XmlDocument doc)
        {
            // Check the arguments.
            if (doc == null)
            {
                throw new ArgumentNullException("Doc");
            }

            SymmetricAlgorithm algorithm = _rijndaelManaged;
            algorithm.Padding = PaddingMode.Zeros;

            // Find the EncryptedData element in the XmlDocument.
            var encryptedElement = doc.GetElementsByTagName("EncryptedData")[0] as XmlElement;

            // If the EncryptedData element was not found, throw an exception.
            if (encryptedElement == null)
            {
                throw new XmlException("The EncryptedData element was not found.");
            }

            // Create an EncryptedData object and populate it.
            var element = new EncryptedData();
            element.LoadXml(encryptedElement);

            // Create a new EncryptedXml object.
            var exml = new EncryptedXml();
            exml.Padding = PaddingMode.Zeros;

            // Decrypt the element using the symmetric key.
            var rgbOutput = exml.DecryptData(element, algorithm);

            ////string rgbOutputTemp = Convert.ToBase64String(rgbOutput);
            ////byte[] rgbOutputSanitized = Convert.FromBase64String(rgbOutputTemp);
            var miscTask = new Miscellaneous();
            var rgbOutputNullRemoved = miscTask.NullRemover(rgbOutput);

            // Replace the encryptedData element with the plaintext XML element.
            if (rgbOutput[rgbOutput.Length - 1] == 0)
            {
                exml.ReplaceData(encryptedElement, rgbOutputNullRemoved);
            }
            else
            {
                if (rgbOutput.Length - rgbOutputNullRemoved.Length == 1)
                {
                    exml.ReplaceData(encryptedElement, rgbOutput);
                }
                else
                {
                    exml.ReplaceData(encryptedElement, rgbOutputNullRemoved);
                }
            }
        }

        public void EncrypyXmlElement(XmlDocument doc, string elementName)
        {
            // Check the arguments.
            if (doc == null)
            {
                throw new ArgumentNullException("Doc");
            }

            if (elementName == null)
            {
                throw new ArgumentNullException("ElementToEncrypt");
            }

            SymmetricAlgorithm algorithm = _rijndaelManaged;
            algorithm.Padding = PaddingMode.Zeros;

            ////////////////////////////////////////////////
            // Find the specified element in the XmlDocument
            // object and create a new XmlElemnt object.
            ////////////////////////////////////////////////
            var elementToEncrypt = doc.GetElementsByTagName(elementName)[0] as XmlElement;

            // Throw an XmlException if the element was not found.
            if (elementToEncrypt == null)
            {
                throw new XmlException("The specified element was not found");
            }

            //////////////////////////////////////////////////
            // Create a new instance of the EncryptedXml class
            // and use it to encrypt the XmlElement with the
            // symmetric key.
            //////////////////////////////////////////////////

            var encryptedXml = new EncryptedXml();
            encryptedXml.Padding = PaddingMode.Zeros;

            var encryptedElement = encryptedXml.EncryptData(elementToEncrypt, algorithm, false);
            ////////////////////////////////////////////////
            // Construct an EncryptedData object and populate
            // it with the desired encryption information.
            ////////////////////////////////////////////////

            var element = new EncryptedData();
            element.Type = EncryptedXml.XmlEncElementUrl;

            // Create an EncryptionMethod element so that the receiver knows which algorithm to use
            // for decryption. Determine what kind of algorithm is being used and supply the
            // appropriate URL to the EncryptionMethod element.
            string encryptionMethod = null;

            if (algorithm is TripleDES)
            {
                encryptionMethod = EncryptedXml.XmlEncTripleDESUrl;
            }
            else if (algorithm is DES)
            {
                encryptionMethod = EncryptedXml.XmlEncDESUrl;
            }

            if (algorithm is Rijndael)
            {
                switch (algorithm.KeySize)
                {
                    case 128:
                        encryptionMethod = EncryptedXml.XmlEncAES128Url;
                        break;

                    case 192:
                        encryptionMethod = EncryptedXml.XmlEncAES192Url;
                        break;

                    case 256:
                        encryptionMethod = EncryptedXml.XmlEncAES256Url;
                        break;
                }
            }
            else
            {
                // Throw an exception if the transform is not in the previous categories
                throw new CryptographicException("The specified algorithm is not supported for XML Encryption.");
            }

            element.EncryptionMethod = new EncryptionMethod(encryptionMethod);

            // Add the encrypted element data to the EncryptedData object.
            element.CipherData.CipherValue = encryptedElement;

            ////////////////////////////////////////////////////
            // Replace the element from the original XmlDocument
            // object with the EncryptedData element.
            ////////////////////////////////////////////////////
            EncryptedXml.ReplaceElement(elementToEncrypt, element, false);
        }

        #endregion Public Methods
    }
}