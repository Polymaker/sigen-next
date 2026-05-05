using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SiGen.Data.Common;
using SiGen.Data.Presets;
using SiGen.Layouts.Configuration;
using SiGen.Services;
using SiGen.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.ViewModels
{
    public partial class HomePageViewModel : ObservableObject, IDocumentTabViewModel
    {
        private readonly ISettingsService settingsService;
        private readonly IDocumentManager documentManager;

        public string Title => Lang.Resources.HomePage_Title;

        bool IDocumentTabViewModel.HasUnsavedChanges => false;

        string? IDocumentTabViewModel.TabToolTip => null;

        public List<RecentFileModel> RecentFiles { get; } = new List<RecentFileModel>();

        public List<InstrumentTemplateGroupModel> InstrumentTemplates { get; } = new List<InstrumentTemplateGroupModel>();

        public bool IsHomePage => true;
        public bool IsDocument => false;

        [ObservableProperty]
        private string searchText = string.Empty;

        public IEnumerable<RecentFileModel> FilteredRecentFiles =>
            string.IsNullOrWhiteSpace(SearchText)
                ? RecentFiles
                : RecentFiles.Where(f => f.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                                     || f.FilePath.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        public RelayCommand<RecentFileModel> OpenRecentFileCommand { get; }

        [ActivatorUtilitiesConstructor]
        public HomePageViewModel(ISettingsService settingsService, IDocumentManager documentManager)
        {
            RecentFiles.AddRange(settingsService.Settings.RecentFiles);
            OnPropertyChanged(nameof(RecentFiles));
            OnPropertyChanged(nameof(FilteredRecentFiles));
            this.settingsService = settingsService;
            this.documentManager = documentManager;
            OpenRecentFileCommand = new RelayCommand<RecentFileModel>(OpenRecentFile);
            
            // Subscribe to recent files changes
            settingsService.RecentFilesChanged += OnRecentFilesChanged;
            settingsService.LanguageChanged += OnLanguageChanged;
            RebuildTemplates();
        }

        private void OnLanguageChanged(object? sender, AppLanguage e)
        {
            Task.Delay(100).ContinueWith((t) =>
            {
                OnPropertyChanged(nameof(Title));
            });
        }

        private void OnRecentFilesChanged(object? sender, EventArgs e)
        {
            ReloadRecentDocuments();
        }

        partial void OnSearchTextChanged(string value)
        {
            OnPropertyChanged(nameof(FilteredRecentFiles));
        }

        private void OpenRecentFile(RecentFileModel? recentFile)
        {
            if (recentFile == null || string.IsNullOrWhiteSpace(recentFile.FilePath))
                return;

            documentManager.OpenDocumentFile(recentFile.FilePath);
        }

        public void ReloadRecentDocuments()
        {
            RecentFiles.Clear();
            RecentFiles.AddRange(settingsService.Settings.RecentFiles);
            OnPropertyChanged(nameof(RecentFiles));
            OnPropertyChanged(nameof(FilteredRecentFiles));
        }

        private void RebuildTemplates()
        {
            var factory = new InstrumentValuesProviderFactory();
            var providers = factory.GetValuesProviders();
            foreach (var provider in providers)
            {
                try
                {
                    var group = new InstrumentTemplateGroupModel(provider.InstrumentType);
                    group.Templates.AddRange(provider.GetLayoutTemplates());
                    if (group.Templates.Count > 0)
                        InstrumentTemplates.Add(group);
                }
                catch { }
            }
        }
    
        public void OpenTemplate(LayoutTemplate template)
        {
            var instrumentName = Lang.Resources.ResourceManager.GetString($"InstrumentType.{template.InstrumentType}", Lang.Resources.Culture) ?? template.InstrumentType.ToString();
            documentManager.OpenLayoutConfiguration($"{instrumentName} - {template.Name}", InstrumentLayoutConfiguration.Duplicate(template.Configuration));
        }
    }

    public class InstrumentTemplateGroupModel
    {
        public InstrumentType InstrumentType { get; }
        public string Name { get; }
        public List<LayoutTemplate> Templates { get; } = new List<LayoutTemplate>();

        public InstrumentTemplateGroupModel(InstrumentType instrumentType)
        {
            InstrumentType = instrumentType;
            Name = Lang.Resources.ResourceManager.GetString($"InstrumentType.{instrumentType}", Lang.Resources.Culture) ?? instrumentType.ToString();
        }
    }
}
