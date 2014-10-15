using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace FileOrganizer
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public List<TreeViewModel> TheList { get; set; }

      public MainWindow()
      {
         InitializeComponent();
      }

      // Runs when window opens
      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         Videos.ItemsSource = TreeViewModel.setTree();
      }
   }
}
