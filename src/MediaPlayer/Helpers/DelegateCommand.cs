using System;

/// <summary>
/// Taken from: http://stackoverflow.com/questions/11960488/any-winrt-icommand-commandbinding-implementaiton-samples-out-there
/// </summary>
namespace MediaPlayer.Helpers
{
    public class DelegateCommand<T> : System.Windows.Input.ICommand
    {
        private readonly Func<T, bool> canExecuteMethod;
        private readonly Action<T> executeMethod;

        #region Initializers
        public DelegateCommand(Action<T> executeMethod) : this(executeMethod, null) { }

        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
        {
            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }
        #endregion

        #region ICommand Members
        public event EventHandler CanExecuteChanged;

        bool System.Windows.Input.ICommand.CanExecute(object parameter)
        {
            try
            {
                return CanExecute((T)parameter);
            }
            catch { return false; }
        }

        void System.Windows.Input.ICommand.Execute(object parameter)
        {
            Execute((T)parameter);
        }
        #endregion

        #region Public Methods
        public bool CanExecute(T parameter)
        {
            return ((canExecuteMethod == null) || canExecuteMethod(parameter));
        }

        public void Execute(T parameter)
        {
            if (executeMethod != null)
            {
                executeMethod(parameter);
            }
        }

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged(EventArgs.Empty);
        }
        #endregion

        #region Protected Methods
        protected virtual void OnCanExecuteChanged(EventArgs e)
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion
    }

}