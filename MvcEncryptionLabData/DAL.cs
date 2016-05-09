using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace MvcEncryptionLabData
{
    public class DAL
    {
        public bool IsCheckPhrase()
        {
            using (DbEntities db = new DbEntities())
            {
                return (
                    from c in db.CheckPhrase
                    select c
                ).Any();
            }
        }

        public string GetCheckPhrase(string key)
        {
            using (DbEntities db = new DbEntities())
            {
                EncryptedCheckPhrase cp =
                (
                    from c in db.CheckPhrase
                    select c
                ).FirstOrDefault();

                if (cp == null)
                {
                    return null;
                    //throw new ApplicationException("GetCheckPhrase() - no check phrase defined.");
                }

                string valueDecrypted = cp.CheckPhrase = SecurityUtils.DecryptWithKey(
                    key,
                    cp.CheckPhraseEncrypted,
                    cp.CheckPhraseIV
                );

                return valueDecrypted;
            }
        }

        public void SetCheckPhrase(string userName, string value)
        {
            string iv = "";
            string encryptedValue = SecurityUtils.Encrypt(
                value,
                ref iv,
                userName
            );

            using (DbEntities db = new DbEntities())
            {
                EncryptedCheckPhrase cp = new EncryptedCheckPhrase();
                cp.CheckPhraseEncrypted = encryptedValue;
                cp.CheckPhraseIV = iv;

                db.CheckPhrase.Add(cp);
                db.SaveChanges();
            }
        }

        public void AddPerson(Person person, string userName)
        {
            // Check user has security key!

            using (DbEntities db = new DbEntities())
            {
                string iv = "";

                person.LastNameEncrypted = SecurityUtils.Encrypt(person.LastName, ref iv, userName);
                person.LastNameIV = iv;

                person.Address.AddressLine1Encrypted = SecurityUtils.Encrypt(person.Address.AddressLine1, ref iv, userName);
                person.Address.AddressLine1IV = iv;

                person.SSNSalt = SecurityUtils.GetSalt();
                person.SSNHash = SecurityUtils.Hash(person.SSN, person.SSNSalt);
                person.SSNEncrypted = SecurityUtils.Encrypt(person.SSN, ref iv, userName);
                person.SSNIV = iv;
                db.Person.Add(person);

                var errors = db.GetValidationErrors();

                db.SaveChanges();
            }
        }

        public Person GetPersonById(int id, string userName)
        {
            using (DbEntities context = new DbEntities())
            {
                Person person = (
                    from p in context.Person

                    join a in context.Address
                    on p.Address.AddressId equals a.AddressId

                    where p.PersonId == id

                    select p
                ).FirstOrDefault();

                person.LastName = SecurityUtils.DecryptWithUserName(person.LastNameEncrypted, person.LastNameIV, userName);
                person.SSN = SecurityUtils.DecryptWithUserName(person.SSNEncrypted, person.SSNIV, userName);

                return person;
            }
        }

        public Person GetPersonBySSN(string ssn, string zipCode, string firstName, string userName)
        {
            using (DbEntities context = new DbEntities())
            {
                List<Person> persons = (
                    from p in context.Person

                    join a in context.Address
                    on p.Address.AddressId equals a.AddressId

                    where p.FirstName.Equals(firstName)
                        && a.Zip.Equals(zipCode)

                    select p
                )
                .Include("Address")
                .ToList();

                if (persons.Count > 0)
                {
                    foreach (Person person in persons)
                    {
                        string ssnToMatchHash = SecurityUtils.Hash(ssn, person.SSNSalt);

                        if (person.SSNHash.Equals(ssnToMatchHash))
                        {
                            person.LastName = SecurityUtils.DecryptWithUserName(person.LastNameEncrypted, person.LastNameIV, userName);
                            person.SSN = SecurityUtils.DecryptWithUserName(person.SSNEncrypted, person.SSNIV, userName);
                            return person;
                        }
                    }

                }
                return null;
            }
        }

        private Random random = new Random();

        private const int FIRST_NAME_COUNT = 5494;
        private const int LAST_NAME_COUNT = 88799;
        private const int ZIP_CODE_COUNT = 2664;
        private const int ZIP_CODE_TOTAL_POP = 37187934;

        public Person GetRandomPerson()
        {
            Person person = new Person();

            int firstNameId = random.Next(1, FIRST_NAME_COUNT);

            using (DbEntities context = new DbEntities())
            {
                FirstName firstName  = (
                    from f in context.FirstName
                    where f.FirstNameId == firstNameId
                    select f
                ).FirstOrDefault();

                person.FirstName = firstName.Value;

                int lastNameId = random.Next(1, LAST_NAME_COUNT);

                LastName lastName  = (
                    from l in context.LastName
                    where l.LastNameId == lastNameId
                    select l
                ).FirstOrDefault();

                person.LastName = lastName.Value;

                person.SSN = RandomUtils.RandomSSN();

                Address address = new Address();
                person.Address = address;

                address.AddressLine1 = RandomUtils.RandomString(25);

                int zipCodeId = random.Next(1, ZIP_CODE_COUNT);

                CaZipCode zipCode  = (
                    from z in context.CaZipCode
                    where z.CaZipCodeId == zipCodeId
                    select z
                ).FirstOrDefault();

                address.City = zipCode.City;
                address.Zip = zipCode.ZipCode;
                address.State = "CA";
            }
            return person;
        }
    }
}
