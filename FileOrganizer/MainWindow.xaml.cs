using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CustomExtensions;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using FileSystem = Microsoft.VisualBasic.FileIO.FileSystem;

namespace FileOrganizer
{

   //TODO: Fix clean
   //TODO: Recheck after setting tree


   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow
   {
      FileSystemWatcher _wLoc;
      FileSystemWatcher _wTv;
      FileSystemWatcher _wMov;
      private double _toMove;
      private double _moved;
      private List<Locs> _locs;
      readonly BackgroundWorker _backgroundWorker1 = new BackgroundWorker();

      struct Locs
      {
         public string Cur;
         public string Dest;
         public string Name;
      };

      public MainWindow()
      {
         InitializeComponent();

         _backgroundWorker1.WorkerReportsProgress = true;
         _backgroundWorker1.DoWork += backgroundWorker1_DoWork;
         _backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;
         _backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
      }

      // Runs when window opens
      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         Xml.ReadXml();

         if (CheckLocations())
            Videos.ItemsSource = TreeViewModel.SetTree();
         else
            CreateLocationWindow();

         InitWatchers();
      }

      #region Location Window
      // Returns true if file directories exist
      private bool CheckLocations()
      {
         if (!Directory.Exists(Xml.Location) || Directory.GetParent(Xml.Location) == null)
         {
            return false;
         }
         if (!Directory.Exists(Xml.DestTv) || Directory.GetParent(Xml.DestTv) == null)
         {
            return false;
         }
         if (!Directory.Exists(Xml.DestMovies) || Directory.GetParent(Xml.DestMovies) == null)
         {
            return false;
         }
         return true;
      }

      // Creates the window to set the location for videos
      private void CreateLocationWindow()
      {
         var prevLocation = Xml.Location;
         var prevTv = Xml.DestTv;
         var prevMov = Xml.DestMovies;

         var l = new Locations {Owner = this};
         l.ShowDialog();

         if (prevTv != Xml.DestTv)
         {
            TreeViewModel.Tvshows = new List<string>();
            TreeViewModel.PopulateDestinationLists(TreeViewModel.DirSearch(Xml.DestTv));
            ResetTree();
         }
         if (prevMov != Xml.DestMovies)
         {
            TreeViewModel.Movies = new List<string>();
            TreeViewModel.PopulateDestinationLists(TreeViewModel.DirSearch(Xml.DestMovies));
            ResetTree();
         }
         if (prevLocation != Xml.Location)
            ResetTree();
      }
      #endregion

      #region Rename
      private void files_MouseRightButtonDown(object sender, RoutedEventArgs e)
      {
         var treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

         if (treeViewItem != null)
         {
            treeViewItem.Focus();
            e.Handled = true;
         }

         var cm = new ContextMenu();
         var rename = new MenuItem {Header = "Rename"};

         rename.Click += rename_Click;

         cm.Items.Add(rename);
         cm.IsOpen = true;
         if (treeViewItem != null) treeViewItem.ContextMenu = cm;
      }

      private void rename_Click(object sender, EventArgs e)
      {
         // TODO: Change input box
         var t = (TreeViewModel)Videos.SelectedItem;

         var array = t.FullPath.Split('\\');
         var dir = string.Join("\\", array.TakeWhile(x => x != array[array.Length - 1]));
         var newName = Interaction.InputBox("What would you like the new file Name to be?", "Rename", t.Name);
         t.Name = newName;
         File.Move(t.FullPath, dir + "\\" + newName + Path.GetExtension(t.FullPath));
      }

      static TreeViewItem VisualUpwardSearch(DependencyObject source)
      {
         while (source != null && !(source is TreeViewItem))
            source = VisualTreeHelper.GetParent(source);

         return (TreeViewItem) source;
      }
      #endregion

      #region Buttons
      // Handler for location menu click
      private void Locations_Click(object sender, RoutedEventArgs e)
      {
         CreateLocationWindow();
      }

      #region Clean Directory
      //private List<string> ReadFile(string filesToDelete)
      //{
      //   if (!File.Exists(filesToDelete)) return new List<string>();

