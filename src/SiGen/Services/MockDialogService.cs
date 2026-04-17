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

        public Task<SaveChangesResult> ShowSaveChangesAsync(string documentName)
        {
            throw new NotImplementedException();
        }

        public Task<List<Measure>?> ShowSpacingDialog(InstrumentLayoutConfiguration layoutConfiguration, FingerboardEnd end)
        {
            throw new NotImplementedException();
        }

        public Task<EditTuningResult?> ShowTuningDialog(ILayoutDocumentContext context)
        {
            throw new NotImplementedException();
        }

        public Task<EditStringsResult?> ShowStringsDialog(ILayoutDocumentContext context)
        {
            throw new NotImplementedException();
        }

        public Task<EditFretsResult?> ShowFretsDialog(ILayoutDocumentContext context)
        {
            throw new NotImplementedException();
        }
    }
}