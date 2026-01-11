using DokumentyJRWA.Models;
using System;
using System.IO;
using System.Text.Json;

namespace DokumentyJRWA.Services
{
    public class JrwaFolderService
    {
        // Wczytaj plik JSON i stwórz foldery
        public void CreateFolderStructure(string jrwaFilePath, string mainFolderPath)
        {
            if (!File.Exists(jrwaFilePath))
            {
                throw new FileNotFoundException($"Plik JRWA nie istnieje: {jrwaFilePath}");
            }

            if (string.IsNullOrWhiteSpace(mainFolderPath))
            {
                throw new ArgumentException("Nie wybrano głównego folderu!");
            }

            // Stwórz główny folder jeśli nie istnieje
            if (!Directory.Exists(mainFolderPath))
            {
                Directory.CreateDirectory(mainFolderPath);
            }

            // Wczytaj JSON
            string json = File.ReadAllText(jrwaFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            JrwaStructure? structure = JsonSerializer.Deserialize<JrwaStructure>(json, options);

            if (structure == null || structure.Categories == null)
            {
                throw new Exception("Nieprawidłowy format pliku JRWA!");
            }

            // Twórz foldery dla każdej kategorii
            foreach (var category in structure.Categories)
            {
                CreateCategoryFolder(mainFolderPath, category);
            }
        }

        // Rekurencyjnie twórz foldery dla kategorii i podkategorii
        private void CreateCategoryFolder(string parentPath, JrwaCategory category)
        {
            // Nazwa folderu: "100 - Organizacja i zarządzanie"
            string folderName = $"{category.Code} - {SanitizeFolderName(category.Name)}";
            string folderPath = Path.Combine(parentPath, folderName);

            // Stwórz folder
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                System.Diagnostics.Debug.WriteLine($"Utworzono: {folderPath}");
            }

            // Jeśli są podkategorie, stwórz je rekurencyjnie
            if (category.Subcategories != null && category.Subcategories.Count > 0)
            {
                foreach (var subcategory in category.Subcategories)
                {
                    CreateCategoryFolder(folderPath, subcategory);
                }
            }
        }

