using DokumentyJRWA.Commands;
using DokumentyJRWA.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;

namespace DokumentyJRWA.ViewModels
{
    public class JrwaBuilderViewModel : INotifyPropertyChanged
    {
        private JrwaCategory? _selectedCategory;
        public JrwaCategory? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsItemSelected));
            }
        }

        public bool IsItemSelected => SelectedCategory != null;

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                _isEditMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCreateMode));
            }
        }

        public bool IsCreateMode => !IsEditMode;

        public ObservableCollection<JrwaCategory> Categories { get; set; } = new();

        public ICommand AddCategoryCommand { get; }
        public ICommand AddSubcategoryCommand { get; }
        public ICommand RemoveCategoryCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand LoadFromFolderCommand { get; }
        public ICommand SaveChangesCommand { get; }

        public JrwaBuilderViewModel()
        {
            AddCategoryCommand = new RelayCommand(_ => AddCategory());
            AddSubcategoryCommand = new RelayCommand(_ => AddSubcategory(), _ => IsItemSelected);
            RemoveCategoryCommand = new RelayCommand(_ => RemoveCategory(), _ => IsItemSelected);
            ExportCommand = new RelayCommand(_ => Export(), _ => IsCreateMode);
            LoadFromFolderCommand = new RelayCommand(_ => LoadFromFolder());
            SaveChangesCommand = new RelayCommand(_ => SaveChanges(), _ => IsEditMode);
        }

        private void AddCategory()
        {
            var newCategory = new JrwaCategory
            {
                Code = "100",
                Name = "Nowa kategoria",
                RetentionPeriod = "10 lat",
                Subcategories = new()
            };
            Categories.Add(newCategory);
            SelectedCategory = newCategory;
        }

        private void AddSubcategory()
        {
            if (SelectedCategory != null)
            {
                // Inicjalizuj Subcategories jeśli nie istnieje
                if (SelectedCategory.Subcategories == null)
                {
                    SelectedCategory.Subcategories = new();
                }

                // Wygeneruj kod podkategorii
                int nextNumber = SelectedCategory.Subcategories.Count + 1;
                var newSub = new JrwaCategory
                {
                    Code = $"{SelectedCategory.Code}-{nextNumber}",
                    Name = "Nowa podkategoria",
                    RetentionPeriod = "5 lat",
                    Subcategories = new() // Umożliw kolejne zagnieżdżenia
                };
                SelectedCategory.Subcategories.Add(newSub);
                
                // TRICK: Wymuś odświeżenie TreeView poprzez przeładowanie kolekcji
                var temp = new ObservableCollection<JrwaCategory>(Categories);
                Categories.Clear();
                foreach (var cat in temp)
                {
                    Categories.Add(cat);
                }
                
                // Zaznacz nowo dodaną podkategorię
                SelectedCategory = newSub;
            }
        }

        private void RemoveCategory()
        {
            if (SelectedCategory != null)
            {
                // W trybie edycji - sprawdź pliki przed usunięciem
                if (IsEditMode)
                {
                    try
                    {
                        var settingsService = new Services.SettingsService();
                        var settings = settingsService.LoadSettings();
                        var folderService = new Services.JrwaFolderService();

                        // Sprawdź czy folder zawiera pliki
                        bool hasFiles = folderService.HasFilesInFolder(settings.MainFolderPath, SelectedCategory.Code);

                        if (hasFiles)
                        {
                            var result = System.Windows.MessageBox.Show(
                                $"UWAGA! Folder '{SelectedCategory.Code} - {SelectedCategory.Name}' zawiera pliki!\n\nCzy na pewno chcesz usunąć folder wraz z WSZYSTKIMI plikami?",
                                "Ostrzeżenie - pliki w folderze!",
                                System.Windows.MessageBoxButton.YesNo,
                                System.Windows.MessageBoxImage.Warning
                            );

                            if (result != System.Windows.MessageBoxResult.Yes)
                            {
                                return; // Anulowano
                            }
                        }

                        // Usuń folder fizyczny z dysku
                        folderService.DeleteFolder(settings.MainFolderPath, SelectedCategory.Code, force: true);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(
                            $"Błąd usuwania folderu: {ex.Message}",
                            "Błąd",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error
                        );
                        return;
                    }
                }

                // Usuń z kolekcji (UI)
                if (Categories.Contains(SelectedCategory))
                {
                    Categories.Remove(SelectedCategory);
                }
                else
                {
                    // Szukaj w podkategoriach
                    foreach (var cat in Categories)
                    {
                        if (cat.Subcategories?.Contains(SelectedCategory) == true)
                        {
                            cat.Subcategories.Remove(SelectedCategory);
                            
                            // Odśwież TreeView
                            var temp = new ObservableCollection<JrwaCategory>(Categories);
                            Categories.Clear();
                            foreach (var c in temp)
                            {
                                Categories.Add(c);
                            }
                            break;
                        }
                    }
                }
                SelectedCategory = null;
                
                // W trybie edycji - automatycznie zapisz zmiany do JSON
                if (IsEditMode)
                {
                    SaveChangesToJson();
                }
            }
        }

        private void SaveChangesToJson()
        {
            try
            {
                var settingsService = new Services.SettingsService();
                var settings = settingsService.LoadSettings();

                var structure = new JrwaStructure
                {
                    Version = "1.0",
                    Name = "Zaktualizowano",
                    LastModified = DateTime.Now.ToString("yyyy-MM-dd"),
                    Categories = new(Categories)
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                string json = JsonSerializer.Serialize(structure, options);
                File.WriteAllText(settings.JrwaFilePath, json);
                
                // Debug: pokaż gdzie zapisano
                System.Diagnostics.Debug.WriteLine($"Auto-save: {settings.JrwaFilePath}");
            }
            catch (Exception ex)
            {
                // Ignoruj błędy auto-zapisu
                System.Diagnostics.Debug.WriteLine($"Błąd auto-save: {ex.Message}");
            }
        }

        private void Export()
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Zapisz strukturę JRWA",
                    Filter = "Pliki JSON (*.json)|*.json",
                    FileName = "jrwa_structure.json",
                    DefaultExt = ".json"
                };

                if (dialog.ShowDialog() == true)
                {
                    var structure = new JrwaStructure
                    {
                        Version = "1.0",
                        Name = "Utworzono przez Kreator JRWA",
                        LastModified = DateTime.Now.ToString("yyyy-MM-dd"),
                        Categories = new(Categories)
                    };

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };

                    string json = JsonSerializer.Serialize(structure, options);
                    File.WriteAllText(dialog.FileName, json);

                    System.Windows.MessageBox.Show(
                        $"Struktura JRWA została zapisana!\n\n{dialog.FileName}",
                        "Sukces",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information
                    );
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Błąd eksportu: {ex.Message}",
                    "Błąd",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        private void LoadFromFolder()
        {
            try
            {
                var settingsService = new Services.SettingsService();
                var settings = settingsService.LoadSettings();

                if (string.IsNullOrEmpty(settings.MainFolderPath))
                {
                    System.Windows.MessageBox.Show(
                        "Najpierw ustaw główny folder w ustawieniach!",
                        "Brak folderu",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning
                    );
                    return;
                }

                var folderService = new Services.JrwaFolderService();
                var structure = folderService.LoadStructureFromFolder(settings.MainFolderPath);

                Categories.Clear();
                foreach (var cat in structure.Categories)
                {
                    Categories.Add(cat);
                }

                IsEditMode = true;

                System.Windows.MessageBox.Show(
                    $"Załadowano {structure.Categories.Count} kategorii z folderu!",
                    "Sukces",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Błąd ładowania: {ex.Message}",
                    "Błąd",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        private void SaveChanges()
        {
            try
            {
                var settingsService = new Services.SettingsService();
                var settings = settingsService.LoadSettings();

                if (string.IsNullOrEmpty(settings.MainFolderPath))
                {
                    System.Windows.MessageBox.Show(
                        "Brak głównego folderu!",
                        "Błąd",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error
                    );
                    return;
                }

                // Zapisz strukturę do JSON
                var structure = new JrwaStructure
                {
                    Version = "1.0",
                    Name = "Zaktualizowano przez edytor",
                    LastModified = DateTime.Now.ToString("yyyy-MM-dd"),
                    Categories = new(Categories)
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                string json = JsonSerializer.Serialize(structure, options);
                File.WriteAllText(settings.JrwaFilePath, json);

                System.Windows.MessageBox.Show(
                    "Zmiany zostały zapisane do pliku JRWA!",
                    "Sukces",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Błąd zapisu: {ex.Message}",
                    "Błąd",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
