using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Runtime.InteropServices;
using WinRT; // For 'As' extension method

namespace WinUIApp
{
    public sealed partial class MainWindow : Window
    {
        private WindowsSystemDispatcherQueueHelper m_wsdqHelper;
        private MicaController m_micaController;
        private SystemBackdropConfiguration m_configurationSource;

        public MainWindow()
        {
            this.InitializeComponent();

            // Navigate to the initial page
            MainFrame.Navigate(typeof(LoginPage));

            // Apply Mica backdrop
            TrySetMicaBackdrop();
        }

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
