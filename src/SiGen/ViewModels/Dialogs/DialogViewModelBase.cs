using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace SiGen.ViewModels.Dialogs
{
    public abstract class DialogViewModelBase<TResult> : ObservableObject//, IDialogViewModel<TResult>
    {
        private TaskCompletionSource<TResult?>? _completionSource;

        public abstract string Title { get; }

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
        protected void Complete() => CompleteDialog(true);
        protected void Cancel() => CompleteDialog(false);
    }
}
