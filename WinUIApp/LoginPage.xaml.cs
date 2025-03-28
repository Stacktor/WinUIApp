using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Security.Cryptography;
using System.Text;

namespace WinUIApp
{
    public sealed partial class LoginPage : Page
    {
        // Dummy hardcoded credentials for demonstration
        private const string ValidUsername = "admin";
        private const string ValidPassword = "password123";

        public LoginPage()
        {
            this.InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (AuthenticateUser(username, password))
            {
                // Optionally, save credentials if "Remember Me" is checked
                if (RememberMeCheckBox.IsChecked == true)
                {
                    SaveCredentials(username);
                }

                // Navigate to MainPage
                this.Frame.Navigate(typeof(MainPage));
            }
            else
            {
                ShowLoginFailedDialog();
            }
        }

        private bool AuthenticateUser(string username, string password)
        {
            // Simple authentication - replace with proper authentication in a real app
            return username == ValidUsername && password == ValidPassword;
        }

        private void SaveCredentials(string username)
        {
            // In a real app, use secure storage like Windows Credential Locker
            // This is a simplified example and not recommended for production
            try
            {
                // Use a secure hashing method to store username
                string hashedUsername = HashString(username);
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["RememberedUsername"] = hashedUsername;
            }
            catch
            {
                // Log error or handle credential saving failure
            }
        }

        private string HashString(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(bytes);
            }
        }

        private async void ShowLoginFailedDialog()
        {
            var dialog = new ContentDialog
            {
                Title = "Login Failed",
                Content = "Invalid username or password. Please try again.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}