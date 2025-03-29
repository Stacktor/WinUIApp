using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace WinUIApp
{
    public class CommandHistoryItem
    {
        public string Command { get; set; }
        public string Timestamp { get; set; }
        public bool Success { get; set; }
    }

    public sealed partial class DashboardPage : Page
    {
        // Bindable properties
        public string WelcomeMessage { get; private set; } = "Welcome to Network Tools";
        public string CurrentDateTime { get; private set; } = DateTime.Now.ToString("dddd, MMMM d, yyyy");

        // Observable collections for lists
        private ObservableCollection<CommandHistoryItem> RecentCommands { get; set; } = new ObservableCollection<CommandHistoryItem>();
        private ObservableCollection<string> FavoriteHosts { get; set; } = new ObservableCollection<string>();

        // HttpClient for network operations
        private readonly HttpClient httpClient = new HttpClient();

        // Flag to track if a command is running
        private bool isCommandRunning = false;

        public DashboardPage()
        {
            this.InitializeComponent();

            // Initialize welcome message
            UpdateWelcomeMessage();

            // Initialize data
            LoadRecentCommands();
            LoadFavoriteHosts();

            // Load initial status info
            LoadInitialSystemInfo();
            LoadInitialNetworkInfo();
        }

        private void UpdateWelcomeMessage()
        {
            var hour = DateTime.Now.Hour;

            if (hour >= 5 && hour < 12)
            {
                WelcomeMessage = "Good Morning!";
            }
            else if (hour >= 12 && hour < 18)
            {
                WelcomeMessage = "Good Afternoon!";
            }
            else
            {
                WelcomeMessage = "Good Evening!";
            }

            CurrentDateTime = DateTime.Now.ToString("dddd, MMMM d, yyyy");

            // Trigger property changed events
            Bindings.Update();
        }

        private void LoadRecentCommands()
        {
            // Add sample data for design purposes - in a real app, load from storage
            RecentCommands.Clear();
            RecentCommands.Add(new CommandHistoryItem
            {
                Command = "ping google.com",
                Timestamp = "Today, 10:45 AM",
                Success = true
            });

            RecentCommands.Add(new CommandHistoryItem
            {
                Command = "ipconfig /all",
                Timestamp = "Today, 9:30 AM",
                Success = true
            });

            RecentCommands.Add(new CommandHistoryItem
            {
                Command = "tracert microsoft.com",
                Timestamp = "Yesterday, 4:15 PM",
                Success = true
            });

            // Bind to the ListView
            RecentCommandsListView.ItemsSource = RecentCommands;
        }

        private void LoadFavoriteHosts()
        {
            // In a real app, load from saved settings
            FavoriteHosts.Clear();

            try
            {
                var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
                if (settings.Values.ContainsKey("FavoriteHostnames"))
                {
                    var favoritesString = settings.Values["FavoriteHostnames"] as string;
                    if (!string.IsNullOrEmpty(favoritesString))
                    {
                        foreach (var hostname in favoritesString.Split(','))
                        {
                            FavoriteHosts.Add(hostname);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If settings can't be loaded, use defaults
            }

            // Add some defaults if empty
            if (FavoriteHosts.Count == 0)
            {
                FavoriteHosts.Add("google.com");
                FavoriteHosts.Add("microsoft.com");
                FavoriteHosts.Add("this-pc");
            }

            // Bind to the ListView
            FavoriteHostsListView.ItemsSource = FavoriteHosts;
        }

        private void LoadInitialSystemInfo()
        {
            // Get basic system information to display
            try
            {
                // Computer name
                ComputerNameTextBlock.Text = Environment.MachineName;

                // OS version - simple implementation
                OsVersionTextBlock.Text = Environment.OSVersion.ToString();

                // Get processor info
                ProcessStartInfo psi = new ProcessStartInfo("wmic", "cpu get name")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process proc = new Process { StartInfo = psi })
                {
                    proc.Start();
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();

                    // Basic parsing - in real app would be more robust
                    var lines = output.Split('\n');
                    if (lines.Length >= 2)
                    {
                        ProcessorTextBlock.Text = lines[1].Trim();
                    }
                }

                // Get memory usage - dummy value for now
                UpdateMemoryUsage(65);
            }
            catch (Exception ex)
            {
                // Handle errors gracefully
                System.Diagnostics.Debug.WriteLine($"Error loading system info: {ex.Message}");
            }
        }

        private void UpdateMemoryUsage(double percentage)
        {
            MemoryUsageBar.Value = percentage;
            MemoryUsageTextBlock.Text = $"{percentage}%";
        }

        private void LoadInitialNetworkInfo()
        {
            // Default status
            InternetStatusTextBlock.Text = "Checking...";
            DefaultGatewayTextBlock.Text = "Loading...";
            DnsServersTextBlock.Text = "Loading...";
            PublicIpTextBlock.Text = "Loading...";

            // Start asynchronous network info loading
            _ = Task.Run(async () =>
            {
                await LoadNetworkInfoAsync();
            });
        }

        private async Task LoadNetworkInfoAsync()
        {
            try
            {
                // Internet connectivity check
                bool isConnected = await CheckInternetConnectivityAsync();

                await UpdateUIAsync(() =>
                {
                    if (isConnected)
                    {
                        InternetStatusTextBlock.Text = "Connected";
                        InternetStatusTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
                    }
                    else
                    {
                        InternetStatusTextBlock.Text = "Disconnected";
                        InternetStatusTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
                    }
                });

                // Get network configuration (ipconfig)
                await GetNetworkConfigurationAsync();

                // Get public IP address
                await GetPublicIpAddressAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading network info: {ex.Message}");

                await UpdateUIAsync(() =>
                {
                    InternetStatusTextBlock.Text = "Error checking status";
                    InternetStatusTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange);
                });
            }
        }

        private async Task<bool> CheckInternetConnectivityAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var response = await client.GetAsync("https://www.google.com");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private async Task GetNetworkConfigurationAsync()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("ipconfig", "/all")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process proc = new Process { StartInfo = psi })
                {
                    proc.Start();
                    string output = await proc.StandardOutput.ReadToEndAsync();
                    proc.WaitForExit();

                    // Extract default gateway
                    string gatewayInfo = ExtractNetworkInfo(output, "Default Gateway", ":");
                    // Extract DNS servers
                    string dnsInfo = ExtractNetworkInfo(output, "DNS Servers", ":");

                    await UpdateUIAsync(() =>
                    {
                        DefaultGatewayTextBlock.Text = string.IsNullOrEmpty(gatewayInfo) ? "Not found" : gatewayInfo;
                        DnsServersTextBlock.Text = string.IsNullOrEmpty(dnsInfo) ? "Not found" : dnsInfo;
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting network config: {ex.Message}");

                await UpdateUIAsync(() =>
                {
                    DefaultGatewayTextBlock.Text = "Error";
                    DnsServersTextBlock.Text = "Error";
                });
            }
        }

        private string ExtractNetworkInfo(string input, string searchText, string delimiter)
        {
            try
            {
                // Find the line containing the search text
                var lines = input.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains(searchText))
                    {
                        // Extract the value after the delimiter
                        int delimiterIndex = line.IndexOf(delimiter);
                        if (delimiterIndex != -1 && delimiterIndex < line.Length - 1)
                        {
                            return line.Substring(delimiterIndex + 1).Trim();
                        }
                    }
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task GetPublicIpAddressAsync()
        {
            try
            {
                // Use a public API to get the external IP address
                string response = await httpClient.GetStringAsync("https://api.ipify.org");

                await UpdateUIAsync(() =>
                {
                    PublicIpTextBlock.Text = response.Trim();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting public IP: {ex.Message}");

                await UpdateUIAsync(() =>
                {
                    PublicIpTextBlock.Text = "Could not determine";
                });
            }
        }

        #region Button Click Event Handlers

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateWelcomeMessage();
            LoadRecentCommands();
            LoadFavoriteHosts();
            LoadInitialSystemInfo();
            LoadInitialNetworkInfo();
        }

        private void RefreshNetworkStatus_Click(object sender, RoutedEventArgs e)
        {
            LoadInitialNetworkInfo();
        }

        private void RefreshSystemStatus_Click(object sender, RoutedEventArgs e)
        {
            LoadInitialSystemInfo();
        }

        private void QuickPingButton_Click(object sender, RoutedEventArgs e)
        {
            QuickCommandTextBox.Text = "ping google.com";
            ExecuteQuickCommand_Click(sender, e);
        }

        private void QuickSystemInfoButton_Click(object sender, RoutedEventArgs e)
        {
            QuickCommandTextBox.Text = "systeminfo";
            ExecuteQuickCommand_Click(sender, e);
        }

        private void QuickTracertButton_Click(object sender, RoutedEventArgs e)
        {
            QuickCommandTextBox.Text = "tracert microsoft.com";
            ExecuteQuickCommand_Click(sender, e);
        }

        private void QuickNetworkStatusButton_Click(object sender, RoutedEventArgs e)
        {
            QuickCommandTextBox.Text = "ipconfig /all";
            ExecuteQuickCommand_Click(sender, e);
        }

        private void PingFavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string hostname)
            {
                QuickCommandTextBox.Text = $"ping {hostname}";
                ExecuteQuickCommand_Click(sender, e);
            }
        }

        private void TraceFavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string hostname)
            {
                QuickCommandTextBox.Text = $"tracert {hostname}";
                ExecuteQuickCommand_Click(sender, e);
            }
        }

        private void RunAgainButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string command)
            {
                QuickCommandTextBox.Text = command;
                ExecuteQuickCommand_Click(sender, e);
            }
        }

        private void QuickCommandTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                ExecuteQuickCommand_Click(sender, e);
                e.Handled = true;
            }
        }

        private async void ExecuteQuickCommand_Click(object sender, RoutedEventArgs e)
        {
            string command = QuickCommandTextBox.Text.Trim();

            if (string.IsNullOrEmpty(command))
            {
                return;
            }

            if (isCommandRunning)
            {
                await ShowMessageDialogAsync("Command Running", "Please wait for the current command to complete.");
                return;
            }

            try
            {
                isCommandRunning = true;
                QuickCommandOutputTextBox.Text = "Executing command...";

                // Execute the command
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

                    // Read output asynchronously
                    StringBuilder outputBuilder = new StringBuilder();
                    StringBuilder errorBuilder = new StringBuilder();

                    // Start reading output and error streams
                    var outputTask = proc.StandardOutput.ReadToEndAsync();
                    var errorTask = proc.StandardError.ReadToEndAsync();

                    // Wait for the process to exit
                    await Task.Run(() => proc.WaitForExit());

                    // Get the output
                    string output = await outputTask;
                    string error = await errorTask;

                    // Display output
                    if (!string.IsNullOrEmpty(output))
                    {
                        outputBuilder.Append(output);
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        errorBuilder.Append("Error:\n").Append(error);
                    }

                    // Update UI
                    if (outputBuilder.Length > 0)
                    {
                        QuickCommandOutputTextBox.Text = outputBuilder.ToString();
                    }
                    else if (errorBuilder.Length > 0)
                    {
                        QuickCommandOutputTextBox.Text = errorBuilder.ToString();
                    }
                    else
                    {
                        QuickCommandOutputTextBox.Text = "Command executed with no output.";
                    }

                    // Add to command history
                    AddToCommandHistory(command);
                }
            }
            catch (Exception ex)
            {
                QuickCommandOutputTextBox.Text = $"Error executing command:\n{ex.Message}";
            }
            finally
            {
                isCommandRunning = false;
            }
        }

        #endregion

        private void AddToCommandHistory(string command)
        {
            // Add to the beginning of the list
            RecentCommands.Insert(0, new CommandHistoryItem
            {
                Command = command,
                Timestamp = DateTime.Now.ToString("g"),
                Success = true
            });

            // Limit the list to 10 items
            while (RecentCommands.Count > 10)
            {
                RecentCommands.RemoveAt(RecentCommands.Count - 1);
            }
        }

        private async Task ShowMessageDialogAsync(string title, string content)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        #region Extension Methods for UI Thread Dispatch

        // Helper method for UI thread dispatching
        private async Task UpdateUIAsync(Action action)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            if (!this.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    action();
                    taskCompletionSource.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.TrySetException(ex);
                }
            }))
            {
                taskCompletionSource.TrySetException(new InvalidOperationException("Failed to enqueue task"));
            }

            await taskCompletionSource.Task;
        }

        #endregion
    }
}