        // Usuń niedozwolone znaki z nazwy folderu
        private string SanitizeFolderName(string name)
        {
            // Usuń znaki niedozwolone w nazwach plików/folderów
            char[] invalidChars = Path.GetInvalidFileNameChars();
            string sanitized = name;

            foreach (char c in invalidChars)
            {
                sanitized = sanitized.Replace(c, '_');
            }

            return sanitized;
        }
// Pobierz wszystkie kody JRWA z pliku (dla dropdowna)
public List<string> GetAllJrwaCodes(string jrwaFilePath)
{
    var codes = new List<string>();
    
    if (!File.Exists(jrwaFilePath))
    {
        return codes; // Pusty jeśli plik nie istnieje
    }

    try
    {
        string json = File.ReadAllText(jrwaFilePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        JrwaStructure? structure = JsonSerializer.Deserialize<JrwaStructure>(json, options);

        if (structure?.Categories != null)
        {
            foreach (var category in structure.Categories)
            {
                ExtractCodes(category, codes);
            }
        }
    }
    catch
    {
        // Ignoruj błędy
    }

    return codes;
}

// Pomocnicza metoda rekurencyjna do wyciągania kodów
private void ExtractCodes(JrwaCategory category, List<string> codes)
{
    codes.Add($"{category.Code} - {category.Name}");
    
    if (category.Subcategories != null)
    {
        foreach (var sub in category.Subcategories)
        {
            ExtractCodes(sub, codes);
        }
    }
}

// Znajdź ścieżkę do folderu na podstawie kodu JRWA
public string GetFolderPathByCode(string mainFolderPath, string jrwaCode)
{
    if (string.IsNullOrWhiteSpace(mainFolderPath) || string.IsNullOrWhiteSpace(jrwaCode))
    {
        return mainFolderPath;
    }

    // Usuń opis z kodu jeśli użytkownik wybrał z dropdowna (np. "100-1 - Nazwa" → "100-1")
    string codeOnly = jrwaCode.Split('-')[0].Trim();
    if (jrwaCode.Contains(" - "))
    {
        codeOnly = jrwaCode.Substring(0, jrwaCode.IndexOf(" - "));
    }

    // Szukaj folderu pasującego do kodu
    return FindFolderByCode(mainFolderPath, codeOnly);
}

private string FindFolderByCode(string parentPath, string code)
{
    if (!Directory.Exists(parentPath))
    {
        return parentPath;
    }

    // Szukaj w bieżącym folderze
    foreach (var dir in Directory.GetDirectories(parentPath))
    {
        string folderName = Path.GetFileName(dir);
        
        // Sprawdź czy folder zaczyna się od kodu (np. "100-1 - Nazwa")
        if (folderName.StartsWith(code + " -") || folderName == code)
        {
            return dir;
        }

        // Szukaj rekurencyjnie w podfolderach
        string found = FindFolderByCode(dir, code);
        if (found != dir)
        {
            return found;
        }
    }

    return parentPath; // Nie znaleziono - zwróć parent
}

// ===== EDIT MODE METHODS =====

// Załaduj strukturę z istniejących folderów na dysku
public JrwaStructure LoadStructureFromFolder(string mainFolderPath)
{
    if (!Directory.Exists(mainFolderPath))
    {
        throw new DirectoryNotFoundException($"Folder nie istnieje: {mainFolderPath}");
    }

    var structure = new JrwaStructure
    {
        Version = "1.0",
        Name = "Załadowano z folderu",
        LastModified = DateTime.Now.ToString("yyyy-MM-dd"),
        Categories = new()
    };

    var directories = Directory.GetDirectories(mainFolderPath);
    foreach (var dir in directories)
    {
        var category = LoadCategoryFromFolder(dir);
        structure.Categories.Add(category);
    }

    return structure;
}

// Rekurencyjnie załaduj kategorię z folderu
private JrwaCategory LoadCategoryFromFolder(string folderPath)
{
    string folderName = Path.GetFileName(folderPath);
    var parts = folderName.Split(new[] { " - " }, 2, StringSplitOptions.None);

    var category = new JrwaCategory
    {
        Code = parts.Length > 0 ? parts[0].Trim() : folderName,
        Name = parts.Length > 1 ? parts[1].Trim() : folderName,
        RetentionPeriod = "???", // Nie znamy z samego folderu
        Subcategories = new()
    };

    // Rekurencyjnie załaduj podkategorie
    if (Directory.Exists(folderPath))
    {
        var subdirs = Directory.GetDirectories(folderPath);
        foreach (var subdir in subdirs)
        {
            category.Subcategories.Add(LoadCategoryFromFolder(subdir));
        }
    }

    return category;
}

// Sprawdź czy folder (i podfoldery) zawierają pliki
public bool HasFilesInFolder(string mainFolderPath, string code)
{
    try
    {
        string folderPath = GetFolderPathByCode(mainFolderPath, code);
        if (Directory.Exists(folderPath))
        {
            // Szukaj plików w tym folderze i wszystkich podfolderach
            return Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories).Length > 0;
        }
        return false;
    }
    catch
    {
        return false;
    }
}

// Usuń folder fizyczny z dysku (tylko jeśli pusty lub force=true)
public void DeleteFolder(string mainFolderPath, string code, bool force = false)
{
    string folderPath = GetFolderPathByCode(mainFolderPath, code);
    
    if (!Directory.Exists(folderPath))
    {
        return; // Folder nie istnieje
    }

    if (!force && HasFilesInFolder(mainFolderPath, code))
    {
        throw new InvalidOperationException("Folder zawiera pliki! Użyj force=true aby usunąć mimo to.");
    }

    Directory.Delete(folderPath, true); // true = usuń rekurencyjnie
}

// Zmień nazwę folderu na dysku
public void RenameFolder(string mainFolderPath, JrwaCategory category, string newCode, string newName)
{
    string oldPath = GetFolderPathByCode(mainFolderPath, category.Code);
    
    if (!Directory.Exists(oldPath))
    {
        return; // Folder nie istnieje
    }

    string parentPath = Path.GetDirectoryName(oldPath) ?? mainFolderPath;
    string newFolderName = $"{newCode} - {SanitizeFolderName(newName)}";
    string newPath = Path.Combine(parentPath, newFolderName);

    if (oldPath != newPath && !Directory.Exists(newPath))
    {
        Directory.Move(oldPath, newPath);
    }
}

        
    }
}
