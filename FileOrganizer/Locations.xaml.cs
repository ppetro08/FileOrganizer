using System;
using System.IO;
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
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Globalization;
using System.Drawing;
using System.Windows.Threading;
using System.Diagnostics;

namespace FileOrganizer
{
   /// <summary>
   /// Interaction logic for Locations.xaml
   /// </summary>
   public partial class Locations : Window
   {
      private bool _validation = true;

      public Locations()
      {
         InitializeComponent();
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         txtLocation.Text = XML.location;
         txtMovies.Text = XML.destMovies;
         txtShows.Text = XML.destTV;

         locLost(sender, e);
         moviesLost(sender, e);
         showsLost(sender, e);
      }

      // Validation for textboxes
      #region TextBox Validation
      // Check if all checkboxes are validated
      /*private void validation() {
          boxValidation("location");
          boxValidation("movies");
          boxValidation("shows");
      }
      // Validates individual textboxes
      private void boxValidation(string box) {
          if (box == "location") {
              textBox = txtLocation;
          } else if (box == "movies") {
              textBox = txtMovies;
          } else {
              textBox = txtShows;
          }

          if (textBox.Text.Trim().Length != 0) {
              if (Directory.Exists(textBox.Text.Trim())) {
                  textBox.MouseEnter -= txtTool; // Removes handler
                  textBox.ToolTip = null; // Removes tooltip
                  textBox.ClearValue(BackgroundProperty); // Resets background property to default
                  textBox.ClearValue(BorderBrushProperty); // Resets border property to default
              } else {
                  textBox.BorderBrush = Brushes.Red;
                  textBox.BorderThickness = new Thickness(1);
                  Color c = Colors.Red;
                  c.A = 20;
                  textBox.Background = new SolidColorBrush(c);
                  textBox.MouseEnter += txtTool;
                  _validation = false;
              }
          } else {
              textBox.BorderBrush = Brushes.Red;
              textBox.BorderThickness = new Thickness(1);
              Color c = Colors.Red;
              c.A = 20;
              textBox.Background = new SolidColorBrush(c);
              textBox.MouseEnter += txtTool;
              _validation = false;
          }
      }*/
      // Lost focus handler for locations
      private void locLost(object sender, RoutedEventArgs e)
      {
         _validation = true;
         if (txtLocation.Text.Trim().Length != 0)
         {
            if (Directory.Exists(txtLocation.Text.Trim()))
            {
               txtLocation.MouseEnter -= locTool; // Removes handler
               txtLocation.ToolTip = null; // Removes tooltip
               txtLocation.ClearValue(BackgroundProperty); // Resets background property to default
               txtLocation.ClearValue(BorderBrushProperty); // Resets border property to default
            }
            else
            {
               txtLocation.BorderBrush = Brushes.Red;
               txtLocation.BorderThickness = new Thickness(1);
               Color c = Colors.Red;
               c.A = 20;
               txtLocation.Background = new SolidColorBrush(c);
               txtLocation.MouseEnter += locTool;
               _validation = false;
            }
         }
         else
         {
            txtLocation.BorderBrush = Brushes.Red;
            txtLocation.BorderThickness = new Thickness(1);
            Color c = Colors.Red;
            c.A = 20;
            txtLocation.Background = new SolidColorBrush(c);
            txtLocation.MouseEnter += locTool;
            _validation = false;
         }
      }
      // Lost focus handler for movies
      private void moviesLost(object sender, RoutedEventArgs e)
      {
         _validation = true;
         if (txtMovies.Text.Trim().Length != 0)
         {
            if (Directory.Exists(txtMovies.Text.Trim()))
            {
               txtMovies.MouseEnter -= movieTool; // Removes handler
               txtMovies.ToolTip = null; // Removes tooltip
               txtMovies.ClearValue(BackgroundProperty); // Resets background property to default
               txtMovies.ClearValue(BorderBrushProperty); // Resets border property to default
            }
            else
            {
               txtMovies.BorderBrush = Brushes.Red;
               txtMovies.BorderThickness = new Thickness(1);
               Color c = Colors.Red;
               c.A = 20;
               txtMovies.Background = new SolidColorBrush(c);
               txtMovies.MouseEnter += movieTool;
               _validation = false;
            }
         }
         else
         {
            txtMovies.BorderBrush = Brushes.Red;
            txtMovies.BorderThickness = new Thickness(1);
            Color c = Colors.Red;
            c.A = 20;
            txtMovies.Background = new SolidColorBrush(c);
            txtMovies.MouseEnter += movieTool;
            _validation = false;
         }
      }
      // Lost focus handler for shows
      private void showsLost(object sender, RoutedEventArgs e)
      {
         _validation = true;
         if (txtShows.Text.Trim().Length != 0)
         {
            if (Directory.Exists(txtShows.Text.Trim()))
            {
               txtShows.MouseEnter -= showTool; // Removes handler
               txtShows.ToolTip = null; // Removes tooltip
               txtShows.ClearValue(BackgroundProperty); // Resets background property to default
               txtShows.ClearValue(BorderBrushProperty); // Resets border property to default
            }
            else
            {
               txtShows.BorderBrush = Brushes.Red;
               txtShows.BorderThickness = new Thickness(1);
               Color c = Colors.Red;
               c.A = 20;
               txtShows.Background = new SolidColorBrush(c);
               txtShows.MouseEnter += showTool;
               _validation = false;
            }
         }
         else
         {
            txtShows.BorderBrush = Brushes.Red;
            txtShows.BorderThickness = new Thickness(1);
            Color c = Colors.Red;
            c.A = 20;
            txtShows.Background = new SolidColorBrush(c);
            txtShows.MouseEnter += showTool;
            _validation = false;
         }
      }
      // Tool tip handler for location
      private void locTool(object sender, System.Windows.Input.MouseEventArgs e)
      {
         System.Windows.Controls.ToolTip tool = new System.Windows.Controls.ToolTip();
         if (txtLocation.Text.Trim() == "")
         {
            tool.Content = "A location is required";
         }
         else
         {
            tool.Content = "Please enter a valid location";
         }
         txtLocation.ToolTip = tool;
      }
      // Tool tip handler for movies
      private void movieTool(object sender, System.Windows.Input.MouseEventArgs e)
      {
         System.Windows.Controls.ToolTip tool = new System.Windows.Controls.ToolTip();
         if (txtMovies.Text.Trim() == "")
         {
            tool.Content = "A location is required";
         }
         else
         {
            tool.Content = "Please enter a valid location";
         }
         txtMovies.ToolTip = tool;
      }
      // Tool tip handler for shows
      private void showTool(object sender, System.Windows.Input.MouseEventArgs e)
      {
         System.Windows.Controls.ToolTip tool = new System.Windows.Controls.ToolTip();
         if (txtShows.Text.Trim() == "")
         {
            tool.Content = "A location is required";
         }
         else
         {
            tool.Content = "Please enter a valid location";
         }
         txtShows.ToolTip = tool;
      }
      #endregion

