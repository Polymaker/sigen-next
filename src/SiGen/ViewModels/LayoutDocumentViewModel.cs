using Avalonia.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SiGen.Layouts;
using SiGen.Layouts.Builders;
using SiGen.Layouts.Configuration;
using SiGen.Serialization;
using SiGen.Services;
using SiGen.ViewModels.EditorPanels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class LayoutDocumentViewModel : ObservableObject, ILayoutDocumentContext
    {
        private readonly IInstrumentValuesProviderFactory valuesProviderFactory;
        public IInstrumentValuesProvider? InstrumentValuesProvider { get; private set; }


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
        private List<EditorPanelViewModelBase> panelViewModels = new();

        [ObservableProperty]
        private StringedInstrumentLayout currentLayout;

        public event EventHandler? LayoutChanged;

        //public MarginPanelViewModel MarginPanel { get; }
        // Add other panels as needed

        public LayoutDocumentViewModel(IServiceProvider serviceProvider, IFileDialogService fileDialogService, IInstrumentValuesProviderFactory valuesProviderFactory = null)
        {
            this.valuesProviderFactory = valuesProviderFactory;

            this.fileDialogService = fileDialogService;

            currentLayout = new StringedInstrumentLayout();
            StableConfiguration = new InstrumentLayoutConfiguration();
            WorkingConfiguration = new InstrumentLayoutConfiguration();


            Type[] panelTypes = [typeof(InstrumentInfoPanelViewModel), typeof(ScaleLengthPanelViewModel)]; //, typeof(MarginPanelViewModel)];
            foreach (var type in panelTypes)
            {
                var panel = (EditorPanelViewModelBase)serviceProvider.GetRequiredService(type);
                panel.AssignContext(this);
                panelViewModels.Add(panel);
            }
        }

        public T GetPanelViewModel<T>() where T : EditorPanelViewModelBase
        {
            return (T)panelViewModels.First(p => p is T) ?? throw new InvalidOperationException($"Panel of type {typeof(T).Name} not found.");
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(CurrentLayout))
            {
                LayoutChanged?.Invoke(this, EventArgs.Empty);
            }
        }


        public void SetDocument(InstrumentLayoutConfiguration configuration, string? filepath = null)
        {
            StableConfiguration = configuration;
            WorkingConfiguration = CloneConfiguration(configuration);
            InstrumentValuesProvider = valuesProviderFactory?.CreateProvider(StableConfiguration.InstrumentType);
            FilePath = filepath;
            OnConfigurationChanged();
            NotifyConfigurationChanged();
        }

        private InstrumentLayoutConfiguration CloneConfiguration(InstrumentLayoutConfiguration config)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new BaseStringConfigurationConverter());
            options.Converters.Add(new MeasureConverter());
            var json = JsonSerializer.Serialize(config, options);
            return JsonSerializer.Deserialize<InstrumentLayoutConfiguration>(json, options)!;
        }


        protected void RebuildLayout()
        {
            // Rebuild the layout using the current configuration
            var result = LayoutBuilder.Build(WorkingConfiguration);
            if (result.Success)
            {
                CurrentLayout = result.Layout!;
            }
            else
            {

                // Handle errors (e.g., show messages, log, etc.)
                // For now, just clear the layout
            }
        }

        #region Configuration Editing

        public InstrumentLayoutConfiguration StableConfiguration { get; private set; }
        public InstrumentLayoutConfiguration WorkingConfiguration { get; private set; }

        InstrumentLayoutConfiguration ILayoutDocumentContext.Configuration => WorkingConfiguration;
        private bool isLoadingConfig = false;

        public void UpdateConfiguration(string reason, Action<InstrumentLayoutConfiguration> updateAction)
        {
            if (isLoadingConfig) return;
            
            int numberOfStrings = WorkingConfiguration.NumberOfStrings;
            var instrumentType = WorkingConfiguration.InstrumentType;

            // Apply the update action to the working configuration
            updateAction(WorkingConfiguration);

            if (WorkingConfiguration.NumberOfStrings != numberOfStrings)
                NotifyNumberOfStringsChanged();

            if (WorkingConfiguration.InstrumentType != instrumentType)
            {
                InstrumentValuesProvider = valuesProviderFactory?.CreateProvider(WorkingConfiguration.InstrumentType);
                NotifyInstrumentTypeChanged();
            }

            OnConfigurationChanged();
            HasUnsavedChanges = true;
        }

        private void NotifyConfigurationChanged()
        {
            isLoadingConfig = true;
            foreach (var panel in panelViewModels)
                panel.NotifyConfigurationChanged();
            isLoadingConfig = false;
        }

        private void NotifyNumberOfStringsChanged()
        {
            foreach (var panel in panelViewModels)
                panel.OnNumberOfStringsChanged();
        }

        private void NotifyInstrumentTypeChanged()
        {
            foreach (var panel in panelViewModels)
                panel.OnInstrumentTypeChanged();
        }

        protected void OnConfigurationChanged()
        {
            RebuildLayout();
        }

        public void ApplyChanges()
        {
            // Apply changes from working configuration to stable configuration
            StableConfiguration = CloneConfiguration(WorkingConfiguration);
            HasUnsavedChanges = false;
            // Notify all panels that the configuration has changed
            foreach (var panel in panelViewModels)
                panel.LoadConfiguration(StableConfiguration);
        }

        #endregion

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
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                options.Converters.Add(new BaseStringConfigurationConverter());
                options.Converters.Add(new MeasureConverter());
                var json = JsonSerializer.Serialize(StableConfiguration, options);
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
