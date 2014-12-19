using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using ToolTip = System.Windows.Controls.ToolTip;

namespace FileOrganizer
{
   /// <summary>
   /// Interaction logic for Locations.xaml
   /// </summary>
   public partial class Locations
   {
      private bool _validation;

      public Locations()
      {
         InitializeComponent();
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         TxtLocation.Text = Xml.Location;
         TxtMovies.Text = Xml.DestMovies;
         TxtShows.Text = Xml.DestTv;

         LocLost(sender, e);
         MoviesLost(sender, e);
         ShowsLost(sender, e);
      }

      // Validation for textboxes
      #region TextBox Validation
      // Check if all checkboxes are validated
      private bool Validation(string directory)
      {
         if (directory.Length != 0)
         {
            if (Directory.Exists(directory) && Directory.GetParent(directory) != null)
            {
               _validation = true;
               return true;
            }
         }
         _validation = false;
         return false;
      }

      private void LocLost(object sender, RoutedEventArgs e)
      {
         if (Validation(TxtLocation.Text.Trim()))
         {
            TxtLocation.MouseEnter -= LocTool; // Removes handler
            TxtLocation.ToolTip = null; // Removes tooltip
            TxtLocation.ClearValue(BackgroundProperty); // Resets background property to default
            TxtLocation.ClearValue(BorderBrushProperty); // Resets border property to default
         }
         else
         {
            TxtLocation.BorderBrush = Brushes.Red;
            TxtLocation.BorderThickness = new Thickness(1);
            var c = Colors.Red;
            c.A = 20;
            TxtLocation.Background = new SolidColorBrush(c);
            TxtLocation.MouseEnter += LocTool;
         }
      }
      // Lost focus handler for movies
      private void MoviesLost(object sender, RoutedEventArgs e)
      {
         if (Validation(TxtMovies.Text.Trim()))
         {
            TxtMovies.MouseEnter -= MovieTool; // Removes handler
            TxtMovies.ToolTip = null; // Removes tooltip
            TxtMovies.ClearValue(BackgroundProperty); // Resets background property to default
            TxtMovies.ClearValue(BorderBrushProperty); // Resets border property to default

         }
         else
         {
            TxtMovies.BorderBrush = Brushes.Red;
            TxtMovies.BorderThickness = new Thickness(1);
            var c = Colors.Red;
            c.A = 20;
            TxtMovies.Background = new SolidColorBrush(c);
            TxtMovies.MouseEnter += MovieTool;
         }
      }
      // Lost focus handler for shows
      private void ShowsLost(object sender, RoutedEventArgs e)
      {
         if (Validation(TxtShows.Text.Trim()))
         {
            TxtShows.MouseEnter -= ShowTool; // Removes handler
            TxtShows.ToolTip = null; // Removes tooltip
            TxtShows.ClearValue(BackgroundProperty); // Resets background property to default
            TxtShows.ClearValue(BorderBrushProperty); // Resets border property to default
         }
         else
         {
            TxtShows.BorderBrush = Brushes.Red;
            TxtShows.BorderThickness = new Thickness(1);
            var c = Colors.Red;
            c.A = 20;
            TxtShows.Background = new SolidColorBrush(c);
            TxtShows.MouseEnter += ShowTool;
         }
      }
      // Tool tip handler for location
      private void LocTool(object sender, MouseEventArgs e)
      {
         var tool = new ToolTip();
         if (TxtLocation.Text.Trim() == string.Empty)
         {
            tool.Content = "A location is required";
         }
         else
         {
            tool.Content = "Please enter a valid location";
         }
         TxtLocation.ToolTip = tool;
      }
      // Tool tip handler for movies
      private void MovieTool(object sender, MouseEventArgs e)
      {
         var tool = new ToolTip();
         if (TxtMovies.Text.Trim() == string.Empty)
         {
            tool.Content = "A location is required";
         }
         else
         {
            tool.Content = "Please enter a valid location";
         }
         TxtMovies.ToolTip = tool;
      }
      // Tool tip handler for shows
      private void ShowTool(object sender, MouseEventArgs e)
      {
         var tool = new ToolTip();
         if (TxtShows.Text.Trim() == string.Empty)
         {
            tool.Content = "A location is required";
         }
         else
         {
            tool.Content = "Please enter a valid location";
         }
         TxtShows.ToolTip = tool;
      }
      #endregion

      #region Buttons
      // File browser for lcoation of torrents
      private void btnLocation_Click(object sender, RoutedEventArgs e)
      {
         var folderBrowserDialog1 = new FolderBrowserDialog();
         if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            TxtLocation.Text = folderBrowserDialog1.SelectedPath;
         }
         LocLost(sender, e);
      }
      // File browser for movies
      private void btnMovies_Click(object sender, RoutedEventArgs e)
      {
         var folderBrowserDialog1 = new FolderBrowserDialog();
         if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            TxtMovies.Text = folderBrowserDialog1.SelectedPath;
         }
         MoviesLost(sender, e);
      }
      // File browser for shows
      private void btnShows_Click(object sender, RoutedEventArgs e)
      {
         var folderBrowserDialog1 = new FolderBrowserDialog();
         if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            TxtShows.Text = folderBrowserDialog1.SelectedPath;
         }
         ShowsLost(sender, e);
      }
      // Writes text boxes to xml config file
      private void btnSave_Click(object sender, RoutedEventArgs e)
      {
         if (_validation)
         {
            // Changes public variables values to textbox value
            if (!TxtLocation.Text.Trim().EndsWith("\\"))
               Xml.Location = TxtLocation.Text.Trim() + "\\";
            else
               Xml.Location = TxtLocation.Text.Trim();
            if (!TxtMovies.Text.Trim().EndsWith("\\"))
               Xml.DestMovies = TxtMovies.Text.Trim() + "\\";
            else
               Xml.DestMovies = TxtMovies.Text.Trim();
            if (!TxtShows.Text.Trim().EndsWith("\\"))
               Xml.DestTv = TxtShows.Text.Trim() + "\\";
            else
               Xml.DestTv = TxtShows.Text.Trim();

            Xml.WriteXml();
            Close();
         }
         else
         {
            MessageBox.Show("The locations entered are not valid.", "Invalid Locations", MessageBoxButton.OK, MessageBoxImage.Error);
         }
      }
      // Closes the window without saving
      private void btnCancel_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
      #endregion

      protected override void OnClosing(CancelEventArgs e)
      {
         base.OnClosing(e);
         Application.Current.MainWindow.IsEnabled = true;
      }
   }
}
