using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarfBuzzSharp;
using Microsoft.Extensions.DependencyInjection;
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SiGen.ViewModels
{
    public partial class DesktopMainViewModel : ObservableObject, IDocumentManager
    {

        #region Fields

        private readonly IServiceProvider serviceProvider;
        private readonly IDialogService dialogService;
        private readonly ISettingsService settingsService;
        private readonly ViewModelFactory documentFactory;

        #endregion

        #region Commands

        public ICommand OpenHomeCommand { get; }
        public IAsyncRelayCommand OpenFileCommand { get; }
        public IAsyncRelayCommand SaveCommand { get; }
        public IAsyncRelayCommand SaveAsCommand { get; }
        public IAsyncRelayCommand OpenCommand { get; }
        public IAsyncRelayCommand<IDocumentTabViewModel> CloseDocumentCommand { get; }
        public IAsyncRelayCommand ShowSettingsCommand { get; }
        public IAsyncRelayCommand CloseSelectedDocumentCommand {  get; }
        public IRelayCommand OpenInFileExplorerCommand { get; }
        public IRelayCommand PrintLayoutCommand { get; }
        #endregion

        #region State

        public ObservableCollection<IDocumentTabViewModel> OpenDocuments { get; } = new ObservableCollection<IDocumentTabViewModel>();

        public bool NoOpenDocuments => OpenDocuments.Count == 0;

        public List<RecentFileMenuModel> RecentFiles { get; } = new();

        [ObservableProperty]
        private IDocumentTabViewModel? selectedDocument;

        #endregion

        #region Constructor

        public DesktopMainViewModel(IServiceProvider serviceProvider, IDialogService dialogService, ISettingsService settingsService, ViewModelFactory documentFactory)
        {
            this.serviceProvider = serviceProvider;
            this.dialogService = dialogService;
            this.settingsService = settingsService;
            this.documentFactory = documentFactory;

            SaveCommand = new AsyncRelayCommand(SaveSelectedDocumentAsync, CanSaveSelectedDocument);
            SaveAsCommand = new AsyncRelayCommand(SaveSelectedDocumentAsAsync, CanSaveSelectedDocument);
            OpenCommand = new AsyncRelayCommand(OpenDocumentAsync);
            CloseDocumentCommand = new AsyncRelayCommand<IDocumentTabViewModel>(CloseDocumentAsync, CanCloseDocument);
            OpenHomeCommand = new RelayCommand(OpenHomePage);
            OpenFileCommand = new AsyncRelayCommand<string>(OpenDocumentFileAsync);
            ShowSettingsCommand = new AsyncRelayCommand(ShowSettingsAsync);
            CloseSelectedDocumentCommand = new AsyncRelayCommand(CloseSelectedDocument, () => SelectedDocument != null);
            OpenInFileExplorerCommand = new RelayCommand(() => OpenInFileExplorer(SelectedDocument as ILayoutDocument), () => SelectedDocument is ILayoutDocument doc && !string.IsNullOrEmpty(doc.FilePath));
            // Subscribe to recent files changes
            settingsService.RecentFilesChanged += OnRecentFilesChanged;
            OpenDocuments.CollectionChanged += OpenDocuments_CollectionChanged;
            PrintLayoutCommand = new RelayCommand(() => {
                if (SelectedDocument is ILayoutDocument layoutDoc)
                {
                    var options = new PdfExportOptions()
                    {
                        ExportFingerboard = true,
                        ExportStrings = true,
                        UseStringThickness = true
                    };
                    var exporter = new PdfLayoutExporter(options, layoutDoc.Layout!);
                    exporter.ExportLayout(ExportTarget.ToFile("layout.pdf"));
                    //var printService = serviceProvider.GetService<IPrintService>();
                    //printService?.PrintLayout(layoutDoc.Configuration);
                }
            }, () => SelectedDocument is ILayoutDocument);
            // Initial menu build
            RebuildRecentFilesMenu();
        }

        #endregion

        #region Property change callbacks

        partial void OnSelectedDocumentChanged(IDocumentTabViewModel? value)
        {
            SaveCommand.NotifyCanExecuteChanged();
            SaveAsCommand.NotifyCanExecuteChanged();
        }

        #endregion

        #region Collection and settings events

        private void OpenDocuments_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(NoOpenDocuments));
        }

        private void OnRecentFilesChanged(object? sender, EventArgs e)
        {
            RebuildRecentFilesMenu();
        }

        #endregion

        #region Command handlers

        private bool CanSaveSelectedDocument() => SelectedDocument != null && SelectedDocument is LayoutDocumentViewModel;

        private bool CanCloseDocument(IDocumentTabViewModel? document) => document != null;

        #endregion

        #region Settings

        private async Task ShowSettingsAsync()
        {
            await dialogService.ShowUserSettingsDialogAsync();
        }

        #endregion

        #region Save

        private async Task<bool> SaveSelectedDocumentAsync()
        {
            if (SelectedDocument is not LayoutDocumentViewModel layoutDocument)
                return false;

            if (string.IsNullOrEmpty(layoutDocument.FilePath))
                return await SaveDocumentAsAsync(layoutDocument);

            return await SaveDocumentAsync(layoutDocument, layoutDocument.FilePath);
        }

        private async Task<bool> SaveSelectedDocumentAsAsync()
        {
            if (SelectedDocument is not LayoutDocumentViewModel layoutDocument)
                return false;

            return await SaveDocumentAsAsync(layoutDocument);
        }

        private async Task<bool> SaveDocumentAsAsync(LayoutDocumentViewModel layoutDocument)
        {
            var filePath = await dialogService.ShowSaveFileDialogAsync(defaultFileName: layoutDocument.Title, filters: [
                new FileDialogFilter {
                    Name = "SiGen Layout Files",
                    Extensions = new List<string> { "sil" }
                }]);

            if (string.IsNullOrEmpty(filePath))
                return false;

            bool success = await SaveDocumentAsync(layoutDocument, filePath);
            if (success)
                settingsService.AddRecentFile(layoutDocument);

            return success;
        }

        private async Task<bool> SaveDocumentAsync(LayoutDocumentViewModel document, string filePath)
        {
            try
            {
                await LayoutFileSerializer.SaveAsync(document.Configuration, filePath);
                document.Title = System.IO.Path.GetFileNameWithoutExtension(filePath);
                document.HasUnsavedChanges = false;
                document.FilePath = filePath;
                return true;
            }
            catch (Exception ex)
            {
                await dialogService.ShowErrorAsync($"Failed to save file:\n{ex.Message}", "Save Error");
                return false;
            }
        }

        #endregion

        #region Open / document management

        public void OpenHomePage()
        {
            
            var homeDoc = OpenDocuments.OfType<HomePageViewModel>().FirstOrDefault();
            if (homeDoc == null)
            {
                var homePageModel = ActivatorUtilities.CreateInstance<HomePageViewModel>(serviceProvider); // new HomePageViewModel(settingsService, this);
                OpenDocuments.Insert(0, homePageModel);
                SelectedDocument = homePageModel;
            }
            else
            {
                SelectedDocument = homeDoc;
            }
        }

        public async Task OpenDocumentAsync()
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

        public async Task<bool> CloseDocumentAsync(IDocumentTabViewModel? document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            int documentIndex = OpenDocuments.IndexOf(document);
            if (documentIndex < 0)
                throw new InvalidOperationException("Document not found in open documents list.");

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
                    return false;

                if (result == MessageBoxResult.Yes)
                {
                    bool saved = await SaveSelectedDocumentAsync();
                    if (!saved)
                        return false;
                }
            }
            //select the next document if the closed one is currently selected, preferring the previous one in the list
            //we do this before removing the document from the list otherwise it will select the first document
            if (documentIsSelected)
                SelectedDocument = OpenDocuments.Take(documentIndex).LastOrDefault();

            OpenDocuments.Remove(document);

            return true;
        }

        public async Task<bool> CloseSelectedDocument()
        {
            if (SelectedDocument != null)
            {
                return await CloseDocumentAsync(SelectedDocument);
            }
            return false;
        }

        public async Task<bool> TryCloseAllUnsavedDocuments()
        {
            // Attempt to close all documents, prompting to save unsaved changes. Returns true if all documents were closed, false if the operation was cancelled.
            foreach (var doc in OpenDocuments.Where(x => x.HasUnsavedChanges).ToList()) //to list to avoid modification during enumeration
            {
                bool success = await CloseDocumentAsync(doc);
                if (!success) 
                    return false;
            }
            return true;
        }

        #endregion

        

        public void OpenInFileExplorer(ILayoutDocument? document)
        {
            if (document == null) return;

            if (string.IsNullOrEmpty(document.FilePath) || !System.IO.File.Exists(document.FilePath))
                return;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Opens Explorer with the file selected/highlighted
                Process.Start("explorer.exe", $"/select,\"{document.FilePath}\"");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Opens Finder with the file revealed
                Process.Start("open", $"-R \"{document.FilePath}\"");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Opens the containing folder (file selection varies by DE)
                Process.Start(new ProcessStartInfo
                {
                    FileName = "xdg-open",
                    Arguments = $"\"{System.IO.Path.GetDirectoryName(document.FilePath)}\"",
                    UseShellExecute = false
                });
            }
        }

        #region Menu

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
