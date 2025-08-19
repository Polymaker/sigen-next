using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SiGen.Data.Common;
using SiGen.Layouts.Configuration;
using SiGen.Services;
using SiGen.ViewModels.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.ViewModels.EditorPanels
{
    public class EditorPanelViewModelBase : ObservableObject
    {
        protected InstrumentLayoutConfiguration? Configuration => LayoutDocumentContext?.Configuration;

        private int numberOfStringsCached;
        private SiGen.Data.Common.InstrumentType? cachedInstrumentType;
        protected ILayoutDocumentContext LayoutDocumentContext { get; private set; }

        public int NumberOfStrings => Configuration?.NumberOfStrings ?? 0;

        public bool HasMadeChanges { get; protected set; }

        public event EventHandler? ConfigurationChanged;

        public EditorPanelViewModelBase()
        {
            ApplyCommand = new RelayCommand(ApplyChanges, CanApplyChanges);
            LayoutDocumentContext = new MockLayoutDocumentContext(); // For design mode, replace with actual context in production
            //LoadConfiguration(LayoutDocumentContext.Configuration);
            OnConfigurationChanged();
        }

        public void AssignContext(ILayoutDocumentContext context)
        {
            //if (LayoutDocumentContext != null)
            //    throw new InvalidOperationException("This panel is already assigned to a document context.");
            LayoutDocumentContext = context ?? throw new ArgumentNullException(nameof(context));
            NotifyConfigurationChanged();
        }


        public virtual void LoadConfiguration(InstrumentLayoutConfiguration? config)
        {
            
        }

        public virtual void OnNumberOfStringsChanged()
        {
        }

        public virtual void OnInstrumentTypeChanged()
        {
        }

        public void NotifyConfigurationChanged()
        {
            // Notify that the configuration has changed
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
            OnConfigurationChanged();
        }


        // Called when the config is replaced (e.g., after undo/redo)
        public virtual void OnConfigurationChanged()
        {
            // Derived panels can override to refresh their state
        }



        #region Apply Changes

        public IRelayCommand ApplyCommand { get; }

        public event EventHandler? ChangesApplied;

        public virtual void ApplyChanges()
        {
            // Derived panels override to push their state into Configuration
            ChangesApplied?.Invoke(this, EventArgs.Empty);
        }

        protected virtual bool CanApplyChanges()
        {
            // Derived panels can override to control when ApplyChanges is enabled
            return true;
        }

        protected void NotifyCanApplyChangesChanged()
        {
            // Notify that the CanExecute state of ApplyCommand may have changed
            ApplyCommand.NotifyCanExecuteChanged();
        }

        protected void NotifyLayoutPropertiesChanged()
        {
            // Notify that properties have changed, e.g., after user input
            OnPropertyChanged(nameof(HasMadeChanges));
            if (!HasMadeChanges)
            {
                HasMadeChanges = true;
                NotifyCanApplyChangesChanged();
            }
        }

        public void CancelChanges()
        {
            if (Configuration == null) return;

            // Reset changes to the last saved state
            LoadConfiguration(Configuration);
            if (HasMadeChanges)
            {
                HasMadeChanges = false;
                NotifyCanApplyChangesChanged();
            }
        }

        #endregion
    }
}
