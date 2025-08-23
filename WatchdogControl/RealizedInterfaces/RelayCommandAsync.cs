using System.Windows.Input;

namespace WatchdogControl.RealizedInterfaces
{
    public class RelayCommandAsync<T> : NotifyPropertyChanged, ICommand
    {
        private readonly Func<T, Task> _execute;
        private readonly Func<T, bool> _canExecute;
        private bool _isRunning;

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (value == _isRunning) return;
                _isRunning = value;

                OnPropertyChanged();
            }
        }

        public RelayCommandAsync(Func<T, Task> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return !_isRunning && (_canExecute?.Invoke(parameter == null ? default : (T)parameter) ?? true);
        }

        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter == null ? default : (T)parameter);
        }

        private async Task ExecuteAsync(T parameter)
        {
            // уведомить среду о том, что идет выполнение метода
            IsRunning = true;

            try
            {
                await _execute(parameter);
            }
            finally
            {
                // уведомить среду о том, что выполнение метода закончено
                IsRunning = false;
            }
        }
    }
}