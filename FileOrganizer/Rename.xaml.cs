using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FileOrganizer
{
   public partial class Rename
   {
      private readonly string _originalFileName;
      private readonly string _originalFilePath;

      public Rename(String originalFileName, String originalFilePath)
      {
         InitializeComponent();
         _originalFileName = originalFileName;
         _originalFilePath = originalFilePath;
      }

      public Rename(String label, String originalFileName, String originalFilePath)
      {
         InitializeComponent();
         LblRename.Content = label;
         LblRename.Foreground = Brushes.Red;
         _originalFileName = originalFileName;
         _originalFilePath = originalFilePath;
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         TxtFileName.Text = _originalFileName;
      }

      #region Buttons
      private void BtnOK_Click(object sender, RoutedEventArgs e)
      {
         MoveFile();
         Close();
      }

      private void BtnCancel_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
      #endregion

      #region Methods
      private void MoveFile()
      {
         var newFileName = TxtFileName.Text;

         if (newFileName == _originalFileName)
         {
            MessageBox.Show("The new file name is the same as the previous name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
         }

         var splitFilePath = _originalFilePath.Split('\\');
         var dir = string.Join("\\", splitFilePath.TakeWhile(x => x != splitFilePath[splitFilePath.Length - 1]));
         File.Move(_originalFilePath, dir + "\\" + newFileName + Path.GetExtension(_originalFilePath));
      }

      private void txtFileName_KeyDown(Object sender, KeyEventArgs e)
      {
         if (e.Key == Key.Enter)
         {
            MoveFile();
         }
      }

      private void Window_KeyDown(Object sender, KeyEventArgs e)
      {
         if (e.Key == Key.Escape)
         {
            Close();
         }
      }

      protected override void OnClosing(CancelEventArgs e)
      {
         base.OnClosing(e);
         Application.Current.MainWindow.IsEnabled = true;
      }
      #endregion
   }
}
