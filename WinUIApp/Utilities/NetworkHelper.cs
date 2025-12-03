using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace WinUIApp.Utilities
{
    /// <summary>
    /// Provides network utility functions with proper HttpClient management
    /// </summary>
    public static class NetworkHelper
    {
        // Singleton HttpClient instance to avoid socket exhaustion
        private static readonly Lazy<HttpClient> _httpClient = new Lazy<HttpClient>(() =>
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            client.DefaultRequestHeaders.Add("User-Agent", "WinUIApp/1.0");
            return client;
        });

        private static HttpClient HttpClient => _httpClient.Value;

        /// <summary>
        /// Checks internet connectivity by attempting to reach a reliable endpoint
        /// </summary>
        public static async Task<bool> CheckInternetConnectivityAsync()
        {
            try
            {
                using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5)))
                {
                    var response = await HttpClient.GetAsync("https://www.google.com", cts.Token);
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the public IP address of the current machine
        /// </summary>
        public static async Task<string> GetPublicIpAddressAsync()
        {
            try
            {
                var response = await HttpClient.GetStringAsync("https://api.ipify.org");
                return response?.Trim() ?? "Could not determine";
            }
            catch
            {
                return "Could not determine";
            }
        }

        /// <summary>
        /// Extracts specific network information from ipconfig output
        /// </summary>
        public static string ExtractNetworkInfo(string output, string searchText, char delimiter = ':')
        {
            if (string.IsNullOrEmpty(output) || string.IsNullOrEmpty(searchText))
                return string.Empty;

            try
            {
                var lines = output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    {
                        int delimiterIndex = line.IndexOf(delimiter);
                        if (delimiterIndex != -1 && delimiterIndex < line.Length - 1)
                        {
                            var value = line.Substring(delimiterIndex + 1).Trim();
                            // Return first non-empty value
                            if (!string.IsNullOrWhiteSpace(value))
                                return value;
                        }
                    }
                }
            }
            catch
            {
                // Silently handle parsing errors
            }

            return string.Empty;
        }
    }
}
