# WinUIApp Troubleshooting Guide

## Common Issues and Solutions

### Button Click Not Working

If a button click doesn't trigger its function, check the following:

#### 1. **Event Handler Exists**
Verify that the event handler method exists in the code-behind file:
- Open the `.xaml.cs` file
- Search for the method name (e.g., `ButtonName_Click`)
- Ensure it has the correct signature: `private void ButtonName_Click(object sender, RoutedEventArgs e)`

#### 2. **Correct Event Name in XAML**
Check that the XAML button has the correct event handler name:
```xml
<Button Content="My Button" Click="MyButton_Click"/>
```

#### 3. **Namespace Issues**
Ensure the page class is in the correct namespace:
- The `x:Class` attribute in XAML should match the namespace in the code-behind
- Example: `x:Class="WinUIApp.MainPage"` matches `namespace WinUIApp`

#### 4. **Build the Project**
Sometimes the designer cache needs to be refreshed:
```bash
dotnet clean
dotnet build
```

### Event Handler Verification Checklist

All event handlers have been verified and exist in the codebase:

**LoginPage.xaml.cs:**
- ✅ LoginButton_Click
- ✅ LoginField_KeyDown

**MainHub.xaml.cs:**
- ✅ NavView_SelectionChanged
- ✅ ThemeToggleButton_Click
- ✅ UserButton_Click
- ✅ LogoutItem_Tapped

**MainPage.xaml.cs:**
- ✅ PingButton_Click
- ✅ TracertButton_Click
- ✅ QuserButton_Click
- ✅ IpconfigButton_Click
- ✅ NslookupButton_Click
- ✅ NetstatButton_Click
- ✅ SystemInfoButton_Click
- ✅ TasklistButton_Click
- ✅ AddToFavoritesButton_Click
- ✅ CopyOutputButton_Click
- ✅ SaveOutputButton_Click
- ✅ ClearOutputButton_Click
- ✅ ThemeToggleButton_Click
- ✅ HelpButton_Click
- ✅ LogoutButton_Click
- ✅ HostnameAutoSuggestBox_QuerySubmitted
- ✅ HostnameAutoSuggestBox_TextChanged

**DashboardPage.xaml.cs:**
- ✅ RefreshButton_Click
- ✅ RefreshNetworkStatus_Click
- ✅ RefreshSystemStatus_Click
- ✅ QuickPingButton_Click
- ✅ QuickSystemInfoButton_Click
- ✅ QuickTracertButton_Click
- ✅ QuickNetworkStatusButton_Click
- ✅ PingFavoriteButton_Click
- ✅ TraceFavoriteButton_Click
- ✅ RunAgainButton_Click
- ✅ QuickCommandTextBox_KeyDown
- ✅ ExecuteQuickCommand_Click

**SystemToolsPage.xaml.cs:**
- ✅ SystemInfoButton_Click
- ✅ SystemVersionButton_Click
- ✅ SystemUptimeButton_Click
- ✅ TaskListButton_Click
- ✅ ServicesButton_Click
- ✅ RunningServicesButton_Click
- ✅ DiskSpaceButton_Click
- ✅ DiskTypeButton_Click
- ✅ LocalUsersButton_Click
- ✅ ActiveSessionsButton_Click
- ✅ CopyButton_Click
- ✅ SaveButton_Click
- ✅ ClearButton_Click
- ✅ TargetSystemBox_QuerySubmitted
- ✅ TargetSystemBox_TextChanged

**HistoryPage.xaml.cs:**
- ✅ FilterComboBox_SelectionChanged
- ✅ ClearHistoryButton_Click
- ✅ ExportHistoryButton_Click
- ✅ HistoryListView_ItemClick
- ✅ RunAgainButton_Click
- ✅ DeleteItemButton_Click
- ✅ ViewFullOutputButton_Click
- ✅ RunDetailCommandButton_Click

