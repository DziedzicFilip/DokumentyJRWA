using Microsoft.Win32;
using System.Windows;
using DokumentyJRWA.Views;
using DokumentyJRWA.ViewModels;
namespace DokumentyJRWA.Services
{
    public class DialogService : IDialogService
    {
        public string? OpenFileDialog()
        {
            var dlg = new OpenFileDialog();
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }

        public bool ShowAddDocumentDialog(AddDocumentViewModel vm)
        {
            var window = new AddDocumentWindow { DataContext = vm };
            return window.ShowDialog() == true;
        }
    }
}