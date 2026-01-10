using DokumentyJRWA.ViewModels;
using MahApps.Metro.Controls;

namespace DokumentyJRWA.Views
{
    public partial class SettingsWindow : MetroWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            
            var viewModel = new SettingsViewModel();
            viewModel.OwnerWindow = this; 
            
            DataContext = viewModel;
        }
    }
}