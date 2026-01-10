using System;

namespace DokumentyJRWA.Models
{
    public class AppSettings
    {
        
        public string MainFolderPath { get; set; } = string.Empty;
        
      
        public string JrwaFilePath { get; set; } = string.Empty;
        
       
        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}