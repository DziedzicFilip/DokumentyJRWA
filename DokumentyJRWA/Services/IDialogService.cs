using System;
using DokumentyJRWA.ViewModels;
namespace DokumentyJRWA.Services
{
    public interface IDialogService
    {
        string? OpenFileDialog();
        bool ShowAddDocumentDialog(AddDocumentViewModel vm);
    }
}