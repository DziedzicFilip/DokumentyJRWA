using DokumentyJRWA.Models;
using System;
using System.IO;
using System.Text.Json;

namespace DokumentyJRWA.Services
{
    public class SettingsService
    {
        
        private static readonly string SettingsFilePath = 
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

  
        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd ładowania ustawień: {ex.Message}");
            }

            return new AppSettings(); 
        }

      
        public void SaveSettings(AppSettings settings)
        {
            try
            {
                settings.LastModified = DateTime.Now;
                
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                };
                
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd zapisu ustawień: {ex.Message}");
                throw new Exception($"Nie udało się zapisać ustawień: {ex.Message}");
            }
        }

        
        public void ExportJrwaFile(string jrwaFilePath, string destinationPath)
        {
            try
            {
                if (File.Exists(jrwaFilePath))
                {
                    File.Copy(jrwaFilePath, destinationPath, overwrite: true);
                }
                else
                {
                    throw new FileNotFoundException("Plik JRWA nie został znaleziony.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Błąd eksportu pliku JRWA: {ex.Message}");
            }
        }

       
        public void ImportJrwaFile(string sourcePath, AppSettings settings)
        {
            try
            {
                if (!File.Exists(sourcePath))
                {
                    throw new FileNotFoundException("Wybrany plik nie istnieje.");
                }

                // Zapisz dokładną ścieżkę podaną przez użytkownika
                // NIE kopiuj pliku do katalogu aplikacji
                settings.JrwaFilePath = sourcePath;
                SaveSettings(settings);
            }
            catch (Exception ex)
            {
                throw new Exception($"Błąd importu pliku JRWA: {ex.Message}");
            }
        }
    }
}