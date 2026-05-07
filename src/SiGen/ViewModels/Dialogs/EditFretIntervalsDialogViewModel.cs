using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace SiGen.ViewModels.Dialogs
{
    public partial class EditFretIntervalsDialogViewModel : DialogViewModelBase<List<double>>
    {
        private readonly string _title;

        public override string Title => _title;

        public ObservableCollection<FretIntervalValueModel> Intervals { get; } = new();

        public IRelayCommand AddIntervalCommand { get; }
        public IRelayCommand<FretIntervalValueModel> RemoveIntervalCommand { get; }
        public IRelayCommand SaveCommand { get; }

        public EditFretIntervalsDialogViewModel(string title, IReadOnlyList<double>? initialIntervals = null)
        {
            _title = title;
            Resizable = true;

            if (initialIntervals != null)
            {
                foreach (var value in initialIntervals)
                    Intervals.Add(new FretIntervalValueModel(value));
            }

            AddIntervalCommand = new RelayCommand(AddInterval);
            RemoveIntervalCommand = new RelayCommand<FretIntervalValueModel>(RemoveInterval);
            SaveCommand = new RelayCommand(Save);
        }

        private void AddInterval()
        {
            Intervals.Add(new FretIntervalValueModel(null));
        }

        private void RemoveInterval(FretIntervalValueModel? value)
        {
            if (value != null)
                Intervals.Remove(value);
        }

        private void Save()
        {
            var values = Intervals
                .Select(x => x.Cents)
                .Where(x => x.HasValue)
                .Select(x => Math.Round(x!.Value, 4))
                .Where(x => x >= 0)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            CompleteDialog(values);
        }
    }

    public partial class FretIntervalValueModel : ObservableObject
    {
        [ObservableProperty]
        private double? cents;

        public FretIntervalValueModel(double? cents)
        {
            Cents = cents;
        }
    }
}
