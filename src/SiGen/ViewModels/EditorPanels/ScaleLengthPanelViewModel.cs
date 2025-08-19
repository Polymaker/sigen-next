using CommunityToolkit.Mvvm.ComponentModel;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
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

        public override void OnConfigurationChanged()
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
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(Mode) && Mode == ScaleLengthMode.PerString)
                UpdateIndividualScaleLengths(false);

            if (e.PropertyName == nameof(BassTrebleSkew))
            {
                LayoutDocumentContext.UpdateConfiguration("BassTrebleSkew", config =>
                {
                    config.ScaleLength.BassTrebleSkew = BassTrebleSkew;
                });
            }

            if (e.PropertyName == nameof(Mode))
            {
                if (Mode == ScaleLengthMode.Single && Measure.IsNullOrEmpty(SingleScale)) return;
                if (Mode == ScaleLengthMode.Multiscale && (Measure.IsNullOrEmpty(TrebleScale) || Measure.IsNullOrEmpty(BassScale))) return;
                if (Mode == ScaleLengthMode.PerString && (PerStringScales.Any(x => Measure.IsNullOrEmpty(x.Scale)))) return;

                LayoutDocumentContext.UpdateConfiguration("ScaleLengthMode", config =>
                {
                    config.ScaleLength.Mode = Mode;
                    if (Mode == ScaleLengthMode.Multiscale)
                        config.ScaleLength.MultiScaleRatio = MultiScaleRatio;
                });
            }

            if (e.PropertyName == nameof(SingleScale) && Mode == ScaleLengthMode.Single)
            {
                LayoutDocumentContext.UpdateConfiguration("SingleScale", config =>
                {
                    if (config.ScaleLength.Mode != ScaleLengthMode.Single)
                        config.ScaleLength.Mode = Mode;
                    config.ScaleLength.SingleScale = SingleScale;
                });
            }

            if ((e.PropertyName == nameof(BassScale) || e.PropertyName == nameof(TrebleScale) )
                && Mode == ScaleLengthMode.Multiscale && BassScale != null && TrebleScale != null)
            {
                LayoutDocumentContext.UpdateConfiguration(e.PropertyName, config =>
                {
                    if (config.ScaleLength.Mode != ScaleLengthMode.Multiscale)
                        config.ScaleLength.Mode = Mode;
                    config.ScaleLength.BassScale = BassScale;
                    config.ScaleLength.TrebleScale = TrebleScale;
                });
            }

            if (e.PropertyName == nameof(MultiScaleRatio) && Mode == ScaleLengthMode.Multiscale)
            {
                LayoutDocumentContext.UpdateConfiguration("MultiScaleRatio", config =>
                {
                    config.ScaleLength.MultiScaleRatio = MultiScaleRatio;
                });
            }


            //if (e.PropertyName != nameof(HasMadeChanges))
            //    NotifyLayoutPropertiesChanged();
        }

        public override void OnNumberOfStringsChanged()
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
            var scaleLengths = new Measure[Configuration.NumberOfStrings];

            for (int i = 0; i < Configuration.NumberOfStrings; i++)
            {
                var stringConfig = Configuration.GetString(i);
                if (stringConfig != null && stringConfig.ScaleLength != null)
                    scaleLengths[i] = stringConfig.ScaleLength;
                var model = new StringScaleViewModel(i + 1, scaleLengths[i], stringConfig?.MultiScaleRatio ?? 0.5);
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
                double ratio = (double)(i + 1) / maxFrets;
                var interval = PitchInterval.From12TET(i, 0);
                var fretRatio = 1d - (1d / interval.Ratio);
                string fretLabel = i == 0 ? SiGen.Localization.Texts.FingerboardEnd_Nut : $"{Lang.Resources.FretLabel} {i}";
                MultiScaleRatioPresets.Add(new AlignmentRatioPreset(fretRatio, fretLabel));
            }
            
        }

        private void StringScaleLength_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not StringScaleViewModel scaleViewModel || Configuration == null)
                return;

            LayoutDocumentContext.UpdateConfiguration("StringScaleLength", config =>
            {
                var strConfig = config.GetString(scaleViewModel.StringNumber - 1);
                if (strConfig != null)
                {
                    strConfig.MultiScaleRatio = scaleViewModel.Ratio;
                    strConfig.ScaleLength = scaleViewModel.Scale;
                }
            });
            //NotifyLayoutPropertiesChanged();
        }

        public override void ApplyChanges()
        {
            if (Configuration == null)
                return;

            Configuration.ScaleLength.CalculationMethod = CalculationMethod;
            Configuration.ScaleLength.Mode = Mode;
            Configuration.ScaleLength.SingleScale = SingleScale;
            Configuration.ScaleLength.TrebleScale = TrebleScale;
            Configuration.ScaleLength.BassScale = BassScale;
            Configuration.ScaleLength.MultiScaleRatio = MultiScaleRatio;
            Configuration.ScaleLength.BassTrebleSkew = BassTrebleSkew;

            if (Mode == ScaleLengthMode.PerString)
            {
                // Update individual scale lengths in the configuration
                for (int i = 0; i < Configuration.NumberOfStrings; i++)
                {
                    var stringConfig = Configuration.GetString(i);
                    if (stringConfig != null && i < PerStringScales.Count)
                    {
                        stringConfig.ScaleLength = PerStringScales[i].Scale;
                    }
                }
            }

            base.ApplyChanges();
        }

        protected override bool CanApplyChanges()
        {
            if (Mode == ScaleLengthMode.PerString)
            {
                // Ensure all individual scale lengths are set
                if (!PerStringScales.All(x => x.Scale != null && !x.Scale.IsEmpty && x.Scale.Value > 0))
                    return false;
            }
            else if (Mode == ScaleLengthMode.Single)
            {
                // Ensure single scale length is set
                if (SingleScale == null || SingleScale.Value <= 0)
                    return false;
            }
            else if (Mode == ScaleLengthMode.Multiscale)
            {
                // Ensure both treble and bass scales are set
                if ((TrebleScale == null || TrebleScale.Value <= 0) ||
                    (BassScale == null || BassScale.Value <= 0))
                    return false;
            }
            return base.CanApplyChanges();
        }
    }

    public partial class StringScaleViewModel : ObservableObject
    {
        public int StringNumber { get; }
        [ObservableProperty]
        private Measure? scale;
        [ObservableProperty]
        private double ratio;
        public string Label => $"{Lang.Resources.StringLabel} {StringNumber}";

        public StringScaleViewModel(int stringNumber, Measure? scale, double ratio)
        {
            StringNumber = stringNumber;
            this.scale = scale;
            this.ratio = ratio;
        }
    }
}
