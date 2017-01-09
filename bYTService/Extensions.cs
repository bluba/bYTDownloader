using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace bYTService
{
    public static class Extensions
    {
        public static string FormatWith(this string text, params object[] args)
        {
            return string.Format(text, args);
        }


        public static string EscapeFileName(this string fileName)
        {
            return string.Join("", fileName.Split(Path.GetInvalidFileNameChars()));
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToTimeStamp(this DateTime value)
        {
            TimeSpan elapsedTime = value - Epoch;
            return (long)elapsedTime.TotalSeconds;
        }
    }
}
