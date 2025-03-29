using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace WinUIApp
{
    public sealed partial class SettingsPage : Page
    {
        private ApplicationDataContainer settings;

        public SettingsPage()
        {
            this.InitializeComponent();
            settings = ApplicationData.Current.LocalSettings;

            // Load saved settings
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                // Load theme setting
                if (settings.Values.ContainsKey("AppTheme"))
                {
                    string themeName = settings.Values["AppTheme"] as string;
                    switch (themeName)
                    {
                        case "Light":
                            ThemeRadioButtons.SelectedIndex = 1;
                            break;
                        case "Dark":
                            ThemeRadioButtons.SelectedIndex = 2;
                            break;
                        default:
                            ThemeRadioButtons.SelectedIndex = 0; // System default
                            break;
                    }
                }

                // Load command timeout
                if (settings.Values.ContainsKey("CommandTimeout"))
                {
                    CommandTimeoutBox.Value = Convert.ToDouble(settings.Values["CommandTimeout"]);
                }

                // Load real-time output option
                if (settings.Values.ContainsKey("RealTimeOutput"))
                {
                    RealTimeOutputCheckBox.IsChecked = Convert.ToBoolean(settings.Values["RealTimeOutput"]);
                }

                // Load save history option
                if (settings.Values.ContainsKey("SaveHistory"))
                {
                    SaveHistoryCheckBox.IsChecked = Convert.ToBoolean(settings.Values["SaveHistory"]);
                }

                // Load ping count
                if (settings.Values.ContainsKey("PingCount"))
                {
                    PingCountBox.Value = Convert.ToDouble(settings.Values["PingCount"]);
                }

                // Load tracert hops
                if (settings.Values.ContainsKey("TracertHops"))
                {
                    TracertHopsBox.Value = Convert.ToDouble(settings.Values["TracertHops"]);
                }

                // Load resolve hostnames option
                if (settings.Values.ContainsKey("ResolveHostnames"))
                {
                    ResolveHostnamesCheckBox.IsChecked = Convert.ToBoolean(settings.Values["ResolveHostnames"]);
                }

                // Load font size
                if (settings.Values.ContainsKey("FontSize"))
                {
                    int fontSize = Convert.ToInt32(settings.Values["FontSize"]);
                    switch (fontSize)
                    {
                        case 12:
                            FontSizeComboBox.SelectedIndex = 0;
                            break;
                        case 16:
                            FontSizeComboBox.SelectedIndex = 2;
                            break;
                        case 18:
                            FontSizeComboBox.SelectedIndex = 3;
                            break;
                        default:
                            FontSizeComboBox.SelectedIndex = 1; // Default to medium (14)
                            break;
                    }
                }

                // Load wrap text option
                if (settings.Values.ContainsKey("WrapText"))
                {
                    WrapTextCheckBox.IsChecked = Convert.ToBoolean(settings.Values["WrapText"]);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        private void ThemeRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeRadioButtons.SelectedItem is RadioButton selectedButton)
            {
                string theme = selectedButton.Tag as string;
                settings.Values["AppTheme"] = theme;

                // Apply theme to current page
                switch (theme)
                {
                    case "Light":
                        this.RequestedTheme = ElementTheme.Light;
                        break;
                    case "Dark":
                        this.RequestedTheme = ElementTheme.Dark;
                        break;
                    default:
                        this.RequestedTheme = ElementTheme.Default;
                        break;
                }
            }
        }

        private void CommandTimeoutBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            settings.Values["CommandTimeout"] = sender.Value;
        }

        private void RealTimeOutputCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            settings.Values["RealTimeOutput"] = RealTimeOutputCheckBox.IsChecked;
        }

        private void SaveHistoryCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            settings.Values["SaveHistory"] = SaveHistoryCheckBox.IsChecked;
        }

        private void PingCountBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            settings.Values["PingCount"] = sender.Value;
        }

        private void TracertHopsBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            settings.Values["TracertHops"] = sender.Value;
        }

        private void ResolveHostnamesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            settings.Values["ResolveHostnames"] = ResolveHostnamesCheckBox.IsChecked;
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontSizeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                int fontSize = Convert.ToInt32(selectedItem.Tag);
                settings.Values["FontSize"] = fontSize;
            }
        }

        private void WrapTextCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            settings.Values["WrapText"] = WrapTextCheckBox.IsChecked;
        }

        private async void CheckUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowUpdateDialogAsync();
        }

        private async void ResetSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Reset Settings",
                Content = "Are you sure you want to reset all settings to their default values?",
                PrimaryButtonText = "Reset",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Clear all settings
                settings.Values.Clear();

                // Reset UI to defaults
                ThemeRadioButtons.SelectedIndex = 0;
                CommandTimeoutBox.Value = 30;
                RealTimeOutputCheckBox.IsChecked = true;
                SaveHistoryCheckBox.IsChecked = true;
                PingCountBox.Value = 4;
                TracertHopsBox.Value = 30;
                ResolveHostnamesCheckBox.IsChecked = true;
                FontSizeComboBox.SelectedIndex = 1;
                WrapTextCheckBox.IsChecked = false;

                // Save default values
                settings.Values["AppTheme"] = "Default";
                settings.Values["CommandTimeout"] = 30;
                settings.Values["RealTimeOutput"] = true;
                settings.Values["SaveHistory"] = true;
                settings.Values["PingCount"] = 4;
                settings.Values["TracertHops"] = 30;
                settings.Values["ResolveHostnames"] = true;
                settings.Values["FontSize"] = 14;
                settings.Values["WrapText"] = false;

                await ShowMessageDialogAsync("Settings Reset", "All settings have been reset to their default values.");
            }
        }

        private async Task ShowUpdateDialogAsync()
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Check for Updates",
                Content = "Your application is up to date.\nCurrent version: 1.0.5",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
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