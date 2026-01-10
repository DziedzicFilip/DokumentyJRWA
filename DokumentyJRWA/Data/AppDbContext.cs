using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokumentyJRWA.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Dokument> Dokumenty { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DokumentyJRWA");

            Directory.CreateDirectory(folder);

            var dbPath = Path.Combine(folder, "dokumenty.db");

            options.UseSqlite($"Data Source={dbPath}");
        }
    }
}
