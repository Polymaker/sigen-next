using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic;
using netDxf.Objects;
using SiGen.Data.Common;
using SiGen.Data.Presets;
using SiGen.Lang;
using SiGen.Layouts.Configuration;
using SiGen.Physics;
using SiGen.Services;
using SiGen.Services.InstrumentProfiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SiGen.ViewModels.Dialogs
{
    public partial class EditTuningDialogViewModel : DialogViewModelBase<EditTuningResult>
    {
        private const double TensionBalanceTolerance = 0.25;

        private readonly InstrumentLayoutConfiguration layoutConfiguration;
        private readonly ILayoutDocumentContext? layoutContext;
        private readonly IStringMaterialEstimationService materialEstimationService;

        public override string Title => Lang.Resources.EditTuningDialog_Title;

        public ObservableCollection<TuningPresetModel> TuningPresets { get; } = new ObservableCollection<TuningPresetModel>();

        public ObservableCollection<TuningCourseViewModel> StringCourses { get; } = new ObservableCollection<TuningCourseViewModel>();

        private IInstrumentValuesProvider? valuesProvider { get; }

        public RelayCommand<InstrumentTuningPreset> ApplyTuningCommand { get; }

        public IRelayCommand SaveCommand => new RelayCommand(SaveTuning);

        public EditTuningDialogViewModel()
        {
            valuesProvider = new ElectricGuitarValuesProvider();
            var layout = valuesProvider.GetDefaultConfiguration();
            this.layoutConfiguration = layout;
            materialEstimationService = new StringMaterialEstimationService(new MockStringDataService());
            UpdateAvailablePresets();
            BuildStringCollection();
            ApplyTuningCommand = new RelayCommand<InstrumentTuningPreset>(ApplyTuningPreset);
        }

        public EditTuningDialogViewModel(ILayoutDocumentContext context)
        {
            this.layoutConfiguration = context.Configuration;
            this.valuesProvider = context.InstrumentValuesProvider;
            materialEstimationService = context.MaterialEstimationService;
            
            ShowTitleBar = true;
            ApplyTuningCommand = new RelayCommand<InstrumentTuningPreset>(ApplyTuningPreset);
            this.layoutContext = context;

            UpdateAvailablePresets();
            BuildStringCollection();
        }

        private void UpdateAvailablePresets()
        {
            TuningPresets.Clear();
            if (valuesProvider == null) return;

            var presets = valuesProvider.GetTuningPresets();
            foreach (var preset in presets)
            {
                if (preset.IsCompatible(layoutConfiguration))
                    TuningPresets.Add(new TuningPresetModel(preset));
            }
        }

        private void BuildStringCollection()
        {
            StringCourses.Clear();
            bool anyCourse = layoutConfiguration.StringConfigurations.OfType<StringGroupConfiguration>().Any();
            
            for (int i = 0; i < layoutConfiguration.NumberOfStrings; i++)
            {
                var stringElem = layoutContext?.Layout?.GetStringElement(i);

                var course = new TuningCourseViewModel(i, layoutConfiguration.StringConfigurations[i], (double)(stringElem?.Path.Length ?? 0), RecalculateTensionBalance);
                //if (anyCourse && course.NumberOfStrings > 1)
                //{
                //    course.Label = $"{Resources.StringCourseLabel} {i + 1}";
                //}
                //else
                //    course.Label = $"{Resources.StringLabel} {i + 1}";
                StringCourses.Add(course);
            }
        }

        public async Task EstimateUnitWeights()
        {
            await materialEstimationService.EstimateUnitWeightsAsync(layoutConfiguration);

            for (int i = 0; i < layoutConfiguration.NumberOfStrings; i++)
            {
                var strConfig = layoutConfiguration.StringConfigurations[i];
                if (strConfig is SingleStringConfiguration single)
                {
                    StringCourses[i].Strings[0].UnitWeight = single.Material?.UnitWeight;
                    StringCourses[i].Strings[0].RecalculateTension();
                }
                else if (strConfig is StringGroupConfiguration group)
                {
                    for (int j = 0; j < group.NumberOfStrings; j++)
                    {
                        StringCourses[i].Strings[j].UnitWeight = group.Strings[j].Material?.UnitWeight;
                        StringCourses[i].Strings[j].RecalculateTension();
                    }
                }
            }

            RecalculateTensionBalance();
        }

        private void RecalculateTensionBalance()
        {
            var strings = StringCourses
                .SelectMany(course => course.Strings)
                .Where(s => s.StiffnessIndex.HasValue && s.StiffnessIndex.Value > 0)
                .ToList();

            foreach (var stringModel in StringCourses.SelectMany(course => course.Strings))
                stringModel.TensionBalanceState = TensionBalanceState.Unknown;

            if (strings.Count < 2)
                return;

            var averageStiffness = strings.Average(s => s.StiffnessIndex!.Value);
            if (averageStiffness <= 0)
                return;

            foreach (var stringModel in strings)
            {
                var deltaRatio = (stringModel.StiffnessIndex!.Value - averageStiffness) / averageStiffness;
                if (Math.Abs(deltaRatio) <= TensionBalanceTolerance)
                    stringModel.TensionBalanceState = TensionBalanceState.Balanced;
                else if (deltaRatio < 0)
                    stringModel.TensionBalanceState = TensionBalanceState.UnderTensioned;
                else
                    stringModel.TensionBalanceState = TensionBalanceState.OverTensioned;
            }
        }

        private void ApplyTuningPreset(InstrumentTuningPreset? preset)
        {
            if (preset == null) return;

            if (preset.CourseCount != StringCourses.Count) return;

            for (int i = 0; i < preset.CourseCount; i++)
                StringCourses[i].ApplyTunig(preset.Courses[i]);
        }

        private void SaveTuning()
        {
            var tunings = new List<StringCourseTuning>();
            foreach (var courseVM in StringCourses)
            {
                var stringTunings = courseVM.Strings.Select(s => s.Tuning).ToArray();
                tunings.Add(new StringCourseTuning(stringTunings));
            }
            CompleteDialog(new EditTuningResult(tunings.ToArray()));
        }
    }

    public partial class StringTuningModel : ObservableObject
    {
        private readonly Action? tensionMetricsChanged;

        [ObservableProperty]
        private NoteAndOctave? tuning;
        [ObservableProperty]
        private double? stringTension;
        [ObservableProperty]
        private double? stiffnessIndex;
        [ObservableProperty]
        private double? unitWeight;
        [ObservableProperty]
        private TensionBalanceState tensionBalanceState;

        public int StringIndex { get; }
        public int StringNumber => StringIndex + 1;
        public StringProperties? Properties { get; }
        public double StringLengthCM { get; }

        public StringTuningModel(int index, StringProperties? properties, double stringLengthCM, Action? tensionMetricsChanged = null)
        {
            StringIndex = index;
            Tuning = properties?.Tuning;
            Properties = properties;
            StringLengthCM = stringLengthCM;
            this.tensionMetricsChanged = tensionMetricsChanged;
            RecalculateTension();
        }

        partial void OnTuningChanged(NoteAndOctave? value)
        {
            RecalculateTension();
            tensionMetricsChanged?.Invoke();
        }

        //partial void OnUnitWeightChanged(double? value)
        //{
        //    RecalculateTension();
        //    tensionMetricsChanged?.Invoke();
        //}

        public void RecalculateTension()
        {
            if (Tuning.HasValue && StringLengthCM > 0 && UnitWeight.HasValue && UnitWeight > 0)
            {
                double scaleLengthInches = StringLengthCM / 2.54;
                var noteFreq = PitchInterval.CalculateFrequency(PitchInterval.FromNote(Tuning.Value));
                StringTension = (UnitWeight.Value * Math.Pow(2 * scaleLengthInches * noteFreq, 2)) / 386.089;
                StiffnessIndex = StringTension / scaleLengthInches;
                return;
            }

            StringTension = null;
            StiffnessIndex = null;
        }
    }

    public enum TensionBalanceState
    {
        Unknown,
        UnderTensioned,
        Balanced,
        OverTensioned
    }

    public record StringCourseTuning(NoteAndOctave?[] Strings);

    public class EditTuningResult
    {
        public StringCourseTuning[] Tunings { get; }

        public EditTuningResult(StringCourseTuning[] tunings)
        {
            Tunings = tunings;
        }

        public void Apply(InstrumentLayoutConfiguration configuration)
        {
            for (int i = 0; i < Tunings.Length; i++)
            {
                var strConfig = configuration.StringConfigurations[i];
                if (strConfig is SingleStringConfiguration single)
                {
                    single.Tuning = Tunings[i].Strings[0];
                } 
                else if (strConfig is StringGroupConfiguration group)
                {
                    for (int j = 0; j < group.NumberOfStrings; j++)
                    {
                        group.Strings[j].Tuning = Tunings[i].Strings.Length == 1 ? Tunings[i].Strings[0] : Tunings[i].Strings[j];
                    } 
                }
            }
        }

    }

    public partial class TuningCourseViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<StringTuningModel> _strings;

        public int CourseIndex { get; set; }
        public int CourseNumber => CourseIndex + 1;
        public bool IsGrouped => Strings.Count > 1;
        public bool IsNotGrouped => Strings.Count == 1;
        public bool IsEven => CourseIndex % 2 == 0;
        public int NumberOfStrings => Strings.Count;
        public string Label { get; set; } = string.Empty;

        public TuningCourseViewModel()
        {
            _strings = new ObservableCollection<StringTuningModel>();
        }

        public TuningCourseViewModel(int index, BaseStringConfiguration stringConfiguration, double stringLength, Action? tensionMetricsChanged = null)
        {
            CourseIndex = index;
            _strings = new ObservableCollection<StringTuningModel>();
            if (stringConfiguration is SingleStringConfiguration singleString)
            {
                _strings.Add(new StringTuningModel(0, singleString.Properties, stringLength, tensionMetricsChanged) { UnitWeight = singleString.Material?.UnitWeight });
            }
            else if (stringConfiguration is StringGroupConfiguration stringGroup)
            {
                for (int i = 0; i < stringGroup.NumberOfStrings; i++)
                    _strings.Add(new StringTuningModel(i, stringGroup.Strings[i], stringLength, tensionMetricsChanged) { UnitWeight = stringGroup.Strings[i].Material?.UnitWeight});
            }
        }

        public void ApplyTunig(TuningCourse tuning)
        {
            if (IsGrouped && NumberOfStrings == tuning.Strings.Length)
            {
                for (int i = 0; i < NumberOfStrings; i++)
                    Strings[i].Tuning = tuning.Strings[i];
            }
            else
            {
                for (int i = 0; i < NumberOfStrings; i++)
                    Strings[i].Tuning = tuning.PrimaryNote;
            }
        }
    }

    public class TuningPresetModel
    {
        public InstrumentTuningPreset Preset { get; }

        public string Name => Preset.Name;
        public string Description { get; set; } = string.Empty;

        public TuningPresetModel(InstrumentTuningPreset preset)
        {
            Preset = preset;
            Description = string.Join(" - ", preset.Courses.ToList());
        }
    }
}
