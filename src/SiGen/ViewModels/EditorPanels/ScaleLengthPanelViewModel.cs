using CommunityToolkit.Mvvm.ComponentModel;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.Physics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.ViewModels.EditorPanels
{
    public partial class ScaleLengthPanelViewModel : EditorPanelViewModelBase
    {
        [ObservableProperty]
        private ScaleLengthCalculationMethod calculationMethod;

        [ObservableProperty]
        private ScaleLengthMode mode;

        [ObservableProperty]
        private Measure? singleScale;

        [ObservableProperty]
        private Measure? trebleScale;

        [ObservableProperty]
        private Measure? bassScale;

        [ObservableProperty]
        private double multiScaleRatio;

        [ObservableProperty]
        private Measure? bassTrebleSkew;

        public record AlignmentRatioPreset(double Ratio, string Label);

        public ObservableCollection<StringScaleViewModel> PerStringScales { get; } = new();

        public ObservableCollection<AlignmentRatioPreset> MultiScaleRatioPresets { get; } = new ();

        [ObservableProperty]
        private AlignmentRatioPreset? selectedRatioPreset;

        public Array ScaleLengthModes => Enum.GetValues(typeof(ScaleLengthMode));

        //public ScaleLengthPanelViewModel(InstrumentLayoutConfiguration config) : base(config)
        //{
        //    CalculationMethod = config.ScaleLength.CalculationMethod;
        //    Mode = config.ScaleLength.Mode;
        //    SingleScale = config.ScaleLength.SingleScale;
        //    TrebleScale = config.ScaleLength.TrebleScale;
        //    BassScale = config.ScaleLength.BassScale;
        //    MultiScaleRatio = config.ScaleLength.MultiScaleRatio;
        //    BassTrebleSkew = config.ScaleLength.BassTrebleSkew;
        //    UpdateIndividualScaleLengths();
        //}

        //public ScaleLengthPanelViewModel() : base(new InstrumentLayoutConfiguration() { NumberOfStrings = 6 })
        //{
        //    // Initialize with default values
        //    CalculationMethod = ScaleLengthCalculationMethod.Auto;
        //    Mode = ScaleLengthMode.Single;
        //    SingleScale = new Measure(LengthUnit.In, 25.5); // Default scale length
        //    TrebleScale = null;
        //    BassScale = null;
        //    MultiScaleRatio = null;
        //    BassTrebleSkew = null;
        //}

        private bool isChangingDefaultRatio = false;

        protected override void OnConfigurationChanged()
        {
            base.OnConfigurationChanged();
            if (Configuration == null)
                return;

            Mode = Configuration.ScaleLength.Mode;
            SingleScale = Configuration.ScaleLength.SingleScale;
            TrebleScale = Configuration.ScaleLength.TrebleScale;
            BassScale = Configuration.ScaleLength.BassScale;
            MultiScaleRatio = Configuration.ScaleLength.MultiScaleRatio ?? 0.5; // Default to 50% if not set
            UpdateIndividualScaleLengths();
            RebuildMultiScaleRatioPresets();
            
            SelectedRatioPreset = MultiScaleRatioPresets
                .FirstOrDefault(x => Math.Abs(x.Ratio - MultiScaleRatio) < 0.01); // Find the closest preset
        }

        partial void OnModeChanged(ScaleLengthMode oldValue, ScaleLengthMode newValue)
        {
            if (newValue == ScaleLengthMode.PerString)
            {
                UpdateIndividualScaleLengths(false);
                for (int i = 0; i < PerStringScales.Count; i++)
                {
                    if (PerStringScales[i].Scale  == null)
                    {
                        if (oldValue == ScaleLengthMode.Single) {
                            PerStringScales[i].Scale = Configuration?.ScaleLength.SingleScale;
                        }
                        else if (Configuration?.ScaleLength.BassScale != null &&
                            Configuration?.ScaleLength.TrebleScale != null)
                        {
                            var scaleLength = MathD.Map(0, PerStringScales.Count - 1,
                                Configuration.ScaleLength.BassScale.Value.NormalizedValue,
                                Configuration.ScaleLength.TrebleScale.Value.NormalizedValue,
                                i);
                            PerStringScales[i].Scale = Measure.Round(Measure.FromNormalizedValue(LengthUnit.In, scaleLength), 0.05d);
                        }
                    }
                }
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(BassTrebleSkew))
            {
                UpdateConfiguration("BassTrebleSkew", config =>
                {
                    config.ScaleLength.BassTrebleSkew = BassTrebleSkew;
                });
            }

            if (e.PropertyName == nameof(Mode))
            {
                if (Mode == ScaleLengthMode.Single && Measure.IsNullOrEmpty(SingleScale)) return;
                if (Mode == ScaleLengthMode.Multiscale && (Measure.IsNullOrEmpty(TrebleScale) || Measure.IsNullOrEmpty(BassScale))) return;
                if (Mode == ScaleLengthMode.PerString && (PerStringScales.Any(x => Measure.IsNullOrEmpty(x.Scale)))) return;

                UpdateConfiguration("ScaleLengthMode", config =>
                {
                    config.ScaleLength.Mode = Mode;
                    if (Mode == ScaleLengthMode.Multiscale)
                        config.ScaleLength.MultiScaleRatio = MultiScaleRatio;
                });
            }

            if (e.PropertyName == nameof(SingleScale) && Mode == ScaleLengthMode.Single)
            {
                UpdateConfiguration("SingleScale", config =>
                {
                    if (config.ScaleLength.Mode != ScaleLengthMode.Single)
                        config.ScaleLength.Mode = Mode;
                    config.ScaleLength.SingleScale = SingleScale;
                });
            }

            if ((e.PropertyName == nameof(BassScale) || e.PropertyName == nameof(TrebleScale) )
                && Mode == ScaleLengthMode.Multiscale && BassScale != null && TrebleScale != null)
            {
                UpdateConfiguration(e.PropertyName, config =>
                {
                    if (config.ScaleLength.Mode != ScaleLengthMode.Multiscale)
                        config.ScaleLength.Mode = Mode;
                    config.ScaleLength.BassScale = BassScale;
                    config.ScaleLength.TrebleScale = TrebleScale;
                });
            }

            if (e.PropertyName == nameof(MultiScaleRatio)/* && Mode == ScaleLengthMode.Multiscale*/)
            {
                SelectedRatioPreset = MultiScaleRatioPresets.FirstOrDefault(x => Math.Abs(x.Ratio - MultiScaleRatio) < 0.001); // Find the closest preset
                isChangingDefaultRatio = true;
                foreach (var scale in PerStringScales.ToArray())
                    scale.SetDefaultRatio(MultiScaleRatio);
                isChangingDefaultRatio = false;
                UpdateConfiguration("MultiScaleRatio", config =>
                {
                    config.ScaleLength.MultiScaleRatio = MultiScaleRatio;
                });
            }
            if (e.PropertyName == nameof(SelectedRatioPreset))
            {
                if (SelectedRatioPreset != null)
                {
                    MultiScaleRatio = Math.Round(SelectedRatioPreset.Ratio * 1000) / 1000d;
                }
            }


            //if (e.PropertyName != nameof(HasMadeChanges))
            //    NotifyLayoutPropertiesChanged();
        }

        protected override void OnNumberOfStringsChanged()
        {
            base.OnNumberOfStringsChanged();

            if (Mode == ScaleLengthMode.PerString)
                UpdateIndividualScaleLengths(false);
        }

        private void UpdateIndividualScaleLengths(bool recreate = true)
        {
            if (Configuration == null)
                return;

            if (PerStringScales.Count == Configuration.NumberOfStrings && !recreate)
                return;

            foreach (var scale in PerStringScales)
                scale.PropertyChanged -= StringScaleLength_PropertyChanged;

            PerStringScales.Clear();
            // Update the individual scale lengths based on the current configuration
            var scaleLengths = new Measure?[Configuration.NumberOfStrings];

            for (int i = 0; i < Configuration.NumberOfStrings; i++)
            {
                var stringConfig = Configuration.GetString(i);
                if (stringConfig != null && !Measure.IsNullOrEmpty(stringConfig.ScaleLength))
                    scaleLengths[i] = stringConfig.ScaleLength.Value;
                var model = new StringScaleViewModel(i + 1, scaleLengths[i], stringConfig?.MultiScaleRatio);
                model.DefaultRatio = Configuration.ScaleLength.MultiScaleRatio ?? 0.5;
                model.PropertyChanged += StringScaleLength_PropertyChanged;
                PerStringScales.Add(model);
            }
        }

        private void RebuildMultiScaleRatioPresets()
        {
            MultiScaleRatioPresets.Clear();
            if (Configuration == null)
                return;

            int maxFrets = Configuration.GetMaxFrets();

            for (int i = 0; i <= maxFrets; i++)
            {
                var fretRatio = 1d - (1d / Math.Pow(2, i / 12d));
                string fretLabel = i == 0 ? SiGen.Localization.Texts.FingerboardEnd_Nut : $"{Lang.Resources.FretLabel} {i}";
                MultiScaleRatioPresets.Add(new AlignmentRatioPreset(fretRatio, fretLabel));
            }

            MultiScaleRatioPresets.Add(new AlignmentRatioPreset(1d, SiGen.Localization.Texts.FingerboardEnd_Bridge));
        }

        private void StringScaleLength_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not StringScaleViewModel scaleViewModel || Configuration == null || isChangingDefaultRatio)
                return;

            UpdateConfiguration("StringScaleLength", config =>
            {
                var strConfig = config.GetString(scaleViewModel.StringNumber - 1);
                if (strConfig != null)
                {
                    strConfig.MultiScaleRatio = scaleViewModel.Ratio;
                    strConfig.ScaleLength = scaleViewModel.Scale;
                }
            });
        }
    }

    public partial class StringScaleViewModel : ObservableObject
    {
        public int StringNumber { get; }
        [ObservableProperty]
        private Measure? scale;
        [ObservableProperty]
        private double? ratio;
        public string Label => $"{Lang.Resources.StringLabel} {StringNumber}";

        [ObservableProperty]
        private double defaultRatio = 0.5;

        public StringScaleViewModel(int stringNumber, Measure? scale, double? ratio)
        {
            StringNumber = stringNumber;
            this.scale = scale;
            this.ratio = ratio;
        }

        public void SetDefaultRatio(double ratio)
        {
            DefaultRatio = ratio;
        }
    }
}
