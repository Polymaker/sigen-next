using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SiGen.Data.Common;
using SiGen.Data.Entities;
using SiGen.Layouts.Configuration;
using SiGen.Measuring;
using SiGen.Services;
using SiGen.Services.InstrumentProfiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SiGen.ViewModels.Dialogs
{
    public partial class EditStringsDialogViewModel : DialogViewModelBase<EditStringsResult>
    {

        //public ObservableCollection<StringItemModel> Strings { get; }

        public ObservableCollection<StringCourseEditModel> Courses { get; } = new ();

        #region Combobox datasources

        public ObservableCollection<StringSet> AvailableSets { get; } = new ObservableCollection<StringSet>();
        public Array StringMaterials => Enum.GetValues(typeof(StringMaterialType));

        #endregion

        public override string Title => "Strings Configuration";

        [ObservableProperty]
        private int totalNumberOfStrings;

        [ObservableProperty]
        private bool isBassInstrument;

        #region Commands

        public ICommand ApplyStringSetCommand { get; }
        public ICommand SaveCommand { get; }
        public IRelayCommand<StringCourseEditModel> AddStringToCourseCommand { get; } 
        public IRelayCommand<StringEditModel> RemoveStringFromCourseCommand { get; }

        #endregion

        private InstrumentLayoutConfiguration LayoutConfiguration { get; }
        private IStringDataService dataService;

        public EditStringsDialogViewModel()
        {
            //Strings = new ObservableCollection<StringItemModel>();
            dataService = new MockStringDataService();
            LayoutConfiguration = new MandolinValuesProvider().GetDefaultConfiguration();
            PopulateStrings(LayoutConfiguration);
            

            ApplyStringSetCommand = new RelayCommand<StringSet?>(ApplyStringSet);
            SaveCommand = new RelayCommand(Save);
            AddStringToCourseCommand = new RelayCommand<StringCourseEditModel>(AddStringToCourse);
            RemoveStringFromCourseCommand = new RelayCommand<StringEditModel>(RemoveStringFromCourse, CanRemoveStringFromCourse);

            AvailableSets.Add(new StringSet
            {
                Name = "Regular",
                NumberOfStrings = 6,
                InstrumentType = InstrumentType.ElectricGuitar.ToString(),
                Strings = new List<SetItem>()
            });
            AvailableSets.Add(new StringSet
            {
                Name = "Heavy",
                NumberOfStrings = 6,
                InstrumentType = InstrumentType.ElectricGuitar.ToString(),
                Strings = new List<SetItem>()
            });
            //LoadAvailableStringSets().RunSynchronously();
        }

        public EditStringsDialogViewModel(ILayoutDocumentContext layoutContext)
        {
            dataService = layoutContext.DataService;

            //Strings = new ObservableCollection<StringItemModel>();
            LayoutConfiguration = layoutContext.Configuration;

            ApplyStringSetCommand = new RelayCommand<StringSet?>(ApplyStringSet);
            SaveCommand = new RelayCommand(Save);
            AddStringToCourseCommand = new RelayCommand<StringCourseEditModel>(AddStringToCourse);
            RemoveStringFromCourseCommand = new RelayCommand<StringEditModel>(str =>
            {
                if (str != null)
                {
                    var course = Courses.FirstOrDefault(c => c.CourseIndex == str.CourseIndex);
                    if (course != null)
                        RemoveStringFromCourse(course, str);
                }
            });
            PopulateStrings(layoutContext.Configuration);
        }

        private void PopulateStrings(InstrumentLayoutConfiguration instrumentLayout)
        {
            IsBassInstrument = instrumentLayout.InstrumentType == InstrumentType.AcousticBass ||
                instrumentLayout.InstrumentType == InstrumentType.ElectricBass;

            int trebleIndex = instrumentLayout.TotalNumberOfStrings - 1;
            int courseIndex = 0;

            TotalNumberOfStrings = instrumentLayout.TotalNumberOfStrings;

            foreach (var strConfig in instrumentLayout.StringConfigurations)
            {
                var course = new StringCourseEditModel(courseIndex++);

                if (strConfig is StringGroupConfiguration sgc && sgc.Spacing.HasValue)
                    course.CourseSpacing = sgc.Spacing.Value;

                foreach (var stringProp in strConfig.EnumerateStrings())
                    course.AddString(stringProp);
                Courses.Add(course);
            }

            RecalculateIndices();

            //foreach (var stringProp in instrumentLayout.EnumerateStringProperties())
            //{
            //    Strings.Add(new StringItemModel(stringProp.Index, trebleIndex--, stringProp.Data));
            //}
        }

        private void RecalculateIndices()
        {
            int overallIndex = 0;
            TotalNumberOfStrings = Courses.Sum(c => c.Strings.Count);

            for ( int c = 0; c < Courses.Count; c++)
            {
                var course = Courses[c];
                course.CourseIndex = c;
                
                for (int s = 0; s < course.Strings.Count; s++)
                {
                    var str = course.Strings[s];
                    str.CourseIndex = c;
                    str.OverallIndex = overallIndex++;
                    str.TrebleIndex = TotalNumberOfStrings - overallIndex;
                    str.SubIndex = course.IsCourse ? s : null;
                    str.CanRemove = TotalNumberOfStrings > 1;
                }
            }
        }

        #region String Management

        public void AddStringToCourse(StringCourseEditModel? course)
        {
            if (course == null) return;
            course.AddNewString();
            RecalculateIndices();
            if (TotalNumberOfStrings == 2)
                RemoveStringFromCourseCommand.NotifyCanExecuteChanged();
        }

        public void RemoveStringFromCourse(StringEditModel? str)
        {
            if (str != null)
            {
                var course = Courses.FirstOrDefault(c => c.CourseIndex == str.CourseIndex);
                if (course != null)
                    RemoveStringFromCourse(course, str);
            }
        }

        public void RemoveStringFromCourse(StringCourseEditModel course, StringEditModel str)
        {
            //if (course == null || str == null) return;
            course.Strings.Remove(str);
            if (course.Strings.Count == 0)
                Courses.Remove(course);
            RecalculateIndices();
            if (TotalNumberOfStrings == 1)
                RemoveStringFromCourseCommand.NotifyCanExecuteChanged();

        }

        protected bool CanRemoveStringFromCourse(StringEditModel? str)
        {
            if (str == null) return false;
            if (Courses.Count > 1)
                return true;
            var course = Courses.FirstOrDefault(c => c.CourseIndex == str.CourseIndex);
            
            return course?.Strings.Count > 1;
        }

        //public void InsertStringAt(int stringIndex)
        //{

        //}

        //public void ConvertStringToCourse(int stringIndex)
        //{

        //}

        #endregion

        public async Task LoadAvailableStringSets()
        {
            var stringSets = await dataService.GetAvailableStringSetsAsync(LayoutConfiguration.TotalNumberOfStrings, LayoutConfiguration.InstrumentType);
            AvailableSets.Clear();
            foreach (var item in stringSets)
                AvailableSets.Add(item);
        }

        public async void ApplyStringSet(StringSet? set)
        {
            if (set == null) return;
            var orderedStrings = set.Strings.OrderBy(x => x.SortOrder).Select(x => x.String).ToList();

            int globalIndex = 0;
            for (int i = 0; i < Courses.Count; i++)
            {
                var course = Courses[i];
                for (int s = 0; s < course.Strings.Count; s++)
                {
                    var str = course.Strings[s];
                    if (globalIndex < orderedStrings.Count)
                    {
                        var stringProp = orderedStrings[globalIndex++];
                        str.Gauge = Measuring.Measure.In(stringProp.Gauge);
                        str.MaterialType = stringProp.MaterialType;
                        str.UnitWeight = stringProp.UnitWeight;
                        str.CoreDiameter = stringProp.CoreDiameter;
                    }
                }
            }
        }

        public void Save()
        {
            //var stringData = Strings.Select(x => new StringEditableInfo(x)).ToArray();
            CompleteDialog(new EditStringsResult(Courses.Select(x=>x.Clone()).ToArray()));
        }

        
        
    }

    #region Models

    public partial class StringCourseEditModel : ObservableObject
    {
        /// <summary>
        /// The index of the string or course in the instrument layout (from 0 to NumberOfStrings - 1)
        /// </summary>
        [ObservableProperty]
        private int courseIndex;

        public int CourseNumber => CourseIndex + 1;

        [ObservableProperty]
        private Measuring.Measure courseSpacing;

        [ObservableProperty]
        private ObservableCollection<StringEditModel> _strings;

        //public ObservableCollection<StringEditModel> Strings { get; set; } = new ObservableCollection<StringEditModel>();
        public ObservableCollection<StringEditModel> Items => Strings;

        public bool IsCourse => Strings.Count > 1;
        public bool IsSingle => Strings.Count == 1;
        public bool IsEven => CourseIndex % 2 == 0;

        public int? OriginalCourseIndex { get; }

        public StringCourseEditModel(int courseIndex)
        {
            _strings = new ObservableCollection<StringEditModel>();
            CourseIndex = courseIndex;
            CourseSpacing = Measure.Mm(2);
            OriginalCourseIndex = courseIndex;
            _strings.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(IsCourse));
                OnPropertyChanged(nameof(IsSingle));
            };
        }

        public void AddNewString()
        {
            Strings.Add(new StringEditModel
            {
                CourseIndex = CourseIndex,
                //OverallIndex = Strings.Count > 0 ? Strings.Max(x => x.OverallIndex) + 1 : 0,
                SubIndex = Strings.Count,
                Gauge = Measure.In(0.010),
                MaterialType = StringMaterialType.SteelPlain
            });
        }

        public void AddString(StringContext<StringProperties> stringData)
        {
            var fallBackMaterial = stringData.Data.Gauge < Measure.In(0.020) ? StringMaterialType.SteelPlain : StringMaterialType.NickelWound;
            Strings.Add(new StringEditModel
            {
                CourseIndex = CourseIndex,
                OverallIndex = stringData.OverallIndex,
                SubIndex = stringData.SubIndex,
                Gauge = stringData.Data.Gauge,
                MaterialType = stringData.Data.Material?.MaterialType ?? fallBackMaterial,
                UnitWeight = stringData.Data.Material?.UnitWeight,
                CoreDiameter = stringData.Data.Material?.CoreDiameter,
                Modulus = stringData.Data.Material?.ModulusOfElasticity,
                SourceStringIndex = stringData.Index
            });
        }

        partial void OnCourseIndexChanged(int value)
        {
            OnPropertyChanged(nameof(IsEven));
            OnPropertyChanged(nameof(CourseNumber));
        }

        public StringCourseEditModel Clone()
        {
            var clone = new StringCourseEditModel(CourseIndex)
            {
                CourseSpacing = CourseSpacing,
            };
            foreach (var str in Strings)
            {
                clone.Strings.Add(new StringEditModel
                {
                    CourseIndex = str.CourseIndex,
                    OverallIndex = str.OverallIndex,
                    SubIndex = str.SubIndex,
                    TrebleIndex = str.TrebleIndex,
                    Gauge = str.Gauge,
                    MaterialType = str.MaterialType,
                    UnitWeight = str.UnitWeight,
                    CoreDiameter = str.CoreDiameter,
                    Modulus = str.Modulus,
                    SourceStringIndex = str.SourceStringIndex
                });
            }
            return clone;
        }
    }

    public partial class StringEditModel : ObservableObject
    {
        /// <summary>
        /// The index of the string or course in the instrument layout (from 0 to NumberOfStrings - 1)
        /// </summary>
        [ObservableProperty]
        private int courseIndex;

        [ObservableProperty]
        private int overallIndex;

        public int StringNumber => OverallIndex + 1;

        /// <summary>
        /// Index of the string within its course (for single strings, this is always null)
        /// </summary>
        [ObservableProperty]
        private int? subIndex;

        /// <summary>
        /// Overall index of the string (across string courses) but starting from the treble side
        /// </summary>
        [ObservableProperty]
        private int trebleIndex;

        [ObservableProperty]
        private bool canRemove;

        [ObservableProperty]
        private Measure? gauge;

        [ObservableProperty]
        private StringMaterialType? materialType;

        [ObservableProperty]
        private double? unitWeight;

        [ObservableProperty]
        private double? coreDiameter;

        [ObservableProperty]
        private double? modulus;

        public StringIndex? SourceStringIndex { get; set; }

        partial void OnOverallIndexChanged(int value)
        {
            OnPropertyChanged(nameof(StringNumber));
        }


        partial void OnGaugeChanged(Measure? value)
        {
            UnitWeight = null;
            Modulus = null;
        }

        partial void OnMaterialTypeChanged(StringMaterialType? value)
        {
            UnitWeight = null;
            Modulus = null;
        }
    }

    #endregion

    public class StringEditableInfo
    {
        public Measuring.Measure? Gauge { get; }
        public StringMaterialType? MaterialType { get; }
        public double? UnitWeight { get; }
        public double? CoreDiameter { get; }
        public StringIndex? SourceIndex { get;}
        public StringIndex TargetIndex { get; }

        //public StringEditableInfo(Measure? gauge, StringMaterialType? materialType, double? unitWeight, double? coreDiameter)
        //{
        //    Gauge = gauge;
        //    MaterialType = materialType;
        //    UnitWeight = unitWeight;
        //    CoreDiameter = coreDiameter;
        //}

        public StringEditableInfo(StringItemModel model)
        {
            Gauge = model.Gauge;
            MaterialType = model.MaterialType;
            UnitWeight = model.UnitWeight;
            CoreDiameter = model.CoreDiameter;
            SourceIndex = model.SourceIndex;
            TargetIndex = new StringIndex(model.TrebleIndex, model.CourseIndex, model.SubIndex);
        }
    }

    public partial class StringItemModel : ObservableObject
    {
        /// <summary>
        /// The index of the string or course in the instrument layout (from 0 to NumberOfStrings - 1)
        /// </summary>
        [ObservableProperty]
        private int courseIndex;

        /// <summary>
        /// Index of the string within its course (for single strings, this is always null)
        /// </summary>
        [ObservableProperty]
        private int? subIndex;

        /// <summary>
        /// Overall index of the string (across string courses) but starting from the treble side
        /// </summary>
        [ObservableProperty]
        private int trebleIndex;

        public int StringIndex { get; }

        [ObservableProperty]
        private Measure? gauge;

        [ObservableProperty]
        private StringMaterialType? materialType;

        [ObservableProperty]
        private double? unitWeight;

        [ObservableProperty]
        private double? coreDiameter;

        public StringIndex? SourceIndex { get; set; }

        public StringItemModel()
        {

        }

        //public StringItemModel(int courseIndex, int stringIndex, int trebleIndex, Measure? gauge, StringMaterialType? materialType)
        //{
        //    CourseIndex = courseIndex;
        //    StringIndex = stringIndex;
        //    Gauge = gauge;
        //    MaterialType = materialType;
        //    TrebleIndex = trebleIndex;
        //}

        public StringItemModel(StringIndex stringIndex, int trebleIndex, StringProperties? stringProperties)
        {
            CourseIndex = stringIndex.CourseIndex;
            SubIndex = stringIndex.SubIndex;
            TrebleIndex = trebleIndex;

            Gauge = stringProperties?.Gauge;
            var fallBackMaterial = Gauge < Measure.In(0.020) ? StringMaterialType.SteelPlain : StringMaterialType.NickelWound;
            MaterialType = stringProperties?.Material?.MaterialType ?? fallBackMaterial;
            UnitWeight = stringProperties?.Material?.UnitWeight;
            CoreDiameter = stringProperties?.Material?.CoreDiameter;
        }
    }

    public class EditStringsResult
    {
        public StringCourseEditModel[] Courses { get; }

        public EditStringsResult(StringCourseEditModel[] strings)
        {
            Courses = strings;
        }

        public void Apply(InstrumentLayoutConfiguration layoutConfiguration)
        {
            var oldStrings = layoutConfiguration.StringConfigurations.ToArray();
            //layoutConfiguration.StringConfigurations.Clear();
            var originalStringsProps = layoutConfiguration.EnumerateStringProperties();
            var newStringConfigs = new List<BaseStringConfiguration>();

            foreach ( var course in Courses)
            {
                var oldConfig = course.OriginalCourseIndex.HasValue ? layoutConfiguration.StringConfigurations[course.OriginalCourseIndex.Value] : null;

                //BaseStringConfiguration newString = course.IsCourse ? new StringGroupConfiguration() : new SingleStringConfiguration();
                if (course.IsCourse)
                {
                    var newCourse = new StringGroupConfiguration();
                    
                    if (oldConfig != null)
                    {
                        newCourse.Frets = oldConfig.Frets;
                        newCourse.ScaleLength = oldConfig.ScaleLength;
                        newCourse.MultiScaleRatio = oldConfig.MultiScaleRatio;
                        newCourse.Spacing = course.CourseSpacing;

                    }

                    foreach (var info in course.Strings)
                    {
                        var origStringProp = originalStringsProps.FirstOrDefault(x => x.Index == info.SourceStringIndex);
                        newCourse.Strings.Add(new StringProperties
                        {
                            Gauge = info.Gauge,
                            Material = new StringMaterialConfiguration
                            {
                                UnitWeight = info.UnitWeight,
                                MaterialType = info.MaterialType,
                                CoreDiameter = info.CoreDiameter,
                                ModulusOfElasticity = info.Modulus
                            },
                            Tuning = origStringProp?.Data?.Tuning,
                        });
                    }
                    newStringConfigs.Add(newCourse);
                }
                else
                {
                    var newString = new SingleStringConfiguration();
                    newStringConfigs.Add(newString);
                    var info = course.Strings.First();
                    var origStringProp = originalStringsProps.FirstOrDefault(x => x.Index == info.SourceStringIndex);
                    if (oldConfig != null)
                    {
                        newString.Frets = oldConfig.Frets;
                        newString.ScaleLength = oldConfig.ScaleLength;
                        newString.MultiScaleRatio = oldConfig.MultiScaleRatio;
                    }
                    newString.Properties = new StringProperties
                    {
                        Gauge = info.Gauge,
                        Material = new StringMaterialConfiguration
                        {
                            UnitWeight = info.UnitWeight,
                            MaterialType = info.MaterialType,
                            CoreDiameter = info.CoreDiameter,
                            ModulusOfElasticity = info.Modulus
                        },
                        Tuning = origStringProp?.Data?.Tuning,
                    };
                }
            }

            layoutConfiguration.StringConfigurations.Clear();
            foreach (var str in newStringConfigs)
                layoutConfiguration.StringConfigurations.Add(str);
            layoutConfiguration.NumberOfStrings = layoutConfiguration.StringConfigurations.Count;
        }
    }
}
