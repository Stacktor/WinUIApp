using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace WinUIApp
{
    public sealed partial class MainPage : Page
    {
        private List<string> dummyHostnames = new List<string> {
            "localhost",
            "google.com",
            "microsoft.com",
            "example.com"
        };

        public MainPage()
        {
            this.InitializeComponent();
            HostnameAutoSuggestBox.ItemsSource = dummyHostnames;
        }

        private string GetSelectedHostname()
        {
            return HostnameAutoSuggestBox.Text?.Trim();
        }

        private bool ValidateHostname(string hostname)
        {
            // Basic hostname validation
            if (string.IsNullOrWhiteSpace(hostname))
                return false;

            // Regex for valid hostname/IP
            var hostnameRegex = new Regex(@"^(localhost|[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$");
            var ipRegex = new Regex(@"^(\d{1,3}\.){3}\d{1,3}$");

            return hostnameRegex.IsMatch(hostname) || ipRegex.IsMatch(hostname);
        }

        private void PingButton_Click(object sender, RoutedEventArgs e)
        {
            string hostname = GetSelectedHostname();
            if (string.IsNullOrEmpty(hostname) || !ValidateHostname(hostname))
            {
                ShowMessage("Error", "Please enter a valid hostname or IP address.");
                return;
            }
            ExecuteCommand($"ping -n 4 {EscapeShellArgument(hostname)}");
        }

        private void TracertButton_Click(object sender, RoutedEventArgs e)
        {
            string hostname = GetSelectedHostname();
            if (string.IsNullOrEmpty(hostname) || !ValidateHostname(hostname))
            {
                ShowMessage("Error", "Please enter a valid hostname or IP address.");
                return;
            }
            ExecuteCommand($"tracert {EscapeShellArgument(hostname)}");
        }

        private void QuserButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand("quser");
        }

        private string EscapeShellArgument(string arg)
        {
            // Basic shell argument escaping to prevent command injection
            return arg.Replace("&", "").Replace("|", "").Replace(";", "").Replace("(", "").Replace(")", "");
        }

        private async void ExecuteCommand(string command)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process proc = new Process { StartInfo = psi })
                {
                    proc.Start();

                    string output = await proc.StandardOutput.ReadToEndAsync();
                    string error = await proc.StandardError.ReadToEndAsync();

                    proc.WaitForExit();

                    // Display output or error
                    string displayText = !string.IsNullOrWhiteSpace(output) ? output :
                                         !string.IsNullOrWhiteSpace(error) ? "Error: " + error :
                                         "Command executed with no output.";

                    ShowMessage("Command Output", displayText);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error", $"Error executing command:\n{ex.Message}");
            }
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ActualTheme == ElementTheme.Light)
            {
                this.RequestedTheme = ElementTheme.Dark;
            }
            else
            {
                this.RequestedTheme = ElementTheme.Light;
            }
        }

        private async void ShowMessage(string title, string content)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private void HostnameAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            // Optional: Add custom query submission logic if needed
        }

        private void HostnameAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var suggestions = dummyHostnames.FindAll(h => h.StartsWith(sender.Text, StringComparison.OrdinalIgnoreCase));
                sender.ItemsSource = suggestions;
            }
        }
    }
}