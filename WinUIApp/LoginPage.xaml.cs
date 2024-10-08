using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinUIApp
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // For demo purposes, accept any username/password
            if (!string.IsNullOrEmpty(UsernameTextBox.Text) && !string.IsNullOrEmpty(PasswordBox.Password))
            {
                // Optionally, save credentials if "Remember Me" is checked
                if(RememberMeCheckBox.IsChecked == true)
                {
                    // Save credentials securely (implementation depends on your requirements)
                }

                // Navigate to MainPage
                this.Frame.Navigate(typeof(MainPage));
            }
            else
            {
                var dialog = new ContentDialog
                {
                    Title = "Login Failed",
                    Content = "Please enter username and password.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = dialog.ShowAsync();
            }
        }
    }
}
