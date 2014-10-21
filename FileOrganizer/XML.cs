using System.IO;
using System.Text;
using System.Xml;

namespace FileOrganizer
{
   class XML
   {
      public static string location;
      public static string destMovies;
      public static string destTV;

      private static XmlDocument doc = new XmlDocument();
      // Lines created if there is no config file
      private static string[] lines = {"<?xml version=\"1.0\" encoding=\"utf-8\" ?>", 
                                          "<locations>",
                                          @"  <location></location>",
                                          @"  <movies></movies>",
                                          @"  <tvshows></tvshows>",
                                          "</locations>"};

      private static string configLoc = "config.xml";

      // Checks if the file exists
      private static void checkFile()
      {
         if (File.Exists(configLoc))
         {
            doc.Load(configLoc); // Loads the xml file
         }
         else
         {
            File.WriteAllLines(configLoc, lines, Encoding.UTF8); // Creates blank config file
            doc.Load(configLoc);
            readXML();
         }
      }

      // Sets Videos file path variables to config file info
      public static void readXML()
      {
         checkFile();
         location = doc.GetElementsByTagName("location")[0].InnerText; // grabs location of Videos
         destMovies = doc.GetElementsByTagName("movies")[0].InnerText; // grabs where to move movies
         destTV = doc.GetElementsByTagName("tvshows")[0].InnerText; // grabs where to move tv shows
      }

      // Writes to the config file with locations
      public static void writeXML()
      {
         checkFile();
         doc.GetElementsByTagName("location")[0].InnerText = location; // grabs location of Videos
         doc.GetElementsByTagName("movies")[0].InnerText = destMovies; // grabs where to move movies
         doc.GetElementsByTagName("tvshows")[0].InnerText = destTV; // grabs where to move tv shows
         doc.Save(configLoc);
      }
   }
}
