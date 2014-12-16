using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CustomExtensions;

namespace FileOrganizer
{
   public class StringManipulations
   {
      public static string[] Extensions = { ".3gp", ".avi", ".flv", "m4v", ".mkv", ".mov", ".mp4", ".mpeg", ".mpg", ".wmv", ".wtv" };

      // Checks if movie or tv show
      public static bool IsMovie(string fullpath)
      {
         var match = Regex.Match(Path.GetFileName(fullpath), @"(s\d{1,2}e\d{1,2})|(s\d{2,4})|(\d{1,2}[a-zA-Z]\d{1,2})", RegexOptions.IgnoreCase);
         return !match.Success && !fullpath.Contains("season", StringComparison.InvariantCultureIgnoreCase);
      }

      // Check if video file
      public static bool IsVideo(string f)
      {
         var fi = new FileInfo(f);
         var fileSize = fi.Length / (1024 * 1024); // converts file size from bytes to mbs

         return Array.IndexOf(Extensions, Path.GetExtension(f)) > -1 && fileSize > 100;
      }

      // Capitalizes the word passed in
      public static string UppercaseFirst(string s)
      {
         try
         {
            if (s == string.Empty)
               return string.Empty;

            return char.ToUpper(s[0]) + s.Substring(1);
         }
         catch (IndexOutOfRangeException)
         {
            return string.Empty;
         }
      }

      // Gets Part
      public static string GetPart(string[] vid, int index)
      {
         var ind = Array.FindIndex(vid, v => v.IndexOf("part", StringComparison.InvariantCultureIgnoreCase) >= 0);
         try
         {
            if (vid[ind].Length == 4 && vid[ind + 1].IndexOfAny("0123456789iI".ToCharArray()) != -1)
            {
               return " Part " + vid[ind + 1];
            }
            return string.Empty;
         }
         catch (IndexOutOfRangeException)
         {
            return string.Empty;
         }
      }

      public static string ReplaceStrings(string str, char[] listOfCharsToReplace)
      {
         for (var i = 0; i == str.Length; i++)
         {
            if (listOfCharsToReplace.Contains(str[i]))
               str = str.Replace(str[i], '.');
         }
         return str;
      }
   }
}
