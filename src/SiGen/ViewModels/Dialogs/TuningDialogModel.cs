using CommunityToolkit.Mvvm.ComponentModel;
using netDxf.Objects;
using SiGen.Data.Presets;
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

namespace SiGen.ViewModels.Dialogs
{
    public partial class TuningDialogModel : DialogViewModelBase
    {
        private readonly InstrumentLayoutConfiguration layoutConfiguration;

        public override string Title => "Tuning";

        public ObservableCollection<TuningPreset> TuningPresets { get; } = new ObservableCollection<TuningPreset>();

        public ObservableCollection<StringTuningModel> Strings { get; } = new ObservableCollection<StringTuningModel>();

        private IInstrumentValuesProvider? valuesProvider { get; }

        public TuningDialogModel()
        {
            valuesProvider = new ElectricGuitarValuesProvider();
            var layout = valuesProvider.GetDefaultConfiguration();
            this.layoutConfiguration = layout;
            UpdateAvailablePresets();
            BuildStringCollection();
        }

        public TuningDialogModel(ILayoutDocumentContext context)
        {
            this.layoutConfiguration = context.Configuration;
            this.valuesProvider = context.InstrumentValuesProvider;
            UpdateAvailablePresets();
            BuildStringCollection();
            ShowTitleBar = true;
        }

        private void UpdateAvailablePresets()
        {
            TuningPresets.Clear();
            if (valuesProvider == null) return;

            var presets = valuesProvider.GetTuningPresets();
            foreach (var preset in presets)
            {
                if (layoutConfiguration.NumberOfStrings == preset.NumberOfStrings)
                    TuningPresets.Add(preset);
            }
        }

        private void BuildStringCollection()
        {
            foreach (var str in layoutConfiguration.StringConfigurations)
            {
                if (str is SingleStringConfiguration single)
                    Strings.Add(new StringTuningModel() { Tuning = single.Tuning });
            }
        }
    }
    public partial class StringTuningModel : ObservableObject
    {
        [ObservableProperty]
        private NoteAndOctave? tuning;


    }
}
