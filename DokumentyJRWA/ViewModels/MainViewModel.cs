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
        private readonly Services.IDialogService _dialogService = new Services.DialogService();

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
    var addVm = new AddDocumentViewModel(_dialogService);
    if (_dialogService.ShowAddDocumentDialog(addVm))
    {
        var dokument = addVm.ToDokument();
        Dokumenty.Add(dokument);
        _documentService.Add(dokument); 
    }
}

        private void ExecuteExportImportArchitecture(object? obj)
        {
            // Logic for Export/Import Architecture will go here
            System.Windows.MessageBox.Show("Export/Import Architektury JWRA - Functionality not implemented yet.");
        }
    }
}
