using CommunityToolkit.Mvvm.ComponentModel;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SiGen.ViewModels.EditorPanels
{
    public partial class InstrumentInfoPanelViewModel : EditorPanelViewModelBase
    {
        private readonly IInstrumentValuesProviderFactory valuesProviderFactory;


        [ObservableProperty]
        private SiGen.Data.Common.InstrumentType? instrumentType;

        [ObservableProperty]
        private int numberOfStrings;

        [ObservableProperty]
        private bool leftHanded;

        [ObservableProperty]
        private int? numberOfFrets;

        private List<BaseStringConfiguration> stringConfigurations = new();

        public bool RightHanded => !LeftHanded;

        public InstrumentInfoPanelViewModel(IInstrumentValuesProviderFactory instrumentValuesProviderFactory)
        {
            this.valuesProviderFactory = instrumentValuesProviderFactory;
        }

        //For Design Mode
        public InstrumentInfoPanelViewModel()
        {
            this.valuesProviderFactory = new InstrumentValuesProviderFactory();
        }

        protected override void OnConfigurationChanged()
        {
            if (Configuration == null)
            {
                return;
            }

            LeftHanded = Configuration.LeftHanded;
            InstrumentType = Configuration.InstrumentType;
            NumberOfStrings = Configuration.NumberOfStrings;
            NumberOfFrets = Configuration.NumberOfFrets;

            if (Configuration.StringConfigurations.Count == 0)
                Configuration.InitializeStringConfigs();

            stringConfigurations = Configuration.StringConfigurations.ToList();

            removedBassStrings.Clear();
            removedTrebleStrings.Clear();
        }

        public override void ApplyChanges()
        {
            if (Configuration == null) return;

            Configuration.NumberOfStrings = NumberOfStrings;
            Configuration.StringConfigurations = stringConfigurations.ToList();
            Configuration.LeftHanded = LeftHanded;
            Configuration.NumberOfFrets = NumberOfFrets;

            base.ApplyChanges();
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(LeftHanded))
                OnPropertyChanged(nameof(RightHanded));
            if (e.PropertyName != nameof(HasMadeChanges))
                NotifyLayoutPropertiesChanged();

            if (e.PropertyName == nameof(InstrumentType))
                RebuildSuggestions();
        }

        protected override void OnLayoutPropertyChanged(string propertyName)
        {
            if (propertyName == nameof(InstrumentType))
                RebuildSuggestions();
        }

        private void RebuildSuggestions()
        {
            if (Configuration == null) return;
            var provider = (valuesProviderFactory ?? new InstrumentValuesProviderFactory()).Create(Configuration.InstrumentType);
            var test = provider.GetScaleLengthPresets();
        }

        #region Add/Remove Strings

        private readonly Stack<BaseStringConfiguration> removedBassStrings = new();
        private readonly Stack<BaseStringConfiguration> removedTrebleStrings = new();

        public void AddString(FingerboardSide side)
        {
            if (stringConfigurations.Count < 20) // Assuming max 20 strings
            {
                if (side == FingerboardSide.Bass)
                {
                    if (removedBassStrings.Count > 0)
                        stringConfigurations.Insert(0, removedBassStrings.Pop());
                    else
                        stringConfigurations.Insert(0, GetNewStringConfiguration(side)); // Add new string at the beginning
                }
                else if (side == FingerboardSide.Treble)
                {
                    if (removedTrebleStrings.Count > 0)
                        stringConfigurations.Add(removedTrebleStrings.Pop());
                    else
                        stringConfigurations.Add(GetNewStringConfiguration(side)); // Add new string at the end
                }
            }
        }

        protected BaseStringConfiguration GetNewStringConfiguration(FingerboardSide side)
        {
            var previousString = side == FingerboardSide.Bass
                ? (stringConfigurations.Count > 0 ? stringConfigurations[0] : null)
                : (stringConfigurations.Count > 0 ? stringConfigurations[^1] : null);

            // Create a new StringConfiguration with default values
            var newStringConfig = new SingleStringConfiguration();
            if (previousString != null)
            {
                newStringConfig.ScaleLength = previousString.ScaleLength;
                
                newStringConfig.MultiScaleRatio = previousString.MultiScaleRatio;
                newStringConfig.Frets = previousString.Frets; //todo: clone object to break reference

                //newStringConfig.Gauge = previousString.Gauge;
            }

            return newStringConfig;
        }

        public void RemoveString(FingerboardSide side)
        {
            if (stringConfigurations.Count > 1) // Assuming at least 1 string
            {
                if (side == FingerboardSide.Bass)
                {
                    //stringConfigurations 
                    removedBassStrings.Push(stringConfigurations[0]);
                    stringConfigurations.RemoveAt(0); // Remove first string
                }
                else if (side == FingerboardSide.Treble)
                {
                    // Remove last string
                    removedTrebleStrings.Push(stringConfigurations[^1]);
                    stringConfigurations.RemoveAt(stringConfigurations.Count - 1);
                }
            }
        }


        #endregion


    }
}
