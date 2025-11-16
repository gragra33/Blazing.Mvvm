using Blazing.Mvvm;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using HybridSample.Core.Services;
using HybridSample.Core.ViewModels;
using HybridSample.WinForms.Extensions;
using HybridSample.WinForms.ViewModels;
using HybridSample.WinForms.Logging;
using Refit;

namespace HybridSample.WinForms;

/// <summary>
/// The main entry point for the WinForms application.
/// </summary>
internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Set up global exception handling to prevent unhandled exceptions from causing project GUID errors
        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
#if DEBUG
            MessageBox.Show(text: error.ExceptionObject.ToString(), caption: "Unhandled Exception");
#else
            MessageBox.Show(text: "An error has occurred.", caption: "Error");
#endif
        };

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (sender, e) =>
        {
#if DEBUG
            MessageBox.Show(text: e.Exception.ToString(), caption: "Thread Exception");
#else
            MessageBox.Show(text: "An error has occurred.", caption: "Error");
#endif
        };

        ApplicationConfiguration.Initialize();

        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        IServiceCollection services = builder.Services;

        services.AddWindowsFormsBlazorWebView();
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif

        services
            .AddSingleton(RestService.For<IRedditService>("https://www.reddit.com/"))
            .AddServicesWinForms() // This adds IFilesService, ISettingsService, and ApplicationLifecycleService
            .AddMvvm(options =>
            {
                options.HostingModelType = BlazorHostingModelType.Hybrid;
                options.RegisterViewModelsFromAssemblyContaining<SamplePageViewModel>();
                options.RegisterViewModelsFromAssemblyContaining<HybridSample.Blazor.Core.Pages.IntroductionPage>();
                options.RegisterViewModelsFromAssemblyContaining<MainFormViewModel>();
            });

#if DEBUG
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        builder.Logging.AddConsole();
#endif

        services.AddScoped<MainForm>();

        using var host = builder.Build();
        
        // Create logger for Program class
        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
        
        ProgramLogger.LogApplicationStarting(logger);
        ProgramLogger.LogConsoleWindowVisible(logger);
        ProgramLogger.LogDebugLoggingEnabled(logger);

        try
        {
            // Start the host synchronously to avoid COM apartment state issues
            host.StartAsync().GetAwaiter().GetResult();

            using var scope = host.Services.CreateScope();

            ProgramLogger.LogServiceProviderCreated(logger);
            
            var mainForm = scope.ServiceProvider.GetRequiredService<MainForm>();
            
            ProgramLogger.LogMainFormObtained(logger);
            ProgramLogger.LogInstructionsForUser(logger);
            
            // Handle proper cleanup on application exit
            Application.ApplicationExit += (sender, e) =>
            {
                logger.LogInformation("Application exit requested, performing cleanup...");
                
                try
                {
                    // Dispose the main form using sync dispose method
                    mainForm?.Dispose();
                    
                    // Stop the host synchronously
                    host.StopAsync(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during application cleanup");
                }
            };
            
            Application.Run(mainForm);
            
            ProgramLogger.LogApplicationExited(logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in application");
            MessageBox.Show($"Application error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            try
            {
                // Ensure host is properly disposed synchronously
                host.StopAsync(TimeSpan.FromSeconds(1)).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error during final cleanup");
            }
        }
    }
}