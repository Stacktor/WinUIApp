using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Runtime.InteropServices;
using Windows.Graphics;
using WinRT; // For 'As' extension method
using WinRT.Interop; // For WindowNative.GetWindowHandle

namespace WinUIApp
{
    public sealed partial class MainWindow : Window
    {
        // Existing fields for Mica backdrop
        private WindowsSystemDispatcherQueueHelper m_wsdqHelper;
        private MicaController m_micaController;
        private SystemBackdropConfiguration m_configurationSource;

        // New field for window handle
        private IntPtr _hwnd;

        public MainWindow()
        {
            this.InitializeComponent();

            // Get the window handle (HWND)
            _hwnd = WindowNative.GetWindowHandle(this);

            // Extend content into the title bar
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);

            // Apply Mica backdrop
            TrySetMicaBackdrop();

            // Navigate to the initial page
            MainFrame.Navigate(typeof(LoginPage));

            // Subscribe to window size changed event
            this.SizeChanged += MainWindow_SizeChanged;
        }

        // Existing Mica backdrop methods
        private bool TrySetMicaBackdrop()
        {
            if (MicaController.IsSupported())
            {
                m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
                m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

                // Create the configuration source
                m_configurationSource = new SystemBackdropConfiguration
                {
                    IsInputActive = true,
                    Theme = GetSystemBackdropTheme()
                };

                // Hook up events
                this.Activated += Window_Activated;
                this.Closed += Window_Closed;
                ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

                // Create and set the backdrop controller
                m_micaController = new MicaController();

                // Set the backdrop to the current window
                m_micaController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                m_micaController.SetSystemBackdropConfiguration(m_configurationSource);

                return true; // Mica is applied successfully
            }
            else
            {
                // Mica is not supported on this OS version.
                return false;
            }
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // Clean up
            if (m_micaController != null)
            {
                m_micaController.Dispose();
                m_micaController = null;
            }
            this.Activated -= Window_Activated;
            m_configurationSource = null;
        }

        private void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            m_configurationSource.Theme = GetSystemBackdropTheme();
        }

        private SystemBackdropTheme GetSystemBackdropTheme()
        {
            return ((FrameworkElement)this.Content).ActualTheme switch
            {
                ElementTheme.Dark => SystemBackdropTheme.Dark,
                ElementTheme.Light => SystemBackdropTheme.Light,
                _ => SystemBackdropTheme.Default
            };
        }

        // New methods for custom title bar functionality

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            ShowWindow(hWnd, ShowWindowCommands.Minimize);
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            if (IsWindowMaximized(hWnd))
            {
                ShowWindow(hWnd, ShowWindowCommands.Restore);
            }
            else
            {
                ShowWindow(hWnd, ShowWindowCommands.Maximize);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // P/Invoke declarations
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsZoomed(IntPtr hWnd);

        private bool IsWindowMaximized(IntPtr hWnd)
        {
            return IsZoomed(hWnd);
        }

        private enum ShowWindowCommands : int
        {
            Hide = 0,
            Normal = 1,
            ShowMinimized = 2,
            Maximize = 3,
            ShowMaximized = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActive = 7,
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11
        }

        private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            var hWnd = WindowNative.GetWindowHandle(this);

            if (IsWindowMaximized(hWnd))
            {
                // Change maximize button icon to 'Restore Down'
                ((FontIcon)MaximizeButton.Content).Glyph = "\uE923"; // Restore Down icon

                // Set tooltip
                ToolTipService.SetToolTip(MaximizeButton, "Restore Down");
            }
            else
            {
                // Change maximize button icon to 'Maximize'
                ((FontIcon)MaximizeButton.Content).Glyph = "\uE922"; // Maximize icon

                // Set tooltip
                ToolTipService.SetToolTip(MaximizeButton, "Maximize");
            }
        }

    }

    // Helper class to initialize the Windows System Dispatcher Queue
    internal class WindowsSystemDispatcherQueueHelper
    {
        [DllImport("CoreMessaging.dll")]
        private static extern int CreateDispatcherQueueController(DispatcherQueueOptions options, out IntPtr dispatcherQueueController);

        [StructLayout(LayoutKind.Sequential)]
        private struct DispatcherQueueOptions
        {
            public int dwSize;
            public int threadType;
            public int apartmentType;
        }

        private object dispatcherQueueController;

        public void EnsureWindowsSystemDispatcherQueueController()
        {
            if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
            {
                // Already initialized
                return;
            }

            DispatcherQueueOptions options;
            options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
            options.threadType = 2;    // DQTYPE_THREAD_CURRENT
            options.apartmentType = 2; // DQTAT_COM_STA

            int hr = CreateDispatcherQueueController(options, out IntPtr ptr);
            if (hr != 0)
            {
                throw new Exception("Failed to create DispatcherQueueController. HRESULT: " + hr);
            }

            dispatcherQueueController = Marshal.GetObjectForIUnknown(ptr);
        }
    }
}
