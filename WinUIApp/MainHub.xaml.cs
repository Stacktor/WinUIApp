using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;

namespace WinUIApp
{
    public sealed partial class MainHub : Page
    {
        // Properties for binding
        public string PageTitle { get; private set; } = "Dashboard";
        public string UserDisplayName { get; private set; } = "Admin User";
        public BitmapImage UserProfilePicture { get; private set; }

        // Page type mapping for navigation
        private readonly Dictionary<string, Type> _pages = new Dictionary<string, Type>
        {
            { "Dashboard", typeof(DashboardPage) },
            { "NetworkTools", typeof(MainPage) },  // Existing page for now
            { "SystemTools", typeof(SystemToolsPage) },
            { "History", typeof(HistoryPage) },
            { "Settings", typeof(SettingsPage) },
            { "Profile", typeof(ProfilePage) }
        };

        // Store command to run after navigation
        private string _pendingCommand = null;
        private string _pendingTarget = null;

        public MainHub()
        {
            this.InitializeComponent();

            // Load user profile info from settings - with safe access
            SafeLoadUserProfile();

            // Navigate to dashboard by default
            ContentFrame.Navigate(typeof(MainPage));  // Default to MainPage for now since Dashboard might not be implemented

            // Try to select the Network Tools item
            foreach (var item in NavView.MenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag.ToString() == "NetworkTools")
                {
                    NavView.SelectedItem = navItem;
                    break;
                }
            }

            // Set theme based on preference (safely)
            SafeApplySavedTheme();
        }

        private void SafeLoadUserProfile()
        {
            try
            {
                // Try to access settings, but handle exceptions
                var settings = Windows.Storage.ApplicationData.Current?.LocalSettings;

                if (settings != null && settings.Values.ContainsKey("UserDisplayName"))
                {
                    UserDisplayName = settings.Values["UserDisplayName"] as string ?? "Admin User";
                }

                // For default profile picture, use a placeholder or built-in asset
                try
                {
                    UserProfilePicture = new BitmapImage(new Uri("ms-appx:///Assets/StoreLogo.png"));
                }
                catch
                {
                    // If the profile picture can't be loaded, just continue without it
                }
            }
            catch (Exception ex)
            {
                // Silently handle the exception - just use default values
                System.Diagnostics.Debug.WriteLine($"Error loading user profile: {ex.Message}");
                UserDisplayName = "Admin User";
            }
        }

        private void SafeApplySavedTheme()
        {
            try
            {
                var settings = Windows.Storage.ApplicationData.Current?.LocalSettings;
                if (settings != null && settings.Values.ContainsKey("AppTheme"))
                {
                    string themeName = settings.Values["AppTheme"] as string;

                    if (themeName == "Dark")
                    {
                        this.RequestedTheme = ElementTheme.Dark;
                    }
                    else if (themeName == "Light")
                    {
                        this.RequestedTheme = ElementTheme.Light;
                    }
                    else
                    {
                        // Default to system theme
                        this.RequestedTheme = ElementTheme.Default;
                    }
                }
            }
            catch
            {
                // If there's an error, just use the default theme
                this.RequestedTheme = ElementTheme.Default;
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                if (args.IsSettingsSelected)
                {
                    // Handle settings navigation if needed
                    return;
                }

                var navItemTag = args.SelectedItemContainer.Tag.ToString();
                NavigateToPage(navItemTag);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in navigation: {ex.Message}");
                // Fall back to MainPage if there's an error
                ContentFrame.Navigate(typeof(MainPage));
            }
        }

        private void NavigateToPage(string navItemTag)
        {
            try
            {
                // Update the page title
                PageTitle = navItemTag;

                // Navigate to the selected page
                Type pageType = typeof(MainPage);  // Default fallback

                if (_pages.ContainsKey(navItemTag))
                {
                    pageType = _pages[navItemTag];
                }

                ContentFrame.Navigate(pageType);

                // If there's a pending command, pass it to the page if it's the right type
                if (_pendingCommand != null)
                {
                    if (navItemTag == "NetworkTools" && ContentFrame.Content is MainPage networkPage)
                    {
                        try
                        {
                            networkPage.ExecuteCommand(_pendingCommand);
                        }
                        catch
                        {
                            // Ignore errors in command execution
                        }
                        _pendingCommand = null;
                        _pendingTarget = null;
                    }
                    else if (navItemTag == "SystemTools" && ContentFrame.Content is SystemToolsPage systemPage)
                    {
                        try
                        {
                            systemPage.ExecuteCommand(_pendingCommand, _pendingTarget);
                        }
                        catch
                        {
                            // Ignore errors in command execution
                        }
                        _pendingCommand = null;
                        _pendingTarget = null;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navigating to page: {ex.Message}");
            }
        }

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.ActualTheme == ElementTheme.Dark)
                {
                    this.RequestedTheme = ElementTheme.Light;
                    SafeSaveThemePreference("Light");
                }
                else
                {
                    this.RequestedTheme = ElementTheme.Dark;
                    SafeSaveThemePreference("Dark");
                }
            }
            catch
            {
                // Silently handle theme toggle errors
                this.RequestedTheme = ElementTheme.Default;
            }
        }

        private void SafeSaveThemePreference(string themeName)
        {
            try
            {
                var settings = Windows.Storage.ApplicationData.Current?.LocalSettings;
                if (settings != null)
                {
                    settings.Values["AppTheme"] = themeName;
                }
            }
            catch
            {
                // Silently handle save errors
            }
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Navigate to profile page
                foreach (var item in NavView.MenuItems)
                {
                    if (item is NavigationViewItem navItem && navItem.Tag.ToString() == "Profile")
                    {
                        NavView.SelectedItem = navItem;
                        break;
                    }
                }
            }
            catch
            {
                // Silently handle navigation errors
            }
        }

        private void LogoutItem_Tapped(object sender, RoutedEventArgs e)
        {
            try
            {
                // Navigate back to login page
                // Use App.GetMainWindow() instead of Window.Current
                var window = App.GetMainWindow();
                Frame rootFrame = window.Content as Frame;
                rootFrame?.Navigate(typeof(LoginPage));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging out: {ex.Message}");
            }
        }

        // Methods to facilitate navigation with commands
        public void NavigateToNetworkTools(string command = null, string target = null)
        {
            try
            {
                _pendingCommand = command;
                _pendingTarget = target;

                // Select the Network Tools item in NavView
                foreach (var item in NavView.MenuItems)
                {
                    if (item is NavigationViewItem navItem && navItem.Tag.ToString() == "NetworkTools")
                    {
                        NavView.SelectedItem = navItem;
                        break;
                    }
                }
            }
            catch
            {
                // Silently handle navigation errors
            }
        }

        public void NavigateToSystemTools(string command = null, string target = null)
        {
            try
            {
                _pendingCommand = command;
                _pendingTarget = target;

                // Select the System Tools item in NavView
                foreach (var item in NavView.MenuItems)
                {
                    if (item is NavigationViewItem navItem && navItem.Tag.ToString() == "SystemTools")
                    {
                        NavView.SelectedItem = navItem;
                        break;
                    }
                }
            }
            catch
            {
                // Silently handle navigation errors
            }
        }
    }
}