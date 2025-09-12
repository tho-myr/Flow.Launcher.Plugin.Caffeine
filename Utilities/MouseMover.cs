using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.Caffeine.Utilities;

/// <summary>
/// Utility class for random mouse movement to prevent screen saver activation
/// </summary>
public static class MouseMover
{
    /// <summary>
    /// Windows POINT structure for mouse coordinates
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        /// <summary>
        /// X coordinate
        /// </summary>
        public int X;
        /// <summary>
        /// Y coordinate
        /// </summary>
        public int Y;
    }
    
    [DllImport("user32.dll")]
    static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll")]
    static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int x, int y);

    // Multi-monitor support
    [DllImport("user32.dll")]
    static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

    private const int SM_XVIRTUALSCREEN = 76;  // Virtual screen left
    private const int SM_YVIRTUALSCREEN = 77;  // Virtual screen top  
    private const int SM_CXVIRTUALSCREEN = 78; // Virtual screen width
    private const int SM_CYVIRTUALSCREEN = 79; // Virtual screen height

    private static CancellationTokenSource _mouseMoverCancellation;
    private static bool _isRunning = false;
    private static POINT _lastKnownPosition;
    private static DateTime _lastUserMovement = DateTime.MinValue;
    private static bool _isAutomatedMovement = false;
    private static int _userMovementDelaySeconds = 5;

    /// <summary>
    /// Gets whether the mouse mover is currently running
    /// </summary>
    public static bool IsRunning => _isRunning;

    /// <summary>
    /// Start the random mouse mover
    /// </summary>
    public static void Start()
    {
        if (_isRunning)
            return;

        _isRunning = true;
        _mouseMoverCancellation = new CancellationTokenSource();
        
        Task.Run(async () =>
        {
            var random = new Random();
            
            // Use virtual screen for multi-monitor support
            var virtualLeft = GetSystemMetrics(SM_XVIRTUALSCREEN);
            var virtualTop = GetSystemMetrics(SM_YVIRTUALSCREEN);
            var virtualWidth = GetSystemMetrics(SM_CXVIRTUALSCREEN);
            var virtualHeight = GetSystemMetrics(SM_CYVIRTUALSCREEN);
            
            // Initialize last known position
            GetCursorPos(out _lastKnownPosition);
            _lastUserMovement = DateTime.Now;
            
            while (!_mouseMoverCancellation.Token.IsCancellationRequested)
            {
                try
                {
                    // Get current mouse position
                    GetCursorPos(out POINT currentPos);
                    
                    // Check if user moved the mouse (only when we're not moving it)
                    if (!_isAutomatedMovement && 
                        (currentPos.X != _lastKnownPosition.X || currentPos.Y != _lastKnownPosition.Y))
                    {
                        _lastUserMovement = DateTime.Now;
                        _lastKnownPosition = currentPos;
                    }
                    
                    // Check if we should pause due to recent user movement
                    var timeSinceUserMovement = DateTime.Now - _lastUserMovement;
                    if (timeSinceUserMovement.TotalSeconds < _userMovementDelaySeconds)
                    {
                        // User moved mouse recently, wait and check again
                        await Task.Delay(100, _mouseMoverCancellation.Token);
                        continue;
                    }
                    
                    // Update position before automated movement
                    GetCursorPos(out currentPos);
                    _lastKnownPosition = currentPos;
                    
                    // Generate random direction with more variety
                    var angle = random.NextDouble() * 2 * Math.PI; // Random angle in radians
                    var distance = random.Next(15, 35); // Random distance
                    
                    // Calculate target position using trigonometry for natural movement
                    var targetX = currentPos.X + (int)(Math.Cos(angle) * distance);
                    var targetY = currentPos.Y + (int)(Math.Sin(angle) * distance);
                    
                    // Ensure target stays within virtual screen bounds (all monitors)
                    targetX = Math.Max(virtualLeft + 10, Math.Min(virtualLeft + virtualWidth - 10, targetX));
                    targetY = Math.Max(virtualTop + 10, Math.Min(virtualTop + virtualHeight - 10, targetY));
                    
                    // Calculate smooth movement with more steps for ultra-smooth motion
                    var deltaX = targetX - currentPos.X;
                    var deltaY = targetY - currentPos.Y;
                    var totalDistance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                    var steps = Math.Max(30, (int)(totalDistance * 1.5)); // More steps = smoother
                    
                    // Perform ultra-smooth continuous movement
                    _isAutomatedMovement = true; // Mark that we're doing automated movement
                    for (int i = 1; i <= steps && !_mouseMoverCancellation.Token.IsCancellationRequested; i++)
                    {
                        // Use smooth easing function for natural acceleration/deceleration
                        var progress = (double)i / steps;
                        var easedProgress = EaseInOutQuad(progress);
                        
                        var newX = currentPos.X + (int)(deltaX * easedProgress);
                        var newY = currentPos.Y + (int)(deltaY * easedProgress);
                        
                        SetCursorPos(newX, newY);
                        
                        // Update our tracking of the last known position
                        _lastKnownPosition = new POINT { X = newX, Y = newY };
                        
                        // Minimal delay for smoothness (16ms ≈ 60fps)
                        await Task.Delay(16, _mouseMoverCancellation.Token);
                    }
                    _isAutomatedMovement = false; // Mark that automated movement is complete
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    // Ignore errors and continue
                }
            }
            
            _isRunning = false;
        }, _mouseMoverCancellation.Token);
    }

    /// <summary>
    /// Smooth easing function for natural movement acceleration/deceleration
    /// </summary>
    private static double EaseInOutQuad(double t)
    {
        return t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }

    /// <summary>
    /// Stop the random mouse mover
    /// </summary>
    public static void Stop()
    {
        if (!_isRunning)
            return;

        _mouseMoverCancellation?.Cancel();
        _isRunning = false;
        _isAutomatedMovement = false;
    }

    /// <summary>
    /// Toggle the mouse mover on/off
    /// </summary>
    public static void Toggle()
    {
        if (_isRunning)
            Stop();
        else
            Start();
    }

    /// <summary>
    /// Set the delay in seconds before mouse mover starts after user movement stops
    /// </summary>
    /// <param name="delaySeconds">Delay in seconds (1-300)</param>
    public static void SetUserMovementDelay(int delaySeconds)
    {
        _userMovementDelaySeconds = Math.Max(1, Math.Min(300, delaySeconds));
    }
}
