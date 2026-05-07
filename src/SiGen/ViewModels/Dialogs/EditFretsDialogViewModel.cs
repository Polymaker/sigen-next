using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SiGen.Layouts.Configuration;
using SiGen.Localization;
using SiGen.Physics;
using SiGen.Services;
using SiGen.Services.InstrumentProfiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SiGen.ViewModels.Dialogs
{
    public partial class EditFretsDialogViewModel : DialogViewModelBase<EditFretsResult>
    {
        private readonly InstrumentLayoutConfiguration layoutConfiguration;
        private readonly IDialogService dialogService;

        public override string Title => Lang.Resources.EditFretsDialog_Title;

        public ObservableCollection<FretCourseEditModel> StringCourses { get; } = new();

        public Array Temperaments { get; } = Enum.GetValues<Temperament>();

        public IReadOnlyList<TemperamentOptionItem> TemperamentsWithGlobalOption { get; } = BuildTemperamentOptions();

        private static IReadOnlyList<TemperamentOptionItem> BuildTemperamentOptions()
        {
            return [
                new TemperamentOptionItem(null, Lang.Resources.EditFretsDialog_UseGlobal),
                ..Enum.GetValues<Temperament>().Select(t =>
                    new TemperamentOptionItem(t, Texts.ResourceManager.GetString($"Temperament.{t}", Texts.Culture) ?? t.ToString()))
            ];
        }

        [ObservableProperty]
        private double? numberOfFrets;

        [ObservableProperty]
        private Temperament temperament;

        [ObservableProperty]
        private double? equalTemperamentSteps;

        [ObservableProperty]
        private bool useCustomFretIntervals;

        public bool IsEqualTemperament => Temperament == Temperament.Equal;

        public List<double>? GlobalIntervals { get; set; }

        public string GlobalIntervalsSummary => BuildIntervalSummary(GlobalIntervals);

        public IRelayCommand SaveCommand { get; }
        public IAsyncRelayCommand EditGlobalIntervalsCommand { get; }

        public EditFretsDialogViewModel() : this(new ElectricGuitarValuesProvider().GetDefaultConfiguration(), new MockDialogService())
        {
        }

        public EditFretsDialogViewModel(ILayoutDocument context, IDialogService dialogService) : this(context.Configuration, dialogService)
        {
        }

        private EditFretsDialogViewModel(InstrumentLayoutConfiguration configuration, IDialogService dialogService)
        {
            layoutConfiguration = configuration;
            this.dialogService = dialogService;
            SaveCommand = new RelayCommand(Save);
            EditGlobalIntervalsCommand = new AsyncRelayCommand(EditGlobalIntervalsAsync);
            Initialize();
        }

        private void Initialize()
        {
            Resizable = true;
            if (layoutConfiguration.StringConfigurations.Count == 0)
                layoutConfiguration.InitializeStringConfigs();

            NumberOfFrets = layoutConfiguration.NumberOfFrets ?? 24;
            Temperament = layoutConfiguration.Temperament;
            EqualTemperamentSteps = layoutConfiguration.Frets.ETSteps ?? 12;
            GlobalIntervals = layoutConfiguration.Frets.Intervals;
            BuildStringCollection();
            UseCustomFretIntervals = GlobalIntervals?.Count > 0 || StringCourses.Any(x => x.Intervals?.Count > 0);
            
            OnPropertyChanged(nameof(GlobalIntervalsSummary));
        }

        partial void OnTemperamentChanged(Temperament value)
        {
            OnPropertyChanged(nameof(IsEqualTemperament));
            UpdateGlobalTemperamentSettings();
        }

        partial void OnEqualTemperamentStepsChanged(double? value)
        {
            UpdateGlobalTemperamentSettings();
        }

        private void UpdateGlobalTemperamentSettings()
        {
            var globalSteps = NormalizeWholeNumber(EqualTemperamentSteps) ?? 12;
            foreach (var course in StringCourses)
                course.UpdateGlobalSettings(Temperament, globalSteps);
        }

        private void BuildStringCollection()
        {
            StringCourses.Clear();
            var globalSteps = NormalizeWholeNumber(EqualTemperamentSteps) ?? 12;
            for (int i = 0; i < layoutConfiguration.StringConfigurations.Count; i++)
                StringCourses.Add(new FretCourseEditModel(i, layoutConfiguration.StringConfigurations[i], Temperament, globalSteps, GlobalIntervals, TemperamentsWithGlobalOption, OpenStringIntervalsDialogAsync));
        }

        private async System.Threading.Tasks.Task EditGlobalIntervalsAsync()
        {
            var title = Lang.Resources.ResourceManager.GetString("FretIntervalsDialog.GlobalTitle", Lang.Resources.Culture)
                ?? "Manual fret positions";
            var intervals = await dialogService.ShowFretIntervalsDialogAsync(title, GlobalIntervals);
            if (intervals == null)
                return;

            GlobalIntervals = intervals.Count > 0 ? intervals : null;
            OnPropertyChanged(nameof(GlobalIntervalsSummary));
        }

        private async System.Threading.Tasks.Task OpenStringIntervalsDialogAsync(FretCourseEditModel course)
        {
            var titleFormat = Lang.Resources.ResourceManager.GetString("FretIntervalsDialog.StringTitleFormat", Lang.Resources.Culture)
                ?? "Manual fret positions (string/course {0})";
            var title = string.Format(titleFormat, course.CourseNumber);
            var intervals = await dialogService.ShowFretIntervalsDialogAsync(title, course.Intervals);
            if (intervals == null)
                return;

            course.Intervals = intervals.Count > 0 ? intervals : null;
        }

        private void Save()
        {
            var globalFrets = new GlobalFretConfiguration
            {
                NumberOfFrets = NormalizeWholeNumber(NumberOfFrets) ?? 0,
                Temperament = Temperament,
                ETSteps = NormalizeWholeNumber(EqualTemperamentSteps) ?? 12,
                Intervals = GlobalIntervals?.ToList()
            };

            CompleteDialog(new EditFretsResult(globalFrets, StringCourses.Select(x => x.ToConfiguration()).ToArray()));
        }

        internal static int? NormalizeWholeNumber(double? value)
        {
            if (!value.HasValue)
                return null;
            return (int)Math.Round(value.Value, MidpointRounding.AwayFromZero);
        }

        internal static string BuildIntervalSummary(IReadOnlyList<double>? intervals)
        {
            if (intervals == null || intervals.Count == 0)
                return Lang.Resources.ResourceManager.GetString("Common.None", Lang.Resources.Culture) ?? "None";

            var format = Lang.Resources.ResourceManager.GetString("FretIntervalsDialog.ValuesCountFormat", Lang.Resources.Culture)
                ?? "{0} values";
            return string.Format(format, intervals.Count);
        }
    }

    public partial class FretCourseEditModel : ObservableObject
    {
        private readonly IReadOnlyList<TemperamentOptionItem> temperamentOptions;

        private readonly Func<FretCourseEditModel, System.Threading.Tasks.Task>? editIntervalsAction;

        private Temperament globalTemperament;
        private int globalEqualTemperamentSteps;
        private IReadOnlyList<double>? globalIntervals;

        [ObservableProperty]
        private int courseIndex;

        [ObservableProperty]
        private double? numberOfFrets;

        [ObservableProperty]
        private double? startingFret;

        [ObservableProperty]
        private Temperament? selectedTemperament;

        [ObservableProperty]
        private double? equalTemperamentSteps;

        [ObservableProperty]
        private List<double>? intervals;

        public int CourseNumber => CourseIndex + 1;
        public int NumberOfStrings { get; }
        public bool IsGrouped => NumberOfStrings > 1;
        public bool IsNotGrouped => NumberOfStrings == 1;
        public bool IsEven => CourseIndex % 2 == 0;

        public TemperamentOptionItem? SelectedTemperamentOption
        {
            get => temperamentOptions.FirstOrDefault(x => x.Value == SelectedTemperament);
            set => SelectedTemperament = value?.Value;
        }

        public bool UsesEqualTemperament => (SelectedTemperament ?? globalTemperament) == Temperament.Equal;
        public string GlobalTemperamentLabel => Texts.ResourceManager.GetString($"Temperament.{globalTemperament}", Texts.Culture) ?? globalTemperament.ToString();

        public IAsyncRelayCommand EditIntervalsCommand { get; }

        public FretCourseEditModel(int courseIndex, BaseStringConfiguration stringConfiguration, Temperament globalTemperament, int globalEqualTemperamentSteps, IReadOnlyList<double>? globalIntervals, IReadOnlyList<TemperamentOptionItem> temperamentOptions, Func<FretCourseEditModel, System.Threading.Tasks.Task>? editIntervalsAction)
        {
            CourseIndex = courseIndex;
            NumberOfStrings = stringConfiguration.NumberOfStrings;
            this.temperamentOptions = temperamentOptions;
            this.editIntervalsAction = editIntervalsAction;

            var fretConfiguration = stringConfiguration.Frets;
            NumberOfFrets = fretConfiguration?.NumberOfFrets;
            StartingFret = fretConfiguration?.StartingFret;
            EqualTemperamentSteps = fretConfiguration?.ETSteps;
            Intervals = fretConfiguration?.Intervals?.ToList();
            SelectedTemperament = fretConfiguration?.Temperament;
            EditIntervalsCommand = new AsyncRelayCommand(EditIntervalsAsync);

            UpdateGlobalSettings(globalTemperament, globalEqualTemperamentSteps);
        }

        public void UpdateGlobalSettings(Temperament temperament, int equalTemperamentSteps)
        {
            globalTemperament = temperament;
            globalEqualTemperamentSteps = equalTemperamentSteps;
            OnPropertyChanged(nameof(SelectedTemperament));
            OnPropertyChanged(nameof(SelectedTemperamentOption));
            OnPropertyChanged(nameof(UsesEqualTemperament));
            OnPropertyChanged(nameof(GlobalTemperamentLabel));
        }

        partial void OnCourseIndexChanged(int value)
        {
            OnPropertyChanged(nameof(CourseNumber));
            OnPropertyChanged(nameof(IsEven));
        }

        partial void OnSelectedTemperamentChanged(Temperament? value)
        {
            OnPropertyChanged(nameof(UsesEqualTemperament));
        }


        private async System.Threading.Tasks.Task EditIntervalsAsync()
        {
            if (editIntervalsAction != null)
                await editIntervalsAction(this);
        }

        public StringFretConfiguration? ToConfiguration()
        {
            var numberOfFrets = EditFretsDialogViewModel.NormalizeWholeNumber(NumberOfFrets);
            var startingFret = EditFretsDialogViewModel.NormalizeWholeNumber(StartingFret);
            var equalTemperamentSteps = EditFretsDialogViewModel.NormalizeWholeNumber(EqualTemperamentSteps);

            var configuration = new StringFretConfiguration
            {
                NumberOfFrets = numberOfFrets,
                StartingFret = startingFret == 0 ? null : startingFret,
                Temperament = SelectedTemperament,
                ETSteps = UsesEqualTemperament ? equalTemperamentSteps : null,
                Intervals = Intervals?.ToList()
            };

            if (configuration.NumberOfFrets == null &&
                configuration.StartingFret == null &&
                configuration.Temperament == null &&
                configuration.ETSteps == null &&
                configuration.Intervals == null)
            {
                return null;
            }

            return configuration;
        }
    }

    public class EditFretsResult
    {
        public GlobalFretConfiguration GlobalFrets { get; }
        public StringFretConfiguration?[] StringFrets { get; }

        public EditFretsResult(GlobalFretConfiguration globalFrets, StringFretConfiguration?[] stringFrets)
        {
            GlobalFrets = globalFrets;
            StringFrets = stringFrets;
        }

        public void Apply(InstrumentLayoutConfiguration layoutConfiguration)
        {
            layoutConfiguration.Frets.NumberOfFrets = GlobalFrets.NumberOfFrets;
            layoutConfiguration.Frets.Temperament = GlobalFrets.Temperament;
            layoutConfiguration.Frets.ETSteps = GlobalFrets.ETSteps;
            layoutConfiguration.Frets.Intervals = GlobalFrets.Intervals;

            var count = Math.Min(layoutConfiguration.StringConfigurations.Count, StringFrets.Length);
            for (int i = 0; i < count; i++)
                layoutConfiguration.StringConfigurations[i].Frets = StringFrets[i];
        }
    }

    public sealed record TemperamentOptionItem(Temperament? Value, string DisplayName)
    {
        public bool IsGlobalOption => Value == null;
    }
}
