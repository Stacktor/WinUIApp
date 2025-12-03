using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace WinUIApp.Utilities
{
    /// <summary>
    /// Provides centralized command execution functionality with security and performance optimizations
    /// </summary>
    public static class CommandExecutor
    {
        private const int DefaultTimeout = 30000; // 30 seconds

        /// <summary>
        /// Escapes shell arguments to prevent command injection attacks
        /// </summary>
        public static string EscapeShellArgument(string argument)
        {
            if (string.IsNullOrEmpty(argument))
                return string.Empty;

            // Remove potentially dangerous characters
            var sb = new StringBuilder(argument.Length);
            foreach (char c in argument)
            {
                // Allow alphanumeric, dots, hyphens, and underscores
                if (char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_')
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Validates if a hostname is valid
        /// </summary>
        public static bool IsValidHostname(string hostname)
        {
            if (string.IsNullOrWhiteSpace(hostname))
                return false;

            // Allow special local identifiers
            if (hostname.Equals("this-pc", StringComparison.OrdinalIgnoreCase) ||
                hostname.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                return true;

            // Basic validation: alphanumeric with dots and hyphens
            foreach (char c in hostname)
            {
                if (!char.IsLetterOrDigit(c) && c != '.' && c != '-')
                    return false;
            }

            return hostname.Length <= 255;
        }

        /// <summary>
        /// Checks if the target represents the local system
        /// </summary>
        public static bool IsLocalSystem(string target)
        {
            return string.IsNullOrEmpty(target) ||
                   target.Equals("this-pc", StringComparison.OrdinalIgnoreCase) ||
                   target.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                   target.Equals("127.0.0.1");
        }

        /// <summary>
        /// Executes a command and returns the output
        /// </summary>
        public static async Task<CommandResult> ExecuteAsync(string command, int timeoutMs = DefaultTimeout)
        {
            var result = new CommandResult();

            try
            {
                var psi = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = psi })
                {
                    var outputBuilder = new StringBuilder();
                    var errorBuilder = new StringBuilder();

                    process.Start();

                    // Read output and error asynchronously
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    // Wait for process with timeout
                    var processTask = Task.Run(() => process.WaitForExit(timeoutMs));
                    await processTask;

                    if (!process.HasExited)
                    {
                        process.Kill();
                        result.Success = false;
                        result.Error = "Command timed out";
                        return result;
                    }

                    result.Output = await outputTask;
                    result.Error = await errorTask;
                    result.ExitCode = process.ExitCode;
                    result.Success = process.ExitCode == 0 && string.IsNullOrWhiteSpace(result.Error);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Executes a command with real-time output streaming
        /// </summary>
        public static async Task ExecuteWithStreamingAsync(
            string command,
            Action<string> onOutputReceived,
            Action<string> onErrorReceived = null,
            int timeoutMs = DefaultTimeout)
        {
            try
            {
                var psi = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = psi })
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            onOutputReceived?.Invoke(e.Data);
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            onErrorReceived?.Invoke(e.Data);
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    await Task.Run(() => process.WaitForExit(timeoutMs));

                    if (!process.HasExited)
                    {
                        process.Kill();
                        onErrorReceived?.Invoke("Command timed out");
                    }
                }
            }
            catch (Exception ex)
            {
                onErrorReceived?.Invoke($"Error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Represents the result of a command execution
    /// </summary>
    public class CommandResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public int ExitCode { get; set; }

        public string GetDisplayText()
        {
            if (!string.IsNullOrWhiteSpace(Output))
                return Output;

            if (!string.IsNullOrWhiteSpace(Error))
                return $"Error: {Error}";

            return "Command executed with no output.";
        }
    }
}
