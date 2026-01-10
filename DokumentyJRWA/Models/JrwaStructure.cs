using System;
using System.Collections.Generic;

namespace DokumentyJRWA.Models
{
    // Model główny pliku JRWA
    public class JrwaStructure
    {
        public string Version { get; set; } = "1.0";
        public string Name { get; set; } = string.Empty;
        public string LastModified { get; set; } = string.Empty;
        public List<JrwaCategory> Categories { get; set; } = new();
    }

    // Model kategorii/podkategorii
    public class JrwaCategory
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string RetentionPeriod { get; set; } = string.Empty;
        public List<JrwaCategory>? Subcategories { get; set; }
    }
}
