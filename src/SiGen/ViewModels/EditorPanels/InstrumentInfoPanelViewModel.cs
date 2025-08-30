using CommunityToolkit.Mvvm.ComponentModel;
using SiGen.Data.Common;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SiGen.ViewModels.EditorPanels
{
    public partial class InstrumentInfoPanelViewModel : EditorPanelViewModelBase
    {
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

        public Array InstrumentTypes => Enum.GetValues(typeof(InstrumentType));

        public InstrumentInfoPanelViewModel()
        {
            
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

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(LeftHanded))
            {
                OnPropertyChanged(nameof(RightHanded));
                UpdateConfiguration("Change handedness", config => config.LeftHanded = LeftHanded);
            }  

            if (e.PropertyName == nameof(InstrumentType) && InstrumentType.HasValue)
            {
                UpdateConfiguration("Change Instrument Type", config => config.InstrumentType = InstrumentType.Value);
            }
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

                UpdateConfiguration("Add string", config =>
                {
                    config.NumberOfStrings = stringConfigurations.Count;
                    config.StringConfigurations = stringConfigurations.ToList();

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
            }
        }

        protected BaseStringConfiguration GetNewStringConfiguration(FingerboardSide side)
        {
            var previousString = side == FingerboardSide.Bass
                ? (stringConfigurations.Count > 0 ? stringConfigurations[0] : null)
                : (stringConfigurations.Count > 0 ? stringConfigurations[^1] : null);

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
                    stringConfigurations.Count > 1 &&
                    !Measuring.Measure.IsNullOrEmpty(previousString.ScaleLength))
                {
                    var secondPrevious = side == FingerboardSide.Bass
                        ? (stringConfigurations.Count > 0 ? stringConfigurations[1] : null)
                        : (stringConfigurations.Count > 0 ? stringConfigurations[^2] : null);

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

                UpdateConfiguration("Remove string", config =>
                {
                    config.NumberOfStrings = stringConfigurations.Count;
                    config.StringConfigurations = stringConfigurations.ToList();

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
            }
        }

        #endregion


    }
}
