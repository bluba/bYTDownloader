using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace bYTService
{
    public static class Logger
    {
        private static object o = new Object();
        private static string fileName { get { return Path.Combine(Configuration.Instance.LogDir, DateTime.Now.ToString("yyyMMdd") + ".txt"); } }

        internal static void ErrorException(string v, Exception e)
        {
            lock (o)
                File.AppendAllText(fileName, "[{0}][Error]{1}:{2}\r\n".FormatWith(DateTime.Now, v, e.ToString()));
        }

        internal static void Debug(string v)
        {
            lock (o)
                File.AppendAllText(fileName, "[{0}][Debug]{1}\r\n".FormatWith(DateTime.Now, v));
        }

        internal static void Trace(string v)
        {
            lock (o)
                File.AppendAllText(fileName, "[{0}][Trace]{1}\r\n".FormatWith(DateTime.Now, v));
        }

        internal static void Info(string v)
        {
            lock (o)
                File.AppendAllText(fileName, "[{0}][Info]{1}\r\n".FormatWith(DateTime.Now, v));
        }

        internal static void Error(string v)
        {
            lock (o)
                File.AppendAllText(fileName, "[{0}][Error]{1}\r\n".FormatWith(DateTime.Now, v));
        }
    }
}
