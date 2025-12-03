using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading.Tasks;
using WinUIApp.Utilities;

namespace WinUIApp
{
    /// <summary>
    /// Login page providing user authentication functionality
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        #region Constants

        // Dummy hardcoded credentials for demonstration - use AppConstants
        private const string VALID_USERNAME = AppConstants.DefaultUsername;
        private const string VALID_PASSWORD = AppConstants.DefaultPassword;
        private const string ALT_USERNAME = AppConstants.AltUsername;
        private const string ALT_PASSWORD = AppConstants.AltPassword;

        #endregion

        #region Fields

        private bool _isLoggingIn = false;

        #endregion

        #region Constructor

        public LoginPage()
        {
            this.InitializeComponent();
            LoadSavedUsernameAsync();
            AppLogger.LogInfo("LoginPage initialized", "LoginPage");
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Loads saved username from local settings asynchronously
        /// </summary>
        private async void LoadSavedUsernameAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    if (settings.Values.ContainsKey(AppConstants.SettingsKeyRememberUsername) &&
                        settings.Values[AppConstants.SettingsKeyRememberUsername] is bool remember &&
                        remember)
                    {
                        if (settings.Values.ContainsKey(AppConstants.SettingsKeySavedUsername))
                        {
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                UsernameTextBox.Text = settings.Values[AppConstants.SettingsKeySavedUsername] as string;
                                RememberMeCheckBox.IsChecked = true;

                                // Set focus to password field if username is already filled
                                if (!string.IsNullOrEmpty(UsernameTextBox.Text))
                                {
                                    PasswordBox.Focus(FocusState.Programmatic);
                                }
                            });
                        }
                    }
                });

                AppLogger.LogInfo("Saved username loaded successfully", "LoginPage");
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Failed to load saved username", ex, "LoginPage");
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles login button click event
        /// </summary>
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            AttemptLoginAsync();
        }

        /// <summary>
        /// Handles Enter key press in login fields
        /// </summary>
        private void LoginField_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                AttemptLoginAsync();
            }
        }

        #endregion

        #region Authentication

        /// <summary>
        /// Attempts to authenticate the user with provided credentials
        /// </summary>
        private async void AttemptLoginAsync()
        {
            // Prevent double-clicks
            if (_isLoggingIn)
            {
                AppLogger.LogWarning("Login attempt blocked - already in progress", "LoginPage");
                return;
            }

            string username = UsernameTextBox.Text?.Trim() ?? string.Empty;
            string password = PasswordBox.Password ?? string.Empty;

            // Validate input
            if (!ValidateLoginInput(username, password))
            {
                return;
            }

            try
            {
                _isLoggingIn = true;
                SetLoginControlsEnabled(false);
                AppLogger.LogInfo($"Login attempt for user: {username}", "LoginPage");

                // Simulate network delay for better UX
                await Task.Delay(500);

                if (AuthenticateUser(username, password))
                {
                    AppLogger.LogInfo($"Login successful for user: {username}", "LoginPage");

                    // Save credentials if "Remember Me" is checked
                    if (RememberMeCheckBox.IsChecked == true)
                    {
                        await SaveCredentialsAsync(username);
                    }
                    else
                    {
                        await ClearSavedCredentialsAsync();
                    }

                    // Navigate to MainHub
                    this.Frame.Navigate(typeof(MainHub));
                }
                else
                {
                    AppLogger.LogWarning($"Login failed for user: {username}", "LoginPage");
                    await ShowLoginFailedDialogAsync();
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Unexpected error during login", ex, "LoginPage");
                await ShowErrorDialogAsync("An unexpected error occurred. Please try again.");
            }
            finally
            {
                _isLoggingIn = false;
                SetLoginControlsEnabled(true);
            }
        }

        /// <summary>
        /// Validates login input fields
        /// </summary>
        private bool ValidateLoginInput(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                AppLogger.LogWarning("Login validation failed - empty username", "LoginPage");
                _ = ShowErrorDialogAsync("Please enter a username.");
                UsernameTextBox.Focus(FocusState.Programmatic);
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                AppLogger.LogWarning("Login validation failed - empty password", "LoginPage");
                _ = ShowErrorDialogAsync("Please enter a password.");
                PasswordBox.Focus(FocusState.Programmatic);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Authenticates user credentials
        /// </summary>
        /// <remarks>
        /// In production, this should call a secure authentication service
        /// </remarks>
        private bool AuthenticateUser(string username, string password)
        {
            // Simple authentication - replace with proper authentication in a real app
            return (username == VALID_USERNAME && password == VALID_PASSWORD) ||
                   (username == ALT_USERNAME && password == ALT_PASSWORD);
        }

        #endregion

        #region Credential Management

        /// <summary>
        /// Saves user credentials to local settings asynchronously
        /// </summary>
        private async Task SaveCredentialsAsync(string username)
        {
            try
            {
                await Task.Run(() =>
                {
                    var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    settings.Values[AppConstants.SettingsKeyRememberUsername] = true;
                    settings.Values[AppConstants.SettingsKeySavedUsername] = username;

                    // In a real app, you would use Windows Credential Locker for more security
                    // PasswordVault vault = new PasswordVault();
                    // vault.Add(new PasswordCredential(AppConstants.AppName, username, password));
                });

                AppLogger.LogInfo("Credentials saved successfully", "LoginPage");
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Failed to save credentials", ex, "LoginPage");
            }
        }

        /// <summary>
        /// Clears saved user credentials asynchronously
        /// </summary>
        private async Task ClearSavedCredentialsAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    settings.Values[AppConstants.SettingsKeyRememberUsername] = false;
                    settings.Values.Remove(AppConstants.SettingsKeySavedUsername);
                });

                AppLogger.LogInfo("Credentials cleared successfully", "LoginPage");
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Failed to clear credentials", ex, "LoginPage");
            }
        }

        #endregion

        #region UI Helpers

        /// <summary>
        /// Enables or disables login controls
        /// </summary>
        private void SetLoginControlsEnabled(bool isEnabled)
        {
            UsernameTextBox.IsEnabled = isEnabled;
            PasswordBox.IsEnabled = isEnabled;
            RememberMeCheckBox.IsEnabled = isEnabled;
            LoginButton.IsEnabled = isEnabled;

            // Visual feedback
            LoginButton.Content = isEnabled ? "Login" : "Logging in...";
        }

        /// <summary>
        /// Shows login failed dialog
        /// </summary>
        private async Task ShowLoginFailedDialogAsync()
        {
            var dialog = new ContentDialog
            {
                Title = "Login Failed",
                Content = "Invalid username or password. Please try again.\n\nHint: Use 'admin/password123' or 'user/user123'",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();

            // Set focus back to username field
            UsernameTextBox.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// Shows generic error dialog
        /// </summary>
        private async Task ShowErrorDialogAsync(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        #endregion
    }
}