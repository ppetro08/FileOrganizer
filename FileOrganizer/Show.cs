using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CustomExtensions;

namespace FileOrganizer
{
   public class Show : StringManipulations
   {
      // Member variables
      public string folder;
      public string file;
      public string extension;
      public string season;
      public string fullpath;

      public Show(string f)
      {
         fullpath = f;
         file = Path.GetFileNameWithoutExtension(fullpath);
         extension = Path.GetExtension(fullpath);
         splitTV();
      }

      // Splits the tv and joins tv name
      private void splitTV()
      {
         // Variables
         string[] splittv = { "" };
         double n;
         string part = "";

         if (file.Contains('-'))
            file = file.Replace("-", ".");

         // Splits tv by space or .
         splittv = file.Split(new char[] { '.', ' ' });

         int mark = splittv.Length;

         // Finds which part it is
         if (file.Contains("part", StringComparison.InvariantCultureIgnoreCase))
         {
            part = getPart(splittv, file);
         }

         // Finds where the end of the tv name is based on common torrent names
         for (int i = 0; i < splittv.Length; i++)
         {
            Match match = Regex.Match(splittv[i], @"s([0-9]{1,2}[0-9]{1,2})", RegexOptions.IgnoreCase);
            if (match.Success)
            {
               mark = i + 1;
               char[] splitseason = splittv[i].ToCharArray();

               getTVSeason(splitseason);

               splittv[i] = splittv[i].ToUpper();
               break;
            }
            else
            {
               season = UppercaseFirst(Path.GetFileName(Path.GetDirectoryName(fullpath)));
            }
         }

         // Capitalizes each word and joins them together
         for (int i = 0; i < mark; i++)
         {
            // Checks to see if split string is a double
            if (double.TryParse(splittv[i], out n))
            {
               // Checks if double is around current year
               if (Convert.ToDouble(splittv[i]) > 1990 && Convert.ToDouble(splittv[i]) < 2020)
               {
                  splittv[i] = "";
                  continue;
               }
            }

            if (splittv[i] == "")
               continue;

            // Changes the begginning of the title and folder 
            if (i == 0)
            {
               file = UppercaseFirst(splittv[i]);
               folder = UppercaseFirst(splittv[i]);
            }
            else
            {
               file = file + " " + UppercaseFirst(splittv[i]); // Puts spaces between each word
            }

            // Changes the name of the folder to include the title of the show
            //if (i < mark - 1)
            //   folder = folder + " " + UppercaseFirst(splittv[i]);
         }

         if (!file.Contains("part", StringComparison.InvariantCultureIgnoreCase))
            file = file + part;
      }

      // Gets tv show season #
      private void getTVSeason(char[] splitseason, bool match2 = false)
      {
         for (int i = 0; i < splitseason.Length; i++)
         {
            if (char.IsNumber(splitseason[i]))
            {
               if (match2 == true)
               {
                  season = splitseason[0].ToString();
                  return;
               }
               if (Convert.ToInt32(splitseason[i].ToString()) == 0)
               {
                  season = splitseason[i + 1].ToString();
                  return;
               }
               else
               {
                  season = splitseason[i].ToString() + splitseason[i + 1].ToString();
                  return;
               }
            }
         }
      }
   }
}