      #region Buttons
      // File browser for lcoation of torrents
      private void btnLocation_Click(object sender, RoutedEventArgs e)
      {
         FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
         if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            txtLocation.Text = folderBrowserDialog1.SelectedPath;
         }
         locLost(sender, e);
      }
      // File browser for movies
      private void btnMovies_Click(object sender, RoutedEventArgs e)
      {
         FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
         if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            txtMovies.Text = folderBrowserDialog1.SelectedPath;
         }
         moviesLost(sender, e);
      }
      // File browser for shows
      private void btnShows_Click(object sender, RoutedEventArgs e)
      {
         FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
         if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            txtShows.Text = folderBrowserDialog1.SelectedPath;
         }
         showsLost(sender, e);
      }
      // Writes text boxes to xml config file
      private void btnSave_Click(object sender, RoutedEventArgs e)
      {
         if (_validation)
         {
            // Changes public variables values to textbox value
            if (!txtLocation.Text.Trim().EndsWith("\\"))
               XML.location = txtLocation.Text.Trim() + "\\";
            if (!txtMovies.Text.Trim().EndsWith("\\"))
               XML.destMovies = txtMovies.Text.Trim() + "\\";
            if (!txtShows.Text.Trim().EndsWith("\\"))
               XML.destTV = txtShows.Text.Trim() + "\\";

            XML.writeXML();
            this.Close();
         }
         else
         {
            return;
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
         base.OnClosing(e);
         System.Windows.Application.Current.MainWindow.IsEnabled = true;
      }
   }
}
