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
        protected InstrumentLayoutConfiguration? Configuration => LayoutDocument?.Configuration;

        public ILayoutDocument LayoutDocument { get; private set; }

        public int NumberOfStrings => Configuration?.NumberOfStrings ?? 0;
        protected bool IsLoading { get; private set; }

        public event EventHandler? ConfigurationChanged;
        public event EventHandler? NumberOfStringsChanged;
        public event EventHandler? InstrumentTypeChanged;

        public EditorPanelViewModelBase()
        {
            LayoutDocument = new MockLayoutDocumentContext(); // For design mode, replace with actual context in production
            //LoadConfiguration(LayoutDocumentContext.Configuration);
            OnConfigurationChanged();
        }

        public void AssignDocument(ILayoutDocument context)
        {

            //if (LayoutDocumentContext != null)
            //    throw new InvalidOperationException("This panel is already assigned to a document context.");
            LayoutDocument = context ?? throw new ArgumentNullException(nameof(context));
            InitializeCore();
        }

        private void InitializeCore()
        {
            IsLoading = true;
            OnInitialize();
            NotifyConfigurationChanged();
            IsLoading = false;
        }

        protected virtual void OnInitialize()
        {
            
        }

        private bool isUpdatingConfig;

        public virtual void LoadConfiguration(InstrumentLayoutConfiguration? config)
        {
            
        }

        protected void UpdateConfiguration(string reason, Action<InstrumentLayoutConfiguration> updateAction)
        {
            if (IsLoading) return;

            isUpdatingConfig = true;
            LayoutDocument.UpdateConfiguration(reason, updateAction);
            isUpdatingConfig = false;
        }

        #region Notify methods

        public void NotifyConfigurationChanged()
        {
            if (isUpdatingConfig)
                return;

            // Notify that the configuration has changed
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
            OnConfigurationChanged();
        }

        public void NotifyInstrumentTypeChanged()
        {
            InstrumentTypeChanged?.Invoke(this, EventArgs.Empty);
            OnInstrumentTypeChanged();
        }

        public void NotifyNumberOfStringsChanged()
        {
            NumberOfStringsChanged?.Invoke(this, EventArgs.Empty);
            OnNumberOfStringsChanged();
        }

        #endregion

        protected virtual void OnNumberOfStringsChanged()
        {
        }

        protected virtual void OnInstrumentTypeChanged()
        {
        }

        protected virtual void OnConfigurationChanged()
        {
        }

    }
}
