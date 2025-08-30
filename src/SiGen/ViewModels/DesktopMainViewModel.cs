using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SiGen.Export;
using SiGen.Layouts.Configuration;
using SiGen.Serialization;
using SiGen.Services;
using SiGen.Settings;
using SiGen.Utilities;
using SiGen.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        //public ICommand NewCommand { get; }
        public ICommand OpenHomeCommand { get; }
        public ICommand OpenFileCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SaveAsCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand TestCommand { get; }
        public RelayCommand<IDocumentTabViewModel> CloseDocumentCommand { get; }

        public ObservableCollection<IDocumentTabViewModel> OpenDocuments { get; } = new ObservableCollection<IDocumentTabViewModel>();

        public List<RecentFileMenuModel> RecentFiles { get; } = new();

        [ObservableProperty]
        private IDocumentTabViewModel? selectedDocument;

        public DesktopMainViewModel(IDialogService dialogService, ISettingsService settingsService)
        {
            this.dialogService = dialogService;
            this.settingsService = settingsService;
            SaveCommand = new RelayCommand(OnSave, CanSave);
            SaveAsCommand = new RelayCommand(OnSaveAs, CanSave);
            OpenCommand = new RelayCommand(OnOpen);
            CloseDocumentCommand = new RelayCommand<IDocumentTabViewModel>(CloseDocument, CanCloseDocument);
            TestCommand = new RelayCommand(() =>
            {
                // For testing purposes only
                //App.Current!.RequestedThemeVariant = ThemeVariant.Light;
                if (SelectedDocument is LayoutDocumentViewModel layoutDoc)
                {
                    var exporter = new DxfLayoutExporter(new DxfExportOptions
                    {
                        ExportFrets = true,
                        ExportStrings = true,
                        ExportCenterLine = true,
                        ExportFingerboard = true
                    }, layoutDoc.Layout!);
                    exporter.ExportLayout("D:\\Programming\\C#\\sigen-next\\tests\\" + layoutDoc.Title + ".dxf");
                }
            });
            OpenHomeCommand = new RelayCommand(OpenHomePage);
            OpenFileCommand = new RelayCommand<string>(OpenDocumentFile);

            RebuildRecentFilesMenu();
        }

        //public DesktopMainViewModel() : this(new DummyFileDialogService())
        //{
        //    // Default constructor for design-time data
        //    OpenDocuments.Add(new DocumentViewModel("Untitled", null, new Layouts.Configuration.InstrumentLayoutConfiguration()) { HasUnsavedChanges = true });
        //    OpenDocuments.Add(new DocumentViewModel("Layout 1", null, new Layouts.Configuration.InstrumentLayoutConfiguration()));
        //    SelectedDocument = OpenDocuments.FirstOrDefault();
        //}

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
            if (SelectedDocument is HomePageViewModel home)
            {
                //todo: only refresh if the recent files have changed since the last time the home page was shown
                //home.ReloadRecentDocuments();
            }
        }

        private bool CanSave() => SelectedDocument != null && SelectedDocument is LayoutDocumentViewModel;

        private bool CanCloseDocument(IDocumentTabViewModel? document) => document != null;

        private void OnSave()
        {
            if (SelectedDocument is not LayoutDocumentViewModel layoutDocument)
                return;

            if (string.IsNullOrEmpty(layoutDocument.FilePath))
                OnSaveAs();
            else
                SaveDocument(layoutDocument, layoutDocument.FilePath);
        }

        private async void OnSaveAs()
        {
            if (SelectedDocument is not LayoutDocumentViewModel layoutDocument)
                return;

            var filePath = await dialogService.ShowSaveFileDialogAsync(defaultFileName: SelectedDocument.Title, filters: [
                new FileDialogFilter {
                    Name = "SiGen Layout Files",
                    Extensions = new List<string> { "json" }
                }]);

            if (!string.IsNullOrEmpty(filePath))
                SaveDocument(layoutDocument, filePath);
        }

        private void SaveDocument(LayoutDocumentViewModel document, string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions();
                options.WriteIndented = true;
                options.Converters.Add(new MeasureConverter());
                options.Converters.Add(new NullableMeasureConverter());
                options.Converters.Add(new BaseStringConfigurationConverter());
                using var stream = System.IO.File.Create(filePath);
                JsonSerializer.Serialize(stream, document.Configuration, options);
                document.Title = System.IO.Path.GetFileNameWithoutExtension(filePath);
                document.HasUnsavedChanges = false;
            }
            catch
            {

            }
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
                    Extensions = new List<string> { "json" }
                },
                new FileDialogFilter {
                    Name = "SiGen V1 Files",
                    Extensions = new List<string> { "sil" }
                }
            ]
            );

            if (!string.IsNullOrEmpty(filePath))
                OpenDocumentFile(filePath);
        }

        public void OpenDocumentFile(string? filePath)
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

            InstrumentLayoutConfiguration? config = null;

            try
            {
                var options = new JsonSerializerOptions();
                options.WriteIndented = true;
                options.Converters.Add(new MeasureConverter());
                options.Converters.Add(new NullableMeasureConverter());
                options.Converters.Add(new BaseStringConfigurationConverter());
                config = JsonSerializer.Deserialize<InstrumentLayoutConfiguration>(
                    System.IO.File.ReadAllText(filePath), options);

            }
            catch //(Exception ex)
            {
                //todo: show error message
            }

            if (config == null) return;


            var document = new LayoutDocumentViewModel(
                System.IO.Path.GetFileNameWithoutExtension(filePath),
                filePath,
                config);

            settingsService.AddRecentFile(document);
            OpenDocuments.Add(document);
            SelectedDocument = document;

            RebuildRecentFilesMenu();
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

        public async void CloseDocument(IDocumentTabViewModel? document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (OpenDocuments.Contains(document))
            {
                if (document is LayoutDocumentViewModel layoutDocument && document.HasUnsavedChanges)
                {
                    if (SelectedDocument != document)
                        SelectedDocument = document;

                    var result = await dialogService.ShowSaveChangesAsync(document.Title);
                    if (result == SaveChangesResult.Cancel)
                        return;

                    if (result == SaveChangesResult.Save)
                    {
                        OnSave();
                        if (string.IsNullOrEmpty(layoutDocument.FilePath))
                            return;
                    }
                }
                
                OpenDocuments.Remove(document);
                if (SelectedDocument == document)
                {
                    //todo, select the next document or the previous one
                    SelectedDocument = OpenDocuments.FirstOrDefault();
                }
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
