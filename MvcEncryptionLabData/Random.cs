using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEncryptionLabData
{
    public class CaZipCode
    {
        public int CaZipCodeId { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
    }

    /*
        Each of the three files, (dist.all.last), (dist. male.first), and (dist female.first) contain four 
         items of data. The four items are:

            A "Name"
            Frequency in percent
            Cumulative Frequency in percent
            Rank
    
        In the file (dist.all.last) one entry appears as:

            MOORE 0.312 5.312 9

        In our search area sample, MOORE ranks 9th in terms of frequency. 5.312 percent of the sample 
        population is covered by MOORE and the 8 names occurring more frequently than MOORE. The surname, 
        MOORE, is possessed by 0.312 percent of our population sample.
    */

    public class LastName
    {
        public int LastNameId { get; set; }
        public string Value { get; set; }
        public float Frequency { get; set; }
        public float CumulativeFrequency { get; set; }
        public short Rank { get; set; }
    }

    public class FirstName
    {
        public int FirstNameId { get; set; }
        public string Value { get; set; }
        public float Frequency { get; set; }
        public float CumulativeFrequency { get; set; }
        public short Rank { get; set; }
        public bool IsMale { get; set; }
    }
}
