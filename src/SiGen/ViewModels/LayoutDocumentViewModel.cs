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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.ViewModels
{
    public partial class LayoutDocumentViewModel : ObservableObject, ILayoutDocumentContext, IDocumentTabViewModel
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

        public event EventHandler? LayoutChanged;

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
        private UnitMode layoutUnitMode = UnitMode.Metric;

        public bool IsZoomToFit { get; set; } = true;

        #endregion

        // New property to expose dialog service to panels through the document context
        public IDialogService? DialogService { get; private set; }
        public IStringDataService DataService { get; }
        public IStringMaterialEstimationService MaterialEstimationService { get; }

        private List<EditorPanelViewModelBase> PanelViewModels = new();

        protected IInstrumentValuesProviderFactory? InstrumentValuesProviderFactory { get; } 
        public IInstrumentValuesProvider? InstrumentValuesProvider { get; private set; }

        string? IDocumentTabViewModel.TabToolTip => FilePath;

        public bool IsBindingPanels { get; set; } = false;

        public LayoutDocumentViewModel(string title, string? filePath, InstrumentLayoutConfiguration configuration)
        {
            this.title = title;
            this.filePath = filePath;
            Configuration = configuration;
            InstrumentValuesProvider = new InstrumentValuesProviderFactory().CreateProvider(Configuration.InstrumentType);
            DataService = new MockStringDataService();
            MaterialEstimationService = new StringMaterialEstimationService(DataService);
            InitializePanelViewModels();
        }

        //DI constructor
        [ActivatorUtilitiesConstructor]
        public LayoutDocumentViewModel(string title, string? filePath, InstrumentLayoutConfiguration configuration, 
            //services
            IInstrumentValuesProviderFactory? instrumentValuesProviderFactory, 
            IDialogService? dialogService,
            IStringDataService dataService,
            IStringMaterialEstimationService materialEstimationService)
        {
            this.title = title;
            this.filePath = string.IsNullOrEmpty(filePath) ? null : filePath;
            Configuration = configuration;
            InstrumentValuesProviderFactory = instrumentValuesProviderFactory;
            InstrumentValuesProvider = instrumentValuesProviderFactory?.CreateProvider(Configuration.InstrumentType);
            DialogService = dialogService;
            DataService = dataService;
            MaterialEstimationService = materialEstimationService;
            InitializePanelViewModels();
        }

        private void InitializePanelViewModels()
        {
            GetType().Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(EditorPanelViewModelBase)) && !t.IsAbstract)
                .ToList()
                .ForEach(t =>
                {
                    if (Activator.CreateInstance(t) is EditorPanelViewModelBase panel)
                    {
                        panel.AssignContext(this);
                        PanelViewModels.Add(panel);
                    }
                });
        }

        public T? GetPanelViewModel<T>() where T : EditorPanelViewModelBase
        {
            return PanelViewModels.OfType<T>().FirstOrDefault();
        }


        partial void OnConfigurationChanged(InstrumentLayoutConfiguration value)
        {
            RebuildLayout();
        }

        //partial void OnLayoutZoomChanging(double oldValue, double newValue)
        //{
        //    Trace.WriteLine($"{Title} zoom changing from {oldValue} to {newValue}");
        //}

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
