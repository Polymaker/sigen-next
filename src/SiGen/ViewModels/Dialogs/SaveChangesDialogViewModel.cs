using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace SiGen.ViewModels.Dialogs
{
    public class SaveChangesDialogViewModel : DialogViewModelBase<SaveChangesResult>
    {
        public override string Title => "Unsaved Changes";
        public string DocumentName { get; set; } = "Untitled Document";
        public bool IsDesktop { get; }

        public ICommand SaveCommand { get; }
        public ICommand DontSaveCommand { get; }
        //public ICommand CancelCommand { get; }

        public SaveChangesDialogViewModel(string documentName = "Untitled Document")
        {
            DocumentName = documentName;
            IsDesktop = DetectPlatform();

            SaveCommand = new RelayCommand(OnSave);
            DontSaveCommand = new RelayCommand(OnDontSave);
            ShowTitleBar = false;
            //CancelCommand = new RelayCommand(OnCancel);
        }

        private void OnSave()
        {
            CompleteDialog(SaveChangesResult.Save);
        }

        private void OnDontSave()
        {
            CompleteDialog(SaveChangesResult.DontSave);
        }

        //private void OnCancel()
        //{
        //    CompleteDialog(SaveChangesResult.Cancel);
        //}

        public override void CancelDialog()
        {
            CompleteDialog(SaveChangesResult.Cancel);
        }

        private static bool DetectPlatform()
        {
#if ANDROID || IOS
        return false;
#else
            return true;
#endif
        }
    }
}
