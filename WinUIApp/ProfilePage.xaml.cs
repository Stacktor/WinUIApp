using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using WinRT.Interop;

namespace WinUIApp
{
    public sealed partial class ProfilePage : Page
    {
        private ApplicationDataContainer settings;

        public ProfilePage()
        {
            this.InitializeComponent();
            settings = ApplicationData.Current.LocalSettings;

            // Load user profile data
            LoadUserProfile();
        }

        private void LoadUserProfile()
        {
            try
            {
                // Load display name
                if (settings.Values.ContainsKey("UserDisplayName"))
                {
                    DisplayNameTextBox.Text = settings.Values["UserDisplayName"] as string;
                }

                // Load email
                if (settings.Values.ContainsKey("UserEmail"))
                {
                    EmailTextBox.Text = settings.Values["UserEmail"] as string;
                }

                // Load remember login preference
                if (settings.Values.ContainsKey("RememberLogin"))
                {
                    RememberLoginToggle.IsOn = Convert.ToBoolean(settings.Values["RememberLogin"]);
                }

                // Load dashboard preferences
                if (settings.Values.ContainsKey("ShowSystemInfo"))
                {
                    ShowSystemInfoToggle.IsOn = Convert.ToBoolean(settings.Values["ShowSystemInfo"]);
                }

                if (settings.Values.ContainsKey("ShowNetworkStatus"))
                {
                    ShowNetworkStatusToggle.IsOn = Convert.ToBoolean(settings.Values["ShowNetworkStatus"]);
                }

                if (settings.Values.ContainsKey("ShowCommandHistory"))
                {
                    ShowCommandHistoryToggle.IsOn = Convert.ToBoolean(settings.Values["ShowCommandHistory"]);
                }

                if (settings.Values.ContainsKey("ShowFavorites"))
                {
                    ShowFavoritesToggle.IsOn = Convert.ToBoolean(settings.Values["ShowFavorites"]);
                }

                // Load default command preference
                if (settings.Values.ContainsKey("DefaultCommand"))
                {
                    string defaultCommand = settings.Values["DefaultCommand"] as string;
                    for (int i = 0; i < DefaultCommandComboBox.Items.Count; i++)
                    {
                        if (DefaultCommandComboBox.Items[i] is ComboBoxItem item &&
                            item.Tag as string == defaultCommand)
                        {
                            DefaultCommandComboBox.SelectedIndex = i;
                            break;
                        }
                    }
                }

                // Try to load profile picture if it exists
                LoadProfilePicture();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user profile: {ex.Message}");
            }
        }

        private async void LoadProfilePicture()
        {
            try
            {
                // Check if profile picture path is saved
                if (settings.Values.ContainsKey("ProfilePicturePath"))
                {
                    string path = settings.Values["ProfilePicturePath"] as string;
                    StorageFile file = await StorageFile.GetFileFromPathAsync(path);

                    if (file != null)
                    {
                        // Load profile picture
                        using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
                        {
                            BitmapImage bitmapImage = new BitmapImage();
                            await bitmapImage.SetSourceAsync(stream);
                            ProfilePicture.ProfilePicture = bitmapImage;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading profile picture: {ex.Message}");
                // If unable to load profile picture, use display name initials
                ProfilePicture.DisplayName = DisplayNameTextBox.Text;
            }
        }

        private async void ChangePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            // Create file picker
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            // Initialize file picker with app window
            var window = App.GetMainWindow();
            var hwnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(openPicker, hwnd);

            // Pick file
            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                try
                {
                    // Save the file path
                    settings.Values["ProfilePicturePath"] = file.Path;

                    // Load the image
                    using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        await bitmapImage.SetSourceAsync(stream);
                        ProfilePicture.ProfilePicture = bitmapImage;
                    }
                }
                catch (Exception ex)
                {
                    await ShowMessageDialogAsync("Error", $"Failed to set profile picture: {ex.Message}");
                }
            }
        }

        private async void UpdateProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(DisplayNameTextBox.Text))
            {
                await ShowMessageDialogAsync("Error", "Display name cannot be empty.");
                return;
            }

            // Save user profile data
            settings.Values["UserDisplayName"] = DisplayNameTextBox.Text;
            settings.Values["UserEmail"] = EmailTextBox.Text;

            // Update profile picture display name
            ProfilePicture.DisplayName = DisplayNameTextBox.Text;

