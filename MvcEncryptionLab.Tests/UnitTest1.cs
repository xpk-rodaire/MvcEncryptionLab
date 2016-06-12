using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcEncryptionLabData;
using System.Linq;
using System.Security.Cryptography;
using System.Security;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Xml.Linq;

namespace MvcEncryptionLab.Tests
{
    [TestClass]
    public class UnitTest1
    {
        string encryptionKey = "8YMiP/3jSj6Zfe79lM8x0GqKOmbo9gR5qurmh68FqmY=";

        [TestMethod]
        public void TestEncryption()
        {
            SecurityUtils.EncryptionKeyTestingOnly = encryptionKey;

            string plainText = "Steve was here but now he is gone";
            string iv = "";
            string cipherText = SecurityUtils.EncryptWithKey(plainText, ref iv, encryptionKey);
            string decryptedText = SecurityUtils.DecryptWithKey(cipherText, iv, encryptionKey);

            Assert.AreEqual(plainText, decryptedText);
        }

        [TestMethod]
        public void AddPersons()
        {
            SecurityUtils.EncryptionKeyTestingOnly = encryptionKey;

            DAL dal = new DAL();

            Person p1 = new Person
            {
                FirstName = "Steve",
                LastName = "Champeau",
                SSN = "123456789", 
                Address = new Address
                {
                    AddressLine1 = "844 Bronze Lane",
                    City = "West Sacramento",
                    State = "CA",
                    Zip = "95691"
                }
            };
            dal.AddPerson(p1, "schampeau");

            Person p2 = new Person
            {
                FirstName = "Jim",
                LastName = "Smith",
                SSN = "11223344", 
                Address = new Address
                {
                    AddressLine1 = "123 Main Street",
                    City = "Sacramento",
                    State = "CA",
                    Zip = "95814"
                }
            };
            dal.AddPerson(p2, "schampeau");

            Person p3 = new Person
            {
                FirstName = "Gloria",
                LastName = "Hematoma",
                SSN = "0099009900",
                Address = new Address
                {
                    AddressLine1 = "123 Main Street",
                    City = "Roseville",
                    State = "CA",
                    Zip = "95984"
                }
            };
            dal.AddPerson(p3, "schampeau");
        }

        [TestMethod]
        public void AddPersons2()
        {
            DAL dal = new DAL();

            for (int i = 0; i < 1000; ++i)
            {
                Person person = dal.GetRandomPerson();

                Debug.WriteLine(String.Format("{0} - {1}", i, person.ToString()));

                dal.AddPerson(person, "schampeau");
            }
        }

        [TestMethod]
        public void TestGetPerson()
        {
            DAL dal = new DAL();
            Person person = dal.GetPersonBySSN("0099009900", "95984", "Gloria", "schampeau");
            Assert.AreEqual(person.LastName, "Hematoma");
        }

        [TestMethod]
        public void TestGetPersonSetSecurityKey()
        {
            SecurityUtils.EncryptionKeyTestingOnly = encryptionKey;

            DAL dal = new DAL();
            Person person = dal.GetPersonBySSN("0099009900", "95984", "Gloria", "schampeau");
            Assert.AreEqual(person.LastName, "Hematoma");
        }

        [TestMethod]
        public void GenerateAesKey()
        {
            AesManaged aes = new AesManaged();
            aes.GenerateKey();
            byte[] keyAsBytes = aes.Key;
            string keyAsString = Convert.ToBase64String(keyAsBytes);
            Debug.WriteLine(keyAsString);
        }

