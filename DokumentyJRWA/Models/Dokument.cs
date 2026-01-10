using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokumentyJRWA.Data
{
    public class Dokument
    {
        public Guid Id { get; set; }
        public string Tytul { get; set; }
        public DateTime DataWplywu { get; set; }
        public string JrwaCode { get; set; }
        public string? Podmioty { get; set; }
        public string FilePath { get; set; }
    }
}
