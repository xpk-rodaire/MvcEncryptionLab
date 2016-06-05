using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Threading;

namespace MvcEncryptionLabData
{
    public class DAL
    {
        public bool SecurityKeyExists()
        {
            using (DbEntities db = new DbEntities())
            {
                return (
                    from s in db.SecurityKey
                    select s
                ).Any();
            }
        }

        public bool CheckSecurityKey(string key)
        {
            if (!SecurityKeyExists())
            {
                throw new ApplicationException("CheckSecurityKey() - no security key defined.");
            }

            using (DbEntities db = new DbEntities())
            {
                SecurityKey sk = (from s in db.SecurityKey select s).FirstOrDefault();

                string hash = SecurityUtils.Hash(key, sk.SecurityKeySalt);

                return hash.Equals(sk.SecurityKeyHash);
            }
        }

        public void SetSecurityKey(string value)
        {
            if (SecurityKeyExists())
            {
                throw new ApplicationException("SetSecurityKey() - security key already exists.");
            }

            using (DbEntities db = new DbEntities())
            {
                SecurityKey sk = new SecurityKey();
                sk.SecurityKeySalt = SecurityUtils.GetSalt();
                sk.SecurityKeyHash = SecurityUtils.Hash(value, sk.SecurityKeySalt);

                db.SecurityKey.Add(sk);
                db.SaveChanges();
            }
        }

        public void ClearSecurityKey()
        {
            using (DbEntities db = new DbEntities())
            {
                SecurityKey sk = (from s in db.SecurityKey select s).FirstOrDefault();
                db.SecurityKey.Remove(sk);
                db.SaveChanges();
            }
        }

