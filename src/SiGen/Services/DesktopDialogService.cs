using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Measuring;
using SiGen.UI.Dialogs;
using SiGen.ViewModels;
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

        

        private async Task<TResult?> ShowDialogAsync<TResult>(UserControl dialogControl, DialogViewModelBase<TResult> viewModel)
        {

            dialogControl.DataContext = viewModel;

            var dialogWindow = new DialogHostWindow();
            dialogWindow.CanResize = viewModel.Resizable;
            dialogWindow.SizeToContent = viewModel.Resizable ? SizeToContent.Manual : SizeToContent.WidthAndHeight;
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


        #region Editor Dialogs

        public async Task<List<Measure>?> ShowSpacingDialog(InstrumentLayoutConfiguration layoutConfiguration, FingerboardEnd end)
        {
            var viewModel = new EditSpacingDialogViewModel(layoutConfiguration, end);
            var dialogControl = new EditSpacingDialogView();
            return await ShowDialogAsync(dialogControl, viewModel);
            //return Task.CompletedTask;
        }

        public async Task<EditTuningResult?> ShowTuningDialog(ILayoutDocument document)
        {
            
            var viewModel = ActivatorUtilities.CreateInstance<EditTuningDialogViewModel>(_serviceProvider, document);
            var dialogControl = new EditTuningDialogView();
            return await ShowDialogAsync(dialogControl, viewModel);

        }

        public async Task<EditStringsResult?> ShowStringsDialog(ILayoutDocument document)
        {
            var viewModel = ActivatorUtilities.CreateInstance<EditStringsDialogViewModel>(_serviceProvider, document);
            var dialogControl = new EditStringsDialogView();
            return await ShowDialogAsync(dialogControl, viewModel);
        }

        public async Task<EditFretsResult?> ShowFretsDialog(ILayoutDocument context)
        {
            var viewModel = new EditFretsDialogViewModel(context);
            var dialogControl = new EditFretsDialogView();
            return await ShowDialogAsync(dialogControl, viewModel);
        }

        public async Task<bool> ShowUserSettingsDialogAsync()
        {
            var settingsService = _serviceProvider.GetService<ISettingsService>();
            if (settingsService == null)
                return false;

            var viewModel = new UserSettingsDialogViewModel(settingsService);
            var dialogControl = new UserSettingsDialogView();
            var result = await ShowDialogAsync(dialogControl, viewModel);
            return result;
        }

        #endregion

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

        #endregion

        #region Simple Message Dialogs

        public async Task ShowErrorAsync(string message, string title = "Error")
        {
            await ShowMessageBoxAsync(message, title, MessageBoxButtons.Ok, MessageBoxIcon.Error);
        }

        public async Task ShowInfoAsync(string message, string title = "Information")
        {
            await ShowMessageBoxAsync(message, title, MessageBoxButtons.Ok, MessageBoxIcon.Information);
        }

        public async Task ShowWarningAsync(string message, string title = "Warning")
        {
            await ShowMessageBoxAsync(message, title, MessageBoxButtons.Ok, MessageBoxIcon.Warning);
        }

        public async Task<bool> ShowConfirmAsync(string message, string title = "Confirm")
        {
            var result = await ShowMessageBoxAsync(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return result == MessageBoxResult.Yes;
        }

        public async Task<MessageBoxResult> ShowMessageBoxAsync(string message, string title, MessageBoxButtons buttons = MessageBoxButtons.Ok, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            var viewModel = new MessageBoxViewModel(message, title, buttons, icon);
            var dialogControl = new MessageBoxView();
            var result = await ShowDialogAsync(dialogControl, viewModel);
            return result;
        }

        #endregion
    }


}
