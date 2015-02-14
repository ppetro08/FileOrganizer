using System;
using System.IO;
using System.Text.RegularExpressions;

namespace FileOrganizer
{
   public class HelperFunctions
   {
      public static string[] Extensions = { ".3gp", ".avi", ".flv", "m4v", ".mkv", ".mov", ".mp4", ".mpeg", ".mpg", ".wmv", ".wtv" };

      //public string Rename(string fullpath)
      //{
      //   var array = fullpath.Split('\\');
      //   var dir = string.Join("\\", array.TakeWhile(x => x != array[array.Length - 1]));
      //   var newName = Interaction.InputBox("What would you like the new file Name to be?", "Rename", t.Name);
      //   fullpath.Name = newName;
      //   File.Move(fullpath, dir + "\\" + newName + Path.GetExtension(t.FullPath));
      //}

      // Checks if movie
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
   }
}