        public void AddPerson(Person person, string userName)
        {
            // Check user has security key!

            using (DbEntities db = new DbEntities())
            {
                string iv = "";

                person.LastNameEncrypted = person.LastName; //.LastNameSecurityUtils.EncryptWithUserName(person.LastName, ref iv, userName);
                person.LastNameIV = person.LastName; //iv;

                person.Address.AddressLine1Encrypted = person.Address.AddressLine1; // SecurityUtils.EncryptWithUserName(person.Address.AddressLine1, ref iv, userName);
                person.Address.AddressLine1IV = person.Address.AddressLine1; //iv;

                person.SSNSalt = SecurityUtils.GetSalt();
                person.SSNHash = SecurityUtils.Hash(person.SSN, person.SSNSalt);
                person.SSNEncrypted = person.SSN; // SecurityUtils.EncryptWithUserName(person.SSN, ref iv, userName);
                person.SSNIV = person.SSN; //iv;
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

        #region Random Stuff

        private Random random = new Random();

        private const int FIRST_NAME_COUNT = 5494;
        private const int FIRST_NAME_MAX_CUM_FREQ_MALE = 90040;
        private const int FIRST_NAME_MAX_CUM_FREQ_FEMALE = 90024;

        private const int LAST_NAME_COUNT = 88799;
        private const int LAST_NAME_MAX_CUM_FREQ = 90483;

        private const int ZIP_CODE_COUNT = 2664;
        private const int ZIP_CODE_TOTAL_POP = 37187934;

        public LastName GetRandomLastName()
        {
            using (DbEntities context = new DbEntities())
            {
                int freq = random.Next(1, LAST_NAME_MAX_CUM_FREQ);
                double freqAsFloat = (double)freq / 1000;

                LastName lastName;

                lastName = (
                    from l in context.LastName
                    where l.CumulativeFrequency >= freqAsFloat
                    select l
                ).FirstOrDefault();

                if (lastName == null)
                {
                    lastName = (
                        from l in context.LastName
                        where l.Rank == 1
                        select l
                    ).FirstOrDefault();
                }

                return lastName;
            }
        }

        public FirstName GetRandomFirstName()
        {
            using (DbEntities context = new DbEntities())
            {
                bool isMale = (random.Next(0, 2) == 1);

                int freq = random.Next(1, isMale ? FIRST_NAME_MAX_CUM_FREQ_MALE : FIRST_NAME_MAX_CUM_FREQ_FEMALE);
                float freqAsFloat = (float)freq / 1000;

                FirstName firstName;

                firstName = (
                    from f in context.FirstName
                    where f.CumulativeFrequency >= freqAsFloat
                    && f.IsMale == isMale
                    select f
                ).FirstOrDefault();

                if (firstName == null)
                {
                    firstName = (
                        from f in context.FirstName
                        where f.Rank == 1
                        select f
                    ).FirstOrDefault();
                }

                return firstName;
            }
        }

        public Person GetRandomPerson()
        {
            Person person = new Person();

            int firstNameId = random.Next(1, FIRST_NAME_COUNT);

            using (DbEntities context = new DbEntities())
            {
                FirstName firstName = (
                    from f in context.FirstName
                    where f.FirstNameId == firstNameId
                    select f
                ).FirstOrDefault();

                person.FirstName = firstName.Value;

                int lastNameId = random.Next(1, LAST_NAME_COUNT);

                LastName lastName = (
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

                CaZipCode zipCode = (
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

        #endregion

        #region Log Item

        public void AddLogItem(string userName, Logger.LogItemType type, string target, string text)
        {
            using (var context = new DbEntities())
            {
                LogItem item = new LogItem
                {
                    UserName = userName,
                    Type = type,
                    Target = target,
                    Text = text,
                    CreateDateTime = DateTime.Now
                };
                context.LogItem.Add(item);

                var validationErrors = context.GetValidationErrors();

                context.SaveChanges();
            }
        }

        public void AddLogItem(
            string userName,
            Logger.LogItemType type,
            string target,
            string text,
            Guid processId,
            int processPercentComplete
        )
        {
            using (var context = new DbEntities())
            {
                LogItem item = new LogItem
                {
                    UserName = userName,
                    Type = type,
                    Target = target,
                    Text = text,
                    CreateDateTime = DateTime.Now,
                    ProcessId = processId,
                    ProcessPercentComplete = processPercentComplete
                };
                context.LogItem.Add(item);

                var validationErrors = context.GetValidationErrors();

                context.SaveChanges();
            }
        }

        private void _AddLogger(string userName, Logger logger, DbEntities context)
        {
            foreach (Logger.LogItem item in logger.LogItems)
            {
                LogItem dbItem = new LogItem()
                {
                    UserName = userName,
                    Target = item.Target,
                    CreateDateTime = item.CreateDateTime,
                    Text = item.Text,
                    Type = item.Type
                };
                context.LogItem.Add(dbItem);
            }
        }

        public void AddLogger(string user, Logger logger)
        {
            using (var context = new DbEntities())
            {
                this._AddLogger(user, logger, context);
                var validationErrors = context.GetValidationErrors();
                context.SaveChanges();
            }
        }

        #endregion

        public void RunReallyLongProcess(Logger logger)
        {
            Process process = new Process(logger)
            {
                Name = "Test Process",
                Description = "Test Process Description",
                UserName = ""
            };

            process.Phases.Add(
                new ProcessPhase(logger, process)
                {
                    Name = "Test Phase 1",
                    Description = "Test Phase 1 Description",
                    PhaseNumber = 1
                }
            );

            process.Phases.Add(
                new ProcessPhase(logger, process)
                {
                    Name = "Test Phase 2",
                    Description = "Test Phase 2 Description",
                    PhaseNumber = 2
                }
            );

            try
            {
                process.Start();

                Thread.Sleep(2000);

                process.Phases[0].Start();

                Thread.Sleep(2000);

                process.Phases[0].Finish();

                Thread.Sleep(2000);

                process.Phases[1].Start();

                Thread.Sleep(2000);

                process.Phases[1].Finish();

                Thread.Sleep(2000);
            }
            finally
            {
                process.Finish();
            }
        }
    }
}
