using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

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

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         TxtFileName.Text = _originalFileName;
      }

      private void BtnOK_Click(object sender, RoutedEventArgs e)
      {
         if (TxtFileName.Text != _originalFileName)
            MoveFile(TxtFileName.Text);

         Close();
      }

      private void MoveFile(String newFileName)
      {
         var splitFilePath = _originalFilePath.Split('\\');
         var dir = string.Join("\\", splitFilePath.TakeWhile(x => x != splitFilePath[splitFilePath.Length - 1]));
         File.Move(_originalFilePath, dir + "\\" + newFileName + Path.GetExtension(_originalFilePath));
      }

      private void BtnCancel_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }

      protected override void OnClosing(CancelEventArgs e)
      {
         base.OnClosing(e);
         Application.Current.MainWindow.IsEnabled = true;
      }
   }
}