        [TestMethod]
        public void TestAesProvider()
        {
            try
            {
                SecurityUtils.EncryptionKeyTestingOnly = encryptionKey;
                string original = "Here is some data to encrypt!";

                string iv = "";
                // Encrypt the string to an array of bytes.
                string encrypted = SecurityUtils.EncryptWithUserName(original, ref iv, "");

                // Decrypt the bytes to a string.
                string roundtrip = SecurityUtils.DecryptWithUserName(encrypted, iv, "");

                Assert.AreEqual(original, roundtrip);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void TestSecureString()
        {
            try
            {
                string original = "Here is some data to encrypt!";

                SecureString originalSS = original.ConvertToSecureString();

                string originalSSConvert = originalSS.ConvertToUnsecureString();

                Assert.AreEqual(original, originalSSConvert);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void TestExpirableSecureValue()
        {
            try
            {
                ExpirableSecureValue value = new ExpirableSecureValue(5);
                value.Value = "Here is some data to encrypt!";

                System.Threading.Thread.Sleep(4000);

                Assert.IsTrue(value.HasValue);

                System.Threading.Thread.Sleep(2000);

                Assert.IsFalse(value.HasValue);

                value.Value = "Here is some data to encrypt!";
                System.Threading.Thread.Sleep(5001);
                Assert.IsFalse(value.HasValue);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void TestXPath()
        {
            string manifestFile = @"C:\Users\slchampeau\Downloads\_ACA-IRS\TestScenario3_Manifest.xml";
            XmlDocument document = new XmlDocument();
            document.Load(manifestFile);
            XPathNavigator navigator = document.CreateNavigator();

            XmlNamespaceManager manager = new XmlNamespaceManager(navigator.NameTable);
            manager.AddNamespace("uibizheader", "urn:us:gov:treasury:irs:msg:acauibusinessheader");
            manager.AddNamespace("bizheader", "urn:us:gov:treasury:irs:msg:acabusinessheader");
            manager.AddNamespace("air70", "urn:us:gov:treasury:irs:ext:aca:air:7.0");
            manager.AddNamespace("irscommon", "urn:us:gov:treasury:irs:common");

            /*
            <ACAUIBusinessHeader
                xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                xmlns:xsd="http://www.w3.org/2001/XMLSchema"
                xmlns="urn:us:gov:treasury:irs:msg:acauibusinessheader">

              <ACABusinessHeader xmlns="urn:us:gov:treasury:irs:msg:acabusinessheader">
                <UniqueTransmissionId xmlns="urn:us:gov:treasury:irs:ext:aca:air:7.0">5e34ed8e-f92f-42f6-ac65-8cd1eddabf23:SYS12:BB0KF::T</UniqueTransmissionId>
                <Timestamp xmlns="urn:us:gov:treasury:irs:common">2016-01-23T13:41:09Z</Timestamp>
              </ACABusinessHeader>
            */

            string xpath = "//uibizheader:ACAUIBusinessHeader/air70:ACATransmitterManifestReqDtl/irscommon:ChecksumAugmentationNum";


            foreach (XPathNavigator nav in navigator.Select(xpath, manager))
            {
                nav.SetValue("Checksum");
            }

            document.Save(manifestFile);
        }

        string userName = "schampea";

        [TestMethod]
        public void TestEncryptionKeyExpire()
        {
            SecurityUtils.SetUserEncryptionKey(userName, "ThisIsATestKey", 5);

            System.Threading.Thread.Sleep(5000);

            Assert.IsFalse(SecurityUtils.UserHasEncryptionKey(userName));

            //////////////////////

            SecurityUtils.SetUserEncryptionKey(userName, "ThisIsATestKey", 2);

            SecurityUtils.LockUserEncryptionKey(userName, true);

            System.Threading.Thread.Sleep(12000);

            Assert.IsTrue(SecurityUtils.UserHasEncryptionKey(userName));

            SecurityUtils.LockUserEncryptionKey(userName, false);

            Assert.IsFalse(SecurityUtils.UserHasEncryptionKey(userName));

            //////////////////////

            SecurityUtils.SetUserEncryptionKey(userName, "ThisIsATestKey", 5000);

            SecurityUtils.ExpireUserEncryptionKey(userName);

            Assert.IsFalse(SecurityUtils.UserHasEncryptionKey(userName));
        }

        [TestMethod]
        public void TestSecurityKey()
        {
            // Security key should not exist

            DAL dal = new DAL();
            dal.SetSecurityKey(encryptionKey);
            Assert.IsTrue(dal.SecurityKeyExists());
            Assert.IsTrue(dal.CheckSecurityKey(encryptionKey));

            try
            {
                dal.SetSecurityKey(encryptionKey);
                Assert.Fail("Should not be able to call SetSecurityKey() 2nd time!");
            }
            catch (Exception ex)
            {

            }

            dal.ClearSecurityKey();
            Assert.IsFalse(dal.SecurityKeyExists());
        }

        [TestMethod]
        public void TestRandomName()
        {
            string filePath = @"C:\Data\Git\MvcEncryptionLab\MvcEncryptionLabData\Documentation\RandomNames.csv";

            DAL dal = new DAL();

            using (StreamWriter writetext = new StreamWriter(filePath))
            {
                int count = 300000;
                for (int i = 0; i < count; ++i)
                {
                    writetext.WriteLine(dal.GetRandomFirstName().Value + "," + dal.GetRandomLastName().Value + "," + RandomUtils.RandomSSN());
                }
            }
        }

        [TestMethod]
        public void TestReallyLongProcess()
        {
            DAL dal = new DAL();
            dal.RunReallyLongProcess(null);
        }

        [TestMethod]
        public void TestTransformManifestXmlFile()
        {
            string fileIn = @"Z:\Utility\SCO2015B_Orig0000\SCO2015B_Orig0000_Manifest.xml";
            string fileOut = @"Z:\Utility\SCO2015B_Orig0000 Transform\SCO2015B_Orig0000_Manifest.xml";

            File.Copy(fileIn, fileOut, true);

            XmlDocument document = new XmlDocument();
            document.Load(fileOut);
            XPathNavigator navigator = document.CreateNavigator();

            foreach (XmlNode node in document)
            {
                if (node.NodeType == XmlNodeType.XmlDeclaration)
                {
                    XmlDeclaration dec = (XmlDeclaration)node;
                    dec.Encoding = "UTF-8";
                    break;
                }
            }

            // <p:ACAUIBusinessHeader xmlns:p="urn:us:gov:treasury:irs:msg:acauibusinessheader"
            //    xmlns:acaBusHeader="urn:us:gov:treasury:irs:msg:acabusinessheader"
            //    xmlns="urn:us:gov:treasury:irs:ext:aca:air:7.0"
            //    xmlns:irs="urn:us:gov:treasury:irs:common"
            //    xmlns:wsu="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"
            //    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
            //    xsi:schemaLocation="urn:us:gov:treasury:irs:msg:acauibusinessheader IRSACAUserInterfaceHeaderMessage.xsd">

            XmlNamespaceManager manager = new XmlNamespaceManager(navigator.NameTable);
            manager.AddNamespace("p", "urn:us:gov:treasury:irs:msg:acauibusinessheader");
            manager.AddNamespace("acaBusHeader", "urn:us:gov:treasury:irs:msg:acabusinessheader");
            manager.AddNamespace("air", "urn:us:gov:treasury:irs:ext:aca:air:7.0");
            manager.AddNamespace("irs", "urn:us:gov:treasury:irs:common");
            manager.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            manager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            document.DocumentElement.RemoveAllAttributes();

            // These must be in this order
            document.DocumentElement.SetAttribute("xmlns:p", "urn:us:gov:treasury:irs:msg:acauibusinessheader");
            document.DocumentElement.SetAttribute("xmlns:acaBusHeader", "urn:us:gov:treasury:irs:msg:acabusinessheader");
            document.DocumentElement.SetAttribute("xmlns", "urn:us:gov:treasury:irs:ext:aca:air:7.0");
            document.DocumentElement.SetAttribute("xmlns:irs", "urn:us:gov:treasury:irs:common");
            document.DocumentElement.SetAttribute("xmlns:wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            document.DocumentElement.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            XmlAttribute attr = document.CreateAttribute("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
            attr.Value = "urn:us:gov:treasury:irs:msg:acauibusinessheader IRS-ACAUserInterfaceHeaderMessage.xsd";
            document.DocumentElement.Attributes.Append(attr);

            ProcessNamespace(document, manager, "p", true, false);
            ProcessNamespace(document, manager, "irs", true, true);
            ProcessNamespace(document, manager, "acaBusHeader", true, true);
            ProcessNamespace(document, manager, "air", false, true);

            document.Save(fileOut);

            // This will remove the BOM (Byte Order Mark)
            string[] lines = File.ReadAllLines(fileOut);
            File.WriteAllLines(fileOut, lines);
        }

        [TestMethod]
        public void TestTransformFormDataXmlFile()
        {
            string fileIn = @"Z:\Utility\SCO2015B_Orig0000\1094C_Request_BB0KC_20160610T022412000Z.xml";
            string fileOut = @"Z:\Utility\SCO2015B_Orig0000 Transform\1094C_Request_BB0KC_20160610T022412000Z.xml";

            File.Copy(fileIn, fileOut, true);

            XmlDocument document = new XmlDocument();
            document.Load(fileOut);
            XPathNavigator navigator = document.CreateNavigator();

            foreach (XmlNode node in document)
            {
                if (node.NodeType == XmlNodeType.XmlDeclaration)
                {
                    XmlDeclaration dec = (XmlDeclaration)node;
                    dec.Encoding = "UTF-8";
                    break;
                }
            }

            // <n1:Form109495CTransmittalUpstream
            //    xmlns="urn:us:gov:treasury:irs:ext:aca:air:7.0"
            //    xmlns:irs="urn:us:gov:treasury:irs:common"
            //    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
            //    xmlns:n1="urn:us:gov:treasury:irs:msg:form1094-1095Ctransmitterupstreammessage"
            //    xsi:schemaLocation="urn:us:gov:treasury:irs:msg:form1094-1095Ctransmitterupstreammessage IRS-Form1094-1095CTransmitterUpstreamMessage.xsd">

            XmlNamespaceManager manager = new XmlNamespaceManager(navigator.NameTable);
            manager.AddNamespace("n1", "urn:us:gov:treasury:irs:msg:form1094-1095Ctransmitterupstreammessage");
            manager.AddNamespace("air", "urn:us:gov:treasury:irs:ext:aca:air:7.0");
            manager.AddNamespace("irs", "urn:us:gov:treasury:irs:common");
            manager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            document.DocumentElement.RemoveAllAttributes();

            // These must be in this order
            document.DocumentElement.SetAttribute("xmlns", "urn:us:gov:treasury:irs:ext:aca:air:7.0");
            document.DocumentElement.SetAttribute("xmlns:irs", "urn:us:gov:treasury:irs:common");
            document.DocumentElement.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            document.DocumentElement.SetAttribute("xmlns:n1", "urn:us:gov:treasury:irs:msg:form1094-1095Ctransmitterupstreammessage");
            XmlAttribute attr = document.CreateAttribute("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
            attr.Value = "urn:us:gov:treasury:irs:msg:form1094-1095Ctransmitterupstreammessage IRS-Form1094-1095CTransmitterUpstreamMessage.xsd";
            document.DocumentElement.Attributes.Append(attr);

            ProcessNamespace(document, manager, "n1", true, false);
            ProcessNamespace(document, manager, "irs", true, true);
            ProcessNamespace(document, manager, "air", false, true);

            //<n1:Form109495CTransmittalUpstream
            //  <Form1094CUpstreamDetail recordType="String" lineNum="0">
            //    <EmployerInformationGrp>
            //      <BusinessName></BusinessName>
            //      <BusinessNameControlTxt>ZZZZ</BusinessNameControlTxt

            string f1094CXpath = "//n1:Form109495CTransmittalUpstream/air:Form1094CUpstreamDetail/air:EmployerInformationGrp/air:BusinessName";
            XPathNavigator f1094CNav = navigator.SelectSingleNode(f1094CXpath, manager);
            f1094CNav.InsertAfter("<BusinessNameControlTxt>ZZZZ</BusinessNameControlTxt>");

            //<n1:Form109495CTransmittalUpstream
            //  <Form1094CUpstreamDetail recordType="String" lineNum="0">
            //    <Form1095CUpstreamDetail recordType="String" lineNum="0">
            //      <EmployeeInfoGrp>
            //        <OtherCompletePersonName></OtherCompletePersonName>
            //        <PersonNameControlTxt>ZZZ</PersonNameControlTxt>

            string f1095CXpath = "//n1:Form109495CTransmittalUpstream/air:Form1094CUpstreamDetail/air:Form1095CUpstreamDetail/air:EmployeeInfoGrp/air:OtherCompletePersonName";
            
            XmlNodeList list = document.DocumentElement.SelectNodes(f1095CXpath, manager);
            foreach (XmlNode node in list)
            {
                XmlElement elem = document.CreateElement("PersonNameControlTxt", "urn:us:gov:treasury:irs:ext:aca:air:7.0");
                elem.InnerText = "ZZZ";
                node.ParentNode.InsertAfter(elem, node);
            }

            document.Save(fileOut);

            // This will remove the BOM (Byte Order Mark)
            string[] lines = File.ReadAllLines(fileOut);
            File.WriteAllLines(fileOut, lines);
        }

        public void ProcessNamespace(
            XmlDocument document,
            XmlNamespaceManager manager,
            string namespacePrefix,
            bool addNamespacePrefix,
            bool removeNamespaceAttribute
        )
        {
            XmlNodeList list = document.SelectNodes(
                String.Format("//{0}:*", namespacePrefix),
                manager
            );
            foreach (XmlNode node in list)
            {
                if (addNamespacePrefix)
                {
                    node.Prefix = namespacePrefix;
                }
                if (removeNamespaceAttribute)
                {
                    XmlElement element = (XmlElement)node;
                    element.RemoveAttribute("xmlns");
                }
            }
        }

        [TestMethod]
        public void XmlExperiment()
        {
            string manifestFileIn = @"C:\Users\schampea\Downloads\IRS-ACA\Match IRS Schema Names\TestSample.xml";

            XmlDocument document = new XmlDocument();
            document.Load(manifestFileIn);
            XPathNavigator navigator = document.CreateNavigator();

            XmlNamespaceManager manager = new XmlNamespaceManager(navigator.NameTable);
            manager.AddNamespace("irs", "urn:us:gov:treasury:irs:common");

            string xpath = "//irs:*";

            XmlNodeList list = document.SelectNodes(xpath, manager);
            foreach (XmlNode node in list)
            {
                //Debug.WriteLine(node.Name + "   " + node.ToString() + "   ");// + ((XmlAttribute)node).OwnerElement.Name);
            }

            // Find attributes:
            //     "//@xmlnsX"
            // Find elements to which attributes belong:
            //     "//*[@xmlnsX]"

            list = document.SelectNodes("//*[@xmlnsX]", manager);
            foreach (XmlNode node in list)
            {
                Debug.WriteLine(node.Name + "   " + node.ToString());
            }
        }

        //
        // http://snippetrepo.com/snippets/how-to-remove-xmlns-from-xmldocument-in-c
        //
        public XmlDocument RemoveXmlns(XmlDocument doc)
        {
            XDocument d;
            using (var nodeReader = new XmlNodeReader(doc))
            {
                d = XDocument.Load(nodeReader);
            }

            d.Root.Descendants().Attributes().Where(x => x.IsNamespaceDeclaration).Remove();

            foreach (var elem in d.Descendants())
            {
                elem.Name = elem.Name.LocalName;
            }

            var xmlDocument = new XmlDocument();
            using (var xmlReader = d.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        public XmlDocument RemoveXmlns(String xml)
        {
            XDocument d = XDocument.Parse(xml);
            d.Root.Descendants().Attributes().Where(x => x.IsNamespaceDeclaration).Remove();

            foreach (var elem in d.Descendants())
                elem.Name = elem.Name.LocalName;

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(d.CreateReader());

            return xmlDocument;
        }
    }
}