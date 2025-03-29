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
    public sealed partial class SystemToolsPage : Page
    {
        private List<string> knownSystems = new List<string> {
            "this-pc",
            "localhost"
        };

        private bool isCommandRunning = false;
        private string currentTarget = "this-pc";
        private string lastCommand = "";

        public SystemToolsPage()
        {
            this.InitializeComponent();

            // Initialize target systems list
            LoadFavoriteSystems();

            // Set default target
            TargetSystemBox.Text = "this-pc";
        }

        private void LoadFavoriteSystems()
        {
            try
            {
                var settings = ApplicationData.Current.LocalSettings;
                if (settings.Values.ContainsKey("FavoriteHostnames"))
                {
                    var favoritesString = settings.Values["FavoriteHostnames"] as string;
                    if (!string.IsNullOrEmpty(favoritesString))
                    {
                        foreach (var hostname in favoritesString.Split(','))
                        {
                            if (!knownSystems.Contains(hostname))
                            {
                                knownSystems.Add(hostname);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading favorites: {ex.Message}");
            }
        }

        private void UpdateResultHeader(string commandName)
        {
            ResultTitleTextBlock.Text = commandName;

            if (IsLocalSystem(currentTarget))
            {
                ResultTargetTextBlock.Text = "(Local System)";
            }
            else
            {
                ResultTargetTextBlock.Text = $"({currentTarget})";
            }
        }

        #region Target System Auto-Suggest Box Handlers

        private void TargetSystemBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            // Update current target when a target is selected
            currentTarget = args.QueryText;
        }

        private void TargetSystemBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // Filter suggestions based on input
                var filtered = knownSystems.FindAll(s =>
                    s.StartsWith(sender.Text, StringComparison.OrdinalIgnoreCase) ||
                    s.Contains(sender.Text, StringComparison.OrdinalIgnoreCase));

                sender.ItemsSource = filtered;

                // Update current target as text changes
                currentTarget = sender.Text;
            }
        }

        #endregion

        #region Command Button Handlers

        private void SystemInfoButton_Click(object sender, RoutedEventArgs e)
        {
            string target = TargetSystemBox.Text;

            if (IsLocalSystem(target))
            {
                ExecuteCommandInternal("systeminfo", "System Information");
            }
            else
            {
                ExecuteCommandInternal($"systeminfo /S {EscapeShellArgument(target)}", "System Information");
            }
        }

        private void SystemVersionButton_Click(object sender, RoutedEventArgs e)
        {
            string target = TargetSystemBox.Text;

            if (IsLocalSystem(target))
            {
                ExecuteCommandInternal("ver", "System Version");
            }
            else
            {
                // For remote systems, use a different approach
                ExecuteCommandInternal($"systeminfo /S {EscapeShellArgument(target)} | findstr /B /C:\"OS Name\" /C:\"OS Version\"", "System Version");
            }
        }

        private void SystemUptimeButton_Click(object sender, RoutedEventArgs e)
        {
            string target = TargetSystemBox.Text;

            if (IsLocalSystem(target))
            {
                // Get system boot time using wmic
                ExecuteCommandInternal("wmic os get lastbootuptime", "System Uptime");
            }
            else
            {
                // For remote systems
                ExecuteCommandInternal($"wmic /node:\"{EscapeShellArgument(target)}\" os get lastbootuptime", "System Uptime");
            }
        }

        private void TaskListButton_Click(object sender, RoutedEventArgs e)
        {
            string target = TargetSystemBox.Text;

            if (IsLocalSystem(target))
            {
                ExecuteCommandInternal("tasklist", "Task List");
            }
            else
            {
                ExecuteCommandInternal($"tasklist /S {EscapeShellArgument(target)}", "Task List");
            }
        }

        private void ServicesButton_Click(object sender, RoutedEventArgs e)
        {
            string target = TargetSystemBox.Text;

            if (IsLocalSystem(target))
            {
                ExecuteCommandInternal("sc query state= all", "Services");
            }
            else
            {
                ExecuteCommandInternal($"sc \\\\{EscapeShellArgument(target)} query state= all", "Services");
            }
        }

        private void RunningServicesButton_Click(object sender, RoutedEventArgs e)
        {
            string target = TargetSystemBox.Text;

            if (IsLocalSystem(target))
            {
                ExecuteCommandInternal("sc query state= running", "Running Services");
            }
            else
            {
                ExecuteCommandInternal($"sc \\\\{EscapeShellArgument(target)} query state= running", "Running Services");
            }
        }

        private void DiskSpaceButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommandInternal("wmic logicaldisk get deviceid,volumename,description,size,freespace", "Disk Space");
        }

        private void DiskTypeButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommandInternal("wmic diskdrive get model,size,interfacetype", "Disk Type");
        }

        private void LocalUsersButton_Click(object sender, RoutedEventArgs e)
        {
            string target = TargetSystemBox.Text;

            if (IsLocalSystem(target))
            {
                ExecuteCommandInternal("net user", "Local Users");
            }
            else
            {
                ExecuteCommandInternal($"net user /domain", "Domain Users");
            }
        }

        private void ActiveSessionsButton_Click(object sender, RoutedEventArgs e)
        {
            string target = TargetSystemBox.Text;

            if (IsLocalSystem(target))
            {
                ExecuteCommandInternal("quser", "Active Sessions");
            }
            else
            {
                ExecuteCommandInternal($"quser /server:{EscapeShellArgument(target)}", "Active Sessions");
            }
        }

        #endregion

        #region Result Action Handlers

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ResultTextBox.Text))
            {
                return;
            }

            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(ResultTextBox.Text);
            Clipboard.SetContent(dataPackage);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ResultTextBox.Text))
            {
                return;
            }

            try
            {
                // Create file save picker
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("Text Files", new List<string>() { ".txt" });

                // Generate a default filename based on command and timestamp
                string commandName = ResultTitleTextBlock.Text.Replace(" ", "");
                string defaultFilename = $"{commandName}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                savePicker.SuggestedFileName = defaultFilename;

                // Initialize file picker with window handle
                var window = App.GetMainWindow();
                var hwnd = WindowNative.GetWindowHandle(window);
                InitializeWithWindow.Initialize(savePicker, hwnd);

                // Pick file location
                StorageFile file = await savePicker.PickSaveFileAsync();

                if (file != null)
                {
                    // Write output to file
                    await FileIO.WriteTextAsync(file, ResultTextBox.Text);
                }
            }
            catch (Exception ex)
            {
                await ShowMessageDialogAsync("Error", $"Failed to save output: {ex.Message}");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ResultTextBox.Text = string.Empty;
        }

        #endregion

        #region Command Execution

        // Public method to allow running commands from other pages
        public void ExecuteCommand(string command, string target = null)
        {
            if (!string.IsNullOrEmpty(target))
            {
                TargetSystemBox.Text = target;
                currentTarget = target;
            }

            // Identify which button to click based on the command
            if (command.StartsWith("systeminfo"))
            {
                SystemInfoButton_Click(null, null);
            }
            else if (command.StartsWith("ver"))
            {
                SystemVersionButton_Click(null, null);
            }
            else if (command.StartsWith("wmic os get lastbootuptime"))
            {
                SystemUptimeButton_Click(null, null);
            }
            else if (command.StartsWith("tasklist"))
            {
                TaskListButton_Click(null, null);
            }
            else if (command.StartsWith("sc query"))
            {
                if (command.Contains("state= running"))
                {
                    RunningServicesButton_Click(null, null);
                }
                else
                {
                    ServicesButton_Click(null, null);
                }
            }
            else if (command.StartsWith("wmic logicaldisk"))
            {
                DiskSpaceButton_Click(null, null);
            }
            else if (command.StartsWith("wmic diskdrive"))
            {
                DiskTypeButton_Click(null, null);
            }
            else if (command.StartsWith("net user"))
            {
                LocalUsersButton_Click(null, null);
            }
            else if (command.StartsWith("quser"))
            {
                ActiveSessionsButton_Click(null, null);
            }
            else
            {
                // If no matching command found, just run it directly
                string commandName = "Custom Command";
                ExecuteCommandInternal(command, commandName);
            }
        }

        private async void ExecuteCommandInternal(string command, string commandName)
        {
            if (isCommandRunning)
            {
                await ShowMessageDialogAsync("Command Running", "A command is currently running. Please wait for it to complete.");
                return;
            }

            try
            {
                isCommandRunning = true;
                lastCommand = command;

                // Clear previous results
                ResultTextBox.Text = "Executing command...";

                // Update header
                UpdateResultHeader(commandName);

                // Execute command
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process proc = new Process { StartInfo = psi })
                {
                    // Set up real-time output
                    StringBuilder outputBuilder = new StringBuilder();

                    // Set up event handlers
                    proc.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            _ = this.DispatcherQueue.TryEnqueue(() =>
                            {
                                outputBuilder.AppendLine(e.Data);
                                ResultTextBox.Text = outputBuilder.ToString();

                                // Auto-scroll to bottom
                                ResultTextBox.SelectionStart = ResultTextBox.Text.Length;
                                ResultTextBox.SelectionLength = 0;
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
                                ResultTextBox.Text = outputBuilder.ToString();
                            });
                        }
                    };

                    proc.Start();

                    // Begin asynchronous reading
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();

                    // Wait for process to exit
                    await Task.Run(() => proc.WaitForExit());

                    // Handle case of no output
                    if (outputBuilder.Length == 0)
                    {
                        _ = this.DispatcherQueue.TryEnqueue(() =>
                        {
                            ResultTextBox.Text = "Command executed with no output.";
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ResultTextBox.Text = $"Error executing command:\n{ex.Message}";
            }
            finally
            {
                isCommandRunning = false;
            }
        }

        private bool IsLocalSystem(string target)
        {
            return string.IsNullOrEmpty(target) ||
                   target.Equals("this-pc", StringComparison.OrdinalIgnoreCase) ||
                   target.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                   target.Equals("127.0.0.1");
        }

        private string EscapeShellArgument(string arg)
        {
            if (string.IsNullOrEmpty(arg))
                return string.Empty;

            // Basic shell argument escaping to prevent command injection
            return arg.Replace("&", "")
                     .Replace("|", "")
                     .Replace(";", "")
                     .Replace("(", "")
                     .Replace(")", "")
                     .Replace("<", "")
                     .Replace(">", "")
                     .Replace("\"", "");
        }

        #endregion

        #region Helper Methods

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

        #endregion
    }
}