using DokumentyJRWA.Data;
using DokumentyJRWA.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokumentyJRWA.ViewModels
{
    public class MainViewModel
    {
        public readonly DocumentService _documentService = new();

        public ObservableCollection<Dokument> Dokumenty { get;}
        public MainViewModel()
        {
            Dokumenty = new ObservableCollection<Dokument> (
                    _documentService.GetAll()
                );
        }
    }
}