            await ShowMessageDialogAsync("Success", "Profile updated successfully.");
        }

        private async void UpdatePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(CurrentPasswordBox.Password))
            {
                await ShowMessageDialogAsync("Error", "Current password cannot be empty.");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPasswordBox.Password))
            {
                await ShowMessageDialogAsync("Error", "New password cannot be empty.");
                return;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                await ShowMessageDialogAsync("Error", "New password and confirmation do not match.");
                return;
            }

            // Validate current password (simplified for demo)
            if (CurrentPasswordBox.Password != "password123")
            {
                await ShowMessageDialogAsync("Error", "Current password is incorrect.");
                return;
            }

            // In a real app, this would securely update the password in a database
            // For this demo, we just show a success message

            // Clear password fields
            CurrentPasswordBox.Password = string.Empty;
            NewPasswordBox.Password = string.Empty;
            ConfirmPasswordBox.Password = string.Empty;

            await ShowMessageDialogAsync("Success", "Password updated successfully.");
        }

        private void RememberLoginToggle_Toggled(object sender, RoutedEventArgs e)
        {
            settings.Values["RememberLogin"] = RememberLoginToggle.IsOn;
        }

        private void DashboardPrefToggle_Toggled(object sender, RoutedEventArgs e)
        {
            // Save individual dashboard panel preferences
            settings.Values["ShowSystemInfo"] = ShowSystemInfoToggle.IsOn;
            settings.Values["ShowNetworkStatus"] = ShowNetworkStatusToggle.IsOn;
            settings.Values["ShowCommandHistory"] = ShowCommandHistoryToggle.IsOn;
            settings.Values["ShowFavorites"] = ShowFavoritesToggle.IsOn;
        }

        private void DefaultCommandComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DefaultCommandComboBox.SelectedItem is ComboBoxItem item)
            {
                settings.Values["DefaultCommand"] = item.Tag as string;
            }
        }

        private async void SavePreferencesButton_Click(object sender, RoutedEventArgs e)
        {
            // All preferences are already saved on toggle/selection change
            await ShowMessageDialogAsync("Success", "Preferences saved successfully.");
        }

        private async void ClearCommandHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Clear Command History",
                Content = "Are you sure you want to clear your command history? This action cannot be undone.",
                PrimaryButtonText = "Clear History",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Clear command history
                if (settings.Containers.ContainsKey("CommandHistory"))
                {
                    settings.DeleteContainer("CommandHistory");
                }

                await ShowMessageDialogAsync("Success", "Command history cleared successfully.");
            }
        }

        private async void ClearFavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Clear Favorite Hosts",
                Content = "Are you sure you want to clear all your favorite hosts? This action cannot be undone.",
                PrimaryButtonText = "Clear Favorites",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Clear favorites
                settings.Values.Remove("FavoriteHostnames");

                await ShowMessageDialogAsync("Success", "Favorite hosts cleared successfully.");
            }
        }

        private async void ResetPreferencesButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Reset Preferences",
                Content = "Are you sure you want to reset all preferences to their default values? This action cannot be undone.",
                PrimaryButtonText = "Reset Preferences",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Reset dashboard preferences
                settings.Values["ShowSystemInfo"] = true;
                settings.Values["ShowNetworkStatus"] = true;
                settings.Values["ShowCommandHistory"] = true;
                settings.Values["ShowFavorites"] = true;

                // Reset default command
                settings.Values["DefaultCommand"] = "ping google.com";

                // Update UI
                ShowSystemInfoToggle.IsOn = true;
                ShowNetworkStatusToggle.IsOn = true;
                ShowCommandHistoryToggle.IsOn = true;
                ShowFavoritesToggle.IsOn = true;
                DefaultCommandComboBox.SelectedIndex = 0;

                await ShowMessageDialogAsync("Success", "Preferences reset successfully.");
            }
        }

        private async void ExportSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create file picker
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("JSON Files", new[] { ".json" });
                savePicker.SuggestedFileName = "NetworkToolsSettings";

                // Initialize file picker with app window
                var window = App.GetMainWindow();
                var hwnd = WindowNative.GetWindowHandle(window);
                InitializeWithWindow.Initialize(savePicker, hwnd);

                // Pick file
                StorageFile file = await savePicker.PickSaveFileAsync();

                if (file != null)
                {
                    // Create settings JSON (simplified implementation)
                    var settingsDict = new Dictionary<string, object>();

                    foreach (var key in settings.Values.Keys)
                    {
                        settingsDict[key] = settings.Values[key];
                    }

                    // Serialize settings to JSON
                    string json = System.Text.Json.JsonSerializer.Serialize(settingsDict);

                    // Write to file
                    await FileIO.WriteTextAsync(file, json);

                    await ShowMessageDialogAsync("Success", "Settings exported successfully.");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageDialogAsync("Error", $"Failed to export settings: {ex.Message}");
            }
        }

        private async void ImportSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create file picker
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.ViewMode = PickerViewMode.List;
                openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                openPicker.FileTypeFilter.Add(".json");

                // Initialize file picker with app window
                var window = App.GetMainWindow();
                var hwnd = WindowNative.GetWindowHandle(window);
                InitializeWithWindow.Initialize(openPicker, hwnd);

                // Pick file
                StorageFile file = await openPicker.PickSingleFileAsync();

                if (file != null)
                {
                    // Read JSON
                    string json = await FileIO.ReadTextAsync(file);

                    // Deserialize settings
                    var importedSettings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                    // Apply settings
                    foreach (var kvp in importedSettings)
                    {
                        settings.Values[kvp.Key] = kvp.Value;
                    }

                    // Reload user profile to reflect changes
                    LoadUserProfile();

                    await ShowMessageDialogAsync("Success", "Settings imported successfully.");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageDialogAsync("Error", $"Failed to import settings: {ex.Message}");
            }
        }

        private async void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Delete Account",
                Content = "Are you sure you want to delete your account? This action cannot be undone and all your data will be permanently lost.",
                PrimaryButtonText = "Delete Account",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // In a real app, this would delete the user account from the database
                // For this demo, we just clear local settings and navigate to login page

                // Clear all settings
                settings.Values.Clear();

                // Navigate to login page
                Frame rootFrame = App.GetMainWindow().Content as Frame;
                rootFrame?.Navigate(typeof(LoginPage));
            }
        }

        private async Task ShowMessageDialogAsync(string title, string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}