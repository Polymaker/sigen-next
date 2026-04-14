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
    public partial class EditSpacingDialogViewModel : DialogViewModelBase<List<Measure>>
    {
        private string _Title;
        public override string Title => _Title;

        public ObservableCollection<CustomSpacingModel> Distances { get; }

        public ICommand SaveCommand { get; }

        public EditSpacingDialogViewModel(InstrumentLayoutConfiguration layoutConfiguration, FingerboardEnd fingerboardEnd)
        {
            Distances = new ObservableCollection<CustomSpacingModel>();
            var spacingCfg = layoutConfiguration.GetStringSpacing(fingerboardEnd);
            string fingerboardEndName = fingerboardEnd == FingerboardEnd.Nut ? Lang.Resources.FingerboardEnd_Nut : Lang.Resources.FingerboardEnd_Bridge;
            _Title = string.Format(Lang.Resources.StringSpacingDialog_TitleFormat, fingerboardEndName);
            for (int i = 0; i < layoutConfiguration.NumberOfStrings -1; i++)
                Distances.Add(new CustomSpacingModel(i, spacingCfg.GetDistance(i)));
            ShowTitleBar = true;
            SaveCommand = new RelayCommand(Save);
        }


        public EditSpacingDialogViewModel()
        {
            var layoutConfiguration = new ElectricGuitarValuesProvider().GetDefaultConfiguration();
            Distances = new ObservableCollection<CustomSpacingModel>();
            var spacingCfg = layoutConfiguration.GetStringSpacing(FingerboardEnd.Nut);
            _Title = string.Format(Lang.Resources.StringSpacingDialog_TitleFormat, Lang.Resources.FingerboardEnd_Nut);
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

        public bool IsEven => Index % 2 == 0;

        public CustomSpacingModel(int index, Measure distance)
        {
            Index = index;
            this.distance = distance;
        }
    }
}
