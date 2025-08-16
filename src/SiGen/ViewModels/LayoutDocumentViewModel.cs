using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SiGen.Layouts;
using SiGen.Layouts.Builders;
using SiGen.Layouts.Configuration;
using SiGen.Services;
using SiGen.ViewModels.EditorPanels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SiGen.ViewModels
{
    /// <summary>
    /// Represents a single layout document in the editor.
    /// </summary>
    public partial class LayoutDocumentViewModel : ObservableObject
    {
        /// <summary>
        /// The configuration for this layout document
        /// </summary>
        public InstrumentLayoutConfiguration? Configuration { get; private set; }

        // File location (null if not yet saved)
        private string? _filePath;
        public string? FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        // Display name (filename or "Untitled")
        public string DisplayName => !string.IsNullOrEmpty(FilePath) ? System.IO.Path.GetFileName(FilePath) : "Untitled";

        // Unsaved changes tracking
        private bool _hasUnsavedChanges;
        private readonly IFileDialogService fileDialogService;

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set => SetProperty(ref _hasUnsavedChanges, value);
        }

        // Panel ViewModels
        public InstrumentInfoPanelViewModel InstrumentInfoPanel { get; }
        public ScaleLengthPanelViewModel ScaleLengthPanel { get; }
        private List<EditorPanelViewModelBase> panelViewModels = new();

        [ObservableProperty]
        private StringedInstrumentLayout layout;

        //public MarginPanelViewModel MarginPanel { get; }
        // Add other panels as needed

        public LayoutDocumentViewModel(IServiceProvider serviceProvider, IFileDialogService fileDialogService)
        {
            //Configuration = config;
            this.fileDialogService = fileDialogService;
            layout = new StringedInstrumentLayout();
            // Initialize panel view models, passing the shared configuration
            InstrumentInfoPanel = serviceProvider.GetRequiredService<InstrumentInfoPanelViewModel>();
            ScaleLengthPanel = serviceProvider.GetRequiredService<ScaleLengthPanelViewModel>();
            //MarginPanel = new MarginPanelViewModel(Configuration);

            Type[] panelTypes = [typeof(InstrumentInfoPanelViewModel), typeof(ScaleLengthPanelViewModel)]; //, typeof(MarginPanelViewModel)];
            foreach (var type in panelTypes)
            {
                var panel = (EditorPanelViewModelBase)serviceProvider.GetRequiredService(type);
                panel.SetConfiguration(Configuration);
                panelViewModels.Add(panel);
            }
            // Subscribe to panel changes to track unsaved changes
            InstrumentInfoPanel.ChangesApplied += OnChangesApplied;
            ScaleLengthPanel.ChangesApplied += OnChangesApplied;
            //MarginPanel.ChangesApplied += OnChangesApplied;
        }
        //public LayoutDocumentViewModel(InstrumentLayoutConfiguration config, IFileDialogService fileDialogService, string? filePath = null)
        //{
        //    Configuration = config;
        //    this.fileDialogService = fileDialogService;
        //    FilePath = filePath;
        //    layout = new StringedInstrumentLayout();
        //    // Initialize panel view models, passing the shared configuration
        //    InstrumentInfoPanel = new InstrumentInfoPanelViewModel();
        //    ScaleLengthPanel = new ScaleLengthPanelViewModel();
        //    //MarginPanel = new MarginPanelViewModel(Configuration);

        //    // Subscribe to panel changes to track unsaved changes
        //    InstrumentInfoPanel.ChangesApplied += OnChangesApplied;
        //    ScaleLengthPanel.ChangesApplied += OnChangesApplied;
        //    //MarginPanel.ChangesApplied += OnChangesApplied;
        //}

        public void SetDocument(InstrumentLayoutConfiguration configuration, string? filepath = null)
        {
            Configuration = configuration;
            FilePath = filepath;

            InstrumentInfoPanel.SetConfiguration(configuration);
            ScaleLengthPanel.SetConfiguration(configuration);
        }

        private void OnChangesApplied(object? sender, EventArgs e)
        {
            // Mark as dirty on any panel change (customize as needed)
            HasUnsavedChanges = true;

            RebuildLayout();
        }

        protected void RebuildLayout()
        {
            if (Configuration == null) return;
            // Rebuild the layout using the current configuration
            var result = LayoutBuilder.Build(Configuration);
            if (result.Success)
            {
                Layout = result.Layout!;
            }
            else
            {
                // Handle errors (e.g., show messages, log, etc.)
                // For now, just clear the layout
                //Layout = new StringedInstrumentLayout();
            }
        }

        private EditorPanelViewModelBase[] GetEditorPanelViewModels()
        {
            return [InstrumentInfoPanel, ScaleLengthPanel]; //, MarginPanel];
        }

        private void LoadConfigToPanels(InstrumentLayoutConfiguration config)
        {
            foreach (var panel in GetEditorPanelViewModels())
                panel.SetConfiguration(config);
        }

        #region Commands

        // Save command
        public IRelayCommand SaveCommand => new RelayCommand(async () => await SaveAsync());
        // SaveAs command
        public IRelayCommand SaveAsCommand => new RelayCommand(async () => await SaveAsAsync());

        public async Task<bool> SaveAsync()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                // No file path, so call SaveAs
                return await SaveAsAsync();
            }

            try
            {
                // Serialize the configuration (replace with your preferred format)
                var json = JsonSerializer.Serialize(Configuration, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(FilePath, json);

                HasUnsavedChanges = false;
                return true;
            }
            catch (Exception)
            {
                // Handle error (log, show message, etc.)
                // For now, just return false
                return false;
            }
        }

        private async Task<bool> SaveAsAsync()
        {
            string? newPath = await fileDialogService.ShowSaveFileDialogAsync();
            if (string.IsNullOrEmpty(newPath))
                return false;

            FilePath = newPath;
            return await SaveAsync();
        }

        #endregion
    }
}
