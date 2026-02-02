using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Measuring;
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
    public partial class CustomStringSpacingDialogViewModel : DialogViewModelBase<List<Measure>>
    {
        public override string Title => "String Spacing";

        public ObservableCollection<CustomSpacingModel> Distances { get; }

        public ICommand SaveCommand { get; }

        public CustomStringSpacingDialogViewModel(InstrumentLayoutConfiguration layoutConfiguration, FingerboardEnd fingerboardEnd)
        {
            Distances = new ObservableCollection<CustomSpacingModel>();
            var spacingCfg = layoutConfiguration.GetStringSpacing(fingerboardEnd);

            for (int i = 0; i < layoutConfiguration.NumberOfStrings -1; i++)
                Distances.Add(new CustomSpacingModel(i, spacingCfg.GetDistance(i)));
            ShowTitleBar = true;
            SaveCommand = new RelayCommand(Save);
        }


        public CustomStringSpacingDialogViewModel()
        {
            var layoutConfiguration = new ElectricGuitarValuesProvider().GetDefaultConfiguration();
            Distances = new ObservableCollection<CustomSpacingModel>();
            var spacingCfg = layoutConfiguration.GetStringSpacing(FingerboardEnd.Nut);

            for (int i = 0; i < layoutConfiguration.NumberOfStrings - 1; i++)
                Distances.Add(new CustomSpacingModel(i, spacingCfg.GetDistance(i)));
            SaveCommand = new RelayCommand(Save);
        }

        public void Save()
        {
            var distances = Distances.Select(x => x.Distance).ToList();
            CompleteDialog(distances);
        }
    }

    public partial class CustomSpacingModel : ObservableObject
    {

        public int Index { get; }
        [ObservableProperty]
        private Measuring.Measure distance;

        public CustomSpacingModel(int index, Measure distance)
        {
            Index = index;
            this.distance = distance;
        }
    }
}
