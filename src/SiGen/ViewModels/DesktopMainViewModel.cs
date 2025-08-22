using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SiGen.Layouts.Configuration;
using SiGen.Serialization;
using SiGen.Services;
using SiGen.Utilities;
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
    public partial class DesktopMainViewModel : ObservableObject
    {
        private readonly IFileDialogService fileDialogService;

        public RelayCommand SaveCommand { get; }
        public RelayCommand SaveAsCommand { get; }
        public ICommand OpenCommand { get; }
        public RelayCommand<DocumentViewModel> CloseDocumentCommand { get; }

        public ObservableCollection<DocumentViewModel> OpenDocuments { get; } = new ObservableCollection<DocumentViewModel>();

        [ObservableProperty]
        private DocumentViewModel? selectedDocument;

        public DesktopMainViewModel(IFileDialogService fileDialogService)
        {
            this.fileDialogService = fileDialogService;
            SaveCommand = new RelayCommand(OnSave, CanSave);
            SaveAsCommand = new RelayCommand(OnSaveAs, CanSave);
            OpenCommand = new RelayCommand(OnOpen);
            CloseDocumentCommand = new RelayCommand<DocumentViewModel>(CloseDocument, CanCloseDocument);
        }

        //public DesktopMainViewModel() : this(new DummyFileDialogService())
        //{
        //    // Default constructor for design-time data
        //    OpenDocuments.Add(new DocumentViewModel("Untitled", null, new Layouts.Configuration.InstrumentLayoutConfiguration()) { HasUnsavedChanges = true });
        //    OpenDocuments.Add(new DocumentViewModel("Layout 1", null, new Layouts.Configuration.InstrumentLayoutConfiguration()));
        //    SelectedDocument = OpenDocuments.FirstOrDefault();
        //}

        partial void OnSelectedDocumentChanged(DocumentViewModel? value)
        {
            SaveCommand.NotifyCanExecuteChanged();
            SaveAsCommand.NotifyCanExecuteChanged();
        }

        private bool CanSave() => SelectedDocument != null;

        private bool CanCloseDocument(DocumentViewModel? document) => document != null;

        private void OnSave()
        {
            if (SelectedDocument == null)
                return;

            if (string.IsNullOrEmpty(SelectedDocument.FilePath))
                OnSaveAs();
            else
                SaveDocument(SelectedDocument, SelectedDocument.FilePath);
        }

        private async void OnSaveAs()
        {
            if (SelectedDocument == null)
                return;

            var filePath = await fileDialogService.ShowSaveFileDialogAsync(defaultFileName: SelectedDocument.Title, filters: [
                new FileDialogFilter {
                    Name = "SiGen Layout Files",
                    Extensions = new List<string> { "json" }
                }]);

            if (!string.IsNullOrEmpty(filePath))
                SaveDocument(SelectedDocument, filePath);
        }

        private void SaveDocument(DocumentViewModel document, string filePath)
        {
            var options = new JsonSerializerOptions();
            options.WriteIndented = true;
            options.Converters.Add(new MeasureConverter());
            options.Converters.Add(new BaseStringConfigurationConverter());
            using var stream = System.IO.File.Create(filePath);
            JsonSerializer.Serialize(stream, document.Configuration, options);
            document.Title = System.IO.Path.GetFileNameWithoutExtension(filePath);
        }

        #region Open layout documents

        private async void OnOpen()
        {
            // Example basic implementation using fileDialogService
            var filePath = await fileDialogService.ShowOpenFileDialogAsync("Open layout", [
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

        public void OpenDocumentFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            // Check if the document is already open, if so, just select it
            if (OpenDocuments.Any(d => string.Equals(d.FilePath, filePath, StringComparison.OrdinalIgnoreCase)))
            {
                SelectedDocument = OpenDocuments.First(d => string.Equals(d.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
                return;
            }

            InstrumentLayoutConfiguration? config = null;

            try
            {
                var options = new JsonSerializerOptions();
                options.WriteIndented = true;
                options.Converters.Add(new MeasureConverter());
                options.Converters.Add(new BaseStringConfigurationConverter());
                config = JsonSerializer.Deserialize<InstrumentLayoutConfiguration>(
                    System.IO.File.ReadAllText(filePath), options);

            }
            catch { }

            if (config == null) return;


            var document = new DocumentViewModel(
                System.IO.Path.GetFileNameWithoutExtension(filePath),
                filePath,
                config);
            OpenDocuments.Add(document);
            SelectedDocument = document;
        }

        public void OpenDocument(DocumentViewModel document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (!OpenDocuments.Contains(document))
            {
                OpenDocuments.Add(document);
            }
            SelectedDocument = document;
        }

        public void CloseDocument(DocumentViewModel? document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (OpenDocuments.Contains(document))
            {
                OpenDocuments.Remove(document);
                if (SelectedDocument == document)
                {
                    //todo, select the next document or the previous one
                    SelectedDocument = OpenDocuments.FirstOrDefault();
                }
            }
        }

        #endregion


    }
}
