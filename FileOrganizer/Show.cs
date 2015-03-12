using System;
using System.IO;
using System.Text.RegularExpressions;

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
         bool endSeason = false;

         File = File.Replace(DateTime.Now.Year.ToString(), "");
         var splittv = Regex.Split(File, "[^a-zA-Z0-9]+");

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
            if (Regex.Match(splittv[i], @"(s\d{1,2}e\d{1,2})|(s\d{3,4})|(\d{1,2}[a-zA-Z]\d{1,2})", RegexOptions.IgnoreCase).Success)
            {
               var episode = GetEpisode(splittv[i]);
               File = File + " " + episode.ToUpper();
               Season = Season + " Season " + GetSeasonNumber(episode);
               break;
            }

            if (Regex.Match(splittv[i], @"s\d{2}", RegexOptions.IgnoreCase).Success)
            {
               Season = Season + " Season " + GetSeasonNumber(splittv[i]);
               endSeason = true;
            }

            if (splittv[i].Contains("part", StringComparison.InvariantCultureIgnoreCase))
               part = HelperFunctions.GetPart(splittv, i);

            if (splittv[i] == string.Empty)
               continue;
            if (!endSeason)
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
      private static string GetEpisode(string splitSeason)
      {
         if (Regex.Match(splitSeason, @"s\d{2}e\d{2}").Success)
            return splitSeason;

         if (Regex.Match(splitSeason, @"(\d{1,2}[A-Za-z]\d{1,2})").Success)
         {
            var season = String.Empty;
            var episode = String.Empty;

            var seasonMatch = Regex.Match(splitSeason, @"\d{1,2}");

            if (seasonMatch.Success)
            {
               season = seasonMatch.Value;
               episode = seasonMatch.NextMatch().Value;
            }

            if (season.Length == 1)
               season = "s0" + season;
            else
               season = "s" + season;

            if (episode.Length == 1)
               episode = "e0" + episode;
            else
               episode = "e" + episode;

            splitSeason = season + episode;

            return splitSeason;
         }
         return splitSeason;
      }
   }
}
