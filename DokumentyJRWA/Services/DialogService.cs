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
            // Użyj pełnej nazwy dla WPF
            var dlg = new Microsoft.Win32.OpenFileDialog();
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }

        public bool ShowAddDocumentDialog(AddDocumentViewModel vm)
        {
            var window = new AddDocumentWindow { DataContext = vm };
            return window.ShowDialog() == true;
        }

        public bool ShowEditDocumentDialog(EditDocumentViewModel vm)
        {
            var window = new EditDocumentWindow { DataContext = vm };
            return window.ShowDialog() == true;
        }
    }
}