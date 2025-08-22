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
                var model = new DesktopMainViewModel(new DummyFileDialogService());
                model.OpenDocuments.Add(new DocumentViewModel("Untitled", null, LayoutTemplates.CreateBassGuitarMultiscaleLayout())
                {
                    HasUnsavedChanges = true
                });
                model.OpenDocuments.Add(new DocumentViewModel("Layout 1", null, LayoutTemplates.CreateMandolinLayout()));
                model.SelectedDocument = model.OpenDocuments.FirstOrDefault();
                return model;
            }
        }
    }
}
