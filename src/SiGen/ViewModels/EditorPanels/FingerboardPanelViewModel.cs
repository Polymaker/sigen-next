using CommunityToolkit.Mvvm.ComponentModel;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Measuring;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.ViewModels.EditorPanels
{
    public partial class FingerboardPanelViewModel : EditorPanelViewModelBase
    {
        
        [ObservableProperty]
        private MarginMode selectedMarginMode;

        public ObservableCollection<MarginMode> MarginModes { get; } = new((MarginMode[])Enum.GetValues(typeof(MarginMode)));

        [ObservableProperty]
        private Measure? nutTrebleMargin;
        [ObservableProperty]
        private Measure? nutBassMargin;
        [ObservableProperty]
        private Measure? bridgeTrebleMargin;
        [ObservableProperty]
        private Measure? bridgeBassMargin;

        [ObservableProperty]
        private bool compensateForStringThickness;

        protected override void OnConfigurationChanged()
        {
            base.OnConfigurationChanged();
            if (Configuration == null)
                return;

            SelectedMarginMode = Configuration.Fingerboard.MarginDefinitionMode;
            NutTrebleMargin = Configuration.Fingerboard.NutTrebleMargin;
            NutBassMargin = Configuration.Fingerboard.NutBassMargin;
            BridgeTrebleMargin = Configuration.Fingerboard.BridgeTrebleMargin;
            BridgeBassMargin = Configuration.Fingerboard.BridgeBassMargin;
            CompensateForStringThickness = Configuration.Fingerboard.CompensateMarginsForStrings;
        }


        partial void OnSelectedMarginModeChanged(MarginMode value)
        {
            if (IsLoading) return;

            if (value == MarginMode.All)
            {
                isUpdatingMargins = true;
                NutTrebleMargin = NutBassMargin;
                BridgeTrebleMargin = NutBassMargin;
                BridgeBassMargin = NutBassMargin;
                isUpdatingMargins = false;
                UpdateConfiguration("Margins", config =>
                {
                    config.Fingerboard.NutBassMargin = NutBassMargin;
                    config.Fingerboard.NutTrebleMargin = NutBassMargin;
                    config.Fingerboard.BridgeBassMargin = NutBassMargin;
                    config.Fingerboard.BridgeTrebleMargin = NutBassMargin;
                });
            }
        }

        partial void OnCompensateForStringThicknessChanged(bool value)
        {
            if (IsLoading) return;

            UpdateConfiguration("Margins", config =>
            {
                config.Fingerboard.CompensateMarginsForStrings = value;
            });
        }

        #region Margin Change Handlers

        private bool isUpdatingMargins = false;

        partial void OnNutBassMarginChanged(Measure? value)
        {
            if (isUpdatingMargins || IsLoading)
                return;

            isUpdatingMargins = true;

            switch (SelectedMarginMode)
            {
                case MarginMode.All:
                    NutTrebleMargin = value;
                    BridgeTrebleMargin = value;
                    BridgeBassMargin = value;
                    UpdateConfiguration("Margins", config =>
                    {
                        config.Fingerboard.NutBassMargin = value;
                        config.Fingerboard.NutTrebleMargin = value;
                        config.Fingerboard.BridgeBassMargin = value;
                        config.Fingerboard.BridgeTrebleMargin = value;
                    });
                    break;
                case MarginMode.NutBridge:
                    NutTrebleMargin = value;
                    UpdateConfiguration("Margins", config =>
                    {
                        config.Fingerboard.NutBassMargin = value;
                        config.Fingerboard.NutTrebleMargin = value;
                    });
                    break;
                case MarginMode.BassTreble:
                    BridgeBassMargin = value;
                    UpdateConfiguration("Margins", config =>
                    {
                        config.Fingerboard.NutBassMargin = value;
                        config.Fingerboard.BridgeBassMargin = value;
                    });
                    break;
                case MarginMode.Individual:
                    UpdateConfiguration("Margins", config =>
                    {
                        config.Fingerboard.NutBassMargin = value;
                    });
                    break;
            }

            isUpdatingMargins = false;
        }

        partial void OnNutTrebleMarginChanged(Measure? value)
        {
            if (isUpdatingMargins || IsLoading)
                return;

            isUpdatingMargins = true;

            switch (SelectedMarginMode)
            {
                case MarginMode.All:
                    NutBassMargin = value;
                    BridgeTrebleMargin = value;
                    BridgeBassMargin = value;
                    UpdateConfiguration("Margins", config =>
                    {
                        config.Fingerboard.NutBassMargin = value;
                        config.Fingerboard.NutTrebleMargin = value;
                        config.Fingerboard.BridgeBassMargin = value;
                        config.Fingerboard.BridgeTrebleMargin = value;
                    });
                    break;
                case MarginMode.NutBridge:
                    NutBassMargin = value;
                    UpdateConfiguration("Margins", config =>
                    {
                        config.Fingerboard.NutBassMargin = value;
                        config.Fingerboard.NutTrebleMargin = value;
                    });
                    break;
                case MarginMode.BassTreble:
                    BridgeTrebleMargin = value;
                    UpdateConfiguration("Margins", config =>
                    {
                        config.Fingerboard.NutTrebleMargin = value;
                        config.Fingerboard.BridgeTrebleMargin = value;
                    });
                    break;
                case MarginMode.Individual:
                    UpdateConfiguration("Margins", config =>
                    {
                        config.Fingerboard.NutTrebleMargin = value;
                    });
                    break;
            }

            isUpdatingMargins = false;
        }

        partial void OnBridgeTrebleMarginChanged(Measure? value)
        {
            if (isUpdatingMargins || IsLoading)
                return;

            isUpdatingMargins = true;

            switch (SelectedMarginMode)
            {
                case MarginMode.All:
                    NutBassMargin = value;
                    NutTrebleMargin = value;
                    BridgeBassMargin = value;
                    UpdateConfiguration("Margins", config =>
                    {
                        config.Fingerboard.NutBassMargin = value;
                        config.Fingerboard.NutTrebleMargin = value;
                        config.Fingerboard.BridgeBassMargin = value;
                        config.Fingerboard.BridgeTrebleMargin = value;
                    });
                    break;
                case MarginMode.NutBridge:
                    BridgeBassMargin = value;
                    UpdateConfiguration("Margins", config =>
                    {
                        config.Fingerboard.BridgeTrebleMargin = value;
                        config.Fingerboard.BridgeBassMargin = value;
                    });
                    break;
                case MarginMode.BassTreble:
                    NutTrebleMargin = value;
                    UpdateConfiguration("Margins", config =>
                    {
                        config.Fingerboard.BridgeTrebleMargin = value;
                        config.Fingerboard.NutTrebleMargin = value;
                    });
                    break;
                case MarginMode.Individual:
                    UpdateConfiguration("Margins", config =>
                    {
                        config.Fingerboard.BridgeTrebleMargin = value;
                    });
                    break;
            }

            isUpdatingMargins = false;
        }

        partial void OnBridgeBassMarginChanged(Measure? value)
        {
            if (isUpdatingMargins || IsLoading)
                return;

            isUpdatingMargins = true;

            switch (SelectedMarginMode)
            {
                case MarginMode.All:
                    NutBassMargin = value;
                    NutTrebleMargin = value;
                    BridgeTrebleMargin = value;
                    UpdateConfiguration("Margins", config =>
                    {
                        config.Fingerboard.NutBassMargin = value;
                        config.Fingerboard.NutTrebleMargin = value;
                        config.Fingerboard.BridgeBassMargin = value;
                        config.Fingerboard.BridgeTrebleMargin = value;
                    });
                    break;
                case MarginMode.NutBridge:
                    BridgeTrebleMargin = value;
                    UpdateConfiguration("Margins", config =>
                    {
                        config.Fingerboard.BridgeBassMargin = value;
                        config.Fingerboard.BridgeTrebleMargin = value;
                    });
                    break;
                case MarginMode.BassTreble:
                    NutBassMargin = value;
                    UpdateConfiguration("Margins", config =>
                    {
                        config.Fingerboard.BridgeBassMargin = value;
                        config.Fingerboard.NutBassMargin = value;
                    });
                    break;
                case MarginMode.Individual:
                    UpdateConfiguration("Margins", config =>
                    {
                        config.Fingerboard.BridgeBassMargin = value;
                    });
                    break;
            }

            isUpdatingMargins = false;
        }

        #endregion
    }
}
