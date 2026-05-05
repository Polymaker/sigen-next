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
                services.AddSingleton<ISettingsService, MockSettingsService>();
                services.AddSingleton<DesktopMainViewModel>();
                services.AddSingleton<ViewModelFactory>();
                services.AddSingleton<IDocumentManager>(sp => sp.GetRequiredService<DesktopMainViewModel>());
                

                var provider = services.BuildServiceProvider();
                var mainModel = provider.GetRequiredService<DesktopMainViewModel>();

                var document1 = ActivatorUtilities.CreateInstance<LayoutDocumentViewModel>(provider, "Untitled", string.Empty, LayoutTemplates.CreateBassGuitarMultiscaleLayout());
                document1.HasUnsavedChanges = true;
                mainModel.OpenDocuments.Add(document1);
                var document2 = ActivatorUtilities.CreateInstance<LayoutDocumentViewModel>(provider, "Mandolin Layout", string.Empty, LayoutTemplates.CreateMandolinLayout());
                mainModel.OpenDocuments.Add(document2);
                mainModel.SelectedDocument = mainModel.OpenDocuments.FirstOrDefault();
                return mainModel;
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
