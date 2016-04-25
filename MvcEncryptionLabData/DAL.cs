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
        private static ExpirableSecureValue _encryptionKey = new ExpirableSecureValue(300);

        public static string EncryptionKey
        {
            set
            {
                _encryptionKey.Value = value;
            }

            private get
            {
                if (!_encryptionKey.HasValue)
                {
                    throw new ApplicationException("Security key not available.");
                }
                return _encryptionKey.Value;
            }
        }

        //private const string privateKey = "8YMiP/3jSj6Zfe79lM8x0GqKOmbo9gR5qurmh68FqmY=";

        public void AddPerson(Person person)
        {
            using (DbEntities db = new DbEntities())
            {
                string iv = "";

                person.LastNameEncrypted = SecurityUtils.Encrypt(EncryptionKey, person.LastName, ref iv);
                person.LastNameIV = iv;

                person.Address.AddressLine1Encrypted = SecurityUtils.Encrypt(EncryptionKey, person.Address.AddressLine1, ref iv);
                person.Address.AddressLine1IV = iv;

                person.SSNSalt = SecurityUtils.GetSalt();
                person.SSNHash = SecurityUtils.Hash(person.SSN, person.SSNSalt);
                person.SSNEncrypted = SecurityUtils.Encrypt(EncryptionKey, person.SSN, ref iv);
                person.SSNIV = iv;
                db.Person.Add(person);

                var errors = db.GetValidationErrors();

                db.SaveChanges();
            }
        }

        public Person GetPersonById(int id)
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

                person.LastName = SecurityUtils.Decrypt(EncryptionKey, person.LastNameEncrypted, person.LastNameIV);
                person.SSN = SecurityUtils.Decrypt(EncryptionKey, person.SSNEncrypted, person.SSNIV);

                return person;
            }
        }

        public Person GetPersonBySSN(string ssn, string zipCode, string firstName)
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
                            person.LastName = SecurityUtils.Decrypt(EncryptionKey, person.LastNameEncrypted, person.LastNameIV);
                            person.SSN = SecurityUtils.Decrypt(EncryptionKey, person.SSNEncrypted, person.SSNIV);
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

                person.SSN = SecurityUtils.RandomSSN();

                Address address = new Address();
                person.Address = address;

                address.AddressLine1 = SecurityUtils.RandomString(25);

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
