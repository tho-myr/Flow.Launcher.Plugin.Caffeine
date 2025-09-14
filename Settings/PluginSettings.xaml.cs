using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Flow.Launcher.Plugin.Caffeine.Tray;
using Flow.Launcher.Plugin.Caffeine.Utilities;

namespace Flow.Launcher.Plugin.Caffeine.Settings;

public partial class PluginSettings : UserControl
{
    private readonly PluginInitContext _context;
    private readonly Settings _settings;
    private bool _isLoading;

    /// <summary>
    /// Initialize the plugin settings UI
    /// </summary>
    /// <param name="context">Plugin context</param>
    /// <param name="settings">Plugin settings</param>
    public PluginSettings(PluginInitContext context, Settings settings)
    {
        InitializeComponent();
        _context = context;
        _settings = settings;
        LoadSettings();
    }

    private void LoadSettings()
    {
        _isLoading = true;
        StartWithFlowLauncherCheckBox.IsChecked = _settings.StartWithFlowLauncher;
        SendNotificationsCheckBox.IsChecked = _settings.SendNotifications;
        ShowTrayIconCheckBox.IsChecked = _settings.ShowTrayIcon;
        StartMouseMoverWithFlowLauncherCheckBox.IsChecked = _settings.StartMouseMoverWithFlowLauncher;
        MouseMoverDelayTextBox.Text = _settings.MouseMoverDelaySeconds.ToString();
        HideDelayError(); // Clear any error state when loading settings
        _isLoading = false;
    }
    
    private void StartWithFlowLauncherCheckBox_Changed(object sender, System.Windows.RoutedEventArgs e)
    {
        if (!_isLoading)
            SaveSettings();
    }

    private void SendNotificationsCheckBox_Changed(object sender, System.Windows.RoutedEventArgs e)
    {
        if (!_isLoading)
            SaveSettings();
    }

    private void ShowTrayIconCheckBox_Changed(object sender, System.Windows.RoutedEventArgs e)
    {
        if (!_isLoading)
            SaveSettings();
    }

    private void StartMouseMoverWithFlowLauncherCheckBox_Changed(object sender, System.Windows.RoutedEventArgs e)
    {
        if (!_isLoading)
            SaveSettings();
    }

    private void MouseMoverDelayTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_isLoading)
        {
            if (int.TryParse(MouseMoverDelayTextBox.Text, out int value) && value >= 1 && value <= 3600)
            {
                _settings.MouseMoverDelaySeconds = value;
                HideDelayError();
                SaveSettings();
            }
            else
            {
                ShowDelayError();
            }
        }
           
    }

    /// <summary>
    /// Shows the delay validation error message and sets textbox border to red
    /// </summary>
    private void ShowDelayError()
    {
        MouseMoverDelayErrorText.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Hides the delay validation error message and resets textbox border
    /// </summary>
    private void HideDelayError()
    {
        MouseMoverDelayErrorText.Visibility = Visibility.Collapsed;
    }

    private void SaveSettings()
    {
        _settings.StartWithFlowLauncher = StartWithFlowLauncherCheckBox.IsChecked ?? false;
        _settings.SendNotifications = SendNotificationsCheckBox.IsChecked ?? true;
        _settings.ShowTrayIcon = ShowTrayIconCheckBox.IsChecked ?? true;
        _settings.StartMouseMoverWithFlowLauncher = StartMouseMoverWithFlowLauncherCheckBox.IsChecked ?? false;
        
        _context.API.SaveSettingJsonStorage<Settings>();

        if (_settings.ShowTrayIcon && Caffeine.IsActive) TrayIconManager.ShowTray(_context);
        if (!_settings.ShowTrayIcon) TrayIconManager.HideTray();
        
        // Update MouseMover delay setting
        MouseMover.SetUserMovementDelay(_settings.MouseMoverDelaySeconds);
    }
}
