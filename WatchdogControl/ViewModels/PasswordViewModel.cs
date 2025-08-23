using System.Windows;
using WatchdogControl.RealizedInterfaces;

namespace WatchdogControl.ViewModels
{
    public class PasswordViewModel : NotifyPropertyChanged
    {
        private const string CorrectPassword = "vac2310";
        private bool _wrongPassword;
        private string _password;

        public string Password
        {
            get => _password;
            set
            {
                if (value == _password)
                    return;

                _password = value;

                // сбросить признак неправильного пароля при вводе нового пароля
                WrongPassword = false;
            }
        }

        public bool WrongPassword
        {
            get => _wrongPassword;
            set
            {
                if (value == _wrongPassword)
                    return;
                _wrongPassword = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<Window> ConfirmCommand { get; }

        public PasswordViewModel()
        {
            ConfirmCommand = new RelayCommand<Window>(Confirm);
        }

        private void Confirm(Window window)
        {
            if (Password != CorrectPassword)
            {
                WrongPassword = true;
                return;
            }

            //Messages.ShowMsg("Пароль введен успешно!");

            window.DialogResult = true;
        }
    }
}
