using DokumentyJRWA.Data;
using DokumentyJRWA.Commands;
using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace DokumentyJRWA.ViewModels
{
    public class AddDocumentViewModel : INotifyPropertyChanged
    {
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

       public AddDocumentViewModel(Services.IDialogService dialogService)
{
    _dialogService = dialogService;
    ChooseFileCommand = new RelayCommand(_ => ChooseFile());
    
    // Załaduj kody JRWA
    LoadJrwaCodes();
}

private void LoadJrwaCodes()
{
    try
    {
        // Wczytaj ustawienia
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
        // Ignoruj błędy - lista będzie pusta
    }
}
private void ChooseFile()
{
    var path = _dialogService.OpenFileDialog();
    if (path != null)
    {
        FilePath = path;
        
        // Podpowiedź: ustaw tytuł z nazwy pliku jeśli pusty
        if (string.IsNullOrEmpty(Tytul))
        {
            Tytul = System.IO.Path.GetFileNameWithoutExtension(path);
        }
    }
}
// Skopiuj plik do folderu JRWA
public string CopyFileToJrwaFolder()
{
    try
    {
        // Sprawdź czy wybrano plik
        if (string.IsNullOrEmpty(FilePath) || !System.IO.File.Exists(FilePath))
        {
            throw new Exception("Nie wybrano pliku lub plik nie istnieje!");
        }

        // Sprawdź czy wybrano kod JRWA
        if (string.IsNullOrEmpty(JrwaCode))
        {
            throw new Exception("Musisz wybrać kod JRWA!");
        }

        // Wczytaj ustawienia
        var settingsService = new Services.SettingsService();
        var settings = settingsService.LoadSettings();

        if (string.IsNullOrEmpty(settings.MainFolderPath))
        {
            throw new Exception("Nie ustawiono głównego folderu w ustawieniach!");
        }

        // Znajdź folder dla danego kodu JRWA
        var folderService = new Services.JrwaFolderService();
        string targetFolder = folderService.GetFolderPathByCode(settings.MainFolderPath, JrwaCode);

        if (!System.IO.Directory.Exists(targetFolder))
        {
            // Utwórz folder jeśli nie istnieje
            System.IO.Directory.CreateDirectory(targetFolder);
        }

        // Generuj unikalną nazwę pliku (z datą)
        string fileName = System.IO.Path.GetFileName(FilePath);
        string fileExtension = System.IO.Path.GetExtension(fileName);
        string baseName = System.IO.Path.GetFileNameWithoutExtension(fileName);
        
        // Format: "Tytuł_2026-01-11.pdf"
        string newFileName = $"{baseName}_{DataWplywu:yyyy-MM-dd}{fileExtension}";
        string destinationPath = System.IO.Path.Combine(targetFolder, newFileName);

        // Jeśli plik już istnieje, dodaj numer
        int counter = 1;
        while (System.IO.File.Exists(destinationPath))
        {
            newFileName = $"{baseName}_{DataWplywu:yyyy-MM-dd}_{counter}{fileExtension}";
            destinationPath = System.IO.Path.Combine(targetFolder, newFileName);
            counter++;
        }

        // Skopiuj plik
        System.IO.File.Copy(FilePath, destinationPath);

        // Zwróć nową ścieżkę
        return destinationPath;
    }
    catch (Exception ex)
    {
        throw new Exception($"Błąd podczas kopiowania pliku: {ex.Message}");
    }
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

        public event PropertyChangedEventHandler? PropertyChanged;
protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
{
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
    }
}