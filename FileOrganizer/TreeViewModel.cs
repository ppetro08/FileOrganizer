using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CustomExtensions;

namespace FileOrganizer
{
   public class TreeViewModel : INotifyPropertyChanged
   {
      public string Name { get; set; }
      public string FullPath { get; private set; }
      public List<TreeViewModel> Children { get; private set; }
      public static TreeViewModel MovieTree;
      public static TreeViewModel TvTree;
      public static List<string> Movies = new List<string>();
      public static List<string> Tvshows = new List<string>();

      bool? _isChecked = false;
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
         get { return _isChecked; }
         set { SetIsChecked(value, true, true); }
      }

      void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
      {
         if (value == _isChecked) return;

         _isChecked = value;

         if (updateChildren && _isChecked.HasValue)
            Children.ToList().ForEach(c => c.SetIsChecked(_isChecked, true, false));

         if (updateParent && _parent != null)
            _parent.VerifyCheckedState();

         NotifyPropertyChanged("IsChecked");
      }

      void VerifyCheckedState()
      {
         bool? state = null;

         for (var i = 0; i < Children.Count; ++i)
         {
            var current = Children[i].IsChecked;
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
         get { return FileExists(Name); }
      }

      public bool FileExists(string name)
      {
         if (_parent == null)
         {
            return false;
         }

         var nameSplit = name.Split(' ', '.');

         var vidStart = nameSplit[0];
         var vidEnd = nameSplit.Length > 1 ? nameSplit[nameSplit.Length - 1] : string.Empty;

         if (Tvshows != null && _parent.Name.Contains("season", StringComparison.InvariantCultureIgnoreCase))
         {
            foreach (var str in nameSplit)
            {
               var match = Regex.Match(str, @"(s\d{1,2}e\d{1,2})|(s\d{2,4})|(\d{1,2}[a-zA-Z]\d{1,2})", RegexOptions.IgnoreCase);
               if (!match.Success) continue;
               return Tvshows.Any(s => s.Contains(vidStart, StringComparison.InvariantCultureIgnoreCase) && s.Contains(str, StringComparison.InvariantCultureIgnoreCase));
            }
         }
         else if (Movies != null && _parent.Name.Contains("movies", StringComparison.InvariantCultureIgnoreCase))
         {
            // check if vidName is not longer than 1 word
            if (nameSplit.Length == 1)
            {
               return Movies.Any(m => m.Equals(vidStart, StringComparison.InvariantCultureIgnoreCase));
            }
            // check if movs video is longer than 1 that is matched
            return Movies.Any(s => s.Split(' ', '.')[0].Equals(vidStart, StringComparison.InvariantCultureIgnoreCase)
                                   && s.Split(' ', '.')[s.Split(' ', '.').Length - 1].Equals(vidEnd, StringComparison.InvariantCultureIgnoreCase)
                                   && s.Split(' ', '.').Length == nameSplit.Length);
         }
         NotifyPropertyChanged("fileExists");

         return false;
      }
      #endregion

      #region Helper Functions
      private static void PopulateTree(List<string> videos)
      {
         foreach (var video in videos.Where(HelperFunctions.IsVideo))
         {
            AddItem(video);
         }
      }

      public static void RenameItem(string oldFullPath, string newFullPath)
      {
         if (HelperFunctions.IsMovie(oldFullPath))
         {
            var item = MovieTree.Children.First(c => c.FullPath == oldFullPath);

            var m = new Movie(newFullPath);
            item.Name = m.File;
            item.FullPath = m.Fullpath;
            MovieTree.Children = SortTree(MovieTree.Children);
         }
         else
         {
            var s = new Show(oldFullPath);
            var season = new TreeViewModel(s.Season);
            var item = CheckTree(season).Children.First(c => c.FullPath == oldFullPath);

            var ns = new Show(newFullPath);
            var newSeason = new TreeViewModel(ns.Season);

            if (season != newSeason)
            {
               CheckTree(season).Children.Remove(item);

               if (!TvTree.Children.First(c => c.Name == CheckTree(season).Name).Children.Any())
                  TvTree.Children.Remove(CheckTree(season));

               CheckTree(newSeason).Children.Add(new TreeViewModel(ns.File, newFullPath));
            }
            else
            {
               item.Name = ns.File;
               item.FullPath = ns.Fullpath;
            }
            TvTree.Children = SortTree(TvTree.Children);
         }
      }

      public static void DeleteItem(string fullPath)
      {
         if (HelperFunctions.IsMovie(fullPath))
         {
            var item = MovieTree.Children.First(c => c.FullPath == fullPath);

            MovieTree.Children.Remove(item);
            MovieTree.Children = SortTree(MovieTree.Children);
         }
         else
         {
            var s = new Show(fullPath);
            var season = new TreeViewModel(s.Season);
            var item = CheckTree(season).Children.First(c => c.FullPath == fullPath);
            CheckTree(season).Children.Remove(item);

            if (!TvTree.Children.First(c => c.Name == CheckTree(season).Name).Children.Any())
               TvTree.Children.Remove(CheckTree(season));

            TvTree.Children = SortTree(TvTree.Children);
         }
      }

      public static void AddItem(string fullPath)
      {
         if (HelperFunctions.IsMovie(fullPath))
         {
            var m = new Movie(fullPath);
            MovieTree.Children.Add(new TreeViewModel(m.File, fullPath));
            MovieTree.Children = SortTree(MovieTree.Children);
         }
         else
         {
            var s = new Show(fullPath);
            var season = new TreeViewModel(s.Season);
            CheckTree(season).Children.Add(new TreeViewModel(s.File, fullPath));
            TvTree.Children = SortTree(TvTree.Children);
         }
      }

      // Adds target location video names to movies and tvshows list
      public static void PopulateDestinationLists(List<string> videos)
      {
         foreach (var video in videos)
         {
            if (video.Contains(Xml.DestTv, StringComparison.InvariantCultureIgnoreCase))
            {
               Tvshows.Add(Path.GetFileNameWithoutExtension(video));
            }
            else if (video.Contains(Xml.DestMovies, StringComparison.InvariantCultureIgnoreCase))
            {
               Movies.Add(Path.GetFileNameWithoutExtension(video));
            }
         }
      }

      // Gets each video in the child folders
      public static List<string> DirSearch(string dir)
      {
         return new List<string>(Directory.GetFiles(dir, @"*.*", SearchOption.AllDirectories));
      }

      // Checks for TreeViewItem
      private static TreeViewModel CheckTree(TreeViewModel season)
      {
         foreach (var parent in TvTree.Children.Where(parent => parent.Name == season.Name))
         {
            return parent;
         }

         TvTree.Children.Add(season);
         return season;
      }

      // Sorts the tree by parent name and then childrens names
      private static List<TreeViewModel> SortTree(List<TreeViewModel> tree)
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
      public static List<TreeViewModel> SetTree()
      {
         var treeView = new List<TreeViewModel>();
         MovieTree = new TreeViewModel("Movies");
         TvTree = new TreeViewModel("TV Shows");

         // Adds TV Show and Movie Parents
         treeView.Add(MovieTree);
         treeView.Add(TvTree);

         // Populates videos
         PopulateTree(DirSearch(Xml.Location));

         if (TvTree.Children.Count > 0)
         {
            TvTree.Initialize();
            PopulateDestinationLists(DirSearch(Xml.DestTv));
         }
         else
            treeView.Remove(TvTree);

         if (MovieTree.Children.Count > 0)
         {
            MovieTree.Initialize();
            PopulateDestinationLists(DirSearch(Xml.DestMovies));
         }
         else
            treeView.Remove(MovieTree);

         treeView = SortTree(treeView);

         return treeView;
      }

      void Initialize()
      {
         foreach (var child in Children)
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
