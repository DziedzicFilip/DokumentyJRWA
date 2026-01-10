using DokumentyJRWA.Data;
using DokumentyJRWA.Commands;
using System;
using System.Windows.Input;

namespace DokumentyJRWA.ViewModels
{
    public class AddDocumentViewModel
    {
        public string Tytul { get; set; } = "";
        public DateTime DataWplywu { get; set; } = DateTime.Now;
        public string JrwaCode { get; set; } = "";
        public string Podmioty { get; set; } = "";
        public string FilePath { get; set; } = "";

        public ICommand ChooseFileCommand { get; }
        private readonly Services.IDialogService _dialogService;

        public AddDocumentViewModel(Services.IDialogService dialogService)
        {
            _dialogService = dialogService;
            ChooseFileCommand = new RelayCommand(_ => ChooseFile());
        }

        private void ChooseFile()
        {
            var path = _dialogService.OpenFileDialog();
            if (path != null)
                FilePath = path;
        }

        public Dokument ToDokument()
        {
            return new Dokument
            {
                Id = Guid.NewGuid(),
                Tytul = Tytul,
                DataWplywu = DataWplywu,
                JrwaCode = JrwaCode,
                Podmioty = Podmioty,
                FilePath = FilePath
            };
        }
    }
}