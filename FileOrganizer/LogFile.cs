using System;
using System.IO;

namespace FileOrganizer
{
   public class LogFile
   {
      public static void Log(string logMsg)
      {
         using (var w = File.AppendText("log.txt"))
         {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
            w.WriteLine("  -{0}", logMsg);
            w.WriteLine("-------------------------------------------------------------------");
         }
      }
   }
}
