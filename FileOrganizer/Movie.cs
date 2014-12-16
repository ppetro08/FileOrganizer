using System.IO;

namespace FileOrganizer
{
   public class Movie
   {
      // Member variables
      public string Folder;
      public string File;
      public string Extension;
      public string Fullpath;

      public Movie(string f)
      {
         Fullpath = f;
         File = Path.GetFileNameWithoutExtension(Fullpath);
         Extension = Path.GetExtension(Fullpath);
         SplitMovie();
      }

      // Splits the movie and joins movie name
      public void SplitMovie()
      {
         var part = string.Empty;

         var splitmovie = StringManipulations.ReplaceStrings(File, new[] {'-', ' '}).Split('.', ' ');
         File = string.Empty;

         //TODO: Check if part of series or has part and put them in a folder together

         // Finds where the end of the movie name is based on common torrent names
         for (var i = 0; i < splitmovie.Length; i++)
         {
            if (splitmovie[i].IndexOfAny("~`!@%^*()+>[]{}|".ToCharArray()) != -1)
               splitmovie[i] = string.Empty;

            File = File + " " + StringManipulations.UppercaseFirst(splitmovie[i]);
         }
         File = File + part;
         File = File.Trim();
      }
   }
}
