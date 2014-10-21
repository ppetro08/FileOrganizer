using System;
using System.IO;
using CustomExtensions;

namespace FileOrganizer
{
   public class Movie : StringManipulations
   {
      // Member variables
      public string folder;
      public string file;
      public string extension;
      public string fullpath;

      public Movie(string f)
      {
         fullpath = f;
         file = Path.GetFileNameWithoutExtension(fullpath);
         extension = Path.GetExtension(fullpath);
         splitMovie();
      }

      // Splits the movie and joins movie name
      private void splitMovie()
      {
         // Variables
         string[] splitmovie = { "" };
         double n;
         string part = "";

         // Splits tv by space or .
         splitmovie = file.Split(new char[] { '.', ' ' });

         int mark = splitmovie.Length;

         // Finds which part it is
         if (file.Contains("part", StringComparison.InvariantCultureIgnoreCase))
         {
            part = getPart(splitmovie, file);
         }

         // Finds where the end of the movie name is based on common torrent names
         for (int i = 0; i < splitmovie.Length; i++)
         {
            // Checks to see if split string is a double
            if (double.TryParse(splitmovie[i], out n))
            {
               // Checks if double is around current year
               if (Convert.ToDouble(splitmovie[i]) > 1990 && Convert.ToDouble(splitmovie[i]) < 2020)
               {
                  mark = i; // Marks position of end of movie title
                  break;
               }
            }
            // Checks to see if split string contains any special characters
            if (splitmovie[i].IndexOfAny("~`!@%^*()+>[]{}|".ToCharArray()) != -1)
            {
               mark = i; // Marks position of end of movie title
               break;
            }
         }

         // Capitalizes each word and joins them together
         for (int i = 0; i < mark; i++)
         {
            if (i == 0)
            {
               file = UppercaseFirst(splitmovie[i]);
            }
            else
            {
               file = file + " " + UppercaseFirst(splitmovie[i]);
            }
         }

         if (!file.Contains("part", StringComparison.InvariantCultureIgnoreCase))
         {
            file = file + part;
         }
      }
   }
}
