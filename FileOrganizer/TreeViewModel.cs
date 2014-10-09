using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileOrganizer
{
   public class TreeViewModel : INotifyPropertyChanged
   {
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

      public string Name { get; private set; }
      public string FullPath { get; private set; }
      public List<TreeViewModel> Children { get; private set; }

      bool isChecked = false;
      TreeViewModel _parent;

      public bool IsChecked
      {
         get { return isChecked; }
         set
         {
            isChecked = value;
            NotifyPropertyChanged("IsChecked");
         }
      }

      void Initialize()
      {
         foreach (TreeViewModel child in Children)
         {
            child._parent = this;
            child.Initialize();
         }
      }

      public static List<TreeViewModel> setTree(TreeViewModel tree)
      {
         List<TreeViewModel> treeView = new List<TreeViewModel>();
         treeView.Add(tree);
         tree.Initialize();

         return treeView;
      }

      public static List<string> GetSelected()
      {
         List<string> selected = new List<string>();

         //select = recursive method to check each tree view item for selection (if required)

         return selected;
      }

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
