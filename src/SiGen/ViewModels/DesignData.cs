using Microsoft.Extensions.DependencyInjection;
using SiGen.Layouts.Configuration;
using SiGen.Services;
using SiGen.UI.Controls;
using SiGen.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.ViewModels
{
    public static class DesignData
    {

        public static DesktopMainViewModel DesktopMainViewModel
        {
            get
            {
                var dialogSvc = new MockDialogService();

                var services = new ServiceCollection();
                services.AddSingleton<IDialogService>(dialogSvc);
                services.AddSingleton<IInstrumentValuesProviderFactory, InstrumentValuesProviderFactory>();
                services.AddSingleton<IStringDataService, MockStringDataService>();
                services.AddSingleton<IStringMaterialEstimationService, StringMaterialEstimationService>();
                var provider = services.BuildServiceProvider();

                var model = new DesktopMainViewModel(
                    dialogSvc,
                    new MockSettingsService(),
                    new ViewModelFactory(provider));
                model.OpenDocuments.Add(new LayoutDocumentViewModel("Untitled", null, LayoutTemplates.CreateBassGuitarMultiscaleLayout())
                {
                    HasUnsavedChanges = true
                });
                model.OpenDocuments.Add(new LayoutDocumentViewModel("Layout 1", null, LayoutTemplates.CreateMandolinLayout()));
                model.SelectedDocument = model.OpenDocuments.FirstOrDefault();
                return model;
            }
        }

        public static HomePageViewModel HomePageViewModel
        {
            get
            {
                var model = new HomePageViewModel(new MockSettingsService(), new MockDocumentManager());
                return model;
            }
        }

        private class MockDocumentManager : IDocumentManager
        {
            public void OpenDocumentFile(string? filePath)
            {
                throw new NotImplementedException();
            }

            public void OpenLayoutConfiguration(string documentName, InstrumentLayoutConfiguration configuration)
            {
                throw new NotImplementedException();
            }
        }
    }
}
