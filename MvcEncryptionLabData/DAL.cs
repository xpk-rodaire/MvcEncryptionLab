using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Threading;

namespace MvcEncryptionLabData
{
    public delegate void ProgressStatusDelegate(string text, int percentComplete, bool complete);

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

        public void AddLogItem(
            string userName,
            LogItemType type,
            string target,
            string text)
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
            LogItemType type,
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

        public void AddLogItem(LogItem item)
        {
            using (var context = new DbEntities())
            {
                context.LogItem.Add(item);
                var validationErrors = context.GetValidationErrors();
                context.SaveChanges();
            }
        }

        private void _AddLogger(string userName, Logger logger, DbEntities context)
        {
            foreach (LogItem item in logger.LogItems)
            {
                context.LogItem.Add(item);
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

        #region Process

        public Guid GetMostRecentProcess()
        {
            using (DbEntities context = new DbEntities())
            {
                Guid processId = (
                    from l in context.LogItem
                    where l.ProcessId != null
                    orderby l.CreateDateTime descending
                    select l.ProcessId
                )
                .FirstOrDefault();

                return processId;
            }
        }

        //public ProcessStatus GetProcessStatus(Guid processId)
        //{
        //    using (DbEntities context = new DbEntities())
        //    {
        //        ProcessStatus status = (
        //            from l in context.LogItem
        //            where l.ProcessId == processId
        //            orderby l.CreateDateTime descending
        //            select new ProcessStatus
        //            {
        //                ProcessId = l.ProcessId,
        //                UserName = l.UserName,
        //                PercentComplete = l.ProcessPercentComplete,
        //                Status = l.Text
        //            }
        //        )
        //        .FirstOrDefault();

        //        return status;
        //    }
        //}

        #endregion

        public void RunReallyLongProcess(ProgressStatusDelegate progressDelegate, out LogItem logItem)
        {
            int pauseinMs = 50;

            Process topProcess = new Process()
            {
                Name = "Test Process",
                Description = "Test Process Description",
                UserName = ""
            };

            topProcess.AddSubProcess(
                name: "Test Sub Process 1",
                description: "Test Sub Process 1 Description"
            );

            topProcess.AddSubProcess(
                name: "Test Sub Process 2",
                description: "Test Sub Process 2 Description"
            );

            LogItem processLogItem = topProcess.Start();
            if (progressDelegate != null)
            {
                progressDelegate(processLogItem.Text, topProcess.PercentComplete, false);
            }
            this.AddLogItem(processLogItem);

            Thread.Sleep(pauseinMs);

            Process subProcess1 = topProcess.SubProcesses[0];
            processLogItem = subProcess1.Start();
            if (progressDelegate != null)
            {
                progressDelegate(processLogItem.Text, topProcess.PercentComplete, false);
            }
            this.AddLogItem(processLogItem);

            foreach (int index in Enumerable.Range(1, 10))
            {
                subProcess1.PercentComplete = index * 10;
                processLogItem = subProcess1.LogItem(String.Format("Processed {0:n0} records", 10000 / 10 * index));
                if (progressDelegate != null)
                {
                    progressDelegate(processLogItem.Text, topProcess.PercentComplete, false);
                }
                this.AddLogItem(processLogItem);
                Thread.Sleep(pauseinMs);
            }

            processLogItem = subProcess1.Finish();
            if (progressDelegate != null)
            {
                progressDelegate(processLogItem.Text, topProcess.PercentComplete, false);
            }
            this.AddLogItem(processLogItem);

            Thread.Sleep(pauseinMs);

            Process subProcess2 = topProcess.SubProcesses[1];

            processLogItem = subProcess2.Start();
            if (progressDelegate != null)
            {
                progressDelegate(processLogItem.Text, topProcess.PercentComplete, false);
            }
            this.AddLogItem(processLogItem);

            foreach (int index in Enumerable.Range(1, 20))
            {
                subProcess2.PercentComplete = index * 5;
                processLogItem = subProcess2.LogItem(String.Format("Processed {0:n0} records", 10000 / 20 * index));
                if (progressDelegate != null)
                {
                    progressDelegate(processLogItem.Text, topProcess.PercentComplete, false);
                }
                this.AddLogItem(processLogItem);
                Thread.Sleep(pauseinMs);
            }

            Thread.Sleep(pauseinMs);

            processLogItem = subProcess2.Finish();
            if (progressDelegate != null)
            {
                progressDelegate(processLogItem.Text, topProcess.PercentComplete, false);
            }
            this.AddLogItem(processLogItem);

            Thread.Sleep(pauseinMs);

            processLogItem = topProcess.Finish();
            if (progressDelegate != null)
            {
                progressDelegate(processLogItem.Text, topProcess.PercentComplete, true);
                logItem = processLogItem;
            }
            else
            {
                logItem = null;
            }
            this.AddLogItem(processLogItem);
        }
    }
}

