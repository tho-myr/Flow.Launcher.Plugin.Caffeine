using System.Windows.Controls;
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

    private void MouseMoverDelayTextBox_Changed(object sender, TextChangedEventArgs e)
    {
        if (!_isLoading)
            SaveSettings();
    }

    private void SaveSettings()
    {
        _settings.StartWithFlowLauncher = StartWithFlowLauncherCheckBox.IsChecked ?? false;
        _settings.SendNotifications = SendNotificationsCheckBox.IsChecked ?? true;
        _settings.ShowTrayIcon = ShowTrayIconCheckBox.IsChecked ?? true;
        _settings.StartMouseMoverWithFlowLauncher = StartMouseMoverWithFlowLauncherCheckBox.IsChecked ?? false;
        
        // Validate and parse delay input
        if (int.TryParse(MouseMoverDelayTextBox.Text, out int delay) && delay >= 1 && delay <= 300)
        {
            _settings.MouseMoverDelaySeconds = delay;
        }
        else
        {
            // Reset to default if invalid
            _settings.MouseMoverDelaySeconds = 5;
            MouseMoverDelayTextBox.Text = "5";
        }
        
        _context.API.SaveSettingJsonStorage<Settings>();

        if (_settings.ShowTrayIcon && Caffeine.IsActive) TrayIconManager.ShowTray(_context);
        if (!_settings.ShowTrayIcon) TrayIconManager.HideTray();
        
        // Update MouseMover delay setting
        MouseMover.SetUserMovementDelay(_settings.MouseMoverDelaySeconds);
    }
}
