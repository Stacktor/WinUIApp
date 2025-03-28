using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
            LoadSavedUsername();
        }

        private void LoadSavedUsername()
        {
            try
            {
                var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
                if (settings.Values.ContainsKey("RememberUsername") &&
                    settings.Values["RememberUsername"] is bool remember &&
                    remember)
                {
                    if (settings.Values.ContainsKey("SavedUsername"))
                    {
                        UsernameTextBox.Text = settings.Values["SavedUsername"] as string;
                        RememberMeCheckBox.IsChecked = true;

                        // Set focus to password field if username is already filled
                        if (!string.IsNullOrEmpty(UsernameTextBox.Text))
                        {
                            PasswordBox.Focus(FocusState.Programmatic);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Just log for debugging, don't bother the user
                System.Diagnostics.Debug.WriteLine($"Error loading saved username: {ex.Message}");
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            AttemptLogin();
        }

        private void LoginField_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                AttemptLogin();
            }
        }

        private async void AttemptLogin()
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            // Show loading indicator or disable controls here if needed

            // Simulate network delay for better UX
            await Task.Delay(500);

            if (AuthenticateUser(username, password))
            {
                // Save credentials if "Remember Me" is checked
                if (RememberMeCheckBox.IsChecked == true)
                {
                    SaveCredentials(username);
                }
                else
                {
                    // Clear saved credentials if "Remember Me" is unchecked
                    ClearSavedCredentials();
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
            return (username == ValidUsername && password == ValidPassword) ||
                   (username == "user" && password == "user123");
        }

        private void SaveCredentials(string username)
        {
            try
            {
                var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
                settings.Values["RememberUsername"] = true;
                settings.Values["SavedUsername"] = username;

                // In a real app, you would use Windows Credential Locker for more security
                // PasswordVault vault = new PasswordVault();
                // vault.Add(new PasswordCredential("WinUIApp", username, password));
            }
            catch (Exception ex)
            {
                // Log error or handle credential saving failure
                System.Diagnostics.Debug.WriteLine($"Error saving credentials: {ex.Message}");
            }
        }

        private void ClearSavedCredentials()
        {
            try
            {
                var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
                settings.Values["RememberUsername"] = false;
                settings.Values.Remove("SavedUsername");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing credentials: {ex.Message}");
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
                Content = "Invalid username or password. Please try again.\n\nHint: Use 'admin/password123' or 'user/user123'",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}