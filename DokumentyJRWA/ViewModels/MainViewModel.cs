using DokumentyJRWA.Data;
using DokumentyJRWA.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DokumentyJRWA.Commands;

namespace DokumentyJRWA.ViewModels
{
    public class MainViewModel
    {
        public readonly DocumentService _documentService = new();

        public ObservableCollection<Dokument> Dokumenty { get;}
        
        // Commands
        public ICommand AddFileCommand { get; }
        public ICommand ExportImportArchitectureCommand { get; }

        public MainViewModel()
        {
            Dokumenty = new ObservableCollection<Dokument> (
                    _documentService.GetAll()
                );

            AddFileCommand = new RelayCommand(ExecuteAddFile);
            ExportImportArchitectureCommand = new RelayCommand(ExecuteExportImportArchitecture);
        }

        private void ExecuteAddFile(object? obj)
        {
            // Logic for adding a new file will go here
            System.Windows.MessageBox.Show("Dodaj nowy plik - Functionality not implemented yet.");
        }

        private void ExecuteExportImportArchitecture(object? obj)
        {
            // Logic for Export/Import Architecture will go here
            System.Windows.MessageBox.Show("Export/Import Architektury JWRA - Functionality not implemented yet.");
        }
    }
}
