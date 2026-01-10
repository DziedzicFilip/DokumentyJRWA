using DokumentyJRWA.Commands;
using DokumentyJRWA.Models;
using DokumentyJRWA.Services;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace DokumentyJRWA.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly SettingsService _settingsService = new();
        private AppSettings _settings = new();

        // Główna ścieżka do folderu dokumentów
        private string _mainFolderPath = string.Empty;
        public string MainFolderPath
        {
            get => _mainFolderPath;
            set
            {
                _mainFolderPath = value;
                OnPropertyChanged();
            }
        }

        // Ścieżka do pliku JRWA
        private string _jrwaFilePath = string.Empty;
        public string JrwaFilePath
        {
            get => _jrwaFilePath;
            set
            {
                _jrwaFilePath = value;
                OnPropertyChanged();
            }
        }

        // Komendy
        public ICommand BrowseMainFolderCommand { get; }
        public ICommand ImportJrwaCommand { get; }
        public ICommand ExportJrwaCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // Referencja do okna (żeby je zamknąć po zapisie)
        public Window? OwnerWindow { get; set; }

        public SettingsViewModel()
        {
            // Załaduj aktualne ustawienia
            _settings = _settingsService.LoadSettings();
            MainFolderPath = _settings.MainFolderPath;
            JrwaFilePath = _settings.JrwaFilePath;

            // Inicjalizuj komendy
            BrowseMainFolderCommand = new RelayCommand(ExecuteBrowseMainFolder);
            ImportJrwaCommand = new RelayCommand(ExecuteImportJrwa);
            ExportJrwaCommand = new RelayCommand(ExecuteExportJrwa);
            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        // Przeglądaj i wybierz główny folder
       private void ExecuteBrowseMainFolder(object? obj)
{
    try
    {
        // Użyj nowoczesnego WPF API (.NET 8)
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Wybierz główny folder dla dokumentów JRWA",
            InitialDirectory = string.IsNullOrEmpty(MainFolderPath)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : MainFolderPath
        };
        bool? result = dialog.ShowDialog();
        
        if (result == true)
        {
            MainFolderPath = dialog.FolderName;
            
            System.Windows.MessageBox.Show(
                $"Folder został wybrany:\n{MainFolderPath}",
                "Sukces",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
    catch (Exception ex)
    {
        System.Windows.MessageBox.Show(
            $"Błąd podczas wyboru folderu:\n{ex.Message}",
            "Błąd",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );
    }
}


        // Importuj plik JRWA
        private void ExecuteImportJrwa(object? obj)
        {
            // Użyj pełnej nazwy dla WPF OpenFileDialog
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Wybierz plik JRWA do zaimportowania",
                Filter = "Pliki JSON (*.json)|*.json|Pliki XML (*.xml)|*.xml|Wszystkie pliki (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // 1. Importuj plik
                    _settingsService.ImportJrwaFile(dialog.FileName, _settings);
                    JrwaFilePath = _settings.JrwaFilePath;
                    
                    // 2. Sprawdź czy jest wybrany główny folder
                    if (!string.IsNullOrWhiteSpace(MainFolderPath))
                    {
                        // 3. Zapytaj użytkownika czy chce utworzyć foldery
                        var createFolders = System.Windows.MessageBox.Show(
                            $"Czy chcesz automatycznie utworzyć strukturę folderów w:\n{MainFolderPath}?",
                            "Utworzyć foldery?",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question
                        );

                        if (createFolders == MessageBoxResult.Yes)
                        {
                            // 4. Twórz foldery
                            var folderService = new JrwaFolderService();
                            folderService.CreateFolderStructure(JrwaFilePath, MainFolderPath);

                            System.Windows.MessageBox.Show(
                                $"Plik JRWA został zaimportowany!\n\nStruktura folderów została utworzona w:\n{MainFolderPath}",
                                "Sukces",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information
                            );
                        }
                        else
                        {
                            System.Windows.MessageBox.Show(
                                "Plik JRWA został zaimportowany (bez tworzenia folderów).",
                                "Sukces",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information
                            );
                        }
                    }
                    else
                    {
                        // Brak głównego folderu - tylko import
                        System.Windows.MessageBox.Show(
                            "Plik JRWA został zaimportowany!\n\nAby utworzyć strukturę folderów, najpierw wybierz główny folder i zaimportuj plik ponownie.",
                            "Sukces",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        $"Błąd: {ex.Message}",
                        "Błąd",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }


        // Eksportuj plik JRWA
        private void ExecuteExportJrwa(object? obj)
        {
            if (string.IsNullOrEmpty(JrwaFilePath) || !File.Exists(JrwaFilePath))
            {
                System.Windows.MessageBox.Show(
                    "Brak pliku JRWA do eksportu. Najpierw zaimportuj plik JRWA.",
                    "Ostrzeżenie",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Eksportuj plik JRWA",
                Filter = "Pliki JSON (*.json)|*.json|Pliki XML (*.xml)|*.xml|Wszystkie pliki (*.*)|*.*",
                FileName = "jrwa_structure.json",
                DefaultExt = ".json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _settingsService.ExportJrwaFile(JrwaFilePath, dialog.FileName);
                    
                    System.Windows.MessageBox.Show(
                        "Plik JRWA został pomyślnie wyeksportowany!",
                        "Sukces",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        $"Błąd eksportu: {ex.Message}",
                        "Błąd",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }

        // Zapisz ustawienia
        private void ExecuteSave(object? obj)
{
    try
    {
        

        if (string.IsNullOrWhiteSpace(MainFolderPath))
        {
            System.Windows.MessageBox.Show("Musisz wpisać ścieżkę!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!Directory.Exists(MainFolderPath))
        {
            Directory.CreateDirectory(MainFolderPath);
        }

        _settings.MainFolderPath = MainFolderPath;
        _settings.JrwaFilePath = JrwaFilePath;
        _settingsService.SaveSettings(_settings);

        System.Windows.MessageBox.Show($"Zapisano!\nFolder: {MainFolderPath}", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
        OwnerWindow?.Close();
    }
    catch (Exception ex)
    {
        System.Windows.MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

        // Anuluj bez zapisu
        private void ExecuteCancel(object? obj)
        {
            OwnerWindow?.Close();
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}