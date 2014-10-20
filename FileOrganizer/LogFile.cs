using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileOrganizer
{
   class LogFile
   {
      public LogFile(string exception)
      {
         using (StreamWriter w = File.AppendText("log.txt"))
         {
            Log(exception, w);
         }
      }
      public static void Log(string logMsg, TextWriter w)
      {
         w.Write("\r\nLog Entry : ");
         w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
             DateTime.Now.ToLongDateString());
         w.WriteLine("  -{0}", logMsg);
         w.WriteLine("-------------------------------------------------------------------");
      }
   }
}
