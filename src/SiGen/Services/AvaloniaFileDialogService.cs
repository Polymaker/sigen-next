using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Services
{
    public class AvaloniaFileDialogService : IFileDialogService
    {
        private Window? _window;

        //todo: find a way to inject the window or use a static reference
        public AvaloniaFileDialogService()
        {
            //_window = window;
        }



        public async Task<string?> ShowSaveFileDialogAsync()
        {
            if (_window == null) 
                return null;

            var file = await _window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Layout",
                SuggestedFileName = "Untitled.sigen",
                FileTypeChoices = new[]
                {
                new FilePickerFileType("SiGen Layout") { Patterns = new[] { "*.sigen" } },
                new FilePickerFileType("JSON") { Patterns = new[] { "*.json" } },
                new FilePickerFileType("All Files") { Patterns = new[] { "*" } }
            },
                DefaultExtension = "sigen"
            });

            return file?.Path.LocalPath;
        }
    }
}
