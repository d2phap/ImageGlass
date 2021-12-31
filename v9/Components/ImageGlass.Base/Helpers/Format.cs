using System.Text.RegularExpressions;

namespace ImageGlass.Base;

public partial class Helpers
{
    /// <summary>
    /// Formats the given file size as a human readable string.
    /// </summary>
    /// <param name="size">File size in bytes.</param>
    /// <returns>The formatted string.</returns>
    public static string FormatSize(long size)
    {
        var mod = 1024d;
        var sized = size * 1d;

        var units = new string[] { "B", "KB", "MB", "GB", "TB", "PB" };
        int i;
        for (i = 0; sized > mod; i++)
        {
            sized /= mod;
        }

        return string.Format("{0} {1}", Math.Round(sized, 2), units[i]);
    }


    /// <summary>
    /// Formats the given date as a human readable string. For use with
    /// grouping with past dates.
    /// </summary>
    /// <param name="date">Date to format.</param>
    public static Tuple<int, string> GroupTextDate(DateTime date)
    {
        DateTime now = DateTime.Now;
        DateTime weekStart = now - new TimeSpan((int)now.DayOfWeek, now.Hour, now.Minute, now.Second, now.Millisecond);
        DateTime monthStart = now - new TimeSpan((int)now.Day, now.Hour, now.Minute, now.Second, now.Millisecond);
        DateTime yearStart = now - new TimeSpan((int)now.DayOfYear, now.Hour, now.Minute, now.Second, now.Millisecond);
        double secs = (now - date).TotalSeconds;

        int order = 0;
        string txt = string.Empty;
        if (secs < 0)
        {
            order = 0;
            txt = "Not Yet";
        }
        else if (secs < 60)
        {
            order = 1;
            txt = "Just now";
        }
        else if (date.Year == now.Year && date.Month == now.Month && date.Day == now.Day)
        {
            order = 2;
            txt = "Today";
        }
        else if (date.Year == now.Year && date.Month == now.Month && date.Day == now.Day - 1)
        {
            order = 3;
            txt = "Yesterday";
        }
        else if (date > weekStart)
        {
            order = 4;
            txt = "This week";
        }
        else if (date > weekStart.AddDays(-7))
        {
            order = 5;
            txt = "Last week";
        }
        else if (date > weekStart.AddDays(-14))
        {
            order = 6;
            txt = "Two weeks ago";
        }
        else if (date > weekStart.AddDays(-21))
        {
            order = 7;
            txt = "Three weeks ago";
        }
        else if (date > monthStart)
        {
            order = 8;
            txt = "Earlier this month";
        }
        else if (date > monthStart.AddMonths(-1))
        {
            order = 9;
            txt = "Last month";
        }
        else if (date > yearStart)
        {
            order = 10;
            txt = "Earlier this year";
        }
        else if (date > yearStart.AddYears(-1))
        {
            order = 11;
            txt = "Last year";
        }
        else
        {
            order = 12;
            txt = "Older";
        }

        return Tuple.Create(order, txt);
    }
    
    
    /// <summary>
    /// Formats the given file size as a human readable string. For use in grouping.
    /// </summary>
    /// <param name="size">File size in bytes.</param>
    public static Tuple<int, string> GroupTextFileSize(long size)
    {
        int order = 0;
        string txt = string.Empty;
        if (size < 10 * 1024)
        {
            order = 0;
            txt = "< 10 KB";
        }
        else if (size < 100 * 1024)
        {
            order = 1;
            txt = "10 - 100 KB";
        }
        else if (size < 1024 * 1024)
        {
            order = 2;
            txt = "100 KB - 1 MB";
        }
        else if (size < 10 * 1024 * 1024)
        {
            order = 3;
            txt = "1 - 10 MB";
        }
        else if (size < 100 * 1024 * 1024)
        {
            order = 4;
            txt = "10 - 100 MB";
        }
        else if (size < 1024 * 1024 * 1024)
        {
            order = 5;
            txt = "100 MB - 1 GB";
        }
        else
        {
            order = 6;
            txt = "> 1 GB";
        }
        return Tuple.Create(order, txt);
    }
    
    
    /// <summary>
    /// Formats the given image size as a human readable string.
    /// </summary>
    /// <param name="size">Image dimension.</param>
    public static Tuple<int, string> GroupTextDimension(Size size)
    {
        int order = 0;
        string txt = string.Empty;
        if (size.Width <= 32 && size.Height <= 32)
        {
            order = 0;
            txt = "Icon";
        }
        else if (size.Width <= 240 && size.Height <= 240)
        {
            order = 1;
            txt = "Small";
        }
        else if (size.Width <= 640 && size.Height <= 640)
        {
            order = 2;
            txt = "Medium";
        }
        else if (size.Width <= 1280 && size.Height <= 1280)
        {
            order = 3;
            txt = "Large";
        }
        else
        {
            order = 4;
            txt = "Very large";
        }
        return Tuple.Create(order, txt);
    }
    
    
    /// <summary>
    /// Formats the given text for display in grouping. Currently returns
    /// the first letter of the text.
    /// </summary>
    /// <param name="text">The text to format.</param>
    public static Tuple<int, string> GroupTextAlpha(string text)
    {
        if (string.IsNullOrEmpty(text))
            text = " ";
        string txt = text.Substring(0, 1).ToUpperInvariant();
        int order = txt[0];
        return Tuple.Create(order, txt);
    }


    /// <summary>
    /// Compares two strings and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    public int CompareStrings(string x, string y, bool natural)
    {
        if (!natural)
            return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase);

        // Following natural sort algorithm is taken from:
        // http://www.interact-sw.co.uk/iangblog/2007/12/13/natural-sorting
        string[] xparts = Regex.Split(x.Replace(" ", ""), "([0-9]+)");
        string[] yparts = Regex.Split(y.Replace(" ", ""), "([0-9]+)");
        for (int i = 0; i < Math.Max(xparts.Length, yparts.Length); i++)
        {
            bool hasx = i < xparts.Length;
            bool hasy = i < yparts.Length;

            if (!(hasx || hasy)) return 0;

            if (!hasx) return -1;
            if (!hasy) return 1;

            string xpart = xparts[i];
            string ypart = yparts[i];
            int res = 0;

            if (int.TryParse(xpart, out int xi) && int.TryParse(ypart, out int yi))
                res = (xi < yi ? -1 : (xi > yi ? 1 : 0));
            else
                res = string.Compare(xpart, ypart, StringComparison.InvariantCultureIgnoreCase);

            if (res != 0) return res;
        }
        return 0;
    }
}