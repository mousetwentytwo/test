using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Events;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public abstract class DialogViewModelBase<T> : ViewModelBase
    {

        #region Properties

        private const string TITLE = "Title";
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyPropertyChanged(TITLE); }
        }

        private const string MESSAGE = "Message";
        private string _message;
        public string Message
        {
            get { return _message; }
            set { _message = value; NotifyPropertyChanged(MESSAGE); }
        }

        #endregion

        #region Commands

        public DelegateCommand<T> OkCommand { get; private set; }

        protected virtual bool CanExecuteOkCommand(T payload)
        {
            return true;
        }

        protected abstract void ExecuteOkCommand(T payload);

        public DelegateCommand CancelCommand { get; private set; }

        protected virtual bool CanExecuteCancelCommand()
        {
            return true;
        }

        protected abstract void ExecuteCancelCommand();

        #endregion

        protected DialogViewModelBase()
        {
            OkCommand = new DelegateCommand<T>(ExecuteOkCommand, CanExecuteOkCommand);
            CancelCommand = new DelegateCommand(ExecuteCancelCommand, CanExecuteCancelCommand);
        }
    }
}