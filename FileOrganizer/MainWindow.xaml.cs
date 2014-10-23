using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.ComponentModel;
using Microsoft.VisualBasic.FileIO;
using CustomExtensions;
using System.Timers;

namespace FileOrganizer
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      FileSystemWatcher wLoc;
      FileSystemWatcher wTV;
      FileSystemWatcher wMov;
      Timer notificationTimer;
      List<RenamedEventArgs> renamedFiles = new List<RenamedEventArgs>();
      List<FileSystemEventArgs> changedFiles = new List<FileSystemEventArgs>();
      private long originalSize;
      private double toMove = 0;
      private double moved = 0;
      private List<Locs> locs;
      private string filesToDelete = "FilesToDelete.txt";
      BackgroundWorker backgroundWorker1 = new BackgroundWorker();

      struct Locs
      {
         public string cur;
         public string dest;
         public string name;
      };

      public MainWindow()
      {
         InitializeComponent();

         backgroundWorker1.WorkerReportsProgress = true;
         backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
         backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
         backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
      }

      // Runs when window opens
      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         XML.readXML();

         if (checkLocations())
            Videos.ItemsSource = TreeViewModel.setTree();
         else
            createLocationWindow();

         initWatchers();
      }

      #region Location Window
      // Returns true if file directories exist
      private bool checkLocations()
      {
         if (!Directory.Exists(XML.location))
         {
            return false;
         }
         if (!Directory.Exists(XML.destTV))
         {
            return false;
         }
         if (!Directory.Exists(XML.destMovies))
         {
            return false;
         }
         return true;
      }

      // Creates the window to set the location for videos
      private void createLocationWindow()
      {
         Locations l = new Locations();
         l.Owner = this;
         l.ShowDialog();
         Videos.ItemsSource = TreeViewModel.setTree();
      }
      #endregion

      #region Rename
      private void files_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
      {
         TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

         if (treeViewItem != null)
         {
            treeViewItem.Focus();
            e.Handled = true;
         }

         System.Windows.Controls.ContextMenu cm = new System.Windows.Controls.ContextMenu();
         System.Windows.Controls.MenuItem rename = new System.Windows.Controls.MenuItem();

         rename.Header = "Rename";
         rename.Click += rename_Click;

         cm.Items.Add(rename);
         cm.IsOpen = true;
         treeViewItem.ContextMenu = cm;
      }

      private void rename_Click(object sender, EventArgs e)
      {
         // TODO: Change input box
         TreeViewModel t = (TreeViewModel)Videos.SelectedItem;

         var array = t.FullPath.Split('\\');
         string dir = string.Join("\\", array.TakeWhile(x => x != array[array.Length - 1]));
         string newName = Microsoft.VisualBasic.Interaction.InputBox("What would you like the new file name to be?", "Rename", t.Name);
         t.Name = newName;
         File.Move(t.FullPath, dir + "\\" + newName + Path.GetExtension(t.FullPath));
      }

      static TreeViewItem VisualUpwardSearch(DependencyObject source)
      {
         while (source != null && !(source is TreeViewItem))
            source = VisualTreeHelper.GetParent(source);

         return source as TreeViewItem;
      }
      #endregion

      #region Buttons
      // Handler for location menu click
      private void Locations_Click(object sender, RoutedEventArgs e)
      {
         createLocationWindow();
      }

      #region Clean Directory
      private List<string> ReadFile(string filesToDelete)
      {
         if (File.Exists(filesToDelete))
         {
            List<string> AllLines = new List<string>();
            using (StreamReader sr = File.OpenText(filesToDelete))
            {
               while (!sr.EndOfStream)
               {
                  AllLines.Add(sr.ReadLine());
               }
            }
            return AllLines;
         }
         else
         {
            return new List<string>();
         }
      }

      private void DeleteLine(List<string> lines)
      {
         File.WriteAllLines(filesToDelete, File.ReadLines(filesToDelete).Where(l => !lines.Contains(l)).ToList());
      }

      // Performs cleanup
      private void Clean_Click(object sender, RoutedEventArgs e)
      {
         var itemsToDelete = ReadFile(filesToDelete);

         if (!itemsToDelete.Any())
            return;

         foreach (string item in itemsToDelete)
         {
            try
            {
               FileAttributes attr = File.GetAttributes(item);
               if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
               {
                  Debug.WriteLine("Directories to Delete: " + item);
                  FileSystem.DeleteDirectory(item, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
               }
               else
               {
                  FileSystem.DeleteFile(item, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                  var dir = Path.GetDirectoryName(item);
                  if (dir != XML.location && TreeViewModel.DirSearch(dir).Where(v => StringManipulations.isVideo(v)).Count() == 0)
                  {
                     FileSystem.DeleteDirectory(dir, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                  }
               }
            }
            catch (IOException ex)
            {
               Debug.WriteLine(ex);
            }
         }
         DeleteLine(itemsToDelete);
      }

      private void AddFileToDelete(string file)
      {
         using (StreamWriter f = File.AppendText(filesToDelete))
         {
            var dir = Path.GetDirectoryName(file);
            if (dir != XML.location && TreeViewModel.DirSearch(dir).Where(v => StringManipulations.isVideo(v)).Count() == 0)
               f.Write(dir + Environment.NewLine);
            else
               f.Write(file + Environment.NewLine);
         }
      }
      #endregion

      #region Move Files
      // Copies files that are selected to destinations
      private void Copy_Click(object sender, RoutedEventArgs e)
      {
         originalSize = (GetDirectorySize(new DirectoryInfo(XML.destTV)) + GetDirectorySize(new DirectoryInfo(XML.destMovies)));
         locs = new List<Locs>();
         foreach (TreeViewModel tr in Videos.Items)
         {
            getFilesSize(tr);
         }
         backgroundWorker1.RunWorkerAsync(true);
      }
      // Moves files that are selected to destinations
      private void Move_Click(object sender, RoutedEventArgs e)
      {
         originalSize = (GetDirectorySize(new DirectoryInfo(XML.destTV)) + GetDirectorySize(new DirectoryInfo(XML.destMovies)));
         locs = new List<Locs>();
         foreach (TreeViewModel tr in Videos.Items)
         {
            getFilesSize(tr);
         }
         backgroundWorker1.RunWorkerAsync(false);
      }

      // Populates the list of files to be moved
      private void getFilesSize(TreeViewModel t)
      {
         foreach (TreeViewModel child in t.Children)
         {
            string parent = child.Name;
            if (StringManipulations.isMovie(child.Name))
            {
               if (child.IsChecked == true)
               {
                  string ext = Path.GetExtension(child.FullPath);
                  Locs m;
                  m.cur = child.FullPath;
                  m.dest = XML.destMovies + child.Name + ext;
                  m.name = child.Name;
                  locs.Add(m);

                  FileInfo fi = new FileInfo(child.FullPath);
                  toMove += fi.Length;
               }
            }
            else
            {
               foreach (TreeViewModel ch in child.Children)
               {
                  if (ch.IsChecked == true)
                  {
                     if (ch.FullPath != null)
                     {
                        string ext = Path.GetExtension(ch.FullPath);
                        Locs s;
                        s.cur = ch.FullPath;
                        s.dest = XML.destTV + createFolders(parent) + "\\" + ch.Name + ext;
                        s.name = ch.Name;
                        locs.Add(s);

                        FileInfo fi = new FileInfo(ch.FullPath);
                        toMove += fi.Length;
                     }
                  }
               }
            }
         }
      }

      // Creates and returns folder for tv show
      private string createFolders(string fold)
      {
         // Gets folder name
         // Gets season
         string[] splitfold = fold.Split(' ');
         string folder = "";
         string seasonFold = "";
         for (int i = 0; i < splitfold.Length; i++)
         {
            if (i < splitfold.Length - 2)
            {
               folder = folder == "" ? splitfold[i] : folder + " " + splitfold[i];
            }
            else
            {
               seasonFold = seasonFold == "" ? splitfold[i] : seasonFold + " " + splitfold[i];
            }
         }

         // Creates directories if they do not exists
         if (!Directory.Exists(XML.destTV + folder))
         {
            Directory.CreateDirectory(XML.destTV + folder); // Creates show folder
         }
         if (!Directory.Exists(XML.destTV + folder + "\\" + seasonFold))
         {
            Directory.CreateDirectory(XML.destTV + folder + "\\" + seasonFold); // Creates season folder
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
            foreach (Locs l in locs)
            {
               if ((bool)e.Argument == true)
               {
                  this.Dispatcher.Invoke(() => {
                     txtProgress.Text = l.name;
                  });
                  try
                  {
                     CopyFile(l.cur, l.dest);
                     AddFileToDelete(l.cur);
                  }
                  catch (Exception ex)
                  {
                     Debug.WriteLine(ex);
                     LogFile lo = new LogFile(ex.ToString());
                  }
               }
               else
               {
                  this.Dispatcher.Invoke(() => {
                     txtProgress.Text = l.name;
                  });
                  CopyFile(l.cur, l.dest);
                  FileSystem.DeleteFile(l.cur, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
               }
            }
         }
         catch (InvalidOperationException ex)
         {
            Debug.WriteLine(ex);
            // Message to tell them the video is open in another program and to close it
            LogFile lo = new LogFile(ex.ToString());
         }
         catch (Exception ex)
         {
            Debug.WriteLine(ex);
            LogFile lo = new LogFile(ex.ToString());
         }
      }

      private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
      {
         progressBar1.Value = 0;  //report the progress
         txtProgress.Text = "";
         checkAll(false);
      }

      // Copies the files 1 MB at a time
      private void CopyFile(string source, string destination)
      {
         int bytesRead = 0;
         int bytesPerChunk = 1024 * 1024;

         using (FileStream fs = new FileStream(source, FileMode.Open, FileAccess.Read))
         {
            using (BinaryReader br = new BinaryReader(fs))
            {
               using (FileStream fsDest = new FileStream(destination, FileMode.Create))
               {
                  BinaryWriter bw = new BinaryWriter(fsDest);
                  byte[] buffer;

                  for (double i = 0; i < fs.Length; i += bytesPerChunk)
                  {
                     buffer = br.ReadBytes(bytesPerChunk);
                     bw.Write(buffer);
                     bytesRead += bytesPerChunk;
                     moved += bytesPerChunk;
                     double ret = (moved / toMove) * 100;
                     this.Dispatcher.Invoke(() => {
                        backgroundWorker1.ReportProgress((int)ret);  //report the progress
                     });
                  }
               }
            }
         }
      }

      // Updates the progressBar value
      private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
         progressBar1.Value = e.ProgressPercentage;
         // Fix for lagging progressBar
         if (progressBar1.Value > 0)
         {
            progressBar1.Value = e.ProgressPercentage - 1;
            progressBar1.Value = e.ProgressPercentage;
         }
         progressBar1.Value = e.ProgressPercentage;
      }

      // Helper function to get the size of a directory
      private static long GetDirectorySize(DirectoryInfo d)
      {
         long Size = 0;
         // Add file sizes.
         FileInfo[] fis = d.GetFiles();
         foreach (FileInfo fi in fis)
         {
            Size += fi.Length;
         }
         // Add subdirectory sizes.
         DirectoryInfo[] dis = d.GetDirectories();
         foreach (DirectoryInfo di in dis)
         {
            Size += GetDirectorySize(di);
         }
         return (Size);
      }
      #endregion

      private List<TreeViewModel> getChecked()
      {
         List<TreeViewModel> check = new List<TreeViewModel>();
         foreach (TreeViewModel tr in Videos.Items)
         {
            foreach (TreeViewModel child in tr.Children)
            {
               if (child.Children.Count < 1)
               {
                  if (child.IsChecked == true)
                  {
                     check.Add(child);
                  }
               }
               else
               {
                  foreach (TreeViewModel grandchild in child.Children)
                  {
                     if (grandchild.IsChecked == true)
                     {
                        check.Add(grandchild);
                     }
                  }
               }
            }
         }
         return check;
      }

      // Checks all check boxes
      private void checkAll(bool check)
      {
         foreach (TreeViewModel tr in Videos.Items)
         {
            tr.IsChecked = check;
         }
      }

      #region FileSystemWatcher Methods
      // Initializes and turns on filewatcher for each directory
      private void initWatchers()
      {
         // Initializes file watchers
         wLoc = new FileSystemWatcher();
         wTV = new FileSystemWatcher();
         wMov = new FileSystemWatcher();

         // Sets path for each file watcher
         wLoc.Path = XML.location;
         wTV.Path = XML.destTV;
         wMov.Path = XML.destMovies;

         // Sets each to watch subdirectories
         wLoc.IncludeSubdirectories = true;
         wTV.IncludeSubdirectories = true;
         wMov.IncludeSubdirectories = true;

         // Adds event handlers for each directory storing videos
         wLoc.Created += new FileSystemEventHandler(OnChanged);
         wLoc.Changed += new FileSystemEventHandler(OnChanged);
         wLoc.Deleted += new FileSystemEventHandler(OnChanged);
         wLoc.Renamed += new RenamedEventHandler(OnRenamed);

         wTV.Changed += new FileSystemEventHandler(OnChanged);
         wTV.Deleted += new FileSystemEventHandler(OnChanged);
         wTV.Renamed += new RenamedEventHandler(OnRenamed);

         wMov.Changed += new FileSystemEventHandler(OnChanged);
         wMov.Deleted += new FileSystemEventHandler(OnChanged);
         wMov.Renamed += new RenamedEventHandler(OnRenamed);

         // Begins watching
         wLoc.EnableRaisingEvents = true;
         wTV.EnableRaisingEvents = true;
         wMov.EnableRaisingEvents = true;

         // Initialize notification timer
         notificationTimer = new Timer();
         notificationTimer.Elapsed += notificationTimer_Elapsed;
         notificationTimer.Interval = 250;
      }

      private void notificationTimer_Elapsed(object sender, ElapsedEventArgs e)
      {
         if (renamedFiles != null)
            onRenamed();
         if (changedFiles != null)
            onChanged();

         // Stop the timer and wait for the next batch of files.  
         notificationTimer.Stop();

         // Clear your file lists
         renamedFiles = new List<RenamedEventArgs>();
         changedFiles = new List<FileSystemEventArgs>();
      }

      private void onRenamed()
      {

         foreach (var e in renamedFiles)
         {
            this.Dispatcher.Invoke(() => {
               if (e.FullPath.Contains(XML.destTV, StringComparison.InvariantCultureIgnoreCase) ||
                  e.FullPath.Contains(XML.destTV, StringComparison.InvariantCultureIgnoreCase))
               {
                  TreeViewModel.tvshows = new List<string>();
                  TreeViewModel.movies = new List<string>();
                  TreeViewModel.PopulateDestinationLists(TreeViewModel.DirSearch(XML.destTV));
                  TreeViewModel.PopulateDestinationLists(TreeViewModel.DirSearch(XML.destMovies));
               }
               else
               {
                  TreeViewModel.RenameItem(e.OldFullPath, e.FullPath);
               }
               Videos.Items.Refresh();
            });
         }
      }

      private void onChanged()
      {
         foreach (var e in changedFiles)
         {
            this.Dispatcher.Invoke(() => {
               if (e.FullPath.Contains(XML.destTV, StringComparison.InvariantCultureIgnoreCase) ||
                  e.FullPath.Contains(XML.destTV, StringComparison.InvariantCultureIgnoreCase))
               {
                  TreeViewModel.tvshows = new List<string>();
                  TreeViewModel.movies = new List<string>();
                  TreeViewModel.PopulateDestinationLists(TreeViewModel.DirSearch(XML.destTV));
                  TreeViewModel.PopulateDestinationLists(TreeViewModel.DirSearch(XML.destMovies));
               }
               else if (e.ChangeType == WatcherChangeTypes.Created)
               {
                  TreeViewModel.AddItem(e.FullPath);
               }
               else if (e.ChangeType == WatcherChangeTypes.Deleted)
               {
                  TreeViewModel.DeleteItem(e.FullPath);
               }
               Videos.Items.Refresh();
            });
         }
      }

      // When files are renamed in folders update tree
      private void OnRenamed(object sender, RenamedEventArgs e)
      {
         try
         {
            wLoc.EnableRaisingEvents = false;
            wTV.EnableRaisingEvents = false;
            wMov.EnableRaisingEvents = false;
            renamedFiles.Add(e);

            // 'Reset' timer
            notificationTimer.Stop();
            notificationTimer.Start();
         }
         finally
         {
            wLoc.EnableRaisingEvents = true;
            wTV.EnableRaisingEvents = true;
            wMov.EnableRaisingEvents = true;
         }
      }

      // When files are added in folders update tree
      private void OnChanged(object sender, FileSystemEventArgs e)
      {
         try
         {
            wLoc.EnableRaisingEvents = false;
            wTV.EnableRaisingEvents = false;
            wMov.EnableRaisingEvents = false;
            changedFiles.Add(e);

            // 'Reset' timer
            notificationTimer.Stop();
            notificationTimer.Start();
         }
         finally
         {
            wLoc.EnableRaisingEvents = true;
            wTV.EnableRaisingEvents = true;
            wMov.EnableRaisingEvents = true;
         }
      }
      #endregion
   }
}
