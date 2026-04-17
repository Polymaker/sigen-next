using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Measuring;
using SiGen.ViewModels;
using SiGen.ViewModels.Dialogs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiGen.Services
{
    /// <summary>
    /// Provides methods for showing dialogs and confirmation messages.
    /// Implement this interface in platform-specific projects.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows a confirmation dialog asking if the user wants to close a document without saving.
        /// Returns true if the user confirms, false otherwise.
        /// </summary>
        Task<SaveChangesResult> ShowSaveChangesAsync(string documentName);

        // For saving files
        Task<string?> ShowSaveFileDialogAsync(string? title = null, string? defaultFileName = null, IEnumerable<FileDialogFilter>? filters = null);

        // For opening files
        Task<string?> ShowOpenFileDialogAsync(string? title = null, IEnumerable<FileDialogFilter>? filters = null);

        #region Editor Dialogs
        Task<List<Measure>?> ShowSpacingDialog(InstrumentLayoutConfiguration layoutConfiguration, FingerboardEnd end);

        Task<EditTuningResult?> ShowTuningDialog(ILayoutDocumentContext context);

        Task<EditStringsResult?> ShowStringsDialog(ILayoutDocumentContext context);

        Task<EditFretsResult?> ShowFretsDialog(ILayoutDocumentContext context);

        #endregion
    }
}
