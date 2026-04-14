using SiGen.Layouts.Configuration;
using SiGen.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Services
{
    public class ViewModelFactory
    {
        private IDialogService dialogService;
        private IInstrumentValuesProviderFactory instrumentValuesProviderFactory;
        private readonly IStringDataService stringDataService;

        public ViewModelFactory(IDialogService dialogService, IInstrumentValuesProviderFactory instrumentValuesProviderFactory, IStringDataService stringDataService)
        {
            this.dialogService = dialogService;
            this.instrumentValuesProviderFactory = instrumentValuesProviderFactory;
            this.stringDataService = stringDataService;
        }

        public LayoutDocumentViewModel CreateLayoutDocumentViewModel(string? filepath, InstrumentLayoutConfiguration configuration, string? templateName = null)
        {
            string title = templateName ?? 
                (!string.IsNullOrEmpty(filepath) ? System.IO.Path.GetFileNameWithoutExtension(filepath) : Lang.Resources.NewDocumentName);

            return new LayoutDocumentViewModel(title, filepath, configuration, instrumentValuesProviderFactory, dialogService, stringDataService);
        }
    }
}
