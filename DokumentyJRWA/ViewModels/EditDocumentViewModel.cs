using DokumentyJRWA.Data;
using DokumentyJRWA.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DokumentyJRWA.ViewModels
{
    public class EditDocumentViewModel : INotifyPropertyChanged
    {
        private readonly Dokument _originalDokument;
        
        private string _tytul = "";
        public string Tytul
        {
            get => _tytul;
            set { _tytul = value; OnPropertyChanged(); }
        }

        private DateTime _dataWplywu = DateTime.Now;
        public DateTime DataWplywu
        {
            get => _dataWplywu;
            set { _dataWplywu = value; OnPropertyChanged(); }
        }

        private string _jrwaCode = "";
        public string JrwaCode
        {
            get => _jrwaCode;
            set { _jrwaCode = value; OnPropertyChanged(); }
        }

        private string _podmioty = "";
        public string Podmioty
        {
            get => _podmioty;
            set { _podmioty = value; OnPropertyChanged(); }
        }

        private string _filePath = "";
        public string FilePath
        {
            get => _filePath;
            set { _filePath = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> AvailableJrwaCodes { get; set; } = new();

        public ICommand ChooseFileCommand { get; }
        private readonly Services.IDialogService _dialogService;

        public EditDocumentViewModel(Dokument dokument, Services.IDialogService dialogService)
        {
            _originalDokument = dokument;
            _dialogService = dialogService;
            
            // Załaduj dane z dokumentu
            Tytul = dokument.Tytul ?? "";
            DataWplywu = dokument.DataWplywu;
            JrwaCode = dokument.JrwaCode ?? "";
            Podmioty = dokument.Podmioty ?? "";
            FilePath = dokument.FilePath ?? "";

            ChooseFileCommand = new RelayCommand(_ => ChooseFile());
            LoadJrwaCodes();
        }

        private void LoadJrwaCodes()
        {
            try
            {
                var settingsService = new Services.SettingsService();
                var settings = settingsService.LoadSettings();
                
                if (!string.IsNullOrEmpty(settings.JrwaFilePath))
                {
                    var folderService = new Services.JrwaFolderService();
                    var codes = folderService.GetAllJrwaCodes(settings.JrwaFilePath);
                    
                    AvailableJrwaCodes.Clear();
                    foreach (var code in codes)
                    {
                        AvailableJrwaCodes.Add(code);
                    }
                }
            }
            catch
            {
                // Ignoruj błędy
            }
        }

        private void ChooseFile()
        {
            var path = _dialogService.OpenFileDialog();
            if (path != null)
            {
                FilePath = path;
            }
        }

        public Dokument GetUpdatedDokument()
        {
            // Zwróć zaktualizowany dokument (z oryginalnym ID)
            _originalDokument.Tytul = Tytul;
            _originalDokument.DataWplywu = DataWplywu;
            _originalDokument.JrwaCode = JrwaCode;
            _originalDokument.Podmioty = Podmioty;
            _originalDokument.FilePath = FilePath;
            
            return _originalDokument;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
