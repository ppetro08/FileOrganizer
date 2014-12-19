using System;
using System.IO;
using System.Text.RegularExpressions;
using CustomExtensions;

namespace FileOrganizer
{
   public class Show
   {
      // Member variables
      public string File;
      public string Extension;
      public string Season;
      public string Fullpath;

      public Show(string f)
      {
         Fullpath = f;
         File = Path.GetFileNameWithoutExtension(Fullpath);
         Extension = Path.GetExtension(Fullpath);
         SplitTv();
      }

      // Splits the tv and joins tv name
      private void SplitTv()
      {
         var part = string.Empty;

         var splittv = HelperFunctions.ReplaceStrings(File, new[] { '-', ' ' }).Split('.', ' ');
         File = string.Empty;

         // Finds where the end of the tv name is based on common torrent names
         for (var i = 0; i < splittv.Length; i++)
         {
            if (i != splittv.Length - 1)
            {
               if (Regex.Match(splittv[i], @"s\d{1,2}", RegexOptions.IgnoreCase).Success
                   && Regex.Match(splittv[i + 1], @"e\d{1,2}", RegexOptions.IgnoreCase).Success)
               {
                  splittv[i] = splittv[i] + splittv[i + 1];
                  splittv[i + 1] = string.Empty;
               }
            }
            if (Regex.Match(splittv[i], @"(s\d{1,2}e\d{1,2})|(s\d{2,4})|(\d{1,2}[a-zA-Z]\d{1,2})", RegexOptions.IgnoreCase).Success)
            {
               var episode = GetEpisode(splittv[i]);
               File = File + " " + episode.ToUpper();
               Season = Season + " Season " + GetSeasonNumber(episode);
               break;
            }

            if (splittv[i].Contains("part", StringComparison.InvariantCultureIgnoreCase))
               part = HelperFunctions.GetPart(splittv, i);
            if (splittv[i].IndexOfAny("~`!@%^*()+>[]{}|".ToCharArray()) != -1)
            {
               splittv[i] = string.Empty;
            }

            if (splittv[i] == string.Empty)
               continue;

            Season = Season + " " + HelperFunctions.UppercaseFirst(splittv[i]);
            File = File + " " + HelperFunctions.UppercaseFirst(splittv[i]);
         }
         File = File + part;
         Season = Season.Trim();
         File = File.Trim();
      }

      private static int GetSeasonNumber(string splitseason)
      {
         return splitseason[1] != 0 ? Convert.ToInt32(splitseason[1] + splitseason[2].ToString()) : Convert.ToInt32(splitseason[2].ToString());
      }

      // Gets tv show season #
      private static string GetEpisode(string splitseason)
      {
         //TODO: Figure out how to catch indexoutofrangeexception and then rename
         if (Regex.Match(splitseason, @"s\d{2}e\d{2}").Success)
            return splitseason;

         if (splitseason.Length > 6) return splitseason;

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
         return splitseason;
      }
   }
}