**ProfilePage.xaml.cs:**
- ✅ ChangePhotoButton_Click
- ✅ UpdateProfileButton_Click
- ✅ UpdatePasswordButton_Click
- ✅ RememberLoginToggle_Toggled
- ✅ DashboardPrefToggle_Toggled
- ✅ DefaultCommandComboBox_SelectionChanged
- ✅ SavePreferencesButton_Click
- ✅ ClearCommandHistoryButton_Click
- ✅ ClearFavoritesButton_Click
- ✅ ResetPreferencesButton_Click
- ✅ ExportSettingsButton_Click
- ✅ ImportSettingsButton_Click
- ✅ DeleteAccountButton_Click

**SettingsPage.xaml.cs:**
- ✅ ThemeRadioButtons_SelectionChanged
- ✅ CommandTimeoutBox_ValueChanged
- ✅ RealTimeOutputCheckBox_Checked
- ✅ SaveHistoryCheckBox_Checked
- ✅ PingCountBox_ValueChanged
- ✅ TracertHopsBox_ValueChanged
- ✅ ResolveHostnamesCheckBox_Checked
- ✅ FontSizeComboBox_SelectionChanged
- ✅ WrapTextCheckBox_Checked
- ✅ CheckUpdatesButton_Click
- ✅ ResetSettingsButton_Click

**MainWindow.xaml.cs:**
- ✅ MinimizeButton_Click
- ✅ MaximizeButton_Click
- ✅ CloseButton_Click

## Debugging Tips

### Enable Debug Logging

The app includes a built-in logger. To view debug messages:

1. Run the app in Debug mode
2. Open the Output window in Visual Studio (View > Output)
3. Look for messages prefixed with `[DEBUG]`, `[INFO]`, `[WARNING]`, or `[ERROR]`

### Check Log File

The application creates a log file at:
```
%LOCALAPPDATA%\Packages\<PackageName>\LocalState\app_log.txt
```

### Common Runtime Errors

#### "The name '...' does not exist in the current context"
- **Cause**: XAML references a control or method that doesn't exist
- **Solution**: Check the `x:Name` attributes in XAML match the code-behind

#### "Method not found" at runtime
- **Cause**: Event handler signature doesn't match expected signature
- **Solution**: Ensure event handlers have correct parameters (object sender, EventArgs e)

#### Commands Not Executing
- **Cause**: Command execution might be blocked by antivirus or permissions
- **Solution**: Run as administrator or add exception for cmd.exe

## Performance Issues

### Slow Command Execution
- Check the `CommandTimeout` setting in Settings page
- Increase timeout if commands are timing out
- Use real-time output mode for long-running commands

### High Memory Usage
- Clear command history regularly
- Reduce the number of favorite hosts
- Restart the application periodically

## Network Issues

### "Could not determine" Public IP
- **Cause**: Network connectivity issues or firewall blocking
- **Solution**: Check internet connection and firewall settings

### Commands Fail on Remote Systems
- **Cause**: Insufficient permissions or remote system not accessible
- **Solution**: Ensure proper credentials and network access

## Getting Help

If you continue to experience issues:

1. Check the log file for detailed error messages
2. Enable debug logging
3. Create an issue on GitHub with:
   - Description of the problem
   - Steps to reproduce
   - Error messages from the log
   - Your Windows version and .NET version

## Useful Commands for Troubleshooting

```bash
# Check .NET version
dotnet --version

# Clean and rebuild
dotnet clean
dotnet build

# Run in debug mode
dotnet run --configuration Debug

# Check for package issues
dotnet restore
```

## Advanced Debugging

### Attach Debugger
1. Open the project in Visual Studio
2. Set breakpoints in event handlers
3. Press F5 to start debugging
4. Click the button that's not working
5. Step through the code to identify the issue

### Inspect XAML at Runtime
- Use Live Visual Tree in Visual Studio (Debug > Windows > Live Visual Tree)
- Check if the button is actually rendered
- Verify event handlers are attached

## Reset Application State

If the app is behaving unexpectedly, reset its state:

1. Close the application
2. Delete the settings folder:
   ```
   %LOCALAPPDATA%\Packages\<PackageName>\LocalState
   ```
3. Restart the application

---

**Last Updated**: December 2025
**Version**: 1.0.5
