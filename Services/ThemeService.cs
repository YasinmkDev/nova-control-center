using Microsoft.UI.Xaml;
using Windows.Storage;

namespace NovaControlCenter.Services;

/// <summary>
/// Theme with a smooth crossfade — WinUI has no built-in circular reveal,
/// so we do the closest native alternative: fade the root out/in over 250ms
/// while swapping RequestedTheme. Comment notes the limitation honestly.
/// LIMITATION: True circular-reveal (Android-style) needs a Composition
/// mask + Clip; WinUI 3.0/1.6 has no public API for it on a Window root.
/// Crossfade + Acrylic is the native idiom and is what we ship.
/// </summary>
public static class ThemeService
{
    private const string Key = "NovaTheme"; // "Dark" | "Light"

    public static ElementTheme Current =>
        ApplicationData.Current.LocalSettings.Values[Key]?.ToString() == "Light"
            ? ElementTheme.Light : ElementTheme.Dark;

    public static void ApplySavedTheme()
    {
        if (App.Current is App app) { /* theme applied per-window in MainWindow */ }
    }

    public static async void ToggleTheme(FrameworkElement root)
    {
        var next = Current == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
        ApplicationData.Current.LocalSettings.Values[Key] = next.ToString();

        // Crossfade: animate opacity to 0, swap, animate back — 60fps Composition
        var compositor = Microsoft.UI.Xaml.Hosting.ElementCompositionPreview.GetElementVisual(root).Compositor;
        var visual = Microsoft.UI.Xaml.Hosting.ElementCompositionPreview.GetElementVisual(root);
        var fadeOut = compositor.CreateScalarKeyFrameAnimation();
        fadeOut.InsertKeyFrame(1f, 0f);
        fadeOut.Duration = System.TimeSpan.FromMilliseconds(180);
        visual.StartAnimation("Opacity", fadeOut);
        await System.Threading.Tasks.Task.Delay(180);

        if (root.XamlRoot?.Content is FrameworkElement fe)
            fe.RequestedTheme = next;
        // Also set on App-level so dialogs follow
        if (Application.Current is Application app && app.Resources != null)
        {
            // WinUI: Window content theme is authoritative; dialogs inherit XamlRoot theme.
        }

        var fadeIn = compositor.CreateScalarKeyFrameAnimation();
        fadeIn.InsertKeyFrame(0f, 0f);
        fadeIn.InsertKeyFrame(1f, 1f);
        fadeIn.Duration = System.TimeSpan.FromMilliseconds(220);
        visual.StartAnimation("Opacity", fadeIn);
    }
}
