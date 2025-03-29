using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace WinUIApp
{
    public sealed partial class MainPage : Page
    {
        private List<string> dummyHostnames = new List<string>
        {
            "this-pc",
            "localhost",
            "google.com",
            "microsoft.com",
            "example.com",
            "github.com",
            "stackoverflow.com"
        };

        private List<string> favoriteHostnames = new List<string>();
        private string currentCommand = string.Empty;
        private bool isCommandRunning = false;

        public MainPage()
        {
            this.InitializeComponent();

            LoadFavorites();
            UpdateHostnameSuggestions();

            // Set initial theme state
            ThemeToggleButton.IsChecked = this.ActualTheme == ElementTheme.Dark;
        }

        private void LoadFavorites()
        {
            try
            {
                var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
                if (settings.Values.ContainsKey("FavoriteHostnames"))
                {
                    var favoritesString = settings.Values["FavoriteHostnames"] as string;
                    if (!string.IsNullOrEmpty(favoritesString))
                    {
                        favoriteHostnames = new List<string>(favoritesString.Split(','));
                    }
                }
            }
            catch (Exception ex)
            {
                // Just log the error, don't interrupt app startup
                System.Diagnostics.Debug.WriteLine($"Error loading favorites: {ex.Message}");
            }
        }

        private void SaveFavorites()
        {
            try
            {
                var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
                settings.Values["FavoriteHostnames"] = string.Join(",", favoriteHostnames);
            }
            catch (Exception ex)
            {
                ShowMessage("Error", $"Failed to save favorites: {ex.Message}");
            }
        }

        private void UpdateHostnameSuggestions()
        {
            var allSuggestions = new List<string>();
            allSuggestions.AddRange(favoriteHostnames);

            // Add non-duplicate dummy hostnames
            foreach (var hostname in dummyHostnames)
            {
                if (!allSuggestions.Contains(hostname))
                {
                    allSuggestions.Add(hostname);
                }
            }

            HostnameAutoSuggestBox.ItemsSource = allSuggestions;
        }

        private string GetSelectedHostname()
        {
            string hostname = HostnameAutoSuggestBox.Text?.Trim();

            // Replace "this-pc" with empty string for local commands
            if (hostname?.Equals("this-pc", StringComparison.OrdinalIgnoreCase) == true)
            {
                // For certain commands that need a specific local identifier
                // We'll handle this special case in the individual command methods
            }

            return hostname;
        }

        private bool ValidateHostname(string hostname)
        {
            // Basic hostname validation
            if (string.IsNullOrWhiteSpace(hostname))
                return false;

            // Allow "this-pc" as a special case
            if (hostname.Equals("this-pc", StringComparison.OrdinalIgnoreCase))
                return true;

            // Allow Windows computer names (alphanumeric with hyphens, e.g., olw10cl480)
            var computerNameRegex = new Regex(@"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?$");

            // Regex for valid hostname/IP
            var hostnameRegex = new Regex(@"^(localhost|[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$");
            var ipRegex = new Regex(@"^(\d{1,3}\.){3}\d{1,3}$");

            return computerNameRegex.IsMatch(hostname) || hostnameRegex.IsMatch(hostname) || ipRegex.IsMatch(hostname);
        }

        #region Command Click Handlers

        private void PingButton_Click(object sender, RoutedEventArgs e)
        {
            string hostname = GetSelectedHostname();
            if (string.IsNullOrEmpty(hostname) || !ValidateHostname(hostname))
            {
                ShowMessage("Error", "Please enter a valid hostname or IP address.");
                return;
            }

            ExecuteCommandInternal($"ping -n 4 {EscapeShellArgument(hostname)}");
        }

        private void TracertButton_Click(object sender, RoutedEventArgs e)
        {
            string hostname = GetSelectedHostname();
            if (string.IsNullOrEmpty(hostname) || !ValidateHostname(hostname))
            {
                ShowMessage("Error", "Please enter a valid hostname or IP address.");
                return;
            }

            ExecuteCommandRealtimeInternal($"tracert {EscapeShellArgument(hostname)}");
        }

        private void QuserButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommandInternal("quser");
        }

        private void IpconfigButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommandInternal("ipconfig /all");
        }

        private void NslookupButton_Click(object sender, RoutedEventArgs e)
        {
            string hostname = GetSelectedHostname();
            if (string.IsNullOrEmpty(hostname) || !ValidateHostname(hostname))
            {
                ShowMessage("Error", "Please enter a valid hostname or IP address.");
                return;
            }

            ExecuteCommandInternal($"nslookup {EscapeShellArgument(hostname)}");
        }

        private void NetstatButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommandInternal("netstat -an");
        }

        private void SystemInfoButton_Click(object sender, RoutedEventArgs e)
        {
            string hostname = GetSelectedHostname();

            if (string.IsNullOrEmpty(hostname) || !ValidateHostname(hostname))
            {
                ShowMessage("Error", "Please enter a valid hostname or IP address.");
                return;
            }

            // For local machine
            if (hostname.Equals("this-pc", StringComparison.OrdinalIgnoreCase) ||
                hostname.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                hostname.Equals("127.0.0.1"))
            {
                ExecuteCommandInternal("systeminfo");
            }
            else
            {
                // For remote machine
                ExecuteCommandInternal($"systeminfo /S {EscapeShellArgument(hostname)}");
            }
        }

        private void TasklistButton_Click(object sender, RoutedEventArgs e)
        {
            string hostname = GetSelectedHostname();

            if (string.IsNullOrEmpty(hostname) || !ValidateHostname(hostname))
            {
                ShowMessage("Error", "Please enter a valid hostname or IP address.");
                return;
            }

            // For local machine
            if (hostname.Equals("this-pc", StringComparison.OrdinalIgnoreCase) ||
                hostname.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                hostname.Equals("127.0.0.1"))
            {
                ExecuteCommandInternal("tasklist");
            }
            else
            {
                // For remote machine
                ExecuteCommandInternal($"tasklist /S {EscapeShellArgument(hostname)}");
            }
        }

        #endregion

        #region UI Action Handlers

        private void AddToFavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            string hostname = GetSelectedHostname();
            if (string.IsNullOrEmpty(hostname) || !ValidateHostname(hostname))
            {
                ShowMessage("Error", "Please enter a valid hostname to add to favorites.");
                return;
            }

            if (!favoriteHostnames.Contains(hostname))
            {
                favoriteHostnames.Add(hostname);
                SaveFavorites();
                UpdateHostnameSuggestions();
                StatusTextBlock.Text = $"Added {hostname} to favorites";
            }
            else
            {
                StatusTextBlock.Text = $"{hostname} is already in favorites";
            }
        }

        private void CopyOutputButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ResultsTextBox.Text))
            {
                StatusTextBlock.Text = "No output to copy";
                return;
            }

            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(ResultsTextBox.Text);
            Clipboard.SetContent(dataPackage);

            StatusTextBlock.Text = "Output copied to clipboard";
        }

        private async void SaveOutputButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ResultsTextBox.Text))
            {
                StatusTextBlock.Text = "No output to save";
                return;
            }

            try
            {
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("Text Files", new List<string>() { ".txt" });

                // Format a default filename based on the command and hostname
                string defaultFilename = $"{currentCommand.Split(' ')[0]}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                savePicker.SuggestedFileName = defaultFilename;

                // Initialize the file picker with the window handle
                var window = App.GetMainWindow();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    await FileIO.WriteTextAsync(file, ResultsTextBox.Text);
                    StatusTextBlock.Text = $"Output saved to {file.Name}";
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error", $"Failed to save output: {ex.Message}");
            }
        }

        private void ClearOutputButton_Click(object sender, RoutedEventArgs e)
        {
            ResultsTextBox.Text = string.Empty;
            StatusTextBlock.Text = "Output cleared";
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (ThemeToggleButton.IsChecked == true)
            {
                this.RequestedTheme = ElementTheme.Dark;
            }
            else
            {
                this.RequestedTheme = ElementTheme.Light;
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            string helpText =
                "Network Tool Commands Help:\n\n" +
                "• Ping - Tests connectivity to a host\n" +
                "• Tracert - Traces the route to a host\n" +
                "• Quser - Displays logged-on users\n" +
                "• Ipconfig - Displays network configuration\n" +
                "• Nslookup - Queries DNS records\n" +
                "• Netstat - Shows network connections\n" +
                "• System Info - Displays system information\n" +
                "• Task List - Shows running processes\n\n" +
                "Enter a hostname or IP address in the text box for destination-specific commands.";

            ShowMessage("Help", helpText);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to login page
            this.Frame.Navigate(typeof(LoginPage));
        }

        #endregion

        #region Command Execution

        // Public method to allow calling from other pages
        public void ExecuteCommand(string command)
        {
            // For debugging: print to debug output to ensure this method is being called
            System.Diagnostics.Debug.WriteLine($"MainPage.ExecuteCommand called with: {command}");

            // You can add your command parsing logic here
            if (command.StartsWith("ping"))
            {
                var match = Regex.Match(command, @"ping\s+(.+)");
                if (match.Success && match.Groups.Count > 1)
                {
                    HostnameAutoSuggestBox.Text = match.Groups[1].Value.Trim();
                }
                PingButton_Click(null, null);
            }
            else if (command.StartsWith("tracert"))
            {
                var match = Regex.Match(command, @"tracert\s+(.+)");
                if (match.Success && match.Groups.Count > 1)
                {
                    HostnameAutoSuggestBox.Text = match.Groups[1].Value.Trim();
                }
                TracertButton_Click(null, null);
            }
            else if (command.StartsWith("ipconfig"))
            {
                IpconfigButton_Click(null, null);
            }
            else if (command.StartsWith("nslookup"))
            {
                var match = Regex.Match(command, @"nslookup\s+(.+)");
                if (match.Success && match.Groups.Count > 1)
                {
                    HostnameAutoSuggestBox.Text = match.Groups[1].Value.Trim();
                }
                NslookupButton_Click(null, null);
            }
            else if (command.StartsWith("netstat"))
            {
                NetstatButton_Click(null, null);
            }
            else if (command.StartsWith("systeminfo"))
            {
                SystemInfoButton_Click(null, null);
            }
            else if (command.StartsWith("tasklist"))
            {
                TasklistButton_Click(null, null);
            }
            else
            {
                // Default to showing command in the results text box
                ExecuteCommandInternal(command);
            }
        }

        private string EscapeShellArgument(string arg)
        {
            // Basic shell argument escaping to prevent command injection
            return arg.Replace("&", "").Replace("|", "").Replace(";", "").Replace("(", "").Replace(")", "").Replace("<", "").Replace(">", "");
        }

        private async void ExecuteCommandInternal(string command)
        {
            if (isCommandRunning)
            {
                ShowMessage("Warning", "A command is already running. Please wait for it to complete.");
                return;
            }

            try
            {
                isCommandRunning = true;
                currentCommand = command;
                StatusTextBlock.Text = $"Running: {command}";

                // Clear previous results
                ResultsTextBox.Text = string.Empty;

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

                    // Start reading output asynchronously
                    var outputTask = proc.StandardOutput.ReadToEndAsync();
                    var errorTask = proc.StandardError.ReadToEndAsync();

                    // Display output in chunks as it becomes available
                    while (!proc.HasExited || !outputTask.IsCompleted || !errorTask.IsCompleted)
                    {
                        await Task.Delay(100); // Small delay to avoid UI freezing
                    }

                    string output = await outputTask;
                    string error = await errorTask;

                    // Display output or error
                    string displayText = !string.IsNullOrWhiteSpace(output) ? output :
                                         !string.IsNullOrWhiteSpace(error) ? "Error: " + error :
                                         "Command executed with no output.";

                    ResultsTextBox.Text = displayText;
                    StatusTextBlock.Text = $"Completed: {command}";
                }
            }
            catch (Exception ex)
            {
                ResultsTextBox.Text = $"Error executing command:\n{ex.Message}";
                StatusTextBlock.Text = "Error executing command";
            }
            finally
            {
                isCommandRunning = false;
            }
        }

        private async void ExecuteCommandRealtimeInternal(string command)
        {
            if (isCommandRunning)
            {
                ShowMessage("Warning", "A command is already running. Please wait for it to complete.");
                return;
            }

            try
            {
                isCommandRunning = true;
                currentCommand = command;
                StatusTextBlock.Text = $"Running: {command}";

                // Clear previous results
                ResultsTextBox.Text = string.Empty;

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

                    // Set up event handlers for real-time output
                    StringBuilder outputBuilder = new StringBuilder();

                    proc.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            _ = this.DispatcherQueue.TryEnqueue(() =>
                            {
                                outputBuilder.AppendLine(e.Data);
                                ResultsTextBox.Text = outputBuilder.ToString();
                                ResultsTextBox.SelectionStart = ResultsTextBox.Text.Length;
                                ResultsTextBox.SelectionLength = 0;
                            });
                        }
                    };

                    proc.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            _ = this.DispatcherQueue.TryEnqueue(() =>
                            {
                                outputBuilder.AppendLine("Error: " + e.Data);
                                ResultsTextBox.Text = outputBuilder.ToString();
                            });
                        }
                    };

                    // Begin asynchronous reading
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();

                    // Wait for the process to exit
                    await Task.Run(() => proc.WaitForExit());

                    // Update status
                    _ = this.DispatcherQueue.TryEnqueue(() =>
                    {
                        StatusTextBlock.Text = $"Completed: {command}";
                    });
                }
            }
            catch (Exception ex)
            {
                ResultsTextBox.Text = $"Error executing command:\n{ex.Message}";
                StatusTextBlock.Text = "Error executing command";
            }
            finally
            {
                isCommandRunning = false;
            }
        }

        #endregion

        #region Helper Methods

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

        #endregion

        #region AutoSuggestBox Event Handlers

        private void HostnameAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            // Auto-submit to run ping on query submission
            if (!string.IsNullOrEmpty(args.QueryText) && ValidateHostname(args.QueryText))
            {
                PingButton_Click(sender, null);
            }
        }

        private void HostnameAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var allSuggestions = new List<string>();
                allSuggestions.AddRange(favoriteHostnames);

                foreach (var hostname in dummyHostnames)
                {
                    if (!allSuggestions.Contains(hostname))
                    {
                        allSuggestions.Add(hostname);
                    }
                }

                var filteredSuggestions = allSuggestions.FindAll(h =>
                    h.StartsWith(sender.Text, StringComparison.OrdinalIgnoreCase) ||
                    h.Contains(sender.Text, StringComparison.OrdinalIgnoreCase));

                sender.ItemsSource = filteredSuggestions;
            }
        }

        #endregion
    }
}