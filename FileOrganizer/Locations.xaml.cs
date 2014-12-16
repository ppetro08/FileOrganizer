using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Forms;

namespace FileOrganizer
{
   /// <summary>
   /// Interaction logic for Locations.xaml
   /// </summary>
   public partial class Locations : Window
   {
      private bool _validation = false;

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

      // Validates individual textboxes
      //private void boxValidation(string box) {
      //    if (box == "location") {
      //        textBox = txtLocation;
      //    } else if (box == "movies") {
      //        textBox = txtMovies;
      //    } else {
      //        textBox = txtShows;
      //    }

      //    if (textBox.Text.Trim().Length != 0) {
      //        if (Directory.Exists(textBox.Text.Trim())) {
      //            textBox.MouseEnter -= txtTool; // Removes handler
      //            textBox.ToolTip = null; // Removes tooltip
      //            textBox.ClearValue(BackgroundProperty); // Resets background property to default
      //            textBox.ClearValue(BorderBrushProperty); // Resets border property to default
      //        } else {
      //            textBox.BorderBrush = Brushes.Red;
      //            textBox.BorderThickness = new Thickness(1);
      //            Color c = Colors.Red;
      //            c.A = 20;
      //            textBox.Background = new SolidColorBrush(c);
      //            textBox.MouseEnter += txtTool;
      //            _validation = false;
      //        }
      //    } else {
      //        textBox.BorderBrush = Brushes.Red;
      //        textBox.BorderThickness = new Thickness(1);
      //        Color c = Colors.Red;
      //        c.A = 20;
      //        textBox.Background = new SolidColorBrush(c);
      //        textBox.MouseEnter += txtTool;
      //        _validation = false;
      //    }
      //}

      // Lost focus handler for locations

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
      private void LocTool(object sender, System.Windows.Input.MouseEventArgs e)
      {
         var tool = new System.Windows.Controls.ToolTip();
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
      private void MovieTool(object sender, System.Windows.Input.MouseEventArgs e)
      {
         var tool = new System.Windows.Controls.ToolTip();
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
      private void ShowTool(object sender, System.Windows.Input.MouseEventArgs e)
      {
         var tool = new System.Windows.Controls.ToolTip();
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
            this.Close();
         }
         else
         {
            System.Windows.MessageBox.Show("The locations entered are not valid.", "Invalid Locations", MessageBoxButton.OK, MessageBoxImage.Error);
         }
      }
      // Closes the window without saving
      private void btnCancel_Click(object sender, RoutedEventArgs e)
      {
         this.Close();
      }
      #endregion

      protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
      {
         if (_validation)
         {
            base.OnClosing(e);
            System.Windows.Application.Current.MainWindow.IsEnabled = true;
         }
         else
         {
            System.Windows.MessageBox.Show("The locations entered are not valid.", "Invalid Locations", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Cancel = true;
         }
      }
   }
}
