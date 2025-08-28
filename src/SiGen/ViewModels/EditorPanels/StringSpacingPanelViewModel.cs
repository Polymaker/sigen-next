using CommunityToolkit.Mvvm.ComponentModel;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Measuring;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.ViewModels.EditorPanels
{
    public partial class StringSpacingPanelViewModel : EditorPanelViewModelBase
    {

        #region Nut Properties

        [ObservableProperty]
        private StringSpacingMode nutSpacingMode;

        [ObservableProperty]
        private LayoutCenterAlignment nutCenterAlignment;

        [ObservableProperty]
        private double nutManualAlignment = 0.5;

        public ObservableCollection<LayoutCenterAlignment> NutCenterAlignments { get; private set; } = new((LayoutCenterAlignment[])Enum.GetValues(typeof(LayoutCenterAlignment)));

        [ObservableProperty]
        private Measure _nutSpacing = Measure.Zero;

        public ObservableCollection<Measure> NutStringDistances { get; } = new ObservableCollection<Measure>();

        public Measure NutStringSpread
        {
            get => GetTotalStringSpread(FingerboardEnd.Nut, NutSpacingMode);
            set
            {
                if (NutSpacingMode != StringSpacingMode.Manual && NumberOfStrings > 1)
                {
                    NutSpacing = Measure.Round(value / (NumberOfStrings - 1) * 100d) / 100d;
                }
            }
        }

        #endregion

        #region Bridge properties

        [ObservableProperty]
        private StringSpacingMode bridgeSpacingMode;

        [ObservableProperty]
        private LayoutCenterAlignment bridgeCenterAlignment;

        [ObservableProperty]
        private double bridgeManualAlignment = 0.5;

        [ObservableProperty]
        private Measure _bridgeSpacing = Measure.Zero;

        public Measure BridgeStringSpread
        {
            get => GetTotalStringSpread(FingerboardEnd.Bridge, BridgeSpacingMode);
            set
            {
                if (BridgeSpacingMode != StringSpacingMode.Manual && NumberOfStrings > 1)
                {
                    BridgeSpacing = Measure.Round(value / (NumberOfStrings - 1) * 100d) / 100d;
                }
            }
        }

        public ObservableCollection<Measure> BridgeStringDistances { get; } = new ObservableCollection<Measure>();

        public ObservableCollection<LayoutCenterAlignment> BridgeCenterAlignments { get; private set; } = new(((LayoutCenterAlignment[])Enum.GetValues(typeof(LayoutCenterAlignment))).Where(x => x != LayoutCenterAlignment.Manual));

        #endregion

        public Array StringSpacingModes => Enum.GetValues(typeof(StringSpacingMode));

        public bool IsEditingBySlider { get; set; }

        protected override void OnInitialize()
        {
            
            NutStringDistances.Clear();
            BridgeStringDistances.Clear();
            if (Configuration != null)
            {
                foreach (var dist in Configuration.NutSpacing.StringDistances)
                    NutStringDistances.Add(dist);

                foreach (var dist in Configuration.BridgeSpacing.StringDistances)
                    BridgeStringDistances.Add(dist);
            }

            base.OnInitialize();
        }

        #region Nut Values Handling

        partial void OnNutSpacingChanged(Measure value)
        {
            OnPropertyChanged(nameof(NutStringSpread));
        }

        partial void OnNutSpacingModeChanged(StringSpacingMode oldValue, StringSpacingMode newValue)
        {
            OnPropertyChanged(nameof(NutStringSpread));

            UpdateConfiguration("Nut Spacing Mode", config =>
            {
                config.NutSpacing.SpacingMode = newValue;
                if (newValue == StringSpacingMode.Manual)
                {
                    for (int i = NutStringDistances.Count; i < config.NumberOfStrings - 1; i++)
                    {
                        var distance = NutStringDistances.LastOrDefault() ?? Measure.Cm(1);
                        NutStringDistances.Add(distance);
                    }
                    config.NutSpacing.StringDistances = NutStringDistances.ToList();
                }
                else if (newValue != StringSpacingMode.Manual && oldValue == StringSpacingMode.Manual)
                {
                    var spread = GetTotalStringSpread(FingerboardEnd.Nut, StringSpacingMode.Manual);
                    var spacing = spread / (config.NumberOfStrings - 1);

                    NutSpacing = Measure.Round(spacing * 100d) / 100d;
                    config.NutSpacing.StringDistances.Clear();
                    config.NutSpacing.StringDistances.Add(NutSpacing);
                }
            });
        }

        partial void OnNutSpacingChanged(Measure? oldValue, Measure newValue)
        {
            if (Configuration?.NutSpacing?.SpacingMode != StringSpacingMode.Manual)
            {
                UpdateConfiguration("Nut Spacing", config =>
                {
                    if (config.NutSpacing.StringDistances.Count == 0)
                        config.NutSpacing.StringDistances.Add(newValue);
                    else
                        config.NutSpacing.StringDistances[0] = newValue;
                });
            }
        }

        partial void OnNutCenterAlignmentChanged(LayoutCenterAlignment value)
        {
            if (IsLoading) return;

            UpdateConfiguration("Nut Center Alignment", config =>
            {
                config.NutSpacing.CenterAlignment = value;
                if (value == LayoutCenterAlignment.Manual)
                {
                    config.NutSpacing.AlignmentRatio = NutManualAlignment;
                }
                else
                {
                    config.NutSpacing.AlignmentRatio = null;
                }
            });
        }

        partial void OnNutManualAlignmentChanged(double value)
        {
            if (IsLoading) return;
            if (NutCenterAlignment != LayoutCenterAlignment.Manual) return;

            UpdateConfiguration("Nut Center Alignment", config =>
            {
                config.NutSpacing.AlignmentRatio = value;
            });
        }

        #endregion

        #region Bridge Values Handling

        partial void OnBridgeSpacingChanged(Measure value)
        {
            OnPropertyChanged(nameof(BridgeStringSpread));
        }

        partial void OnBridgeSpacingModeChanged(StringSpacingMode oldValue, StringSpacingMode newValue)
        {
            OnPropertyChanged(nameof(BridgeStringSpread));

            UpdateConfiguration("Bridge Spacing Mode", config =>
            {
                config.BridgeSpacing.SpacingMode = newValue;
                if (newValue == StringSpacingMode.Manual)
                {
                    for (int i = BridgeStringDistances.Count; i < config.NumberOfStrings - 1; i++)
                    {
                        var distance = BridgeStringDistances.LastOrDefault() ?? Measure.Cm(1);
                        BridgeStringDistances.Add(distance);
                    }
                    config.BridgeSpacing.StringDistances = BridgeStringDistances.ToList();
                }
                else if (newValue != StringSpacingMode.Manual && oldValue == StringSpacingMode.Manual)
                {
                    var spread = GetTotalStringSpread(FingerboardEnd.Bridge, StringSpacingMode.Manual);
                    var spacing = spread / (config.NumberOfStrings - 1);

                    BridgeSpacing = Measure.Round(spacing * 100d) / 100d;
                    config.BridgeSpacing.StringDistances.Clear();
                    config.BridgeSpacing.StringDistances.Add(BridgeSpacing);
                }
            });
        }

        partial void OnBridgeSpacingChanged(Measure? oldValue, Measure newValue)
        {
            if (Configuration?.BridgeSpacing?.SpacingMode != StringSpacingMode.Manual)
            {
                UpdateConfiguration("Bridge Spacing", config =>
                {
                    if (config.BridgeSpacing.StringDistances.Count == 0)
                        config.BridgeSpacing.StringDistances.Add(newValue);
                    else
                        config.BridgeSpacing.StringDistances[0] = newValue;
                });
            }
        }

        partial void OnBridgeCenterAlignmentChanged(LayoutCenterAlignment value)
        {
            if (IsLoading) return;

            UpdateConfiguration("Bridge Center Alignment", config =>
            {
                config.BridgeSpacing.CenterAlignment = value;
                if (value == LayoutCenterAlignment.Manual)
                {
                    config.BridgeSpacing.AlignmentRatio = BridgeManualAlignment;
                }
                else
                {
                    config.BridgeSpacing.AlignmentRatio = null;
                }
            });
        }

        partial void OnBridgeManualAlignmentChanged(double value)
        {
            if (IsLoading) return;
            if (BridgeCenterAlignment != LayoutCenterAlignment.Manual) return;

            UpdateConfiguration("Bridge Center Alignment", config =>
            {
                config.BridgeSpacing.AlignmentRatio = value;
            });
        }

        #endregion

        private Measure GetTotalStringSpread(FingerboardEnd end, StringSpacingMode mode)
        {
            if (mode != StringSpacingMode.Manual)
                return end == FingerboardEnd.Nut ? NutSpacing * (NumberOfStrings - 1) : BridgeSpacing * (NumberOfStrings - 1);

            var distances = end == FingerboardEnd.Nut ? NutStringDistances : BridgeStringDistances;

            if (distances.Count < NumberOfStrings - 1)
                return Measure.Zero;

            var total = Measure.Mm(0);
            for (int i = 0; i < NumberOfStrings - 1; i++)
                total += distances[i];

            return total;
        }

        private static readonly LayoutCenterAlignment[] OrderedAlignments =
            Enum.GetValues(typeof(LayoutCenterAlignment)).Cast<LayoutCenterAlignment>().ToArray();

        private void UpdateNutCenterAlignments(bool isSymmetric, bool nutHasSymmetricMargins)
        {
            var allowed = new List<LayoutCenterAlignment>();
            foreach (var alignment in OrderedAlignments)
            {
                //todo: allow manual only if nut total spread is less than the bridge 
                if (alignment == LayoutCenterAlignment.Manual)
                    continue; // Add at the end
                if (isSymmetric && (alignment == LayoutCenterAlignment.SymmetricStrings || alignment == LayoutCenterAlignment.SymmetricFingerboard))
                    continue;
                if (!isSymmetric && nutHasSymmetricMargins && alignment == LayoutCenterAlignment.SymmetricFingerboard)
                    continue;
                if (nutHasSymmetricMargins && alignment == LayoutCenterAlignment.Fingerboard)
                    continue;
                allowed.Add(alignment);
            }
            allowed.Add(LayoutCenterAlignment.Manual);
            // Ensure selected value is valid BEFORE updating the collection
            if (!allowed.Contains(NutCenterAlignment))
            {
                if (NutCenterAlignment == LayoutCenterAlignment.SymmetricStrings)
                    NutCenterAlignment = LayoutCenterAlignment.OuterStrings;
                else if (NutCenterAlignment == LayoutCenterAlignment.SymmetricFingerboard)
                    NutCenterAlignment = LayoutCenterAlignment.Fingerboard;
                else if (NutCenterAlignment == LayoutCenterAlignment.Fingerboard)
                    NutCenterAlignment = LayoutCenterAlignment.OuterStrings;
                else
                    NutCenterAlignment = allowed.First();
            }
            var currentValue = NutCenterAlignment;
            NutCenterAlignments = new ObservableCollection<LayoutCenterAlignment>(allowed);
            OnPropertyChanged(nameof(NutCenterAlignments));
            NutCenterAlignment = currentValue;
        }

        private void UpdateBridgeCenterAlignments(bool isSymmetric, bool bridgeHasSymmetricMargins)
        {
            var allowed = new List<LayoutCenterAlignment>();
            foreach (var alignment in OrderedAlignments)
            {
                //todo: allow manual only if bridge total spread is less than the nut (rare but possible) 
                if (alignment == LayoutCenterAlignment.Manual)
                    continue; // Add at the end
                if (isSymmetric && (alignment == LayoutCenterAlignment.SymmetricStrings || alignment == LayoutCenterAlignment.SymmetricFingerboard))
                    continue;
                if (!isSymmetric && bridgeHasSymmetricMargins && alignment == LayoutCenterAlignment.SymmetricFingerboard)
                    continue;
                if (bridgeHasSymmetricMargins && alignment == LayoutCenterAlignment.Fingerboard)
                    continue;
                allowed.Add(alignment);
            }
            allowed.Add(LayoutCenterAlignment.Manual);

            // Ensure selected value is valid BEFORE updating the collection
            if (!allowed.Contains(BridgeCenterAlignment))
            {
                if (BridgeCenterAlignment == LayoutCenterAlignment.SymmetricStrings)
                    BridgeCenterAlignment = LayoutCenterAlignment.OuterStrings;
                else if (BridgeCenterAlignment == LayoutCenterAlignment.SymmetricFingerboard)
                    BridgeCenterAlignment = LayoutCenterAlignment.Fingerboard;
                else if (BridgeCenterAlignment == LayoutCenterAlignment.Fingerboard)
                    BridgeCenterAlignment = LayoutCenterAlignment.OuterStrings;
                else
                    BridgeCenterAlignment = allowed.First();
            }

            var currentValue = BridgeCenterAlignment;
            BridgeCenterAlignments = new ObservableCollection<LayoutCenterAlignment>(allowed);
            OnPropertyChanged(nameof(BridgeCenterAlignments));
            BridgeCenterAlignment = currentValue;
        }

        protected override void OnConfigurationChanged()
        {
            base.OnConfigurationChanged();
            if (Configuration == null)
                return;

            NutCenterAlignment = Configuration.NutSpacing.CenterAlignment;
            NutSpacingMode = Configuration.NutSpacing.SpacingMode;
            NutManualAlignment = Configuration.NutSpacing.AlignmentRatio ?? 0;

            if (Configuration.NutSpacing.SpacingMode != StringSpacingMode.Manual)
            {
                NutSpacing = Configuration.NutSpacing.StringDistances.FirstOrDefault() ?? Measure.Zero;
            }

            bool isSymmetric = Configuration.ScaleLength.Mode == ScaleLengthMode.Single && Measure.IsNullOrEmpty(Configuration.ScaleLength.BassTrebleSkew);
            bool nutHasSymmetricMargins = Configuration.Fingerboard.NutBassMargin == Configuration.Fingerboard.NutTrebleMargin && !Configuration.Fingerboard.CompensateMarginsForStrings;
            UpdateNutCenterAlignments(isSymmetric, nutHasSymmetricMargins);

            BridgeCenterAlignment = Configuration.BridgeSpacing.CenterAlignment;
            BridgeSpacingMode = Configuration.BridgeSpacing.SpacingMode;

            if (Configuration.BridgeSpacing.SpacingMode != StringSpacingMode.Manual)
            {
                BridgeSpacing = Configuration.BridgeSpacing.StringDistances.FirstOrDefault() ?? Measure.Zero;
            }

            bool bridgeHasSymmetricMargins = Configuration.Fingerboard.BridgeBassMargin == Configuration.Fingerboard.BridgeTrebleMargin && !Configuration.Fingerboard.CompensateMarginsForStrings;
            UpdateBridgeCenterAlignments(isSymmetric, bridgeHasSymmetricMargins);
        }
    }
}
