using System;
using System.IO;
using System.Linq;
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
            File = File?.Replace(DateTime.Now.Year.ToString(), "");
            Extension = Path.GetExtension(Fullpath);
            SplitTv();
        }

        // Splits the tv and joins tv name
        private void SplitTv()
        {
            var file = BuildName(File);

            foreach (var s in file.Split(' '))
            {
                if (Regex.Match(s, @"s\d{2}e\d{2}", RegexOptions.IgnoreCase).Success)
                    break;
                Season += " " + s;
            }

            var seasonNumber = Regex.Match(file.Split(' ').Last(), @"\d{2}", RegexOptions.IgnoreCase).Value;
            seasonNumber = (int)char.GetNumericValue(seasonNumber[0]) == 0 ? seasonNumber[1].ToString() : seasonNumber;

            File = file.Trim();
            Season = Season.Trim() + " Season " + seasonNumber;
        }

        public string BuildName(string file)
        {
            var part = string.Empty;
            var name = string.Empty;
            var splittv = Regex.Split(file, "[^a-zA-Z0-9]+");
            var hasPart = file.Contains("part", StringComparison.InvariantCultureIgnoreCase);
            var endSeason = false;

            for (var i = 0; i < splittv.Length; i++)
            {
                var newSplit = string.Empty;
                if (Regex.Match(splittv[i], @"(\d{1,2}[a-z]\d{1,2})|(e\d{1,2})|(s\d{1,2}e\d{1,2})", RegexOptions.IgnoreCase).Success)
                {
                    newSplit = GetEpisode(splittv[i]);
                    if (!hasPart)
                        endSeason = true;
                }

                if (splittv[i].Contains("part", StringComparison.InvariantCultureIgnoreCase))
                {
                    part = HelperFunctions.GetPart(splittv, i);
                    endSeason = true;
                }

                newSplit = newSplit != string.Empty ? newSplit.ToUpperInvariant() : splittv[i];
                name += " " + newSplit;
                if (endSeason) break;
            }

            return name.Trim() + part;
        }

        private string GetEpisode(string splitSeason)
        {
            if (Regex.Match(splitSeason, @"s\d{2}e\d{2}", RegexOptions.IgnoreCase).Success)
                return splitSeason;

            if (Regex.Match(splitSeason, @"(e\d{1,2})", RegexOptions.IgnoreCase).Success)
            {
                var season = string.Empty;
                if (Fullpath.Contains("season", StringComparison.InvariantCultureIgnoreCase))
                    season = Regex.Split(Fullpath, "season", RegexOptions.IgnoreCase)[1];

                if (Regex.Match(Fullpath, @"s\d{2}e\d{2}", RegexOptions.IgnoreCase).Success)
                    season = Regex.Match(Fullpath, @"s\d{2}e\d{2}", RegexOptions.IgnoreCase).Value;

                season = Regex.Match(season, @"\d{1,2}").Value;
                splitSeason = season + Regex.Match(splitSeason, @"(e\d{1,2})", RegexOptions.IgnoreCase).Value;

            }

            if (Regex.Match(splitSeason, @"(\d{1,2}[a-z]\d{1,2})", RegexOptions.IgnoreCase).Success)
            {
                var season = string.Empty;
                var episode = string.Empty;

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

        //public string CheckFile(string file)
        //{
        //    var directory = Path.GetDirectoryName(Fullpath);
        //    if (directory == null) return file;

        //    foreach (var d in directory.Split('\\'))
        //    {
        //        if (d.Contains("season", StringComparison.InvariantCultureIgnoreCase)
        //            || Regex.Match(d, @"s\d{1,2}e\d{1,2}", RegexOptions.IgnoreCase).Success)
        //        {
        //            return BuildName(d);
        //        }
        //    }

        //    return file;
        //}

        // Builds SxxExx for episode
    }
}
