using System;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public class DelegateCommand : IDelegateCommand
    {
        private readonly Action _executeAction;
        private readonly Func<bool> _canExecuteAction;

        public DelegateCommand(Action executeAction, Func<bool> canExecuteAction = null)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null. </param>
        public bool CanExecute(object parameter = null)
        {
            return _canExecuteAction == null || _canExecuteAction();
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null. </param>
        public void Execute(object parameter = null)
        {
            if (CanExecute(parameter))
            {
                _executeAction();
            }
        }

        private void OnCanExecuteChanged(object sender, EventArgs args)
        {
            var handler = CanExecuteChanged;
            if (handler != null) handler(sender, args);
        }

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged(this, EventArgs.Empty);
        }

        public event EventHandler CanExecuteChanged;
    }

    public class DelegateCommand<T> : IDelegateCommand
    {
        private readonly Action<T> _executeAction;
        private readonly Func<T, bool> _canExecuteAction;

        public DelegateCommand(Action<T> executeAction, Func<T, bool> canExecuteAction = null)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null. </param>
        public bool CanExecute(object parameter)
        {
            return _canExecuteAction == null || _canExecuteAction((T)parameter);
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null. </param>
        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _executeAction((T)parameter);
            }
        }

        private void OnCanExecuteChanged(object sender, EventArgs args)
        {
            var handler = CanExecuteChanged;
            if (handler != null) handler(sender, args);
        }

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged(this, EventArgs.Empty);
        }

        public event EventHandler CanExecuteChanged;
    }

}