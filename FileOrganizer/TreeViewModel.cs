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
using CustomExtensions;

namespace FileOrganizer
{
   public class TreeViewModel : INotifyPropertyChanged
   {
      public string Name { get; set; }
      public string FullPath { get; private set; }
      public ObservableCollection<TreeViewModel> Children { get; private set; }
      public static TreeViewModel movieTree;
      public static TreeViewModel tvTree;
      public static List<string> movies = new List<string>();
      public static List<string> tvshows = new List<string>();

      bool? isChecked = false;
      TreeViewModel _parent;

      #region Constructors
      public TreeViewModel(string name)
      {
         Name = name;
         Children = new ObservableCollection<TreeViewModel>();
      }

      public TreeViewModel(string name, string fullPath)
      {
         Name = name;
         FullPath = fullPath;
         Children = new ObservableCollection<TreeViewModel>();
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

         if (updateChildren && isChecked.HasValue)
            Children.ToList().ForEach(c => c.SetIsChecked(isChecked, true, false));

         if (updateParent && _parent != null)
            _parent.VerifyCheckedState();

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

         if (tvshows != null && _parent.Name.Contains("season", StringComparison.InvariantCultureIgnoreCase))
         {
            for (int i = 0; i < nameSplit.Length; i++)
            {
               Match match = Regex.Match(nameSplit[i], @"s([0-9].*$)", RegexOptions.IgnoreCase);
               if (match.Success)
               {
                  if (tvshows.Any(s => s.Contains(vidStart, StringComparison.InvariantCultureIgnoreCase) && s.Contains(nameSplit[i], StringComparison.InvariantCultureIgnoreCase)))
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
         else if (movies != null && _parent.Name.Contains("movies", StringComparison.InvariantCultureIgnoreCase))
         {
            // check if vidName is not longer than 1 word
            if (nameSplit.Length == 1)
            {
               if (movies.Any(m => m.Equals(vidStart, StringComparison.InvariantCultureIgnoreCase)))
                  return true;
               else
                  return false;
            }
            else
            {
               // check if movs video is longer than 1 that is matched
               if (movies.Any(s => s.Split(new char[] { ' ', '.' })[0].Equals(vidStart, StringComparison.InvariantCultureIgnoreCase)
                        && s.Split(new char[] { ' ', '.' })[s.Split(new char[] { ' ', '.' }).Length - 1].Equals(vidEnd, StringComparison.InvariantCultureIgnoreCase)
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
      private static void PopulateTree(List<string> videos)
      {
         foreach (var video in videos)
         {
            if (StringManipulations.isVideo(video))
               AddItem(video);
         }
      }

      public static void RenameItem(string oldFullPath, string newFullPath)
      {
         if (StringManipulations.isMovie(oldFullPath))
         {
            var item = movieTree.Children.First(c => c.FullPath == oldFullPath);

            Movie m = new Movie(newFullPath);
            item.Name = m.file;
            item.FullPath = m.fullpath;
         }
         else
         {
            Show s = new Show(oldFullPath);
            var season = new TreeViewModel(s.folder + " Season " + s.season);
            var item = checkTree(season).Children.First(c => c.FullPath == oldFullPath);

            Show ns = new Show(newFullPath);
            var newSeason = new TreeViewModel(ns.folder + " Season " + ns.season);

            if (season != newSeason)
            {
               checkTree(season).Children.Remove(item);
               checkTree(season).Children.Add(new TreeViewModel(ns.file, newFullPath));
            }
            else
            {
               item.Name = ns.file;
               item.FullPath = ns.fullpath;
            }
         }
      }

      public static void DeleteItem(string fullPath)
      {
         if (StringManipulations.isMovie(fullPath))
         {
            var item = movieTree.Children.First(c => c.FullPath == fullPath);

            movieTree.Children.Remove(item);
         }
         else
         {
            Show s = new Show(fullPath);
            var season = new TreeViewModel(s.folder + " Season " + s.season);
            var item = checkTree(season).Children.First(c => c.FullPath == fullPath);
            checkTree(season).Children.Remove(item);
         }
      }

      public static void AddItem(string fullPath)
      {
         if (StringManipulations.isMovie(fullPath))
         {
            Movie m = new Movie(fullPath);
            movieTree.Children.Add(new TreeViewModel(m.file, fullPath));

         }
         else
         {
            Show s = new Show(fullPath);
            TreeViewModel season = new TreeViewModel(s.folder + " Season " + s.season);
            checkTree(season).Children.Add(new TreeViewModel(s.file, fullPath));
         }
      }

      // Adds target location video names to movies and tvshows list
      public static void PopulateDestinationLists(List<string> videos)
      {
         foreach (var video in videos)
         {
            if (video.Contains(XML.destTV, StringComparison.InvariantCultureIgnoreCase))
            {
               tvshows = new List<string>();
               tvshows.Add(Path.GetFileNameWithoutExtension(video));
            }
            else if (video.Contains(XML.destMovies, StringComparison.InvariantCultureIgnoreCase))
            {
               movies = new List<string>();
               movies.Add(Path.GetFileNameWithoutExtension(video));
            }
         }
      }

      // Gets each video in the child folders
      public static List<string> DirSearch(string dir)
      {
         return new List<string>(Directory.GetFiles(dir,
            "*.*", SearchOption.AllDirectories));
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
      private static ObservableCollection<TreeViewModel> sortTree(ObservableCollection<TreeViewModel> tree)
      {
         tree = new ObservableCollection<TreeViewModel>(tree.OrderBy(i => i.Name));

         return tree;
      }
      #endregion

      #region Create Tree
      // Returns tree with TV Shows and Movies
      public static ObservableCollection<TreeViewModel> setTree()
      {
         ObservableCollection<TreeViewModel> treeView = new ObservableCollection<TreeViewModel>();
         movieTree = new TreeViewModel("Movies");
         tvTree = new TreeViewModel("TV Shows");

         // Adds TV Show and Movie Parents
         treeView.Add(movieTree);
         treeView.Add(tvTree);

         // Populates videos
         PopulateTree(DirSearch(XML.location));

         if (tvTree.Children.Count > 0)
         {
            tvTree.Initialize();
            PopulateDestinationLists(DirSearch(XML.destTV));
         }
         else
            treeView.Remove(tvTree);
         if (movieTree.Children.Count > 0)
         {
            movieTree.Initialize();
            PopulateDestinationLists(DirSearch(XML.destMovies));
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
