﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using CustomExtensions;

namespace FileOrganizer
{
   public class StringManipulations
   {
      public static string[] extensions = { ".3gp", ".avi", ".flv", "m4v", ".mkv", ".mov", ".mp4", ".mpeg", ".mpg", ".wmv", ".wtv" };

      // Checks if movie or tv show
      public static bool isMovie(string fullpath)
      {
         Match match = Regex.Match(Path.GetFileName(fullpath), @"(s\d{1,2}e\d{1,2})|(s\d{2,4})|(\d{1,2}[a-zA-Z]\d{1,2})", RegexOptions.IgnoreCase);
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

         if (Array.IndexOf(extensions, Path.GetExtension(f)) > -1 && fileSize > 150)
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
            LogFile lo = new LogFile(ex.ToString());
            return string.Empty;
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
            LogFile lo = new LogFile(ex.ToString());
            return string.Empty;
         }
      }
   }
}
