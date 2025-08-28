using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SiGen.Data.Common;
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

        public string Title => "Home";

        bool IDocumentTabViewModel.HasUnsavedChanges => false;

        string? IDocumentTabViewModel.TabToolTip => null;

        public List<RecentFileModel> RecentFiles { get; } = new List<RecentFileModel>();

        [ObservableProperty]
        private string searchText = string.Empty;

        public IEnumerable<RecentFileModel> FilteredRecentFiles =>
            string.IsNullOrWhiteSpace(SearchText)
                ? RecentFiles
                : RecentFiles.Where(f => f.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                                     || f.FilePath.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        public RelayCommand<RecentFileModel> OpenRecentFileCommand { get; }

        public HomePageViewModel(ISettingsService settingsService, IDocumentManager documentManager)
        {
            RecentFiles.AddRange(settingsService.Settings.RecentFiles);
            OnPropertyChanged(nameof(RecentFiles));
            OnPropertyChanged(nameof(FilteredRecentFiles));
            this.settingsService = settingsService;
            this.documentManager = documentManager;
            OpenRecentFileCommand = new RelayCommand<RecentFileModel>(OpenRecentFile);
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
    }
}
