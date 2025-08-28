using Avalonia.Controls;
using Avalonia.Platform.Storage;
using SiGen.UI.Dialogs;
using SiGen.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiGen.Services
{
    public class DesktopDialogService : IDialogService
    {
        private readonly Window _parentWindow;
        private readonly IServiceProvider _serviceProvider;

        public DesktopDialogService(Window parentWindow, IServiceProvider serviceProvider)
        {
            _parentWindow = parentWindow;
            _serviceProvider = serviceProvider;
        }

        public Task<SaveChangesResult> ShowSaveChangesAsync(string documentName)
        {
            var viewModel = new SaveChangesDialogViewModel(documentName);
            var dialogControl = new ConfirmCloseDocumentView();
            return ShowDialogAsync(dialogControl, viewModel) ;
        }

        private async Task<TResult?> ShowDialogAsync<TResult>(UserControl dialogControl, DialogViewModelBase<TResult> viewModel)
        {

            dialogControl.DataContext = viewModel;

            var dialogWindow = new DialogHostWindow();
            dialogWindow.SystemDecorations = SystemDecorations.BorderOnly;
            dialogWindow.CanResize = false;
            dialogWindow.SetDialogContent(dialogControl, viewModel);

            // The magic happens here: we create a TaskCompletionSource and give it to the view model
            var completionSource = new TaskCompletionSource<TResult?>();
            viewModel.SetCompletionSource(completionSource);

            var dialogTask = dialogWindow.ShowDialog<bool?>(_parentWindow);
            var resultTask = completionSource.Task;

            var completedTask = await Task.WhenAny(dialogTask, resultTask);

            if (completedTask == resultTask)
            {
                // ViewModel completed first, close the dialog
                dialogWindow.Close();
                return await resultTask;
            }
            else
            {
                // Dialog was closed by user (X button, etc.)
                // Handle as needed (maybe return default value or throw)
                completionSource.TrySetCanceled();
                throw new OperationCanceledException("Dialog was closed by user");
            }

            //return result;
        }

        #region Open / Save

        public async Task<string?> ShowSaveFileDialogAsync(string? title = null, string? defaultFileName = null, IEnumerable<FileDialogFilter>? filters = null)
        {
            if (_parentWindow == null)
                return null;

            var fileTypeChoices = filters?.Select(f =>
                new FilePickerFileType(f.Name)
                {
                    Patterns = f.Extensions.Select(ext => ext.StartsWith(".") ? $"*{ext}" : $"*.{ext}").ToArray()
                }).ToArray();

            var file = await _parentWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = title ?? "Save File",
                SuggestedFileName = defaultFileName ?? "Untitled",
                FileTypeChoices = fileTypeChoices ?? new[] { new FilePickerFileType("All Files") { Patterns = new[] { "*" } } },
                DefaultExtension = fileTypeChoices?.FirstOrDefault()?.Patterns?.FirstOrDefault()?.TrimStart('*', '.') ?? ""
            });

            return file?.Path.LocalPath;
        }

        public async Task<string?> ShowOpenFileDialogAsync(string? title = null, IEnumerable<FileDialogFilter>? filters = null)
        {
            if (_parentWindow == null)
                return null;

            var fileTypeChoices = filters?.Select(f =>
                new FilePickerFileType(f.Name)
                {
                    Patterns = f.Extensions.Select(ext => ext.StartsWith(".") ? $"*{ext}" : $"*.{ext}").ToArray()
                }).ToArray();

            var files = await _parentWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title ?? "Open File",
                AllowMultiple = false,
                FileTypeFilter = fileTypeChoices ?? new[] { new FilePickerFileType("All Files") { Patterns = new[] { "*" } } }
            });

            return files.FirstOrDefault()?.Path.LocalPath;
        }
    }

    #endregion
}
