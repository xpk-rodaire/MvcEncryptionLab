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
        public int Rank { get; set; }
    }

    public class FirstName
    {
        public int FirstNameId { get; set; }
        public string Value { get; set; }
        public float Frequency { get; set; }
        public float CumulativeFrequency { get; set; }
        public int Rank { get; set; }
        public bool IsMale { get; set; }
    }

    public class RandomUtils
    {
        private static Random random;
        private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const string NUMBERS = "0123456789";

        static RandomUtils()
        {
            random = new Random();
        }

        public static string RandomString(int length, string chars)
        {
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string RandomString(int length)
        {
            return RandomString(length, CHARS);
        }

        public static string RandomSSN()
        {
            return RandomString(9, NUMBERS);
        }
    }
}
