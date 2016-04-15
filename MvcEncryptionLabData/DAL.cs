using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEncryptionLabData
{
    public class DAL
    {
        public void AddPerson(Person person)
        {
            using (DbEntities db = new DbEntities())
            {
                person.SSNSalt = Utils.GetSalt();
                person.SSNHash = Utils.Hash(person.SSN, person.SSNSalt);
                person.SSNEncrypted = Utils.Encrypt(person.SSN);

                db.Person.Add(person);

                var errors = db.GetValidationErrors();

                db.SaveChanges();
            }
        }

        public Person GetPersonBySSN(string ssn, string zipCode)
        {
            using (DbEntities context = new DbEntities())
            {
                List<Person> persons = (
                    from p in context.Person

                    join a in context.Address
                    on p.Address.AddressId equals a.AddressId

                    where a.Zip.Equals(zipCode)

                    select p
                ).ToList();

                if (persons.Count > 0)
                {
                    foreach (Person person in persons)
                    {
                        string ssnToMatchHash = Utils.Hash(ssn, person.SSNSalt);

                        if (person.SSNHash.Equals(ssnToMatchHash))
                        {
                            return person;
                        }
                    }

                }
                return null;
            }
        }
    }
}
