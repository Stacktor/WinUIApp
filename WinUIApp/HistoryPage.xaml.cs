using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using WinRT.Interop;

namespace WinUIApp
{
    public class CommandHistoryEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Command { get; set; } = "";
        public string Target { get; set; } = "this-pc";
        public string Timestamp { get; set; } = DateTime.Now.ToString("g");
        public DateTime ExecutionTime { get; set; } = DateTime.Now;
        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(1);
        public bool Success { get; set; } = true;
        public string Output { get; set; } = "";
        public string Category { get; set; } = "Network"; // Network, System, etc.

        // UI properties
        public string StatusIcon => Success ? "\uE930" : "\uE948"; // Checkmark or Error
        public SolidColorBrush StatusColor => Success ?
            new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
    }

    public sealed partial class HistoryPage : Page
    {
        private ObservableCollection<CommandHistoryEntry> allHistory = new ObservableCollection<CommandHistoryEntry>();
        private ObservableCollection<CommandHistoryEntry> filteredHistory = new ObservableCollection<CommandHistoryEntry>();
        private CommandHistoryEntry selectedEntry = null;

        public HistoryPage()
        {
            this.InitializeComponent();

            // Load command history
            LoadCommandHistory();

            // Bind to filtered list
            HistoryListView.ItemsSource = filteredHistory;
        }

        private void LoadCommandHistory()
        {
            // Clear existing items
            allHistory.Clear();
            filteredHistory.Clear();

            // Add some sample data for design purposes
            // In a real app, this would load from local storage

            // Sample network commands
            allHistory.Add(new CommandHistoryEntry
            {
                Command = "ping google.com",
                Target = "this-pc",
                Timestamp = DateTime.Now.AddHours(-1).ToString("g"),
                ExecutionTime = DateTime.Now.AddHours(-1),
                Duration = TimeSpan.FromSeconds(2.5),
                Success = true,
                Output = "Pinging google.com [142.250.190.78] with 32 bytes of data:\nReply from 142.250.190.78: bytes=32 time=15ms TTL=128\nReply from 142.250.190.78: bytes=32 time=14ms TTL=128\nReply from 142.250.190.78: bytes=32 time=14ms TTL=128\nReply from 142.250.190.78: bytes=32 time=13ms TTL=128\n\nPing statistics for 142.250.190.78:\n    Packets: Sent = 4, Received = 4, Lost = 0 (0% loss),\nApproximate round trip times in milli-seconds:\n    Minimum = 13ms, Maximum = 15ms, Average = 14ms",
                Category = "Network"
            });

            allHistory.Add(new CommandHistoryEntry
            {
                Command = "tracert microsoft.com",
                Target = "this-pc",
                Timestamp = DateTime.Now.AddHours(-2).ToString("g"),
                ExecutionTime = DateTime.Now.AddHours(-2),
                Duration = TimeSpan.FromSeconds(15.2),
                Success = true,
                Output = "Tracing route to microsoft.com [20.112.250.133]\nover a maximum of 30 hops:\n\n  1     2 ms     1 ms     1 ms  192.168.1.1\n  2    15 ms    14 ms    14 ms  172.16.0.1\n  3    16 ms    15 ms    15 ms  routing.net [10.0.0.1]\n...",
                Category = "Network"
            });

            allHistory.Add(new CommandHistoryEntry
            {
                Command = "ipconfig /all",
                Target = "this-pc",
                Timestamp = DateTime.Now.AddDays(-1).ToString("g"),
                ExecutionTime = DateTime.Now.AddDays(-1),
                Duration = TimeSpan.FromSeconds(0.8),
                Success = true,
                Output = "Windows IP Configuration\n\n   Host Name . . . . . . . . . . . . : Desktop-PC\n   Primary Dns Suffix  . . . . . . . : \n   Node Type . . . . . . . . . . . . : Hybrid\n   IP Routing Enabled. . . . . . . . : No\n...",
                Category = "Network"
            });

            // Sample system commands
            allHistory.Add(new CommandHistoryEntry
            {
                Command = "systeminfo",
                Target = "this-pc",
                Timestamp = DateTime.Now.AddDays(-2).ToString("g"),
                ExecutionTime = DateTime.Now.AddDays(-2),
                Duration = TimeSpan.FromSeconds(3.1),
                Success = true,
                Output = "Host Name:                 DESKTOP-PC\nOS Name:                   Microsoft Windows 11 Pro\nOS Version:                10.0.22621 N/A Build 22621\nOS Manufacturer:           Microsoft Corporation\n...",
                Category = "System"
            });

            allHistory.Add(new CommandHistoryEntry
            {
                Command = "tasklist",
                Target = "remote-pc",
                Timestamp = DateTime.Now.AddDays(-3).ToString("g"),
                ExecutionTime = DateTime.Now.AddDays(-3),
                Duration = TimeSpan.FromSeconds(1.3),
                Success = false,
                Output = "Error: The specified computer is not available.",
                Category = "System"
            });

            // Apply current filter
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            filteredHistory.Clear();

            if (FilterComboBox.SelectedItem == null)
            {
                // Default to all commands
                foreach (var entry in allHistory)
                {
                    filteredHistory.Add(entry);
                }
                return;
            }

            string filter = (FilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            switch (filter)
            {
                case "Network Commands":
                    foreach (var entry in allHistory.Where(e => e.Category == "Network"))
                    {
                        filteredHistory.Add(entry);
                    }
                    break;

                case "System Commands":
                    foreach (var entry in allHistory.Where(e => e.Category == "System"))
                    {
                        filteredHistory.Add(entry);
                    }
                    break;

                case "Today Only":
                    DateTime today = DateTime.Today;
                    foreach (var entry in allHistory.Where(e => e.ExecutionTime.Date == today))
                    {
                        filteredHistory.Add(entry);
                    }
                    break;

                case "This Week":
                    DateTime weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                    foreach (var entry in allHistory.Where(e => e.ExecutionTime >= weekStart))
                    {
                        filteredHistory.Add(entry);
                    }
                    break;

                default: // "All Commands"
                    foreach (var entry in allHistory)
                    {
                        filteredHistory.Add(entry);
                    }
                    break;
            }
        }

        private void UpdateDetailsPanel(CommandHistoryEntry entry)
        {
            if (entry == null)
            {
                // Clear details
                DetailCommandTextBox.Text = string.Empty;
                DetailTargetTextBox.Text = string.Empty;
                DetailTimestampTextBox.Text = string.Empty;
                DetailDurationTextBox.Text = string.Empty;
                DetailStatusTextBox.Text = string.Empty;
                DetailOutputTextBox.Text = string.Empty;
                ViewFullOutputButton.IsEnabled = false;
                RunDetailCommandButton.IsEnabled = false;
                return;
            }

            // Update details
            DetailCommandTextBox.Text = entry.Command;
            DetailTargetTextBox.Text = entry.Target;
            DetailTimestampTextBox.Text = entry.Timestamp;
            DetailDurationTextBox.Text = $"{entry.Duration.TotalSeconds:F1} seconds";
            DetailStatusTextBox.Text = entry.Success ? "Success" : "Failed";

            // Show preview of output (first few lines)
            string outputPreview = entry.Output;
            if (outputPreview.Length > 500)
            {
                outputPreview = outputPreview.Substring(0, 500) + "...\n(Output truncated, click 'View Full Output' to see all)";
            }
            DetailOutputTextBox.Text = outputPreview;

            // Enable buttons
            ViewFullOutputButton.IsEnabled = !string.IsNullOrWhiteSpace(entry.Output);
            RunDetailCommandButton.IsEnabled = true;
        }

        #region Event Handlers

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private async void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Clear History",
                Content = "Are you sure you want to clear all command history? This action cannot be undone.",
                PrimaryButtonText = "Clear History",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Clear history
                allHistory.Clear();
                filteredHistory.Clear();

                // Clear details
                UpdateDetailsPanel(null);

                // In a real app, also clear from storage
            }
        }

        private async void ExportHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (allHistory.Count == 0)
            {
                await ShowMessageDialogAsync("Export History", "There is no command history to export.");
                return;
            }

            try
            {
                // Create file picker
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("CSV Files", new[] { ".csv" });
                savePicker.SuggestedFileName = "CommandHistory";

                // Initialize file picker with app window
                var window = App.GetMainWindow();
                var hwnd = WindowNative.GetWindowHandle(window);
                InitializeWithWindow.Initialize(savePicker, hwnd);

                // Pick file
                StorageFile file = await savePicker.PickSaveFileAsync();

                if (file != null)
                {
                    // Create CSV content
                    System.Text.StringBuilder csv = new System.Text.StringBuilder();

                    // Add header
                    csv.AppendLine("Command,Target,Timestamp,Duration,Status,Category");

                    // Add entries
                    foreach (var entry in allHistory)
                    {
                        string csvLine = $"\"{entry.Command.Replace("\"", "\"\"")}\",\"{entry.Target}\",\"{entry.Timestamp}\",\"{entry.Duration.TotalSeconds:F1}\",\"{(entry.Success ? "Success" : "Failed")}\",\"{entry.Category}\"";
                        csv.AppendLine(csvLine);
                    }

                    // Write to file
                    await FileIO.WriteTextAsync(file, csv.ToString());

                    await ShowMessageDialogAsync("Export History", "Command history exported successfully.");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageDialogAsync("Error", $"Failed to export history: {ex.Message}");
            }
        }

        private void HistoryListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var entry = e.ClickedItem as CommandHistoryEntry;
            if (entry != null)
            {
                selectedEntry = entry;
                UpdateDetailsPanel(entry);
            }
        }

        private void RunAgainButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CommandHistoryEntry entry)
            {
                RunCommand(entry);
            }
        }

        private async void DeleteItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string entryId)
            {
                var entry = allHistory.FirstOrDefault(h => h.Id == entryId);
                if (entry != null)
                {
                    ContentDialog dialog = new ContentDialog
                    {
                        Title = "Delete History Item",
                        Content = $"Are you sure you want to delete this history item?\n\nCommand: {entry.Command}",
                        PrimaryButtonText = "Delete",
                        CloseButtonText = "Cancel",
                        DefaultButton = ContentDialogButton.Close,
                        XamlRoot = this.XamlRoot
                    };

                    var result = await dialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        // Remove from collections
                        allHistory.Remove(entry);
                        filteredHistory.Remove(entry);

                        // Clear details if selected entry was deleted
                        if (selectedEntry == entry)
                        {
                            selectedEntry = null;
                            UpdateDetailsPanel(null);
                        }

                        // In a real app, also remove from storage
                    }
                }
            }
        }

        private async void ViewFullOutputButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedEntry == null)
                return;

            ContentDialog dialog = new ContentDialog
            {
                Title = $"Command Output: {selectedEntry.Command}",
                Content = new ScrollViewer
                {
                    Content = new TextBox
                    {
                        Text = selectedEntry.Output,
                        IsReadOnly = true,
                        AcceptsReturn = true,
                        TextWrapping = TextWrapping.NoWrap,
                        FontFamily = new FontFamily("Consolas"),
                        MinWidth = 600,
                        MinHeight = 400
                    },
                    VerticalScrollMode = ScrollMode.Auto,
                    HorizontalScrollMode = ScrollMode.Auto
                },
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private void RunDetailCommandButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedEntry != null)
            {
                RunCommand(selectedEntry);
            }
        }

        private void RunCommand(CommandHistoryEntry entry)
        {
            // Navigate to the appropriate page based on the command category
            Frame rootFrame = App.GetMainWindow().Content as Frame;

            if (entry.Category == "Network")
            {
                // Navigate to NetworkTools page (MainPage for now)
                rootFrame?.Navigate(typeof(MainHub));

                // Find the NavView in MainHub and select the Network Tools item
                var mainHub = rootFrame.Content as MainHub;
                mainHub?.NavigateToNetworkTools(entry.Command, entry.Target);
            }
            else if (entry.Category == "System")
            {
                // Navigate to SystemTools page
                rootFrame?.Navigate(typeof(MainHub));

                // Find the NavView in MainHub and select the System Tools item
                var mainHub = rootFrame.Content as MainHub;
                mainHub?.NavigateToSystemTools(entry.Command, entry.Target);
            }
        }

        #endregion

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