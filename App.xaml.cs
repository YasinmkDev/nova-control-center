using Microsoft.UI.Xaml;

namespace NovaControlCenter;

/// <summary>
/// WinUI 3 entry point. No WPF App.xaml.cs magic here — WinUI uses
/// Microsoft.UI.Xaml.Application and a single MainWindow created in OnLaunched.
/// Theme is applied before the window shows to avoid a flash.
/// </summary>
public partial class App : Application
{
    private Window? _window;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Restore last theme (default Dark, Nova's signature look)
        Services.ThemeService.ApplySavedTheme();
        _window = new MainWindow();
        _window.Activate();
    }
}
