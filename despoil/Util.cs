using System.Text.RegularExpressions;

namespace despoil
{
    public class Util
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

    }



    public class CollectionComparer : IComparer<string>
    {
        static readonly string[] CollectionPrefixes = { "Vol", "Overture", "Death", "Books", "Book", "Absolute", "Deluxe" };

        public int Compare(string? x, string? y)
        {
            if (x != null && y != null)
            {
                if (x == y) return 0;

                var prefixX = Array.IndexOf(CollectionPrefixes, x.Split(" ").First());
                var prefixY = Array.IndexOf(CollectionPrefixes, y.Split(" ").First());

                var diff = Comparer<int>.Default.Compare(prefixX, prefixY);

                if (diff != 0) return diff;
            }

            return Comparer<string>.Default.Compare(x, y);
        }
    }
}
