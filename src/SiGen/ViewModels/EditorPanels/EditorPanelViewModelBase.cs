using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SiGen.Data.Common;
using SiGen.Layouts.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.ViewModels.EditorPanels
{
    public class EditorPanelViewModelBase : ObservableObject
    {
        protected InstrumentLayoutConfiguration? Configuration { get; private set; }
        private int numberOfStringsCached;
        private SiGen.Data.Common.InstrumentType? cachedInstrumentType;

        public int NumberOfStrings => Configuration?.NumberOfStrings ?? 0;

        public bool HasMadeChanges { get; protected set; }

        public EditorPanelViewModelBase()
        {
            ApplyCommand = new RelayCommand(ApplyChanges, CanApplyChanges);
            numberOfStringsCached = 0;

            //if (Design.IsDesignMode)
            {
                var layoutConfig = new InstrumentLayoutConfiguration
                {
                    InstrumentType = Data.Common.InstrumentType.ElectricGuitar,
                    NumberOfStrings = 6,
                    LeftHanded = false,
                    NumberOfFrets = 24,
                };
                layoutConfig.ScaleLength.Mode = ScaleLengthMode.Single;
                layoutConfig.ScaleLength.SingleScale = Measuring.Measure.In(25.5);

                layoutConfig.NutSpacing.StringDistances.Add(Measuring.Measure.Mm(7.5));
                layoutConfig.NutSpacing.CenterAlignment = Layouts.Data.LayoutCenterAlignment.OuterStrings;
                layoutConfig.BridgeSpacing.StringDistances.Add(Measuring.Measure.Mm(10.5));
                layoutConfig.BridgeSpacing.CenterAlignment = Layouts.Data.LayoutCenterAlignment.OuterStrings;

                layoutConfig.Margin.SetAll(Measuring.Measure.Mm(3.25));
                layoutConfig.InitializeStringConfigs();
                SetConfiguration(layoutConfig);
            }
        }

        //public EditorPanelViewModelBase(InstrumentLayoutConfiguration config)
        //{
        //    Configuration = config;
        //    ApplyCommand = new RelayCommand(ApplyChanges, CanApplyChanges);
        //    numberOfStringsCached = config.NumberOfStrings;
        //    OnConfigurationChanged();
        //}

        // Optionally, allow updating the config reference (e.g., on undo/redo)
        public virtual void SetConfiguration(InstrumentLayoutConfiguration? config)
        {
            Configuration = config;
            OnConfigurationChanged();
            HasMadeChanges = false;

            if (config != null && config.NumberOfStrings != numberOfStringsCached)
            {
                numberOfStringsCached = config.NumberOfStrings;
                OnLayoutPropertyChanged(nameof(InstrumentLayoutConfiguration.NumberOfStrings));
            }
            else
            {
                numberOfStringsCached = 0;
            }

            if (config != null && config.InstrumentType != cachedInstrumentType)
            {
                cachedInstrumentType = config.InstrumentType;
                OnLayoutPropertyChanged(nameof(InstrumentLayoutConfiguration.InstrumentType));
            }
            else
            {
                cachedInstrumentType = null;
            }
        }

        protected virtual void OnLayoutPropertyChanged(string propertyName)
        {
            
        }

        // Called when the config is replaced (e.g., after undo/redo)
        protected virtual void OnConfigurationChanged()
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
            SetConfiguration(Configuration);
            if (HasMadeChanges)
            {
                HasMadeChanges = false;
                NotifyCanApplyChangesChanged();
            }
        }

        #endregion
    }
}
