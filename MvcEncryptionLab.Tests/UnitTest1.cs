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

            //Guid pid = dal.GetMostRecentProcess();

            Logger logger = new Logger();

            dal.RunReallyLongProcess(logger);

            dal.AddLogger("TestUser", logger);
        }
    }
}