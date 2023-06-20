using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace despoil
{
    public static class Util
    {
        public static String Rainbow(Int32 numOfSteps, Int32 step)
        {
            var r = 0.0;
            var g = 0.0;
            var b = 0.0;
            var h = (Double)step / numOfSteps;
            var i = (Int32)(h * 6);
            var f = h * 6.0 - i;
            var q = 1 - f;

            switch (i % 6)
            {
                case 0:
                    r = 1;
                    g = f;
                    b = 0;
                    break;
                case 1:
                    r = q;
                    g = 1;
                    b = 0;
                    break;
                case 2:
                    r = 0;
                    g = 1;
                    b = f;
                    break;
                case 3:
                    r = 0;
                    g = q;
                    b = 1;
                    break;
                case 4:
                    r = f;
                    g = 0;
                    b = 1;
                    break;
                case 5:
                    r = 1;
                    g = 0;
                    b = q;
                    break;
            }
            return "#" + ((Int32)(r * 255)).ToString("X2") + ((Int32)(g * 255)).ToString("X2") + ((Int32)(b * 255)).ToString("X2");
        }

        private static string[] months = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        public static double ParseDate(string date)
        {
            if (date.Contains("|"))
            {                
                date = date.Substring(0, date.IndexOf("|"));
            }
            date = date.Replace("~", "").Trim();
            if (date.Contains(" "))
            {
                var bits = date.Split(' ');
                if (bits.Length != 2) throw new Exception("Bad date format " + date);
                var month = bits[0];
                var year = bits[1];

                var monthIdx = Array.IndexOf(months, month);
                if (monthIdx == -1) throw new Exception("Bad date format " + date);


                return double.Parse(year) + (((double)monthIdx) / 12.0);
            }
            else
            {
                return double.Parse(date);
            }
        }
    
        public static string NumberOrdinal(int number)
        {
            int lastPair = number % 100;

            if (lastPair >10 && lastPair < 20)
            {
                return $"{number}th";
            }

            int lastDigit = number % 10;

            switch(lastDigit)
            {
                case 1: return $"{number}st";

                case 2: return $"{number}nd";

                case 3: return $"{number}rd";

                default: return $"{number}th";
            }
        }

        public static string RemoveEmptyLines(string lines)
        {
            return Regex.Replace(lines, @"^\s*$\n|\r", string.Empty, RegexOptions.Multiline).TrimEnd();
        }

        public static string MoveThe(string text)
        {
            if (text.StartsWith("The "))
            {
                text = text.Substring(4) + ", the";
            }
            return text;
        }

        public static bool IsUppercase(this char c)
        {
            return c >='A' && c <='Z';
        }

        public static string MakeId(string input)
        {
            using (var md5 = MD5.Create())
                return BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", string.Empty);
        }

        public static string SortAndDedupeEntities(string inputLine)
        {
            if (!inputLine.Contains(" ++ ")) return inputLine;

            var split = inputLine.Split(" ++ ");
            var mainEntry = split[0];
            var additionals = split[1];

            var entities =  string.Join(" ", Regex.Matches(additionals, @"<.+?>").Select(m => m.Value).DistinctBy(v => v).OrderBy(v => v.StartsWith("<The ") ? v.Replace("<The ","<") : v));
            return mainEntry + " ++ " + entities;
        }

    }



    public class CollectionComparer : IComparer<string>
    {

        static readonly string[] Collections = { "The Sandman", "Death", "Books of Magic", "Lucifer", "The Dreaming", "House of Whispers", "John Constantine Hellblazer", "Winter's Edge", "Free Country A Tale of The Children's Crusade" };

        static readonly string[] Prefixes = { "Vol", "Overture", "Book", "Absolute", "Deluxe", "Omnibus" };

        public int Compare(string? x, string? y)
        {
            if (x != null && y != null)
            {
                if (x == y) return 0;

                var prefixX = Array.IndexOf(Collections, x.Split(":").First()) * 10;
                var prefixY = Array.IndexOf(Collections, y.Split(":").First()) * 10;

                try
                {
                    prefixX += Array.IndexOf(Prefixes, x.Split(":").Skip(1).First().Trim().Split(" ").First());
                }
                catch
                {}

                try
                {
                    prefixY += Array.IndexOf(Prefixes, y.Split(":").Skip(1).First().Trim().Split(" ").First());
                }
                catch {}

                var diff = Comparer<int>.Default.Compare(prefixX, prefixY);

                if (diff != 0) return diff;
            }

            return Comparer<string>.Default.Compare(x, y);
        }
    }
}
