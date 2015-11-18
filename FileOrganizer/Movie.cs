using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FileOrganizer
{
    public class Movie
    {
        // Member variables
        public string File;
        public string Fullpath;
        public string Extension;
        private readonly List<string> _resolutions = new List<string>
        {
            "720p",
            "1080p"
        };

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

            File = File.Replace(DateTime.Now.Year.ToString(), "");
            var splitmovie = Regex.Split(File, "[^a-zA-Z0-9]+");
            File = string.Empty;

            // Finds where the end of the movie name is based on common torrent names
            foreach (var t in splitmovie)
            {
                if (t == DateTime.Now.Year.ToString() || t == DateTime.Now.AddYears(-1).Year.ToString()
                    || _resolutions.Any(r => r.Equals(t)) || Regex.Match(t, @"([A-Z]{2,}[a-z]+)").Success)
                    break;

                File = File + " " + HelperFunctions.UppercaseFirst(t);
            }
            File = File + part;
            File = File.Trim();
        }
    }
}
