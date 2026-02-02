using SiGen.Layouts.Configuration;
using SiGen.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Services
{
    public class LayoutDocumentModelFactory
    {
        private IDialogService dialogService;

        public LayoutDocumentModelFactory(IDialogService dialogService)
        {
            this.dialogService = dialogService;
        }

        public LayoutDocumentViewModel CreateViewModel(string? filepath, InstrumentLayoutConfiguration configuration)
        {
            string title = !string.IsNullOrEmpty(filepath) ? System.IO.Path.GetFileNameWithoutExtension(filepath) : string.Empty;

            return new LayoutDocumentViewModel(title, filepath, configuration);
        }
    }
}
