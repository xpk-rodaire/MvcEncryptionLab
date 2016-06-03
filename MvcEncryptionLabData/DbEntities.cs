using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEncryptionLabData
{
    public class DbEntities : DbContext
    {
        public DbEntities()
            : base("name=EncryptionDb")
        {
            //this.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
        }

        public virtual DbSet<SecurityKey> SecurityKey { get; set; }

        public virtual DbSet<Person> Person { get; set; }
        public virtual DbSet<Address> Address { get; set; }

        public virtual DbSet<CaZipCode> CaZipCode { get; set; }
        public virtual DbSet<LastName> LastName { get; set; }
        public virtual DbSet<FirstName> FirstName { get; set; }

        public virtual DbSet<LogItem> LogItem { get; set; }
        public virtual DbSet<Process> Process { get; set; }
        public virtual DbSet<ProcessPhase> ProcessPhase { get; set; }
    }
}
