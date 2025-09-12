using System;
using System.Collections.Generic;
using System.IO;
using Flow.Launcher.Plugin.Caffeine.Settings;
using Flow.Launcher.Plugin.Caffeine.Tray;
using Flow.Launcher.Plugin.Caffeine.Utilities;

namespace Flow.Launcher.Plugin.Caffeine;

/// <summary>
/// Main plugin class for the Caffeine Flow Launcher plugin
/// </summary>
public class Caffeine : IPlugin, ISettingProvider, IDisposable
{
    internal static bool IsActive { get; private set; } = false;

    private PluginInitContext _context;
    private Settings.Settings _settings;
    private string _iconPath;
    private string _mouseMoverIconPath;
    private string _settingsIconPath;
    
    /// <summary>
    /// Initialize the plugin
    /// </summary>
    /// <param name="context">Plugin initialization context</param>
    public void Init(PluginInitContext context)
    {
        _context = context;
        _settings = context.API.LoadSettingJsonStorage<Settings.Settings>();
        _iconPath = Path.Combine(context.CurrentPluginMetadata.PluginDirectory, "Images/icon.png");
        _mouseMoverIconPath = Path.Combine(context.CurrentPluginMetadata.PluginDirectory, "Images/mouse-icon.png");
        _settingsIconPath = Path.Combine(context.CurrentPluginMetadata.PluginDirectory, "Images/settings-icon.png");

        // Configure mouse mover delay
        MouseMover.SetUserMovementDelay(_settings.MouseMoverDelaySeconds);

        // Start caffeine automatically if the setting is enabled
        if (_settings.StartWithFlowLauncher)
        {
            StartCaffeine();
        }
        
        // Start mouse mover automatically if the setting is enabled
        if (_settings.StartMouseMoverWithFlowLauncher)
        {
            MouseMover.Start();
        }
    }

    /// <summary>
    /// Query method for Flow Launcher
    /// </summary>
    /// <param name="query">The query from Flow Launcher</param>
    /// <returns>List of results</returns>
    public List<Result> Query(Query query)
    {
        var caffeineResult = new Result
        {
            Title = $"Turn {CaffeineState()} Caffeine",
            SubTitle = "Toggle Caffeine off and on.",
            Score = 100000,
            Action = c =>
            {
                if (!IsActive)
                {
                    StartCaffeine();
                }
                else
                {
                    StopCaffeine();
                }
                return true;
            },
            IcoPath = _iconPath
        };

        var mouseMoverResult = new Result
        {
            Title = $"Turn {MouseMoverState()} Mouse Mover",
            SubTitle = "Toggle smart mouse movement (pauses when you move mouse, resumes after 5s).",
            Score = 80000,
            Action = c =>
            {
                MouseMover.Toggle();
                return true;
            },
            IcoPath = _mouseMoverIconPath
        };

        var settingsResult = new Result
        {
            Title = $"Open Flow Launcher Settings",
            SubTitle = "Open settings to customize plugin further (◠‿◠)",
            Score = 60000,
            Action = c =>
            {
                _context.API.OpenSettingDialog();
                return true;
            },
            IcoPath = _settingsIconPath
        };

        return new List<Result> { caffeineResult, mouseMoverResult, settingsResult };
    }

    /// <summary>
    /// Get the current state of caffeine for display
    /// </summary>
    /// <returns>String indicating next action (On/Off)</returns>
    private static string CaffeineState()
    {
        return !IsActive ? "On" : "Off";
    }

    /// <summary>
    /// Get the current state of mouse mover for display
    /// </summary>
    /// <returns>String indicating next action (On/Off)</returns>
    private static string MouseMoverState()
    {
        return !MouseMover.IsRunning ? "On" : "Off";
    }

    /// <summary>
    /// Start the caffeine service
    /// </summary>
    private void StartCaffeine()
    {
        if (!IsActive)
        {
            PowerUtilities.PreventPowerSave();
            IsActive = true;
            if (_settings.ShowTrayIcon) TrayIconManager.ShowTray(_context);
            if (_settings.SendNotifications) _context.API.ShowMsg("Caffeine - Flow Launcher ☕", "Caffeine is now active 🟢", _iconPath);
        }
    }

    /// <summary>
    /// Stop the caffeine service
    /// </summary>
    private void StopCaffeine()
    {
        if (IsActive)
        {
            PowerUtilities.Shutdown();
            IsActive = false;
            if (_settings.ShowTrayIcon) TrayIconManager.HideTray();
            if (_settings.SendNotifications) _context.API.ShowMsg("Caffeine - Flow Launcher ☕", "Caffeine is now inactive 🔴", _iconPath);
        }
    }

    /// <summary>
    /// Create the settings panel for the plugin
    /// </summary>
    /// <returns>The settings user control</returns>
    public System.Windows.Controls.Control CreateSettingPanel()
    {
        return new PluginSettings(_context, _settings);
    }

    /// <summary>
    /// Dispose of resources when the plugin is unloaded
    /// </summary>
    public void Dispose()
    {
        StopCaffeine();
        MouseMover.Stop();
    }
}
