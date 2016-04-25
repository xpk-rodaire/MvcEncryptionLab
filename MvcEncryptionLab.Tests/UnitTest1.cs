using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcEncryptionLabData;
using System.Linq;
using System.Security.Cryptography;
using System.Security;

namespace MvcEncryptionLab.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestEncryption()
        {
            string securityKey = "8YMiP/3jSj6Zfe79lM8x0GqKOmbo9gR5qurmh68FqmY=";
            string plainText = "Steve was here but now he is gone";
            string iv = "";
            string cipherText = SecurityUtils.Encrypt(securityKey, plainText, ref iv);
            string decryptedText = SecurityUtils.Decrypt(securityKey, cipherText, iv);

            Assert.AreEqual(plainText, decryptedText);
        }

        [TestMethod]
        public void AddPersons()
        {
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
            dal.AddPerson(p1);

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
            dal.AddPerson(p2);

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
            dal.AddPerson(p3);
        }

        [TestMethod]
        public void AddPersons2()
        {
            DAL dal = new DAL();

            for (int i = 0; i < 1000; ++i)
            {
                Person person = dal.GetRandomPerson();

                Debug.WriteLine(String.Format("{0} - {1}", i, person.ToString()));

                dal.AddPerson(person);
            }
        }

        [TestMethod]
        public void TestGetPerson()
        {
            DAL dal = new DAL();
            Person person = dal.GetPersonBySSN("0099009900", "95984", "Gloria");
            Assert.AreEqual(person.LastName, "Hematoma");
        }

        [TestMethod]
        public void TestGetPersonSetSecurityKey()
        {
            DAL.SecurityKey = "8YMiP/3jSj6Zfe79lM8x0GqKOmbo9gR5qurmh68FqmY=";

            DAL dal = new DAL();
            Person person = dal.GetPersonBySSN("0099009900", "95984", "Gloria");
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
                string securityKey = "8YMiP/3jSj6Zfe79lM8x0GqKOmbo9gR5qurmh68FqmY=";
                string original = "Here is some data to encrypt!";

                string iv = "";
                // Encrypt the string to an array of bytes.
                string encrypted = SecurityUtils.Encrypt(securityKey, original, ref iv);

                // Decrypt the bytes to a string.
                string roundtrip = SecurityUtils.Decrypt(securityKey, encrypted, iv);

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
    }
}