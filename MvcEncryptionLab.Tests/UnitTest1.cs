using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcEncryptionLabData;
using System.Linq;
using System.Security.Cryptography;

namespace MvcEncryptionLab.Tests
{
    [TestClass]
    public class UnitTest1
    {

        private const string privateKey = "8YMiP/3jSj6Zfe79lM8x0GqKOmbo9gR5qurmh68FqmY=";

        [TestMethod]
        public void TestEncryption()
        {
            string plainText = "Steve was here but now he is gone";
            string iv = "";
            string cipherText = Utils.Encrypt(privateKey, plainText, ref iv);
            string decryptedText = Utils.Decrypt(privateKey, cipherText, iv);

            Assert.AreEqual(plainText, decryptedText);
        }


        //[TestMethod]
        //public void TestEncryptionSize()
        //{
        //    string str;
        //    string eStr;
        //    string dStr;

        //    for( int index = 0; index < 1000; index++ )
        //    {
        //        str = Utils.RandomString(100);
        //        eStr = Utils.Encrypt(str);
        //        dStr = Utils.Decrypt(eStr);

        //        Assert.AreEqual(str, dStr);







        //        Debug.WriteLine(str);
        //        Debug.WriteLine(eStr.Length);
        //    }
        //}

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
        public void GenerateAesKey()
        {
            AesManaged aes = new AesManaged();
            aes.GenerateKey();
            byte[] keyAsBytes = aes.Key;
            string keyAsString = Convert.ToBase64String(keyAsBytes);
            Debug.WriteLine(keyAsString);
        }
    }
}