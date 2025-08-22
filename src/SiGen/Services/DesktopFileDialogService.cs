using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Services
{
    public class DesktopFileDialogService : IFileDialogService
    {
        private Window? _window;

        //todo: find a way to inject the window or use a static reference
        public DesktopFileDialogService()
        {
            //_window = window;
        }

        public DesktopFileDialogService(Window? window)
        {
            _window = window;
        }

        public async Task<string?> ShowSaveFileDialogAsync(string? title = null, string? defaultFileName = null, IEnumerable<FileDialogFilter>? filters = null)
        {
            if (_window == null)
                return null;

            var fileTypeChoices = filters?.Select(f =>
                new FilePickerFileType(f.Name)
                {
                    Patterns = f.Extensions.Select(ext => ext.StartsWith(".") ? $"*{ext}" : $"*.{ext}").ToArray()
                }).ToArray();

            var file = await _window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = title ?? "Save File",
                SuggestedFileName = defaultFileName ?? "Untitled",
                FileTypeChoices = fileTypeChoices ?? new[] { new FilePickerFileType("All Files") { Patterns = new[] { "*" } } },
                DefaultExtension = fileTypeChoices?.FirstOrDefault()?.Patterns.FirstOrDefault()?.TrimStart('*','.') ?? ""
            });

            return file?.Path.LocalPath;
        }

        public async Task<string?> ShowOpenFileDialogAsync(string? title = null, IEnumerable<FileDialogFilter>? filters = null)
        {
            if (_window == null)
                return null;

            var fileTypeChoices = filters?.Select(f =>
                new FilePickerFileType(f.Name)
                {
                    Patterns = f.Extensions.Select(ext => ext.StartsWith(".") ? $"*{ext}" : $"*.{ext}").ToArray()
                }).ToArray();

            var files = await _window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title ?? "Open File",
                AllowMultiple = false,
                FileTypeFilter = fileTypeChoices ?? new[] { new FilePickerFileType("All Files") { Patterns = new[] { "*" } } }
            });

            return files.FirstOrDefault()?.Path.LocalPath;
        }
    }

    public class DummyFileDialogService : IFileDialogService
    {
        public Task<string?> ShowSaveFileDialogAsync()
        {
            // This is a dummy implementation that does nothing.
            // In a real application, you would implement the logic to show a file dialog.
            return Task.FromResult<string?>(null);
        }

        public Task<string?> ShowSaveFileDialogAsync(string? title = null, string? defaultFileName = null, IEnumerable<FileDialogFilter>? filters = null)
        {
            return Task.FromResult<string?>(null);
        }

        public Task<string?> ShowOpenFileDialogAsync(string? title = null, IEnumerable<FileDialogFilter>? filters = null)
        {
            return Task.FromResult<string?>(null);
        }
    }
}