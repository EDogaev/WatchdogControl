using System.Windows;
using WatchdogControl.Interfaces;
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

        public EditWatchdogView(Watchdog watchdog, IWatchdogManager watchdogManager)
        {
            InitializeComponent();
            VM = new EditWatchdogViewModel(watchdog, watchdogManager);
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
