using DokumentyJRWA.Models;
using DokumentyJRWA.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;

namespace DokumentyJRWA.Views
{
    public partial class JrwaBuilderWindow : MetroWindow
    {
        public JrwaBuilderWindow()
        {
            InitializeComponent();
            DataContext = new JrwaBuilderViewModel();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is JrwaBuilderViewModel vm && e.NewValue is JrwaCategory category)
            {
                vm.SelectedCategory = category;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
