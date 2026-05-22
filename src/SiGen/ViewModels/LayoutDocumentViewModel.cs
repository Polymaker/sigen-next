using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using SiGen.Layouts;
using SiGen.Layouts.Builders;
using SiGen.Layouts.Configuration;
using SiGen.Measuring;
using SiGen.Services;
using SiGen.UI.LayoutViewer;
using SiGen.ViewModels.EditorPanels;
using SiGen.Layouts.Snapping;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.ViewModels
{
    public partial class LayoutDocumentViewModel : ObservableObject, ILayoutDocument, IDocumentTabViewModel
    {
        [ObservableProperty]
        private string title;
        private readonly IServiceProvider serviceProvider;
        [ObservableProperty]
        private InstrumentLayoutConfiguration _configuration;

        [ObservableProperty]
        private string? filePath;

        [ObservableProperty]
        private bool hasUnsavedChanges;

        [ObservableProperty]
        //private StringedInstrumentLayout? layout;
        public partial StringedInstrumentLayout? Layout { get; private set; }

        public event EventHandler? LayoutChanged;

        public bool IsHomePage => false;
        public bool IsDocument => true;

        #region Layout Viewer Properties

        // These properties control the zoom, pan, orientation, and unit mode of the layout viewer.
        // They are stored in the document so when the user switches between different documents, the viewer settings for each document are preserved.

        [ObservableProperty]
        private double layoutZoom = 1.0;

        [ObservableProperty]
        private Point layoutTrans = new Point(0, 0);

        [ObservableProperty]
        private LayoutOrientation layoutOrientation = LayoutOrientation.HorizontalNutRight;

        [ObservableProperty]
        private UnitSystem layoutUnitMode = UnitSystem.Metric;

        [ObservableProperty]
        private SnapLineType activeSnapFilters = SnapLineType.Fret | SnapLineType.String | SnapLineType.Fingerboard | SnapLineType.CenterLine;

        [ObservableProperty]
        private bool isMeasureToolEnabled;

        [ObservableProperty]
        private LayoutViewerVisibleItems activeVisibilityFilters = LayoutViewerVisibleItems.All;

        public bool IsZoomToFit { get; set; } = true;

        #endregion

        private List<EditorPanelViewModelBase> PanelViewModels = new();

        protected IInstrumentValuesProviderFactory? InstrumentValuesProviderFactory { get; } 
        public IInstrumentValuesProvider? InstrumentValuesProvider { get; private set; }

        string? IDocumentTabViewModel.TabToolTip => FilePath;

        public bool IsBindingPanels { get; set; } = false;

        //design-time constructor
        public LayoutDocumentViewModel(string title, string? filePath, InstrumentLayoutConfiguration configuration)
        {
            this.title = title;
            this.filePath = filePath;
            Configuration = configuration;
            InstrumentValuesProvider = new InstrumentValuesProviderFactory().CreateProvider(Configuration.InstrumentType);
            serviceProvider = new ServiceCollection().BuildServiceProvider();

            InitializePanelViewModels();
        }

        //DI constructor
        [ActivatorUtilitiesConstructor]
        public LayoutDocumentViewModel(string title, string? filePath, InstrumentLayoutConfiguration configuration, 
            //services
            IInstrumentValuesProviderFactory? instrumentValuesProviderFactory, 
            IServiceProvider serviceProvider)
        {
            this.title = title;
            this.filePath = string.IsNullOrEmpty(filePath) ? null : filePath;
            Configuration = configuration;
            InstrumentValuesProviderFactory = instrumentValuesProviderFactory;
            InstrumentValuesProvider = instrumentValuesProviderFactory?.CreateProvider(Configuration.InstrumentType);
            this.serviceProvider = serviceProvider;
            InitializePanelViewModels();
        }

        private void InitializePanelViewModels()
        {
            GetType().Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(EditorPanelViewModelBase)) && !t.IsAbstract)
                .ToList()
                .ForEach(t =>
                {
                    try
                    {
                        var panelModel = (EditorPanelViewModelBase)ActivatorUtilities.CreateInstance(serviceProvider, t);
                        PanelViewModels.Add(panelModel);
                        panelModel.AssignDocument(this);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Could not create panel type: "   + t.FullName, ex);
                    }
                    
                });
        }

        public T? GetPanelViewModel<T>() where T : EditorPanelViewModelBase
        {
            return PanelViewModels.OfType<T>().FirstOrDefault();
        }

        partial void OnConfigurationChanged(InstrumentLayoutConfiguration value)
        {
            if (IsBindingPanels) return;
            RebuildLayout();
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

        partial void OnLayoutChanged(StringedInstrumentLayout? value)
        {
            LayoutChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateConfiguration(string reason, Action<InstrumentLayoutConfiguration> updateAction)
        {
            if (IsBindingPanels)
                return;

            int numberOfStrings = Configuration.NumberOfStrings;
            var instrumentType = Configuration.InstrumentType;

            updateAction(Configuration);

            HasUnsavedChanges = true;

            if (Configuration.NumberOfStrings != numberOfStrings)
            {
                NotifyNumberOfStringsChanged();
            }
            if (Configuration.InstrumentType != instrumentType)
            {
                InstrumentValuesProvider = InstrumentValuesProviderFactory?.CreateProvider(Configuration.InstrumentType);
                NotifyInstrumentTypeChanged();
            }

            NotifyConfigurationChanged();
            RebuildLayout();
        }

        private void NotifyConfigurationChanged()
        {
            foreach (var panel in PanelViewModels)
            {
                panel.NotifyConfigurationChanged();
            }
        }

        private void NotifyNumberOfStringsChanged()
        {
            foreach (var panel in PanelViewModels)
            {
                panel.NotifyNumberOfStringsChanged();
            }
        }

        private void NotifyInstrumentTypeChanged()
        {
            foreach (var panel in PanelViewModels)
            {
                panel.NotifyInstrumentTypeChanged();
            }
        }
    }
}
