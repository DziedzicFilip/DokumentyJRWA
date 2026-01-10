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
    }
}
