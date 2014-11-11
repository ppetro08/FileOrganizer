using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            Match match = Regex.Match(splittv[i], @"(s\d{1,2}e\d{1,2})|(s\d{2,4})|(\d{1,2}[a-zA-Z]\d{1,2})", RegexOptions.IgnoreCase);
            if (match.Success)
            {
               mark = i + 1;

               splittv[i] = getSeasonName(splittv[i]).ToUpper();
               getSeasonNumber(splittv[i]);

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
            if (i < mark - 1 && i != 0)
               folder = folder + " " + UppercaseFirst(splittv[i]);
         }

         if (!file.Contains("part", StringComparison.InvariantCultureIgnoreCase))
            file = file + part;
      }

      // Gets tv show season #
      private void getSeasonNumber(string splitseason)
      {
         for (int i = 0; i < splitseason.Length; i++)
         {
            if (char.IsNumber(splitseason[i]))
            {
               if (Convert.ToInt32(splitseason[i].ToString()) == 0)
               {
                  season = splitseason[i + 1].ToString();
                  return;
               }
               else
               {
                  if (splitseason.Length < 4)
                     season = splitseason[i].ToString();
                  else
                     season = splitseason[i].ToString() + splitseason[i + 1].ToString();
                  return;
               }
            }
         }
      }

      // Gets tv show season #
      private string getSeasonName(string splitseason)
      {
         //(s\d{1,2}e\d{1,2})|(s\d{2,4})|(\d{1,2}[a-zA-Z]\d{1,2})
         if (Regex.Match(splitseason, @"^s\d{2}e\d{2}$").Success)
            return splitseason;

         if (splitseason.Length <= 6)
         {
            if (char.ToLower(splitseason[0]) != 's')
            {
               if (splitseason[0] != 0 && (char.ToLower(splitseason[1]) == 'e' || splitseason.Length < 4))
                  splitseason = "0" + splitseason;

               splitseason = "s" + splitseason;
            }
            if (splitseason[1] != '0' && splitseason[2] == 'e')
               splitseason = splitseason[0] + "0" + splitseason.Substring(1);
            if (char.ToLower(splitseason[3]) != 'e')
               splitseason = splitseason.Replace(splitseason[3].ToString(), "e");
            if (splitseason[4] != 0 && splitseason.Length < 6)
               splitseason = splitseason.Substring(0, 4) + "0" + splitseason[splitseason.Length - 1];

         }
         return splitseason;
      }
   }
}
