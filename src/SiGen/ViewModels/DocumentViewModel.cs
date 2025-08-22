using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using SiGen.Layouts;
using SiGen.Layouts.Builders;
using SiGen.Layouts.Configuration;
using SiGen.Measuring;
using SiGen.UI.LayoutViewer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.ViewModels
{
    public partial class DocumentViewModel : ObservableObject
    {
        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private InstrumentLayoutConfiguration _configuration;

        [ObservableProperty]
        private string? filePath;

        [ObservableProperty]
        private bool hasUnsavedChanges;

        [ObservableProperty]
        public partial StringedInstrumentLayout? Layout { get; private set; }

        #region Layout Viewer Properties

        [ObservableProperty]
        private double layoutZoom = 1.0;

        [ObservableProperty]
        private Point layoutTrans = new Point(0, 0);

        [ObservableProperty]
        private LayoutOrientation layoutOrientation = LayoutOrientation.HorizontalNutRight;

        [ObservableProperty]
        private UnitMode layoutUnitMode = UnitMode.Metric;

        #endregion

        public DocumentViewModel(string title, string? filePath, InstrumentLayoutConfiguration configuration)
        {
            this.title = title;
            this.filePath = filePath;
            Configuration = configuration;
        }

        protected void UpdateConfiguration(InstrumentLayoutConfiguration newConfig)
        {
            Configuration = newConfig;
            HasUnsavedChanges = true;
        }

        partial void OnConfigurationChanged(InstrumentLayoutConfiguration value)
        {
            RebuildLayout();
        }

        partial void OnLayoutZoomChanging(double oldValue, double newValue)
        {
            Trace.WriteLine($"{Title} zoom changing from {oldValue} to {newValue}");
        }

        private void RebuildLayout()
        {
            var result = LayoutBuilder.Build(Configuration);
            if (result.Success)
            {
                Layout = result.Layout;
            }
            else
            {
                //Layout = null;
            }
        }
    }
}
