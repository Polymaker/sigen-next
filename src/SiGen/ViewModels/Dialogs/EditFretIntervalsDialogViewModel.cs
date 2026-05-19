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

        [ObservableProperty]
        private double? newIntervalCents;

        [ObservableProperty]
        private double? generateStartCents;

        [ObservableProperty]
        private double? generateIntervalCents;

        [ObservableProperty]
        private double? generateCount;

        public IRelayCommand AddIntervalCommand { get; }
        public IRelayCommand<FretIntervalValueModel> RemoveIntervalCommand { get; }
        public IRelayCommand ClearIntervalsCommand { get; }
        public IRelayCommand GenerateIntervalsCommand { get; }
        public IRelayCommand SaveCommand { get; }

        public EditFretIntervalsDialogViewModel() : this("Edit Fret Intervals", new double[] { 100, 150, 200 }) { }

        public EditFretIntervalsDialogViewModel(string title, IReadOnlyList<double>? initialIntervals = null)
        {
            _title = title;
            Resizable = true;

            if (initialIntervals != null)
            {
                foreach (var value in initialIntervals)
                    Intervals.Add(new FretIntervalValueModel(value));
            }

            GenerateStartCents = 0;
            GenerateIntervalCents = 100;
            GenerateCount = 12;

            AddIntervalCommand = new RelayCommand(AddInterval, CanAddInterval);
            RemoveIntervalCommand = new RelayCommand<FretIntervalValueModel>(RemoveInterval);
            ClearIntervalsCommand = new RelayCommand(ClearIntervals);
            GenerateIntervalsCommand = new RelayCommand(GenerateIntervals);
            SaveCommand = new RelayCommand(Save);
        }

        partial void OnNewIntervalCentsChanged(double? value)
        {
            AddIntervalCommand.NotifyCanExecuteChanged();
        }

        private bool CanAddInterval()
        {
            return NewIntervalCents.HasValue && NewIntervalCents.Value >= 0;
        }

        private void AddInterval()
        {
            if (!NewIntervalCents.HasValue)
                return;

            var value = Math.Round(NewIntervalCents.Value, 4);
            if (value < 0)
                return;

            Intervals.Add(new FretIntervalValueModel(value));
            NewIntervalCents = null;
        }

        private void RemoveInterval(FretIntervalValueModel? value)
        {
            if (value != null)
                Intervals.Remove(value);
        }

        private void ClearIntervals()
        {
            Intervals.Clear();
        }

        private void GenerateIntervals()
        {
            int count = EditFretsDialogViewModel.NormalizeWholeNumber(GenerateCount) ?? 0;
            if (count <= 0)
                return;

            double start = GenerateStartCents ?? 0;
            double step = GenerateIntervalCents ?? 0;

            var existing = Intervals
                .Select(x => x.Cents)
                .Where(x => x.HasValue)
                .Select(x => Math.Round(x!.Value, 4))
                .ToHashSet();

            for (int i = 0; i < count; i++)
            {
                var value = Math.Round(start + (i * step), 4);
                if (value < 0 || existing.Contains(value))
                    continue;

                Intervals.Add(new FretIntervalValueModel(value));
                existing.Add(value);
            }
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
