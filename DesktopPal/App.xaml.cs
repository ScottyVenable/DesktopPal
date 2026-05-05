using System;
using System.Windows;
using System.Windows.Threading;

namespace DesktopPal
{
    /// <summary>
    /// Application entry point. Installs global exception handlers so that
    /// unhandled errors are logged before the process terminates.
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DebugLogger.Info("DesktopPal starting up.");

            // Handle exceptions on the UI thread.
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            // Handle exceptions on background threads.
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

            // Handle Task exceptions that were never observed.
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            DebugLogger.Info($"DesktopPal exiting with code {e.ApplicationExitCode}.");
            base.OnExit(e);
        }

        // ── Exception handlers ───────────────────────────────────────────────────

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            DebugLogger.Error("Unhandled UI-thread exception.", e.Exception);
            e.Handled = true; // Prevent crash; allow the app to keep running where possible.

            System.Windows.MessageBox.Show(
                $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nThe application will try to continue.",
                "DesktopPal – Unexpected Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private static void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            DebugLogger.Error("Unhandled background-thread exception.", ex);
        }

        private static void OnUnobservedTaskException(object? sender,
            System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
        {
            DebugLogger.Error("Unobserved Task exception.", e.Exception);
            e.SetObserved(); // Prevent process termination.
        }
    }
}

