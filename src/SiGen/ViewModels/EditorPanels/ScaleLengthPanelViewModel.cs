using CommunityToolkit.Mvvm.ComponentModel;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Measuring;
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
        private double? multiScaleRatio;

        [ObservableProperty]
        private Measure? bassTrebleSkew;

        public ObservableCollection<StringScaleViewModel> PerStringScales { get; } = new();

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

        protected override void OnConfigurationChanged()
        {
            base.OnConfigurationChanged();
            if (Configuration == null)
                return;

            Mode = Configuration.ScaleLength.Mode;
            SingleScale = Configuration.ScaleLength.SingleScale;
            TrebleScale = Configuration.ScaleLength.TrebleScale;
            BassScale = Configuration.ScaleLength.BassScale;
            UpdateIndividualScaleLengths();
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(Mode) && Mode == ScaleLengthMode.PerString)
                UpdateIndividualScaleLengths(false);

            if (e.PropertyName != nameof(HasMadeChanges))
                NotifyLayoutPropertiesChanged();
        }

        private void UpdateIndividualScaleLengths(bool recreate = true)
        {
            if (Configuration == null)
                return;

            if (PerStringScales.Count == Configuration.NumberOfStrings && !recreate)
                return;

            foreach (var scale in PerStringScales)
                scale.PropertyChanged -= ScaleLengthPanelViewModel_PropertyChanged;

            PerStringScales.Clear();
            // Update the individual scale lengths based on the current configuration
            var scaleLengths = new Measure[Configuration.NumberOfStrings];

            for (int i = 0; i < Configuration.NumberOfStrings; i++)
            {
                var stringConfig = Configuration.GetString(i);
                if (stringConfig != null && stringConfig.ScaleLength != null)
                    scaleLengths[i] = stringConfig.ScaleLength;
                var model = new StringScaleViewModel(i + 1, scaleLengths[i]);
                model.PropertyChanged += ScaleLengthPanelViewModel_PropertyChanged;
                PerStringScales.Add(model);
            }
        }

        private void ScaleLengthPanelViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            NotifyLayoutPropertiesChanged();
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

        public string Label => $"{Lang.Resources.StringLabel} {StringNumber}";

        public StringScaleViewModel(int stringNumber, Measure? scale)
        {
            StringNumber = stringNumber;
            this.scale = scale;
        }
    }
}
