using DokumentyJRWA.Data;
using DokumentyJRWA.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DokumentyJRWA.Commands;

namespace DokumentyJRWA.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DocumentService _documentService = new();
        private readonly Services.IDialogService _dialogService = new Services.DialogService();
        
        // Pełna lista dokumentów z bazy
        private ObservableCollection<Dokument> _dokumenty;
        public ObservableCollection<Dokument> Dokumenty
        {
            get => _dokumenty;
            set
            {
                _dokumenty = value;
                OnPropertyChanged();
                UpdateFilteredDokumenty(); // Odśwież przefiltrowaną listę
            }
        }

        // Tekst filtra
        private string _filterText = string.Empty;
        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                OnPropertyChanged();
                UpdateFilteredDokumenty(); // Odśwież listę po zmianie filtra
            }
        }

        // Przefiltrowana lista dokumentów (ta jest bindowana do DataGrid)
        private ObservableCollection<Dokument> _filteredDokumenty;
        public ObservableCollection<Dokument> FilteredDokumenty
        {
            get => _filteredDokumenty;
            set
            {
                _filteredDokumenty = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand AddFileCommand { get; }
        public ICommand ExportImportArchitectureCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public MainViewModel()
        {
            // Załaduj dokumenty z bazy
            Dokumenty = new ObservableCollection<Dokument>(
                _documentService.GetAll()
            );

            // Na początku pokaż wszystkie dokumenty
            FilteredDokumenty = new ObservableCollection<Dokument>(Dokumenty);

            // Inicjalizuj komendy
            AddFileCommand = new RelayCommand(ExecuteAddFile);
            ExportImportArchitectureCommand = new RelayCommand(ExecuteExportImportArchitecture);
            EditCommand = new RelayCommand(ExecuteEdit);
            DeleteCommand = new RelayCommand(ExecuteDelete);
        }

        // Metoda filtrująca dokumenty
        private void UpdateFilteredDokumenty()
        {
            if (string.IsNullOrWhiteSpace(FilterText))
            {
                // Jeśli filtr jest pusty, pokaż wszystko
                FilteredDokumenty = new ObservableCollection<Dokument>(Dokumenty);
            }
            else
            {
                // Filtruj po tytule, JRWA lub podmiotach (case-insensitive)
                var filtered = Dokumenty.Where(d =>
                    (d.Tytul?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.JrwaCode?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.Podmioty?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();

                FilteredDokumenty = new ObservableCollection<Dokument>(filtered);
            }
        }

        private void ExecuteAddFile(object? obj)
        {
            var addVm = new AddDocumentViewModel(_dialogService);
            if (_dialogService.ShowAddDocumentDialog(addVm))
            {
                var dokument = addVm.ToDokument();
                
                // Dodaj do bazy
                _documentService.Add(dokument);
                
                // Dodaj do kolekcji (UI się automatycznie odświeży)
                Dokumenty.Add(dokument);
                
                // Odśwież przefiltrowaną listę
                UpdateFilteredDokumenty();
            }
        }

        private void ExecuteEdit(object? obj)
        {
            if (obj is Dokument dokument)
            {
                // TODO: Implementacja edycji
                System.Windows.MessageBox.Show($"Edycja dokumentu: {dokument.Tytul}");
            }
        }

        private void ExecuteDelete(object? obj)
        {
            if (obj is Dokument dokument)
            {
                var result = System.Windows.MessageBox.Show(
                    $"Czy na pewno chcesz usunąć dokument:\n{dokument.Tytul}?",
                    "Potwierdzenie usunięcia",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning
                );

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    // TODO: Usuń z bazy przez DocumentService
                    // _documentService.Delete(dokument.Id);
                    
                    Dokumenty.Remove(dokument);
                    UpdateFilteredDokumenty();
                }
            }
        }

        private void ExecuteExportImportArchitecture(object? obj)
        {
             var settingsWindow = new Views.SettingsWindow();
    settingsWindow.Owner = System.Windows.Application.Current.MainWindow;
    settingsWindow.ShowDialog();
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}