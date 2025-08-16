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
        private readonly Window _window;

        public AvaloniaFileDialogService(Window window)
        {
            _window = window;
        }

        public async Task<string?> ShowSaveFileDialogAsync()
        {
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
