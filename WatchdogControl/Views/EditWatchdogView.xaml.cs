using System.Windows;
using WatchdogControl.Enums;
using WatchdogControl.Models.Watchdog;
using WatchdogControl.ViewModels;

namespace WatchdogControl.Views
{
    /// <summary>
    /// Логика взаимодействия для EditWatchdogView.xaml
    /// </summary>
    public partial class EditWatchdogView : Window
    {
        public EditWatchdogViewModel VM;

        public EditWatchdogView(Watchdog watchdog)
        {
            InitializeComponent();
            VM = new EditWatchdogViewModel(watchdog);
            DataContext = VM;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = VM.Watchdog.DbData.Password.Password;
        }

        private void PasswordBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (VM.Watchdog.DbData.Password.Password != PasswordBox.Password)
                VM.Watchdog.DbData.Password.Password = PasswordBox.Password;
        }
    }
}