      //   var allLines = new List<string>();
      //   using (var sr = File.OpenText(filesToDelete))
      //   {
      //      while (!sr.EndOfStream)
      //      {
      //         allLines.Add(sr.ReadLine());
      //      }
      //   }
      //   return allLines;
      //}
      
      // Performs cleanup
      private void Clean_Click(object sender, RoutedEventArgs e)
      {
         foreach (var child in Videos.Items.Cast<TreeViewModel>().SelectMany(tr => tr.Children))
         {
            if (child.Children.Count < 1)
            {
               if (child.Exists)
               {
                  DeleteFilesOrDirectories(child.FullPath);
               }
            }
            else
            {
               foreach (var grandchild in child.Children.Where(grandchild => grandchild.Exists))
               {
                  DeleteFilesOrDirectories(grandchild.FullPath);
               }
            }
         }

         ResetTree();
      }

      private static void DeleteFilesOrDirectories(string filePath)
      {
         try
         {
            var dir = Path.GetDirectoryName(filePath);
            if (!Directory.EnumerateFiles(dir).Any(StringManipulations.IsVideo) && dir != Xml.Location)
               FileSystem.DeleteDirectory(dir, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

            FileSystem.DeleteFile(filePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
         }
         catch (IOException ex)
         {
            LogFile.Log(ex.ToString());
         }
      }
      #endregion

      #region Move Files
      // Copies files that are selected to destinations
      private void Copy_Click(object sender, RoutedEventArgs e)
      {
         _locs = new List<Locs>();
         foreach (TreeViewModel tr in Videos.Items)
         {
            GetFilesSize(tr);
         }
         _backgroundWorker1.RunWorkerAsync(true);
      }

      // Moves files that are selected to destinations
      private void Move_Click(object sender, RoutedEventArgs e)
      {
         _locs = new List<Locs>();
         foreach (TreeViewModel tr in Videos.Items)
         {
            GetFilesSize(tr);
         }
         _backgroundWorker1.RunWorkerAsync(false);
      }

      // Populates the list of files to be moved
      private void GetFilesSize(TreeViewModel t)
      {
         foreach (var child in t.Children)
         {
            var parent = child.Name;
            if (StringManipulations.IsMovie(child.Name))
            {
               if (child.IsChecked != true) continue;

               var ext = Path.GetExtension(child.FullPath);
               Locs m;
               m.Cur = child.FullPath;
               m.Dest = Xml.DestMovies + child.Name + ext;
               m.Name = child.Name;
               _locs.Add(m);

               var fi = new FileInfo(child.FullPath);
               _toMove += fi.Length;
            }
            else
            {
               foreach (var ch in child.Children)
               {
                  if (ch.IsChecked != true || ch.FullPath == null) continue;

                  var ext = Path.GetExtension(ch.FullPath);
                  Locs s;
                  s.Cur = ch.FullPath;
                  s.Dest = Xml.DestTv + CreateFolders(parent) + "\\" + ch.Name + ext;
                  s.Name = ch.Name;
                  _locs.Add(s);

                  var fi = new FileInfo(ch.FullPath);
                  _toMove += fi.Length;
               }
            }
         }
      }

      // Creates and returns folder for tv show
      private string CreateFolders(string fold)
      {
         // Gets folder Name
         // Gets season
         var splitfold = fold.Split(' ');
         var folder = string.Empty;
         var seasonFold = string.Empty;
         for (var i = 0; i < splitfold.Length; i++)
         {
            if (i < splitfold.Length - 2)
            {
               folder = folder == string.Empty ? splitfold[i] : folder + " " + splitfold[i];
            }
            else
            {
               seasonFold = seasonFold == string.Empty ? splitfold[i] : seasonFold + " " + splitfold[i];
            }
         }

         // Creates directories if they do not exists
         if (!Directory.Exists(Xml.DestTv + folder))
         {
            Directory.CreateDirectory(Xml.DestTv + folder); // Creates show folder
         }
         if (!Directory.Exists(Xml.DestTv + folder + "\\" + seasonFold))
         {
            Directory.CreateDirectory(Xml.DestTv + folder + "\\" + seasonFold); // Creates season folder
         }

         return string.Join("\\", folder, seasonFold);
      }
      #endregion
      #endregion

      #region Background Worker
      private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
      {
         try
         {
            foreach (var l in _locs)
            {
               if ((bool)e.Argument)
               {
                  Dispatcher.Invoke(() => {
                     TxtProgress.Text = l.Name;
                  });
                  try
                  {
                     CopyFile(l.Cur, l.Dest);
                  }
                  catch (Exception ex)
                  {
                     LogFile.Log(ex.ToString());
                  }
               }
               else
               {
                  Dispatcher.Invoke(() => {
                     TxtProgress.Text = l.Name;
                  });

                  CopyFile(l.Cur, l.Dest);
                  DeleteFilesOrDirectories(l.Cur);
               }
            }
         }
         catch (InvalidOperationException ex)
         {
            // Message to tell them the video is open in another program and to close it
            LogFile.Log(ex.ToString());
         }
         catch (Exception ex)
         {
            LogFile.Log(ex.ToString());
         }
      }

      private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
      {
         ProgressBar1.Value = 0;  //report the progress
         TxtProgress.Text = string.Empty;
         CheckAll(false);
      }

      // Copies the files 1 MB at a time
      private void CopyFile(string source, string destination)
      {
         const int bytesPerChunk = 1024 * 1024;

         using (var fs = new FileStream(source, FileMode.Open, FileAccess.Read))
         {
            using (var br = new BinaryReader(fs))
            {
               using (var fsDest = new FileStream(destination, FileMode.Create))
               {
                  var bw = new BinaryWriter(fsDest);

                  for (double i = 0; i < fs.Length; i += bytesPerChunk)
                  {
                     var buffer = br.ReadBytes(bytesPerChunk);
                     bw.Write(buffer);
                     _moved += bytesPerChunk;
                     var ret = (_moved / _toMove) * 100;
                     Dispatcher.Invoke(() => {
                        _backgroundWorker1.ReportProgress((int)ret);  //report the progress
                     });
                  }
               }
            }
         }
      }

      // Updates the progressBar value
      private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
         ProgressBar1.Value = e.ProgressPercentage;
         // Fix for lagging progressBar
         if (ProgressBar1.Value > 0)
         {
            ProgressBar1.Value = e.ProgressPercentage - 1;
            ProgressBar1.Value = e.ProgressPercentage;
         }
         ProgressBar1.Value = e.ProgressPercentage;
      }

      // Helper function to get the size of a directory
      private static long GetDirectorySize(DirectoryInfo d)
      {
         // Add file sizes.
         var fis = d.GetFiles();
         var size = fis.Sum(fi => fi.Length);

         // Add subdirectory sizes.
         var directories = d.GetDirectories();
         size += directories.Sum(di => GetDirectorySize(di));
         return (size);
      }
      #endregion

      #region Helper Functions
      // Reloads tree
      private void ResetTree()
      {
         Dispatcher.Invoke(() => {
            var checkedNames = GetChecked();
            Videos.ItemsSource = TreeViewModel.SetTree();
            CheckPreviouslyChecked(checkedNames);
         });
      }

      // Retrieves checked items
      private List<string> GetChecked()
      {
         var check = new List<string>();
         foreach (TreeViewModel tr in Videos.Items)
         {
            foreach (var child in tr.Children)
            {
               if (child.Children.Count < 1)
               {
                  if (child.IsChecked == true)
                  {
                     check.Add(child.Name);
                  }
               }
               else
               {
                  check.AddRange(from grandchild in child.Children where grandchild.IsChecked == true select grandchild.Name);
               }
            }
         }
         return check;
      }

      // Checks previously checked items after updating the tree
      private void CheckPreviouslyChecked(List<string> checkedNames)
      {
         foreach (var child in Videos.Items.Cast<TreeViewModel>().SelectMany(tr => tr.Children))
         {
            if (child.Children.Count < 1)
            {
               if (child.IsChecked != true && checkedNames.FirstOrDefault(c => c == child.Name) != null)
               {
                  child.IsChecked = true;
               }
            }
            else
            {
               foreach (var grandchild in child.Children.Where(grandchild => grandchild.IsChecked != true 
                  && checkedNames.FirstOrDefault(c => c == grandchild.Name) != null))
               {
                  grandchild.IsChecked = true;
               }
            }
         }
      }

      // Checks all check boxes
      private void CheckAll(bool check)
      {
         foreach (TreeViewModel tr in Videos.Items)
         {
            tr.IsChecked = check;
         }
      }
      #endregion

      #region FileSystemWatcher Methods
      // Initializes and turns on filewatcher for each directory
      private void InitWatchers()
      {
         // Initializes file watchers
         _wLoc = new FileSystemWatcher();
         _wTv = new FileSystemWatcher();
         _wMov = new FileSystemWatcher();

         // Sets path for each file watcher
         _wLoc.Path = Xml.Location;
         _wTv.Path = Xml.DestTv;
         _wMov.Path = Xml.DestMovies;

         // Sets each to watch subdirectories
         _wLoc.IncludeSubdirectories = true;
         _wTv.IncludeSubdirectories = true;
         _wMov.IncludeSubdirectories = true;

         // Adds event handlers for each directory storing videos
         _wLoc.Created += OnChanged;
         _wLoc.Changed += OnChanged;
         _wLoc.Deleted += OnChanged;
         _wLoc.Renamed += OnRenamed;

         _wTv.Changed += OnChanged;
         _wTv.Deleted += OnChanged;
         _wTv.Renamed += OnRenamed;

         _wMov.Changed += OnChanged;
         _wMov.Deleted += OnChanged;
         _wMov.Renamed += OnRenamed;

         // Begins watching
         _wLoc.EnableRaisingEvents = true;
         _wTv.EnableRaisingEvents = true;
         _wMov.EnableRaisingEvents = true;
      }

      // When files are renamed in folders update tree
      private void OnRenamed(object sender, RenamedEventArgs e)
      {
         try
         {
            _wLoc.EnableRaisingEvents = false;
            _wTv.EnableRaisingEvents = false;
            _wMov.EnableRaisingEvents = false;
            if (!e.FullPath.Contains(Xml.DestTv, StringComparison.InvariantCultureIgnoreCase) &&
                !e.FullPath.Contains(Xml.DestMovies, StringComparison.InvariantCultureIgnoreCase)) return;

            TreeViewModel.Tvshows = new List<string>();
            TreeViewModel.Movies = new List<string>();
            TreeViewModel.PopulateDestinationLists(TreeViewModel.DirSearch(Xml.DestTv));
            TreeViewModel.PopulateDestinationLists(TreeViewModel.DirSearch(Xml.DestMovies));
         }
         finally
         {
            _wLoc.EnableRaisingEvents = true;
            _wTv.EnableRaisingEvents = true;
            _wMov.EnableRaisingEvents = true;
            ResetTree();
         }
      }

      // When files are added or deleted in folders update tree
      private void OnChanged(object sender, FileSystemEventArgs e)
      {
         try
         {
            _wLoc.EnableRaisingEvents = false;
            _wTv.EnableRaisingEvents = false;
            _wMov.EnableRaisingEvents = false;
            if (!e.FullPath.Contains(Xml.DestTv, StringComparison.InvariantCultureIgnoreCase) &&
                !e.FullPath.Contains(Xml.DestMovies, StringComparison.InvariantCultureIgnoreCase)) return;

            TreeViewModel.Tvshows = new List<string>();
            TreeViewModel.Movies = new List<string>();
            TreeViewModel.PopulateDestinationLists(TreeViewModel.DirSearch(Xml.DestTv));
            TreeViewModel.PopulateDestinationLists(TreeViewModel.DirSearch(Xml.DestMovies));
         }
         finally
         {
            _wLoc.EnableRaisingEvents = true;
            _wTv.EnableRaisingEvents = true;
            _wMov.EnableRaisingEvents = true;
            ResetTree();
         }
      }
      #endregion
   }
}
