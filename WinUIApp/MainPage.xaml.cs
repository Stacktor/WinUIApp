using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WinUIApp
{
    public sealed partial class MainPage : Page
    {
        private List<string> dummyHostnames = new List<string> { "localhost", "google.com", "microsoft.com" };

        public MainPage()
        {
            this.InitializeComponent();
            HostnameAutoSuggestBox.ItemsSource = dummyHostnames;
        }

        private string GetSelectedHostname()
        {
            return HostnameAutoSuggestBox.Text;
        }

        private void PingButton_Click(object sender, RoutedEventArgs e)
        {
            string hostname = GetSelectedHostname();
            if (string.IsNullOrEmpty(hostname))
            {
                ShowMessage("Error", "Please select or enter a hostname.");
                return;
            }
            ExecuteCommand($"ping {hostname}");
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

        private void TracertButton_Click(object sender, RoutedEventArgs e)
        {
            string hostname = GetSelectedHostname();
            if (string.IsNullOrEmpty(hostname))
            {
                ShowMessage("Error", "Please select or enter a hostname.");
                return;
            }
            ExecuteCommand($"tracert {hostname}");
        }

        private void QuserButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand("quser");
        }

        private async void ExecuteCommand(string command)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process proc = new Process
                {
                    StartInfo = psi
                };

                proc.Start();
                string output = await proc.StandardOutput.ReadToEndAsync();
                proc.WaitForExit();

                // Display output
                ShowMessage("Command Output", output);
            }
            catch (Exception ex)
            {
                ShowMessage("Error", $"Error executing command:\n{ex.Message}");
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
            // Handle query submission if needed
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
