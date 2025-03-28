using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinUIApp
{
    public partial class App : Application
    {
        private static Window m_window;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();

            Frame rootFrame = new Frame();
            m_window.Content = rootFrame;
            rootFrame.Navigate(typeof(LoginPage));

            m_window.Activate();
        }

        /// <summary>
        /// Gets the main application window
        /// </summary>
        public static Window GetMainWindow()
        {
            return m_window;
        }
    }
}