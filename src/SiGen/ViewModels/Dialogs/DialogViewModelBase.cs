using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SiGen.ViewModels.Dialogs
{
    public interface IDialogViewModel
    {
        string Title { get; }
        bool ShowTitleBar { get; set; }
        ICommand CancelCommand { get; }
    }

    public abstract partial class DialogViewModelBase<TResult> : ObservableObject, IDialogViewModel//, IDialogViewModel<TResult>
    {
        private TaskCompletionSource<TResult?>? _completionSource;

        public abstract string Title { get; }

        [ObservableProperty]
        private bool showTitleBar = true;

        [ObservableProperty]
        private bool resizable;

        public ICommand CancelCommand { get; }

        public DialogViewModelBase()
        {
            CancelCommand = new RelayCommand(CancelDialog, CanClose);
        }

        // Set the completion source (like assigning a promise)
        public void SetCompletionSource(TaskCompletionSource<TResult?> completionSource)
        {
            _completionSource = completionSource;
        }

        // Complete the dialog with a result (like resolving a promise)
        protected virtual void CompleteDialog(TResult? result)
        {
            _completionSource?.SetResult(result);
        }

        // Cancel the dialog (like rejecting a promise)
        public virtual void CancelDialog()
        {
            _completionSource?.SetResult(default(TResult));
        }

        // IComplexDialogViewModel implementation
        public virtual bool CanClose() => true;

        public virtual async Task<TResult?> GetResultAsync()
        {
            if (_completionSource == null)
            {
                _completionSource = new TaskCompletionSource<TResult?>();
            }

            return await _completionSource.Task;
        }

        public virtual Task OnClosingAsync() => Task.CompletedTask;
    }

    // For dialogs that don't return a specific result (just success/cancel)
    public abstract class DialogViewModelBase : DialogViewModelBase<bool>
    {
        //public override string Title => "Test";

        //public DialogViewModelBase()
        //{
        //}

        protected void Complete() => CompleteDialog(true);
        protected void Cancel() => CompleteDialog(false);
    }

    public class MockDialogViewModel : IDialogViewModel
    {
        public string Title => "Dialog";

        public bool ShowTitleBar { get; set; } = true;

        public ICommand CancelCommand { get; } = new RelayCommand(() => { });
    }
}
