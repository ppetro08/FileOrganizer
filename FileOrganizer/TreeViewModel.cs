using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomExtensions;
using System.Text.RegularExpressions;

namespace FileOrganizer
{
   public class TreeViewModel : INotifyPropertyChanged
   {
      public string Name { get; private set; }
      public string FullPath { get; private set; }
      public List<TreeViewModel> Children { get; private set; }
      public static TreeViewModel movieTree;
      public static TreeViewModel tvTree;
      public static List<string> movies = new List<string>();
      public static List<string> tvshows = new List<string>();

      bool? isChecked = false;
      bool exists = false;
      TreeViewModel _parent;

      #region Constructors
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
      #endregion

      #region IsChecked

      public bool? IsChecked
      {
         get { return isChecked; }
         set { SetIsChecked(value, true, true); }
      }

      void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
      {
         if (value == isChecked) return;

         isChecked = value;

         if (updateChildren && isChecked.HasValue) Children.ForEach(c => c.SetIsChecked(isChecked, true, false));

         if (updateParent && _parent != null) _parent.VerifyCheckedState();

         NotifyPropertyChanged("IsChecked");
      }

      void VerifyCheckedState()
      {
         bool? state = null;

         for (int i = 0; i < Children.Count; ++i)
         {
            bool? current = Children[i].IsChecked;
            if (i == 0)
            {
               state = current;
            }
            else if (state != current)
            {
               state = null;
               break;
            }
         }

         SetIsChecked(state, false, true);
      }

      #endregion

      #region Exists
      public bool Exists
      {
         get { return fileExists(Name); }
      }

      public bool fileExists(string name)
      {
         if (_parent == null)
            return false;

         string vidStart, vidEnd;
         var nameSplit = name.Split(new char[] { ' ', '.' });

         vidStart = nameSplit[0];
         vidEnd = nameSplit.Length > 1 ? nameSplit[nameSplit.Length - 1] : "";

         if (tvshows != null && _parent.Name.ToLower().Contains("season"))
         {
            for (int i = 0; i < nameSplit.Length; i++)
            {
               Match match = Regex.Match(nameSplit[i], @"s([0-9].*$)", RegexOptions.IgnoreCase);
               if (match.Success)
               {
                  if (tvshows.Any(s => s.ToLower().Contains(vidStart.ToLower()) && s.ToLower().Contains(nameSplit[i].ToLower())))
                  {
                     return true;
                  }
                  else
                  {
                     return false;
                  }
               }
            }
         }
         else if (movies != null && _parent.Name.ToLower().Contains("movies"))
         {
            // check if vidName is not longer than 1 word
            if (nameSplit.Length == 1)
            {
               if (movies.Any(m => m.ToLower().Equals(vidStart)))
                  return true;
               else
                  return false;
            }
            else 
            {
               // check if movs video is longer than 1 that is matched
               if (movies.Any(s => s.Split(new char[] { ' ', '.' })[0].ToLower().Equals(vidStart)
                        && s.Split(new char[] { ' ', '.' })[s.Split(new char[] { ' ', '.' }).Length - 1].ToLower().Equals(vidEnd)
                        && s.Split(new char[] { ' ', '.' }).Length == nameSplit.Length))
               {
                  return true;
               }
               else
               {
                  return false;
               }
            }
         }
         NotifyPropertyChanged("fileExists");

         return false;
      }
      #endregion

      #region Helper Functions
      // Recursively looks through directory passed in and adds videos to respective parents
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

      // Adds target location video names to movies and tvshows list
      private static void populateLists(string dir)
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
            populateLists(d);
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
                     Movie m = new Movie(f);
                     // TODO: add tooltip of file location for dupes
                     // Checks for existing movie
                     //if (!movieExists(m.fil))
                     //{
                     movieTree.Children.Add(new TreeViewModel(m.file, f));
                     //}
                  }
                  else
                  {
                     Show s = new Show(f);
                     // TODO: add tooltip of file location for dupes
                     // Checks for existing episode
                     //if (!episodeExists(s.file))
                     //{
                     // TODO: fix this season check
                     TreeViewModel season = new TreeViewModel(s.folder + " Season " + s.season);
                     checkTree(season).Children.Add(new TreeViewModel(s.file, f));
                     //}
                  }

                  // TODO: Add directories to a txt file so they save then when cleaning check the location for each folder and if they exist delete them
                  // then delete the file one the clean button is used
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

      // Sorts the tree by parent name and then childrens names
      private static List<TreeViewModel> sortTree(List<TreeViewModel> tree)
      {
         // Sorts movies
         tree.ForEach(x => {
            x.Children = x.Children.OrderBy(g => g.Name).ToList();
         });

         // Sorts shows
         tree.ForEach(x => {
            x.Children.OrderBy(g => g.Name).ToList().ForEach(y => {
               y.Children = y.Children.OrderBy(h => h.Name).ToList();
            });
         });

         return tree;
      }
      #endregion

      #region Create Tree
      // Returns tree with TV Shows and Movies
      public static List<TreeViewModel> setTree()
      {
         List<TreeViewModel> treeView = new List<TreeViewModel>();
         movieTree = new TreeViewModel("Movies");
         tvTree = new TreeViewModel("TV Shows");

         // Adds TV Show and Movie Parents
         treeView.Add(movieTree);
         treeView.Add(tvTree);

         DirSearch(XML.location); // Populates videos

         if (tvTree.Children.Count > 0)
         {
            tvTree.Initialize();
            populateLists(XML.destTV);
         }
         else
            treeView.Remove(tvTree);
         if (movieTree.Children.Count > 0)
         {
            movieTree.Initialize();
            populateLists(XML.destMovies);
         }
         else
            treeView.Remove(movieTree);

         treeView = sortTree(treeView);

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
      #endregion

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
