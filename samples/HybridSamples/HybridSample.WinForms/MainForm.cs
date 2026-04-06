using HybridSample.WinForms.ViewModels;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace HybridSample.WinForms;

/// <summary>
/// The main form of the WinForms application.
/// </summary>
public partial class MainForm : Form
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainForm> _logger;
    private MainFormViewModel? _viewModel;
    private bool _isDisposing = false;

    /// <summary>
    /// Source-generated logging methods for the MainForm class.
    /// </summary>
    [LoggerMessage(LogLevel.Error, "_viewModel is null in CreateNavigationButtons")]
    private partial void LogViewModelNullError();

    [LoggerMessage(LogLevel.Information, "Creating navigation buttons for {ActionCount} actions")]
    private partial void LogCreatingNavigationButtons(int actionCount);

    [LoggerMessage(LogLevel.Debug, "Created button: {ButtonText} (Key: {ButtonKey}) at position {Position}")]
    private partial void LogCreatedButton(string buttonText, string buttonKey, int position);

    [LoggerMessage(LogLevel.Debug, "Added button to panel: {ButtonText} at position {Position}")]
    private partial void LogAddedButtonToPanel(string buttonText, int position);

    [LoggerMessage(LogLevel.Information, "Total buttons created: {ButtonCount}")]
    private partial void LogTotalButtonsCreated(int buttonCount);

    [LoggerMessage(LogLevel.Debug, "NAVIGATION BUTTON CLICKED: {ButtonKey}")]
    private partial void LogNavigationButtonClicked(string buttonKey);

    [LoggerMessage(LogLevel.Debug, "Button click handler triggered for key: {ButtonKey}")]
    private partial void LogButtonClickHandlerTriggered(string buttonKey);

    [LoggerMessage(LogLevel.Debug, "Executing NavigateToCommand with key: {ButtonKey}")]
    private partial void LogExecutingNavigateToCommand(string buttonKey);

    [LoggerMessage(LogLevel.Error, "NavigateToCommand is null")]
    private partial void LogNavigateToCommandNull();

    [LoggerMessage(LogLevel.Error, "Navigation error: {ErrorMessage}")]
    private partial void LogNavigationError(string errorMessage);

    [LoggerMessage(LogLevel.Error, "Navigation error stack trace: {StackTrace}")]
    private partial void LogNavigationErrorStackTrace(string? stackTrace);

    [LoggerMessage(LogLevel.Error, "Invalid button click - sender: {Sender}, tag: {Tag}")]
    private partial void LogInvalidButtonClick(object? sender, object? tag);

    [LoggerMessage(LogLevel.Debug, "TEST BUTTON CLICKED!")]
    private partial void LogTestButtonClicked();

    /// <summary>
    /// Initializes a new instance of the <see cref="MainForm"/> class.
    /// </summary>
    public MainForm(IServiceProvider serviceProvider, ILogger<MainForm> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        InitializeComponent();
        
        // Set up MVVM binding
        _viewModel = _serviceProvider.GetRequiredService<MainFormViewModel>();
        DataContext = _viewModel;

        // Configure BlazorWebView after InitializeComponent
        ConfigureBlazorWebView();
        
        // Add navigation buttons
        CreateNavigationButtons();
        
        // Handle form closing to ensure proper cleanup
        this.FormClosing += MainForm_FormClosing;
    }

    /// <summary>
    /// Gets or sets the data context for MVVM binding.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new object? DataContext { get; set; }

    private void ConfigureBlazorWebView()
    {
        blazorWebView1.Services = _serviceProvider;
        blazorWebView1.HostPage = "wwwroot/index.html";
        blazorWebView1.RootComponents.Add(new RootComponent("#app", typeof(Shared.App), null));
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_isDisposing)
            return;

        _isDisposing = true;
        
        try
        {
            // Synchronous cleanup to prevent COM apartment state issues
            DisposeBlazorWebViewSync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during form closing cleanup");
        }
    }

    private void DisposeBlazorWebViewSync()
    {
        try
        {
            if (blazorWebView1 != null)
            {
                // Clear root components first
                blazorWebView1.RootComponents.Clear();
                
                // Set services to null (this is intentional to clear the reference)
                blazorWebView1.Services = null!;
                
                // Allow some processing time for cleanup
                Application.DoEvents();
                
                // Dispose the BlazorWebView synchronously
                blazorWebView1.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error disposing BlazorWebView");
        }
    }

    private void CreateNavigationButtons()
    {
        if (_viewModel == null) 
        {
            LogViewModelNullError();
            return;
        }

        LogCreatingNavigationButtons(_viewModel.NavigationActions.Count);

        // Clear existing controls
        navigationPanel.Controls.Clear();

        int buttonTop = 10;
        const int buttonHeight = 32;
        const int buttonSpacing = 5;
        const int leftMargin = 10;
        int buttonWidth = navigationPanel.Width - (leftMargin * 2);

        // Create navigation buttons without test button to avoid clutter
        foreach (var navAction in _viewModel.NavigationActions)
        {
            var button = new Button
            {
                Text = navAction.Value.Title,
                Left = leftMargin,
                Top = buttonTop,
                Width = buttonWidth,
                Height = buttonHeight,
                UseVisualStyleBackColor = true,
                Tag = navAction.Key,
                Name = $"btn_{navAction.Key}",
                BackColor = SystemColors.Control,
                FlatStyle = FlatStyle.System,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleCenter, // Changed from MiddleLeft to MiddleCenter
                Font = new Font(Font.FontFamily, 8.5F, FontStyle.Regular)
            };

            LogCreatedButton(navAction.Value.Title, navAction.Key, buttonTop);

            // Store the key in a local variable to avoid closure issues
            string buttonKey = navAction.Key;

            button.Click += (sender, e) =>
            {
                LogNavigationButtonClicked(buttonKey);
                
                if (sender is Button btn && btn.Tag is string key)
                {
                    LogButtonClickHandlerTriggered(key);
                    try
                    {
                        if (_viewModel?.NavigateToCommand != null)
                        {
                            LogExecutingNavigateToCommand(key);
                            _viewModel.NavigateToCommand.Execute(key);
                        }
                        else
                        {
                            LogNavigateToCommandNull();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogNavigationError(ex.Message);
                        LogNavigationErrorStackTrace(ex.StackTrace);
                    }
                }
                else
                {
                    LogInvalidButtonClick(sender, (sender as Button)?.Tag);
                }
            };

            navigationPanel.Controls.Add(button);
            buttonTop += buttonHeight + buttonSpacing;

            LogAddedButtonToPanel(navAction.Value.Title, buttonTop - buttonHeight - buttonSpacing);
        }

        LogTotalButtonsCreated(navigationPanel.Controls.Count);

        // Handle panel resize to adjust button widths
        navigationPanel.Resize += (sender, e) =>
        {
            int newButtonWidth = navigationPanel.Width - (leftMargin * 2);
            foreach (Control control in navigationPanel.Controls)
            {
                if (control is Button btn)
                {
                    btn.Width = newButtonWidth;
                }
            }
        };
    }
}