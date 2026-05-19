using SiGen.Export;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Measuring;
using SiGen.ViewModels;
using SiGen.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiGen.Services
{
    public class MockDialogService : IDialogService
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


        public Task<List<Measure>?> ShowSpacingDialog(InstrumentLayoutConfiguration layoutConfiguration, FingerboardEnd end)
        {
            throw new NotImplementedException();
        }

        public Task<EditTuningResult?> ShowTuningDialog(ILayoutDocument context)
        {
            throw new NotImplementedException();
        }

        public Task<EditStringsResult?> ShowStringsDialog(ILayoutDocument context)
        {
            throw new NotImplementedException();
        }

        public Task<EditFretsResult?> ShowFretsDialog(ILayoutDocument context)
        {
            throw new NotImplementedException();
        }

        public Task<List<double>?> ShowFretIntervalsDialogAsync(string title, IReadOnlyList<double>? initialIntervals = null)
        {
            return Task.FromResult<List<double>?>(null);
        }

        public Task<bool> ShowUserSettingsDialogAsync()
        {
            return Task.FromResult(false);
        }

        public Task ShowErrorAsync(string message, string title = "Error")
        {
            return Task.CompletedTask;
        }

        public Task ShowInfoAsync(string message, string title = "Information")
        {
            return Task.CompletedTask;
        }

        public Task ShowWarningAsync(string message, string title = "Warning")
        {
            return Task.CompletedTask;
        }

        public Task<bool> ShowConfirmAsync(string message, string title = "Confirm")
        {
            return Task.FromResult(false);
        }

        public Task<MessageBoxResult> ShowMessageBoxAsync(string message, string title, MessageBoxButtons buttons = MessageBoxButtons.Ok, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            return Task.FromResult(MessageBoxResult.None);
        }

        public Task ShowExportDialog(ILayoutDocument context, ExportTargetFormat? format)
        {
            return Task.FromResult(false);
        }
    }
}