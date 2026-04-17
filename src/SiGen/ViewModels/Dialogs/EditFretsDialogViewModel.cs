using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SiGen.Layouts.Configuration;
using SiGen.Localization;
using SiGen.Physics;
using SiGen.Services;
using SiGen.Services.InstrumentProfiles;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SiGen.ViewModels.Dialogs
{
    public partial class EditFretsDialogViewModel : DialogViewModelBase<EditFretsResult>
    {
        private readonly InstrumentLayoutConfiguration layoutConfiguration;

        public override string Title => Lang.Resources.EditFretsDialog_Title;

        public ObservableCollection<FretCourseEditModel> StringCourses { get; } = new();

        public Array Temperaments { get; } = Enum.GetValues<Temperament>()
            .Where(x => x != Temperament.Custom)
            .ToArray();

        [ObservableProperty]
        private double? numberOfFrets;

        [ObservableProperty]
        private Temperament temperament;

        [ObservableProperty]
        private double? equalTemperamentSteps;

        public bool IsEqualTemperament => Temperament == Temperament.Equal;

        public IRelayCommand SaveCommand { get; }

        public EditFretsDialogViewModel()
        {
            layoutConfiguration = new ElectricGuitarValuesProvider().GetDefaultConfiguration();
            SaveCommand = new RelayCommand(Save);
            Initialize();
        }

        public EditFretsDialogViewModel(ILayoutDocumentContext context)
        {
            layoutConfiguration = context.Configuration;
            SaveCommand = new RelayCommand(Save);
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

            BuildStringCollection();
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
                StringCourses.Add(new FretCourseEditModel(i, layoutConfiguration.StringConfigurations[i], Temperament, globalSteps));
        }

        private void Save()
        {
            var globalFrets = new FretConfiguration
            {
                NumberOfFrets = NormalizeWholeNumber(NumberOfFrets) ?? 0,
                Temperament = Temperament,
                ETSteps = NormalizeWholeNumber(EqualTemperamentSteps) ?? 12
            };

            CompleteDialog(new EditFretsResult(globalFrets, StringCourses.Select(x => x.ToConfiguration()).ToArray()));
        }

        internal static int? NormalizeWholeNumber(double? value)
        {
            if (!value.HasValue)
                return null;
            return (int)Math.Round(value.Value, MidpointRounding.AwayFromZero);
        }
    }

    public partial class FretCourseEditModel : ObservableObject
    {
        private Temperament globalTemperament;
        private int globalEqualTemperamentSteps;

        [ObservableProperty]
        private int courseIndex;

        [ObservableProperty]
        private double? numberOfFrets;

        [ObservableProperty]
        private double? startingFret;

        [ObservableProperty]
        private bool useGlobalTemperament;

        [ObservableProperty]
        private Temperament localTemperament;

        [ObservableProperty]
        private double? equalTemperamentSteps;

        public int CourseNumber => CourseIndex + 1;
        public int NumberOfStrings { get; }
        public bool IsGrouped => NumberOfStrings > 1;
        public bool IsNotGrouped => NumberOfStrings == 1;
        public bool IsEven => CourseIndex % 2 == 0;
        public bool HasLocalTemperament => !UseGlobalTemperament;
        public Temperament EffectiveTemperament => UseGlobalTemperament ? globalTemperament : LocalTemperament;
        public bool UsesEqualTemperament => EffectiveTemperament == Temperament.Equal;
        public string GlobalTemperamentLabel => Texts.ResourceManager.GetString($"Temperament.{globalTemperament}", Texts.Culture) ?? globalTemperament.ToString();
        public string GlobalEqualTemperamentStepsLabel => globalEqualTemperamentSteps.ToString();

        public FretCourseEditModel(int courseIndex, BaseStringConfiguration stringConfiguration, Temperament globalTemperament, int globalEqualTemperamentSteps)
        {
            CourseIndex = courseIndex;
            NumberOfStrings = stringConfiguration.NumberOfStrings;

            var fretConfiguration = stringConfiguration.Frets;
            NumberOfFrets = fretConfiguration?.NumberOfFrets;
            StartingFret = fretConfiguration?.StartingFret;
            UseGlobalTemperament = fretConfiguration?.Temperament == null;
            LocalTemperament = fretConfiguration?.Temperament ?? globalTemperament;
            EqualTemperamentSteps = fretConfiguration?.ETSteps;

            UpdateGlobalSettings(globalTemperament, globalEqualTemperamentSteps);
        }

        public void UpdateGlobalSettings(Temperament temperament, int equalTemperamentSteps)
        {
            globalTemperament = temperament;
            globalEqualTemperamentSteps = equalTemperamentSteps;
            OnPropertyChanged(nameof(EffectiveTemperament));
            OnPropertyChanged(nameof(UsesEqualTemperament));
            OnPropertyChanged(nameof(GlobalTemperamentLabel));
            OnPropertyChanged(nameof(GlobalEqualTemperamentStepsLabel));
        }

        partial void OnCourseIndexChanged(int value)
        {
            OnPropertyChanged(nameof(CourseNumber));
            OnPropertyChanged(nameof(IsEven));
        }

        partial void OnUseGlobalTemperamentChanged(bool value)
        {
            OnPropertyChanged(nameof(HasLocalTemperament));
            OnPropertyChanged(nameof(EffectiveTemperament));
            OnPropertyChanged(nameof(UsesEqualTemperament));
        }

        partial void OnLocalTemperamentChanged(Temperament value)
        {
            OnPropertyChanged(nameof(EffectiveTemperament));
            OnPropertyChanged(nameof(UsesEqualTemperament));
        }

        public FretConfiguration? ToConfiguration()
        {
            var numberOfFrets = EditFretsDialogViewModel.NormalizeWholeNumber(NumberOfFrets);
            var startingFret = EditFretsDialogViewModel.NormalizeWholeNumber(StartingFret);
            var equalTemperamentSteps = EditFretsDialogViewModel.NormalizeWholeNumber(EqualTemperamentSteps);

            var configuration = new FretConfiguration
            {
                NumberOfFrets = numberOfFrets,
                StartingFret = startingFret == 0 ? null : startingFret,
                Temperament = UseGlobalTemperament ? null : LocalTemperament,
                ETSteps = UsesEqualTemperament ? equalTemperamentSteps : null
            };

            if (configuration.NumberOfFrets == null &&
                configuration.StartingFret == null &&
                configuration.Temperament == null &&
                configuration.ETSteps == null)
            {
                return null;
            }

            return configuration;
        }
    }

    public class EditFretsResult
    {
        public FretConfiguration GlobalFrets { get; }
        public FretConfiguration?[] StringFrets { get; }

        public EditFretsResult(FretConfiguration globalFrets, FretConfiguration?[] stringFrets)
        {
            GlobalFrets = globalFrets;
            StringFrets = stringFrets;
        }

        public void Apply(InstrumentLayoutConfiguration layoutConfiguration)
        {
            layoutConfiguration.Frets.NumberOfFrets = GlobalFrets.NumberOfFrets;
            layoutConfiguration.Frets.Temperament = GlobalFrets.Temperament;
            layoutConfiguration.Frets.ETSteps = GlobalFrets.ETSteps;

            var count = Math.Min(layoutConfiguration.StringConfigurations.Count, StringFrets.Length);
            for (int i = 0; i < count; i++)
                layoutConfiguration.StringConfigurations[i].Frets = StringFrets[i];
        }
    }
}
