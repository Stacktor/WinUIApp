# WinUIApp

A modern Windows desktop application built with WinUI 3 and .NET 8, featuring a sleek user interface with Mica backdrop effects and comprehensive user management capabilities.

## 📋 Overview

WinUIApp is a feature-rich Windows application that provides a multi-page navigation experience with user authentication, dashboard analytics, system tools, and personalized settings. The application leverages the latest Windows App SDK to deliver a native Windows 11 experience.

## ✨ Features

- **User Authentication**: Secure login system with user credential management
- **Modern UI Design**:
  - Mica backdrop effect for a translucent, modern appearance
  - Custom title bar with minimize, maximize, and close controls
  - Responsive design that adapts to window size changes
- **Multi-Page Navigation**:
  - **Dashboard**: Main application hub with overview and analytics
  - **History**: Track and review application history
  - **Profile**: User profile management and personalization
  - **Settings**: Application configuration and preferences
  - **System Tools**: Utilities and system management features
- **Cross-Platform Support**: Built for x86, x64, and ARM64 architectures

## 🛠️ Technology Stack

- **Framework**: .NET 8.0
- **UI Framework**: WinUI 3 (Windows App SDK 1.6)
- **Target Platform**: Windows 10 (19041) and above
- **Minimum Version**: Windows 10 (17763)
- **Language**: C# with XAML
- **Deployment**: Unpackaged application (self-contained)

## 📦 Prerequisites

Before building and running WinUIApp, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (17.8 or later) with:
  - .NET Desktop Development workload
  - Windows App SDK (automatically included)
- Windows 10 SDK (10.0.22621.0 or later)
- Windows 10 version 17763 or later

## 🚀 Getting Started

### Clone the Repository

```bash
git clone https://github.com/Stacktor/WinUIApp.git
cd WinUIApp
```

### Build the Project

Using Visual Studio:
1. Open `WinUIApp.sln` in Visual Studio 2022
2. Select your target platform (x86, x64, or ARM64)
3. Build the solution (Ctrl+Shift+B)

Using .NET CLI:
```bash
dotnet restore
dotnet build
```

### Run the Application

Using Visual Studio:
- Press F5 or click the "Start" button

Using .NET CLI:
```bash
dotnet run --project WinUIApp/WinUIApp.csproj
```

## 📁 Project Structure

```
WinUIApp/
├── WinUIApp/
│   ├── Assets/              # Application assets (icons, images)
│   ├── Properties/          # Assembly information
│   ├── Windows/             # Windows-specific configurations
│   ├── App.xaml             # Application definition
│   ├── App.xaml.cs          # Application startup logic
│   ├── MainWindow.xaml      # Main window UI
│   ├── MainWindow.xaml.cs   # Main window logic with Mica backdrop
│   ├── LoginPage.xaml       # User authentication page
│   ├── MainHub.xaml         # Central navigation hub
│   ├── DashboardPage.xaml   # Dashboard view
│   ├── HistoryPage.xaml     # History tracking
│   ├── ProfilePage.xaml     # User profile
│   ├── SettingsPage.xaml    # Application settings
│   └── SystemToolsPage.xaml # System utilities
├── WinUIApp.sln             # Visual Studio solution file
└── README.md                # This file
```

## 🔧 Configuration

### Supported Platforms

The application supports the following platforms:
- x86 (32-bit)
- x64 (64-bit)
- ARM64

### Build Configuration

The project is configured as an unpackaged application with self-contained Windows App SDK deployment. This means:
- No MSIX packaging required
- Easier deployment and distribution
- All dependencies bundled with the application

## 🎨 UI Features

### Mica Backdrop
The application features a modern Mica backdrop effect that:
- Provides a translucent, layered appearance
- Adapts to system light/dark theme
- Falls back gracefully on unsupported systems

### Custom Title Bar
A fully customized title bar with:
- Minimize, maximize/restore, and close buttons
- Extended content area for immersive experience
- Dynamic icon changes based on window state

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is available for use under standard open-source practices. Please check the repository for specific license information.

## 📞 Support

For questions, issues, or suggestions:
- Open an issue on [GitHub Issues](https://github.com/Stacktor/WinUIApp/issues)
- Contact the maintainers through GitHub

## 🔄 Recent Updates

- ✅ Implemented user authentication system
- ✅ Enhanced UI with modern navigation patterns
- ✅ Improved application window management
- ✅ Added comprehensive page navigation (Dashboard, History, Profile, Settings, System Tools)

## 🗺️ Roadmap

Future enhancements may include:
- Database integration for persistent user data
- Cloud synchronization capabilities
- Additional system tools and utilities
- Enhanced analytics and reporting
- Plugin system for extensibility

---

**Built with ❤️ using WinUI 3 and .NET 8**
