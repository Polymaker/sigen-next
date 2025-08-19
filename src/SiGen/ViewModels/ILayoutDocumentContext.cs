using SiGen.Layouts;
using SiGen.Layouts.Configuration;
using SiGen.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.ViewModels
{
    public interface ILayoutDocumentContext
    {
        ///// <summary>
        ///// Gets the working (editable) configuration for the instrument layout.
        ///// </summary>
        //InstrumentLayoutConfiguration WorkingConfiguration { get; }

        /// <summary>
        /// Gets the last valid (applied) configuration for the instrument layout.
        /// </summary>
        InstrumentLayoutConfiguration Configuration { get; }

        /// <summary>
        /// Gets or sets whether the document has unsaved changes.
        /// </summary>
        bool HasUnsavedChanges { get; }

        /// <summary>
        /// Gets the current generated layout (may be from working or stable config).
        /// </summary>
        StringedInstrumentLayout? CurrentLayout { get; }

        /// <summary>
        /// Provides access to an optional <see cref="IInstrumentValuesProvider"/> instance,
        /// which supplies common instrument-related values and presets (such as string counts,
        /// scale lengths, tunings, and spacings) for use in instrument layout configuration and UI.
        /// May be <c>null</c> if no provider is available.
        /// </summary>
        IInstrumentValuesProvider? InstrumentValuesProvider { get; }

        /// <summary>
        /// Updates the working configuration and triggers layout regeneration and change notification.
        /// </summary>
        /// <param name="reason">A description for the change (for undo/redo, etc).</param>
        /// <param name="updateAction">An action that modifies the working configuration.</param>
        void UpdateConfiguration(string reason, Action<InstrumentLayoutConfiguration> updateAction);
    }
}
