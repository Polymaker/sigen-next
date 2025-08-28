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
        public static StringCountViewModel StringControlViewModel { get; } =
            new StringCountViewModel(new StringCountControl { StringCount = 6, IsLeftHanded = false });

        public static DesktopMainViewModel DesktopMainViewModel
        {
            get
            {
                var model = new DesktopMainViewModel(new MockDialogService(), new MockSettingsService());
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
                var model = new HomePageViewModel(new SettingsService(), new MockDocumentManager());
                return model;
            }
        }

        private class MockDocumentManager : IDocumentManager
        {
            public void OpenDocumentFile(string? filePath)
            {
                throw new NotImplementedException();
            }
        }
    }
}
