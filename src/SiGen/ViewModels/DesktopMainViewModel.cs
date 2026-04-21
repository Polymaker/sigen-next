using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarfBuzzSharp;
using SiGen.Export;
using SiGen.Layouts.Configuration;
using SiGen.Measuring;
using SiGen.Serialization;
using SiGen.Serialization.Layouts;
using SiGen.Services;
using SiGen.Settings;
using SiGen.Utilities;
using SiGen.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SiGen.ViewModels
{
    public partial class DesktopMainViewModel : ObservableObject, IDocumentManager
    {
        private readonly IDialogService dialogService;
        private readonly ISettingsService settingsService;
        private readonly ViewModelFactory documentFactory;

        //public ICommand NewCommand { get; }
        public ICommand OpenHomeCommand { get; }
        public ICommand OpenFileCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SaveAsCommand { get; }
        public ICommand OpenCommand { get; }
        public RelayCommand<IDocumentTabViewModel> CloseDocumentCommand { get; }

        public RelayCommand ShowSettingsCommand { get; }


        public ObservableCollection<IDocumentTabViewModel> OpenDocuments { get; } = new ObservableCollection<IDocumentTabViewModel>();

        public List<RecentFileMenuModel> RecentFiles { get; } = new();

        [ObservableProperty]
        private IDocumentTabViewModel? selectedDocument;

        public DesktopMainViewModel(IDialogService dialogService, ISettingsService settingsService, ViewModelFactory documentFactory)
        {
            this.dialogService = dialogService;
            this.settingsService = settingsService;
            this.documentFactory = documentFactory;

            SaveCommand = new RelayCommand(OnSave, CanSave);
            SaveAsCommand = new RelayCommand(OnSaveAs, CanSave);
            OpenCommand = new RelayCommand(OnOpen);
            CloseDocumentCommand = new RelayCommand<IDocumentTabViewModel>(CloseDocument, CanCloseDocument);
            OpenHomeCommand = new RelayCommand(OpenHomePage);
            OpenFileCommand = new RelayCommand<string>(OpenDocumentFile);
            ShowSettingsCommand = new RelayCommand(async () => await ShowSettingsAsync());

            // Subscribe to recent files changes
            settingsService.RecentFilesChanged += OnRecentFilesChanged;

            // Initial menu build
            RebuildRecentFilesMenu();
        }

        private void OnRecentFilesChanged(object? sender, EventArgs e)
        {
            RebuildRecentFilesMenu();
        }

        private async Task ShowSettingsAsync()
        {
            await dialogService.ShowUserSettingsDialogAsync();
        }

        public void ReorderDocuments(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= OpenDocuments.Count || newIndex < 0 || newIndex >= OpenDocuments.Count)
                throw new ArgumentOutOfRangeException();
            if (oldIndex == newIndex)
                return;
            var doc = OpenDocuments[oldIndex];
            OpenDocuments.RemoveAt(oldIndex);
            OpenDocuments.Insert(newIndex, doc);
            SelectedDocument = doc;
        }

        partial void OnSelectedDocumentChanged(IDocumentTabViewModel? value)
        {
            SaveCommand.NotifyCanExecuteChanged();
            SaveAsCommand.NotifyCanExecuteChanged();
        }

        private bool CanSave() => SelectedDocument != null && SelectedDocument is LayoutDocumentViewModel;

        private bool CanCloseDocument(IDocumentTabViewModel? document) => document != null;

        private async void OnSave()
        {
            if (SelectedDocument is not LayoutDocumentViewModel layoutDocument)
                return;

            if (string.IsNullOrEmpty(layoutDocument.FilePath))
                OnSaveAs();
            else
                await SaveDocumentAsync(layoutDocument, layoutDocument.FilePath);
        }

        private async void OnSaveAs()
        {
            if (SelectedDocument is not LayoutDocumentViewModel layoutDocument)
                return;

            var filePath = await dialogService.ShowSaveFileDialogAsync(defaultFileName: SelectedDocument.Title, filters: [
                new FileDialogFilter {
                    Name = "SiGen Layout Files",
                    Extensions = new List<string> { "sil" }
                }]);

            if (!string.IsNullOrEmpty(filePath))
                await SaveDocumentAsync(layoutDocument, filePath);
        }

        private async Task SaveDocumentAsync(LayoutDocumentViewModel document, string filePath)
        {
            try
            {
                await LayoutFileSerializer.SaveAsync(document.Configuration, filePath);
                document.Title = System.IO.Path.GetFileNameWithoutExtension(filePath);
                document.HasUnsavedChanges = false;
                document.FilePath = filePath;
            }
            catch (Exception ex)
            {
                await dialogService.ShowErrorAsync($"Failed to save file:\n{ex.Message}", "Save Error");
            }
        }

        private void SaveDocument(LayoutDocumentViewModel document, string filePath)
        {
            // Synchronous wrapper for backward compatibility
            _ = SaveDocumentAsync(document, filePath);
        }

        public void OpenHomePage()
        {
            
            var homeDoc = OpenDocuments.OfType<HomePageViewModel>().FirstOrDefault();
            if (homeDoc == null)
            {
                var homePage = new HomePageViewModel(settingsService, this);
                OpenDocuments.Insert(0, homePage);
                SelectedDocument = homePage;
            }
            else
            {
                SelectedDocument = homeDoc;
            }
        }

        #region Open layout documents

        private async void OnOpen()
        {
            // Example basic implementation using fileDialogService
            var filePath = await dialogService.ShowOpenFileDialogAsync("Open layout", [
                new FileDialogFilter {
                    Name = "SiGen Layout Files",
                    Extensions = new List<string> { "sil", "json" }
                }
            ]
            );

            if (!string.IsNullOrEmpty(filePath))
                await OpenDocumentFileAsync(filePath);
        }

        public async Task OpenDocumentFileAsync(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            // Check if the document is already open, if so, just select it
            var existingDoc = OpenDocuments.OfType<LayoutDocumentViewModel>().FirstOrDefault(d => string.Equals(d.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
            if (existingDoc != null)
            {
                SelectedDocument = existingDoc;
                return;
            }

            if (!File.Exists(filePath))
            {
                var message = string.Format(Lang.Resources.OpenDocument_FileNotFound_Message, filePath);
                await dialogService.ShowErrorAsync(message, Lang.Resources.OpenDocument_FileNotFound_Title);
                return;
            }

            InstrumentLayoutConfiguration? config = null;

            try
            {
                config = await LayoutFileSerializer.LoadAsync(filePath);
            }
            catch (Exception ex)
            {
                var message = string.Format(Lang.Resources.OpenDocument_LoadError_Message, ex.Message);
                await dialogService.ShowErrorAsync(message, Lang.Resources.OpenDocument_LoadError_Title);
            }

            if (config == null) return;

            // Check if the file was migrated (old version loaded)
            bool wasMigrated = config.Version < LayoutFileSerializer.CurrentVersion;

            // Update to current version now that we've detected migration
            if (wasMigrated)
            {
                config.Version = LayoutFileSerializer.CurrentVersion;
            }

            var document = documentFactory.CreateLayoutDocumentViewModel(filePath, config);

            // Mark as unsaved if migration occurred
            if (wasMigrated)
            {
                document.HasUnsavedChanges = true;
            }

            settingsService.AddRecentFile(document);
            OpenDocuments.Add(document);
            SelectedDocument = document;

            if (wasMigrated)
            {
                await dialogService.ShowWarningAsync(
                    Lang.Resources.OpenDocument_FileMigrated_Message,
                    Lang.Resources.OpenDocument_FileMigrated_Title);
            }
        }

        public void OpenDocumentFile(string? filePath)
        {
            // Synchronous wrapper for backward compatibility
            _ = OpenDocumentFileAsync(filePath);
        }

        public void OpenDocument(LayoutDocumentViewModel document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (!OpenDocuments.Contains(document))
            {
                OpenDocuments.Add(document);
            }
            SelectedDocument = document;
        }

        public void OpenLayoutConfiguration(string documentName, InstrumentLayoutConfiguration configuration)
        {
            var document = this.documentFactory.CreateLayoutDocumentViewModel(null, configuration, documentName);
            OpenDocuments.Add(document);
            SelectedDocument = document;
        }

        public async void CloseDocument(IDocumentTabViewModel? document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            int documentIndex = OpenDocuments.IndexOf(document);
            if (documentIndex >= 0)
            {
                bool documentIsSelected = SelectedDocument == document;
                if (document is LayoutDocumentViewModel layoutDocument && document.HasUnsavedChanges)
                {
                    if (SelectedDocument != document)
                        SelectedDocument = document;

                    var message = string.Format(Lang.Resources.Dialog_SaveChanges_Message, document.Title);
                    var result = await dialogService.ShowMessageBoxAsync(
                        message, 
                        Lang.Resources.Dialog_SaveChanges_Title, 
                        MessageBoxButtons.YesNoCancel, 
                        MessageBoxIcon.Question);
                    
                    if (result == MessageBoxResult.Cancel)
                        return;

                    if (result == MessageBoxResult.Yes)
                    {
                        OnSave();
                        if (string.IsNullOrEmpty(layoutDocument.FilePath)) //user canceled save as dialog
                            return;
                    }
                }
                //select the next document if the closed one is currently selected, preferring the previous one in the list
                //we do this before removing the document from the list otherwise it will select the first document
                if (documentIsSelected)
                    SelectedDocument = OpenDocuments.Take(documentIndex).LastOrDefault();

                OpenDocuments.Remove(document);
            }
        }

        #endregion

        #region Menu handling

        private void RebuildRecentFilesMenu()
        {
            RecentFiles.Clear();
            RecentFiles.AddRange(settingsService.Settings.RecentFiles.Take(10).Select((f, i) => new RecentFileMenuModel(i + 1, f, OpenDocumentFile)));
            OnPropertyChanged(nameof(RecentFiles));
        }

        #endregion
    }

    public class RecentFileMenuModel
    {
        public int Index { get; }
        public RecentFileModel RecentFile { get; }
        public string FilePath => RecentFile.FilePath;
        public string DisplayName => RecentFile.DisplayName;
        public DateTime LastOpened => RecentFile.LastOpened;
        public ICommand OpenCommand { get; }

        public RecentFileMenuModel(int index, RecentFileModel recentFile, Action<string?> openCommand)
        {
            Index = index;
            RecentFile = recentFile ?? throw new ArgumentNullException(nameof(recentFile));
            OpenCommand = new RelayCommand(() => openCommand(recentFile.FilePath), ()=> true);
        }
    }
}
