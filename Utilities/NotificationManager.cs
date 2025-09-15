using System;
using System.IO;
using Flow.Launcher.Plugin.Caffeine.Settings;

namespace Flow.Launcher.Plugin.Caffeine.Utilities;

/// <summary>
/// Manages notifications for caffeine and mouse mover activities
/// </summary>
public static class NotificationManager
{
    private static readonly string TitleCaffeine = "Caffeine - Flow Launcher ☕";
    private static PluginInitContext _context;
    private static Settings.Settings _settings;
    private static string _caffeineIconPath;
    private static string _mouseIconPath;
    private static string _combinedIconPath;

    /// <summary>
    /// Initialize the notification manager
    /// </summary>
    /// <param name="context">Plugin context</param>
    /// <param name="settings">Plugin settings</param>
    public static void Initialize(PluginInitContext context, Settings.Settings settings)
    {
        _context = context;
        _settings = settings;
        
        _caffeineIconPath = Path.Combine(context.CurrentPluginMetadata.PluginDirectory, "Images/icon.png");
        _mouseIconPath = Path.Combine(context.CurrentPluginMetadata.PluginDirectory, "Images/mouse-icon.png");
        _combinedIconPath = Path.Combine(context.CurrentPluginMetadata.PluginDirectory, "Images/caffeine-mouse-combined-icon.png");
    }

    /// <summary>
    /// Send notification when caffeine status changes
    /// </summary>
    /// <param name="caffeineActive">Whether caffeine is now active</param>
    public static void NotifyCaffeineStatusChanged(bool caffeineActive)
    {
        if (!_settings.SendNotifications)
            return;

        bool mouseMoverActive = MouseMover.IsRunning;
        
        if (_settings.SendMouseMoverNotifications && mouseMoverActive)
        {
            // Both notifications enabled and mouse mover is active - send combined notification
            SendCombinedNotification(caffeineActive, mouseMoverActive);
        }
        else
        {
            // Only caffeine notification or mouse mover is not active
            if (caffeineActive)
            {
                _context.API.ShowMsg(TitleCaffeine, "Caffeine is now active 🟢", _caffeineIconPath);
            }
            else
            {
                _context.API.ShowMsg(TitleCaffeine, "Caffeine is now inactive 🔴", _caffeineIconPath);
            }
        }
    }

    /// <summary>
    /// Send notification when mouse mover status changes
    /// </summary>
    /// <param name="mouseMoverActive">Whether mouse mover is now active</param>
    public static void NotifyMouseMoverStatusChanged(bool mouseMoverActive)
    {
        if (!_settings.SendMouseMoverNotifications)
            return;

        bool caffeineActive = Caffeine.IsActive;
        
        if (_settings.SendNotifications && caffeineActive)
        {
            // Both notifications enabled and caffeine is active - send combined notification
            SendCombinedNotification(caffeineActive, mouseMoverActive);
        }
        else
        {
            // Only mouse mover notification or caffeine is not active
            if (mouseMoverActive)
            {
                _context.API.ShowMsg(TitleCaffeine, "Mouse mover is now active 🟢", _mouseIconPath);
            }
            else
            {
                _context.API.ShowMsg(TitleCaffeine, "Mouse mover is now inactive 🔴", _mouseIconPath);
            }
        }
    }

    /// <summary>
    /// Send a combined notification showing status of both caffeine and mouse mover
    /// </summary>
    /// <param name="caffeineActive">Whether caffeine is active</param>
    /// <param name="mouseMoverActive">Whether mouse mover is active</param>
    private static void SendCombinedNotification(bool caffeineActive, bool mouseMoverActive)
    {
        string message;

        message = (caffeineActive, mouseMoverActive) switch
        {
            (true, true) => "Caffeine is active 🟢 Mouse mover is active 🟢",
            (true, false) => "Caffeine is active 🟢 Mouse mover is inactive 🔴",
            (false, true) => "Caffeine is inactive 🔴 Mouse mover is active 🟢",
            (false, false) => "Caffeine is inactive 🔴 Mouse mover is inactive 🔴"
        };

        _context.API.ShowMsg(TitleCaffeine, message, _combinedIconPath);
    }

    /// <summary>
    /// Send notification when both services are started/stopped together
    /// </summary>
    /// <param name="caffeineActive">Whether caffeine is active</param>
    /// <param name="mouseMoverActive">Whether mouse mover is active</param>
    public static void NotifyStatusChanged(bool caffeineActive, bool mouseMoverActive)
    {
        bool shouldSendCaffeineNotification = _settings.SendNotifications;
        bool shouldSendMouseMoverNotification = _settings.SendMouseMoverNotifications;

        // If both notification settings are disabled, don't send any notification
        if (!shouldSendCaffeineNotification && !shouldSendMouseMoverNotification)
            return;

        // If both are enabled, send combined notification
        if (shouldSendCaffeineNotification && shouldSendMouseMoverNotification)
        {
            SendCombinedNotification(caffeineActive, mouseMoverActive);
        }
        // If only one is enabled, send individual notification for the enabled one
        else if (shouldSendCaffeineNotification)
        {
            NotifyCaffeineStatusChanged(caffeineActive);
        }
        else if (shouldSendMouseMoverNotification)
        {
            NotifyMouseMoverStatusChanged(mouseMoverActive);
        }
    }
}