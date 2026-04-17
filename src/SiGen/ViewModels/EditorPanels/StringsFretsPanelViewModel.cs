using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SiGen.Data.Common;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SiGen.ViewModels.EditorPanels
{
    public partial class StringsFretsPanelViewModel : EditorPanelViewModelBase
    {
        [ObservableProperty]
        private int numberOfFrets;

        //[ObservableProperty]
        //private int numberOfStrings;

        public bool LeftHanded { get; private set; }

        public List<BaseStringConfiguration> StringConfigurations { get; private set; } = new();

        public ICommand EditTuningCommand { get; }
        public ICommand EditStringsCommand { get; }
        public ICommand EditFretsCommand { get; }

        public StringsFretsPanelViewModel()
        {
            EditTuningCommand = new RelayCommand(EditTuning);
            EditStringsCommand = new RelayCommand(EditStrings);
            EditFretsCommand = new RelayCommand(EditFrets);
        }


        protected override void OnConfigurationChanged()
        {
            if (Configuration == null)
            {
                return;
            }

            if (Configuration.LeftHanded != LeftHanded)
            {
                LeftHanded = Configuration.LeftHanded;
                OnPropertyChanged(nameof(LeftHanded));
            }

            //NumberOfStrings = Configuration.NumberOfStrings;
            NumberOfFrets = Configuration.NumberOfFrets ?? 0;

            if (Configuration.StringConfigurations.Count == 0)
                Configuration.InitializeStringConfigs();

            StringConfigurations = Configuration.StringConfigurations.ToList();
            OnPropertyChanged(nameof(StringConfigurations));
            removedBassStrings.Clear();
            removedTrebleStrings.Clear();
        }

        partial void OnNumberOfFretsChanged(int value)
        {
            UpdateConfiguration("Number of frets", config =>
            {
                config.NumberOfFrets = value;
            });
        }

        #region Add/Remove Strings

        private readonly Stack<BaseStringConfiguration> removedBassStrings = new();
        private readonly Stack<BaseStringConfiguration> removedTrebleStrings = new();

        public void AddString(FingerboardSide side)
        {
            if (StringConfigurations.Count < 20) // Assuming max 20 strings
            {
                if (side == FingerboardSide.Bass)
                {
                    if (removedBassStrings.Count > 0)
                        StringConfigurations.Insert(0, removedBassStrings.Pop());
                    else
                        StringConfigurations.Insert(0, GetNewStringConfiguration(side)); // Add new string at the beginning
                }
                else if (side == FingerboardSide.Treble)
                {
                    if (removedTrebleStrings.Count > 0)
                        StringConfigurations.Add(removedTrebleStrings.Pop());
                    else
                        StringConfigurations.Add(GetNewStringConfiguration(side)); // Add new string at the end
                }
                
                UpdateConfiguration("Add string", config =>
                {
                    config.NumberOfStrings = StringConfigurations.Count;
                    config.StringConfigurations.Clear();

                    foreach (var strConfig in StringConfigurations)
                        config.StringConfigurations.Add(strConfig); // Ensure all string configs are added to the configuration
                    if (config.NutSpacing.StringDistances.Count > 1)
                    {
                        var prev = side == FingerboardSide.Bass ? config.NutSpacing.StringDistances[0] : config.NutSpacing.StringDistances[^1];
                        config.NutSpacing.AddDistance(side, prev);
                    }
                    if (config.BridgeSpacing.StringDistances.Count > 1)
                    {
                        var prev = side == FingerboardSide.Bass ? config.BridgeSpacing.StringDistances[0] : config.BridgeSpacing.StringDistances[^1];
                        config.BridgeSpacing.AddDistance(side, prev);
                    }
                });

                OnPropertyChanged(nameof(StringConfigurations));
            }
        }

        protected BaseStringConfiguration GetNewStringConfiguration(FingerboardSide side)
        {
            var previousString = side == FingerboardSide.Bass
                ? (StringConfigurations.Count > 0 ? StringConfigurations[0] : null)
                : (StringConfigurations.Count > 0 ? StringConfigurations[^1] : null);

            // Create a new StringConfiguration with default values
            BaseStringConfiguration newStringConfig = previousString is StringGroupConfiguration ?
                new StringGroupConfiguration() :
                new SingleStringConfiguration();

            if (previousString != null)
            {
                newStringConfig.ScaleLength = previousString.ScaleLength;
                newStringConfig.MultiScaleRatio = previousString.MultiScaleRatio;
                newStringConfig.Frets = previousString.Frets; //todo: clone object to break reference

                if (Configuration!.ScaleLength.Mode == ScaleLengthMode.PerString &&
                    StringConfigurations.Count > 1 &&
                    !Measuring.Measure.IsNullOrEmpty(previousString.ScaleLength))
                {
                    var secondPrevious = side == FingerboardSide.Bass
                        ? (StringConfigurations.Count > 0 ? StringConfigurations[1] : null)
                        : (StringConfigurations.Count > 0 ? StringConfigurations[^2] : null);

                    if (secondPrevious != null && !Measuring.Measure.IsNullOrEmpty(secondPrevious.ScaleLength))
                    {
                        var scaleDiff = previousString.ScaleLength - secondPrevious.ScaleLength;
                        newStringConfig.ScaleLength = previousString.ScaleLength + scaleDiff;
                    }
                }

                if (newStringConfig is SingleStringConfiguration currConfig &&
                    previousString is SingleStringConfiguration prevConfig)
                {
                    currConfig.Gauge = prevConfig.Gauge;
                    if (!Measuring.Measure.IsNullOrEmpty(prevConfig.Gauge))
                    {
                        var newGauge = prevConfig.Gauge.Value * (side == FingerboardSide.Bass ? 1.15 : 0.85);
                        currConfig.Gauge = Measuring.Measure.Min(Measuring.Measure.Max(newGauge, Measuring.Measure.In(0.007)), Measuring.Measure.In(0.15));
                    }
                    currConfig.MaterialType = prevConfig.MaterialType;
                }
                else if (newStringConfig is StringGroupConfiguration groupConfiguration1 &&
                    previousString is StringGroupConfiguration groupConfiguration2)
                {
                    foreach (var str in groupConfiguration2.Strings)
                    {
                        groupConfiguration1.Strings.Add(new StringProperties
                        {
                            Gauge = (str.Gauge ?? Measuring.Measure.Zero) * (side == FingerboardSide.Bass ? 1.10 : 0.9),
                            Tuning = str.Tuning
                        });
                    }
                    groupConfiguration1.Spacing = groupConfiguration2.Spacing;
                }
                //newStringConfig.Gauge = previousString.Gauge;
            }

            return newStringConfig;
        }

        public void RemoveString(FingerboardSide side)
        {
            if (StringConfigurations.Count > 1) // Assuming at least 1 string
            {
                if (side == FingerboardSide.Bass)
                {
                    //stringConfigurations 
                    removedBassStrings.Push(StringConfigurations[0]);
                    StringConfigurations.RemoveAt(0); // Remove first string
                }
                else if (side == FingerboardSide.Treble)
                {
                    // Remove last string
                    removedTrebleStrings.Push(StringConfigurations[^1]);
                    StringConfigurations.RemoveAt(StringConfigurations.Count - 1);
                }

                UpdateConfiguration("Remove string", config =>
                {
                    config.NumberOfStrings = StringConfigurations.Count;
                    config.StringConfigurations = new (StringConfigurations);

                    if (config.NutSpacing.SpacingMode == StringSpacingMode.Manual &&
                        config.NutSpacing.StringDistances.Count > 1)
                    {
                        config.NutSpacing.RemoveDistance(side);
                    }
                    if (config.BridgeSpacing.SpacingMode == StringSpacingMode.Manual &&
                        config.BridgeSpacing.StringDistances.Count > 1)
                    {
                        config.BridgeSpacing.RemoveDistance(side);
                    }
                });

                OnPropertyChanged(nameof(StringConfigurations));
            }
        }

        #endregion
    
        public async void EditTuning()
        {
            if (LayoutDocumentContext.DialogService == null)
                return;

            var result = await LayoutDocumentContext.DialogService.ShowTuningDialog(LayoutDocumentContext);
            if (result != null)
            {
                UpdateConfiguration("Tuning edited", result.Apply);
            }
        }


        public async void EditStrings()
        {
            if (LayoutDocumentContext.DialogService == null)
                return;

            var result = await LayoutDocumentContext.DialogService.ShowStringsDialog(LayoutDocumentContext);
            if (result != null)
            {
                UpdateConfiguration("Edit Strings", result.Apply);
                OnConfigurationChanged(); //
            }
        }

        public async void EditFrets()
        {
            if (LayoutDocumentContext.DialogService == null)
                return;

            var result = await LayoutDocumentContext.DialogService.ShowFretsDialog(LayoutDocumentContext);
            if (result != null)
            {
                UpdateConfiguration("Edit Frets", result.Apply);
                OnConfigurationChanged(); //force update of number of frets
            }
        }
    }
}
