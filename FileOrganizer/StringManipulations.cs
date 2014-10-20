using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CustomExtensions;

namespace FileOrganizer
{
   public class StringManipulations
   {
      public static string[] extensions = { ".3gp", ".avi", ".flv", "m4v", ".mkv", ".mov", ".mp4", ".mpeg", ".mpg", ".wmv", ".wtv" };

      // Checks if movie or tv show
      public static bool isMovie(string fullpath)
      {
         Match match = Regex.Match(Path.GetFileName(fullpath), @"s([0-9].*$)", RegexOptions.IgnoreCase);
         if (match.Success || fullpath.Contains("season", StringComparison.InvariantCultureIgnoreCase))
         {
            return false;
         }
         else
         {
            return true;
         }
      }

      // Check if video file
      public static bool isVideo(string f)
      {
         FileInfo fi = new FileInfo(f);
         long fileSize = fi.Length / (1024 * 1024); // converts file size from bytes to mbs

         if (Array.IndexOf(extensions, Path.GetExtension(f)) > -1 && fileSize > 50)
         {
            return true;
         }
         else
         {
            return false;
         }
      }

      // Capitalizes the word passed in
      public static string UppercaseFirst(string s)
      {
         try
         {
            return char.ToUpper(s[0]) + s.Substring(1);
         }
         catch (IndexOutOfRangeException ex)
         {
            Debug.WriteLine(ex);
            return "";
         }
      }

      // Gets Part
      public static string getPart(string[] vid, string fil)
      {
         int ind = Array.FindIndex(vid, v => v.IndexOf("part", StringComparison.InvariantCultureIgnoreCase) >= 0);
         try
         {
            if (vid[ind].Length == 4 && vid[ind + 1].IndexOfAny("0123456789iI".ToCharArray()) != -1)
            {
               return " Part " + vid[ind + 1];
            }
            else
            {
               return "";
            }
         }
         catch (IndexOutOfRangeException ex)
         {
            LogFile l = new LogFile(ex.ToString());
            return "";
         }
      }
   }
}
