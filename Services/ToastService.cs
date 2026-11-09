using Microsoft.UI.Xaml.Controls;

namespace NovaControlCenter.Services;

/// <summary>Toast queue — MainWindow owns the visual host; this is the API pages call.</summary>
public static class ToastService
{
    public static event Action<string, string, InfoBarSeverity>? ToastRequested;
    public static void Show(string title, string message, InfoBarSeverity severity = InfoBarSeverity.Informational)
        => ToastRequested?.Invoke(title, message, severity);
}
