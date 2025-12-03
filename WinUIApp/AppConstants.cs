using System;

namespace WinUIApp
{
    /// <summary>
    /// Application-wide constants and configuration values
    /// </summary>
    public static class AppConstants
    {
        // Application Info
        public const string AppName = "WinUIApp";
        public const string AppVersion = "1.0.5";

        // Default Credentials (for development only - should be replaced with proper auth)
        public const string DefaultUsername = "admin";
        public const string DefaultPassword = "password123";
        public const string AltUsername = "user";
        public const string AltPassword = "user123";

        // Settings Keys
        public const string SettingsKeyUserDisplayName = "UserDisplayName";
        public const string SettingsKeyUserEmail = "UserEmail";
        public const string SettingsKeyRememberUsername = "RememberUsername";
        public const string SettingsKeySavedUsername = "SavedUsername";
        public const string SettingsKeyRememberLogin = "RememberLogin";
        public const string SettingsKeyAppTheme = "AppTheme";
        public const string SettingsKeyFavoriteHostnames = "FavoriteHostnames";
        public const string SettingsKeyProfilePicturePath = "ProfilePicturePath";
        public const string SettingsKeyCommandTimeout = "CommandTimeout";
        public const string SettingsKeyRealTimeOutput = "RealTimeOutput";
        public const string SettingsKeySaveHistory = "SaveHistory";
        public const string SettingsKeyPingCount = "PingCount";
        public const string SettingsKeyTracertHops = "TracertHops";
        public const string SettingsKeyResolveHostnames = "ResolveHostnames";
        public const string SettingsKeyFontSize = "FontSize";
        public const string SettingsKeyWrapText = "WrapText";
        public const string SettingsKeyShowSystemInfo = "ShowSystemInfo";
        public const string SettingsKeyShowNetworkStatus = "ShowNetworkStatus";
        public const string SettingsKeyShowCommandHistory = "ShowCommandHistory";
        public const string SettingsKeyShowFavorites = "ShowFavorites";
        public const string SettingsKeyDefaultCommand = "DefaultCommand";

        // Default Values
        public const int DefaultCommandTimeout = 30;
        public const int DefaultPingCount = 4;
        public const int DefaultTracertHops = 30;
        public const int DefaultFontSize = 14;
        public const string DefaultTheme = "Default";

        // Special Hostnames
        public const string LocalSystemIdentifier = "this-pc";
        public const string Localhost = "localhost";
        public const string LocalhostIP = "127.0.0.1";

        // Network Endpoints
        public const string PublicIPCheckUrl = "https://api.ipify.org";
        public const string ConnectivityCheckUrl = "https://www.google.com";

        // UI Messages
        public const string MessageValidationError = "Please enter valid information.";
        public const string MessageCommandRunning = "A command is currently running. Please wait for it to complete.";
        public const string MessageNoOutput = "No output to display.";
        public const string MessageNoHistory = "No command history available.";
        public const string MessageCopiedToClipboard = "Copied to clipboard";
        public const string MessageSaveSuccess = "Saved successfully";
        public const string MessageSaveFailed = "Failed to save";
    }
}
