using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using Microsoft.VisualBasic.FileIO;
using System.Threading;

namespace FileOrganizer
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      private long originalSize;
      private double toMove = 0;
      private double moved = 0;
      private List<Locs> locs;

      struct Locs
      {
         public string cur;
         public string dest;
         public string name;
      };

      public MainWindow()
      {
         InitializeComponent();
      }    

      // Runs when window opens
      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         XML.readXML();

         if (checkLocations())
            Videos.ItemsSource = TreeViewModel.setTree();
         else
            createLocationWindow();
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
         // TODO: Fix this function
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

      // Performs cleanup
      private void Clean_Click(object sender, RoutedEventArgs e)
      {
         // TODO: Fix cleanup
         //try
         //{
         //   DirSearch(XML.location);
         //   foreach (string dir in _directories)
         //   {
         //      Debug.WriteLine("Directories to Delete: " + dir);
         //      FileSystem.DeleteDirectory(dir, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
         //   }
         //}
         //catch (IOException ex)
         //{
         //   Debug.WriteLine(ex);
         //}
      }

      // Supplemental method for cleanup
      // Gets each video in the child folders
      private void DirSearch(string sDir)
      {
         // TODO: Fix this function
         // Goes through each folder
         foreach (string d in Directory.GetDirectories(sDir))
         {
            //Finds each file in folder
            if (GetDirectorySize(new DirectoryInfo(d)) < (1024 * 1024))
            {
               //Ensures one level up directory is not already listed to be deleted
               if (!_directories.Contains(Path.GetDirectoryName(d)))
               {
                  _directories.Add(d);
               }
            }
            DirSearch(d);
         }
      }

      #region Move Files
      // Copies files that are selected to destinations
      private void Copy_Click(object sender, RoutedEventArgs e)
      {
         originalSize = (GetDirectorySize(new DirectoryInfo(XML.destTV)) + GetDirectorySize(new DirectoryInfo(XML.destMovies)));
         locs = new List<Locs>();
         foreach (TreeViewModel tr in Videos.Files.Items)
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
         foreach (TreeViewModel tr in Videos.files.Items)
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

                  string pathdir = Path.GetDirectoryName(child.FullPath);
                  if (pathdir != XML.location)
                  {
                     getDir(child.FullPath);
                  }
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

                        string pathdir = Path.GetDirectoryName(ch.FullPath);
                        if (pathdir != XML.location)
                        {
                           getDir(ch.FullPath);
                        }
                     }
                  }
               }
            }
         }
      }
      // Gets the directory the show came from
      private void getDir(string fullp)
      {
         string[] splitdir = fullp.Split('\\');
         string test = XML.location.Split('\\').Last();
         int pos = Array.IndexOf(splitdir, test);
         string dir = "";
         for (int i = 0; i != pos + 2; i++)
         {
            if (i == 0)
            {
               dir = splitdir[i]; ;
            }
            else
            {
               dir = dir + "\\" + splitdir[i];
            }
         }
         if (_directories.FindIndex(x => x == dir) < 0)
         {
            _directories.Add(dir);
         }
      }
      // Gets each video in the child folders
      private static bool vidSearch(string sDir)
      {
         // Finds each file in folder
         foreach (string f in Directory.GetFiles(sDir))
         {
            // Gets each video
            if (StringManipulations.isVideo(f))
            {
               return true;
            }
         }
         // Goes through each folder
         foreach (string d in Directory.GetDirectories(sDir))
         {
            // Finds each file in folder
            foreach (string f in Directory.GetFiles(d))
            {
               // Gets each video
               if (StringManipulations.isVideo(f))
               {
                  return true;
               }
            }
            vidSearch(d);
         }
         return false;
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
               folder = string.Join(" ", folder, splitfold[i]);
            }
            else
            {
               seasonFold = string.Join(" ", seasonFold, splitfold[i]);
            }
         }
         folder = folder.Trim();
         seasonFold = seasonFold.Trim();

         // Creates directories if they do not exists
         if (Directory.Exists(XML.destTV))
         {
            if (!Directory.Exists(XML.destTV + folder))
            {
               Directory.CreateDirectory(XML.destTV + folder); // Creates show folder
            }
            if (!Directory.Exists(XML.destTV + folder + "\\" + seasonFold))
            {
               Directory.CreateDirectory(XML.destTV + folder + "\\" + seasonFold); // Creates season folder
            }
         }
         else
         {
            Directory.CreateDirectory(XML.destTV); // Creates directory to move
            Directory.CreateDirectory(XML.destTV + folder); // Creates show folder
            Directory.CreateDirectory(XML.destTV + folder + "\\" + seasonFold); // Creates season folder
         }

         return string.Join("\\", folder, seasonFold);
      }
      #endregion
      #endregion

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
   }
}
