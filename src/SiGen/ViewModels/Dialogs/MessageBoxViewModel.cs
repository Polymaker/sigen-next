using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SiGen.ViewModels.Dialogs
{
    public partial class MessageBoxViewModel : DialogViewModelBase<MessageBoxResult>
    {
        private string title = string.Empty;

        [ObservableProperty]
        private string message = string.Empty;

        [ObservableProperty]
        private MessageBoxButtons buttons;

        [ObservableProperty]
        private MessageBoxIcon icon;

        public override string Title => title;

        public bool ShowOkButton => Buttons == MessageBoxButtons.Ok || Buttons == MessageBoxButtons.OkCancel;
        public bool ShowYesNoButtons => Buttons == MessageBoxButtons.YesNo || Buttons == MessageBoxButtons.YesNoCancel;
        public bool ShowCancelButton => Buttons == MessageBoxButtons.OkCancel || Buttons == MessageBoxButtons.YesNoCancel;

        public bool IsInformation => Icon == MessageBoxIcon.Information;
        public bool IsError => Icon == MessageBoxIcon.Error;
        public bool IsWarning => Icon == MessageBoxIcon.Warning;
        public bool IsQuestion => Icon == MessageBoxIcon.Question;

        public MessageBoxViewModel()
        {

        }

        public MessageBoxViewModel(string message, string title, MessageBoxButtons buttons = MessageBoxButtons.Ok, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            this.title = title;
            this.message = message;
            this.buttons = buttons;
            this.icon = icon;
            Resizable = false;
        }

        [RelayCommand]
        private void Ok()
        {
            CompleteDialog(MessageBoxResult.Ok);
        }

        [RelayCommand]
        private void Yes()
        {
            CompleteDialog(MessageBoxResult.Yes);
        }

        [RelayCommand]
        private void No()
        {
            CompleteDialog(MessageBoxResult.No);
        }


        public override void CancelDialog()
        {
            CompleteDialog(MessageBoxResult.Cancel);
        }
        //[RelayCommand]
        //private void Cancel()
        //{
        //    CompleteDialog(MessageBoxResult.Cancel);
        //}
    }

    public enum MessageBoxButtons
    {
        Ok,
        OkCancel,
        YesNo,
        YesNoCancel
    }

    public enum MessageBoxIcon
    {
        None,
        Information,
        Warning,
        Error,
        Question
    }

    public enum MessageBoxResult
    {
        None,
        Ok,
        Cancel,
        Yes,
        No
    }
}


