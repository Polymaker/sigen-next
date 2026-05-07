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

        // For saving files
        Task<string?> ShowSaveFileDialogAsync(string? title = null, string? defaultFileName = null, IEnumerable<FileDialogFilter>? filters = null);

        // For opening files
        Task<string?> ShowOpenFileDialogAsync(string? title = null, IEnumerable<FileDialogFilter>? filters = null);

        #region Editor Dialogs
        Task<List<Measure>?> ShowSpacingDialog(InstrumentLayoutConfiguration layoutConfiguration, FingerboardEnd end);

        Task<EditTuningResult?> ShowTuningDialog(ILayoutDocument context);

        Task<EditStringsResult?> ShowStringsDialog(ILayoutDocument context);

        Task<EditFretsResult?> ShowFretsDialog(ILayoutDocument context);

        Task<List<double>?> ShowFretIntervalsDialogAsync(string title, IReadOnlyList<double>? initialIntervals = null);

        /// <summary>
        /// Shows the user settings dialog.
        /// Returns true if settings were saved, false if cancelled.
        /// </summary>
        Task<bool> ShowUserSettingsDialogAsync();

        #endregion

        #region Simple Message Dialogs

        /// <summary>
        /// Shows an error message dialog.
        /// </summary>
        Task ShowErrorAsync(string message, string title = "Error");

        /// <summary>
        /// Shows an informational message dialog.
        /// </summary>
        Task ShowInfoAsync(string message, string title = "Information");

        /// <summary>
        /// Shows a warning message dialog.
        /// </summary>
        Task ShowWarningAsync(string message, string title = "Warning");

        /// <summary>
        /// Shows a confirmation dialog with Yes/No buttons.
        /// Returns true if the user clicked Yes, false if No.
        /// </summary>
        Task<bool> ShowConfirmAsync(string message, string title = "Confirm");

        /// <summary>
        /// Shows a generic message box with custom buttons and icon.
        /// </summary>
        Task<MessageBoxResult> ShowMessageBoxAsync(string message, string title, MessageBoxButtons buttons = MessageBoxButtons.Ok, MessageBoxIcon icon = MessageBoxIcon.None);

        #endregion
    }
}
