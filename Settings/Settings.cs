using System;

namespace Flow.Launcher.Plugin.Caffeine.Settings;

/// <summary>
/// Plugin settings configuration
/// </summary>
public class Settings
{
    /// <summary>
    /// Whether to start caffeine service automatically when Flow Launcher starts
    /// </summary>
    public bool StartWithFlowLauncher { get; set; } = false;

    /// <summary>
    /// Whether to send notifications when caffeine starts/stops
    /// </summary>
    public bool SendNotifications { get; set; } = true;

    /// <summary>
    /// Whether to show tray icon when caffeine is active
    /// </summary>
    public bool ShowTrayIcon { get; set; } = true;

    /// <summary>
    /// Whether to start mouse mover automatically when Flow Launcher starts
    /// </summary>
    public bool StartMouseMoverWithFlowLauncher { get; set; } = false;

    /// <summary>
    /// Delay in seconds before mouse mover starts after no user mouse movement
    /// </summary>
    public int MouseMoverDelaySeconds { get; set; } = 5;
}
