using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomExtensions;

namespace FileOrganizer
{
   public class TreeViewModel : INotifyPropertyChanged
   {
      public string Name { get; private set; }
      public string FullPath { get; private set; }
      public List<TreeViewModel> Children { get; private set; }
      public static TreeViewModel movieTree;
      public static TreeViewModel tvTree;
      public List<string> movies = new List<string>();
      public List<string> tvshows = new List<string>();

      bool isChecked = false;
      bool exists = false;
      TreeViewModel _parent;

      public TreeViewModel(string name)
      {
         Name = name;
         Children = new List<TreeViewModel>();
      }

      public TreeViewModel(string name, string fullPath)
      {
         Name = name;
         FullPath = fullPath;
         Children = new List<TreeViewModel>();
      }

      public bool IsChecked
      {
         get { return isChecked; }
         set
         {
            isChecked = value;
            NotifyPropertyChanged("IsChecked");
         }
      }

      //public bool Exists
      //{
      //   get { return fileExists(Name); }
      //}

      

      //public bool fileExists(T video)
      //{
      //   string vidStart, vidEnd;
      //   var nameSplit = name.Split(new char[] { ' ', '.' }); 

      //   vidStart = nameSplit[0];
      //   vidEnd = nameSplit.Length > 1 ? nameSplit[nameSplit.Length - 1] : "";



      //   return false;
      //}

      private void GetTargetLocationVideos(string dir)
      {
         foreach (string f in Directory.GetFiles(dir))
         {
            if (StringManipulations.isVideo(f))
            {
               if (dir.ToLower().Contains(XML.destTV.ToLower()))
               {
                  tvshows.Add(Path.GetFileNameWithoutExtension(f));
               }
               else if (dir.ToLower().Contains(XML.destMovies.ToLower()))
               {
                  movies.Add(Path.GetFileNameWithoutExtension(f));
               }
            }
         }
         foreach (string d in Directory.GetDirectories(dir))
         {
            if (d != "TV Shows" && d != "Movies")
            {
               foreach (string f in Directory.GetFiles(d))
               {
                  if (StringManipulations.isVideo(f))
                  {
                     if (dir.ToLower().Contains(XML.destTV.ToLower()))
                     {
                        tvshows.Add(Path.GetFileNameWithoutExtension(f));
                     }
                     else if (dir.ToLower().Contains(XML.destMovies))
                     {
                        movies.Add(Path.GetFileNameWithoutExtension(f));
                     }
                  }
               }
            }
            GetTargetLocationVideos(d);
         }
      }

      // Gets each video in the child folders
      private static void DirSearch(string sDir)
      {
         // Goes through each folder
         foreach (string d in Directory.GetDirectories(sDir))
         {
            // Finds each file in folder
            foreach (string f in Directory.GetFiles(d))
            {
               // Gets each video
               if (StringManipulations.isVideo(f))
               {
                  if (StringManipulations.isMovie(f))
                  {
                     Movies m = new Movies(f);
                     // Checks for existing movie
                     //if (!movieExists(m.fil))
                     //{
                        movieTree.Children.Add(new TreeViewModel(m.file, f));
                     //}
                  }
                  else
                  {
                     Show s = new Show(f);
                     // Checks for existing episode
                     //if (!episodeExists(s.file))
                     //{
                        TreeViewModel season = new TreeViewModel(s.folder + " Season " + s.season);
                        checkTree(season).Children.Add(new TreeViewModel(s.file, f));
                     //}
                  }

                  // TODO: Add directories to a txt file so they save then when cleaning check the location for each folder and if they exist delete them
                  //if (!directories.Contains(d.ToLower()))
                  //{
                  //   directories.Add(d.ToLower());
                  //}
               }
            }
            DirSearch(d);
         }
      }

      // Checks for TreeViewItem
      private static TreeViewModel checkTree(TreeViewModel season)
      {
         foreach (TreeViewModel parent in tvTree.Children)
         {
            if (parent.Name == season.Name)
            {
               return parent;
            }
         }

         tvTree.Children.Add(season);
         return season;
      }

      public static List<TreeViewModel> setTree()
      {
         List<TreeViewModel> treeView = new List<TreeViewModel>();
         movieTree = new TreeViewModel("Movies");
         tvTree = new TreeViewModel("TV Shows");

         treeView.Add(movieTree);
         treeView.Add(tvTree);

         DirSearch(XML.location);

         if (tvTree.Children.Count > 0)
            tvTree.Initialize();
         else
            treeView.Remove(tvTree);
         if (movieTree.Children.Count > 0)
            movieTree.Initialize();
         else
            treeView.Remove(movieTree);

         return treeView;
      }

      void Initialize()
      {
         foreach (TreeViewModel child in Children)
         {
            child._parent = this;
            child.Initialize();
         }
      }

      public static List<string> GetSelected()
      {
         List<string> selected = new List<string>();

         //select = recursive method to check each tree view item for selection (if required)

         return selected;
      }

      #region Notify
      public event PropertyChangedEventHandler PropertyChanged;
      protected void NotifyPropertyChanged(string strPropertyName)
      {
         if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(strPropertyName));
      }
      #endregion
   }
}
