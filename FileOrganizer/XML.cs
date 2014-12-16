using System.IO;
using System.Text;
using System.Xml;

namespace FileOrganizer
{
   public class Xml
   {
      public static string Location;
      public static string DestMovies;
      public static string DestTv;

      private static XmlDocument _doc = new XmlDocument();
      // Lines created if there is no config file
      private static string[] _lines = {"<?xml version=\"1.0\" encoding=\"utf-8\" ?>", 
                                          "<locations>",
                                          @"  <location></location>",
                                          @"  <movies></movies>",
                                          @"  <tvshows></tvshows>",
                                          "</locations>"};

      private static string _configLoc = "config.xml";

      // Checks if the file exists
      private static void CheckFile()
      {
         if (File.Exists(_configLoc))
         {
            _doc.Load(_configLoc); // Loads the xml file
         }
         else
         {
            File.WriteAllLines(_configLoc, _lines, Encoding.UTF8); // Creates blank config file
            _doc.Load(_configLoc);
            ReadXml();
         }
      }

      // Sets Videos file path variables to config file info
      public static void ReadXml()
      {
         CheckFile();
         Location = _doc.GetElementsByTagName("location")[0].InnerText; // grabs location of Videos
         DestMovies = _doc.GetElementsByTagName("movies")[0].InnerText; // grabs where to move movies
         DestTv = _doc.GetElementsByTagName("tvshows")[0].InnerText; // grabs where to move tv shows
      }

      // Writes to the config file with locations
      public static void WriteXml()
      {
         CheckFile();
         _doc.GetElementsByTagName("location")[0].InnerText = Location; // grabs location of Videos
         _doc.GetElementsByTagName("movies")[0].InnerText = DestMovies; // grabs where to move movies
         _doc.GetElementsByTagName("tvshows")[0].InnerText = DestTv; // grabs where to move tv shows
         _doc.Save(_configLoc);
      }
   }
}
