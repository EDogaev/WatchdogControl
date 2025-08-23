using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WatchdogControl.ViewModels;

namespace WatchdogControl.Views
{
    /// <summary>
    /// Логика взаимодействия для PasswordView.xaml
    /// </summary>
    public partial class PasswordView : Window
    {
        public PasswordViewModel VM = new PasswordViewModel();

        public PasswordView()
        {
            InitializeComponent();
            DataContext = VM;
        }

        private void PasswordBox_KeyUp(object sender, KeyEventArgs e)
        {
            VM.Password = PasswordBox.Password;
        }
    }
}
