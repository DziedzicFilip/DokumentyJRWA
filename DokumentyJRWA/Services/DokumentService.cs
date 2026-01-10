using DokumentyJRWA.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokumentyJRWA.Services
{
    public class DocumentService
    {
        public List<Dokument> GetAll()
        {
            using var db = new AppDbContext();
            return db.Dokumenty
                     .OrderByDescending(d => d.DataWplywu)
                     .ToList();
        }

        public void Add(Dokument dokument)
        {
            using var db = new AppDbContext();
            db.Dokumenty.Add(dokument);
            db.SaveChanges();
        }

        public void Delete(Guid id)
        {
            using var db = new AppDbContext();
            var dokument = db.Dokumenty.Find(id);
            if (dokument != null)
            {
                db.Dokumenty.Remove(dokument);
                db.SaveChanges();
            }
        }

        public void Update(Dokument dokument)
        {
            using var db = new AppDbContext();
            db.Dokumenty.Update(dokument);
            db.SaveChanges();
        }
        
    }
}
