using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NovaControlCenter.Services;

namespace NovaControlCenter.Views;

/// <summary>Settings: animated toggles/sliders/dropdowns are native WinUI controls (their animations are built-in). Theme toggle uses ThemeService crossfade.</summary>
public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        ThemeToggle.IsOn = ThemeService.Current == ElementTheme.Dark;
    }
    private void ThemeToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch t && t.IsOn != (ThemeService.Current == ElementTheme.Dark))
            ThemeService.ToggleTheme(this);
    }
    private void AccentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => ToastService.Show("Accent changed", (AccentCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "", InfoBarSeverity.Informational);
    private void ScaleSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        => ScaleText.Text = $"Scale: {(int)(e.NewValue * 100)}%";
    private void Live_Toggled(object sender, RoutedEventArgs e)
        => ToastService.Show("Live updates", (sender as ToggleSwitch)?.IsOn == true ? "On — Dashboard refreshes every 2s." : "Paused.", InfoBarSeverity.Informational);
    private void ReduceMotion_Toggled(object sender, RoutedEventArgs e)
        => ToastService.Show("Reduce motion", "Entrance animations will be shortened on next navigation.", InfoBarSeverity.Informational);
    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        ThemeToggle.IsOn = true; AccentCombo.SelectedIndex = 0; ScaleSlider.Value = 1;
        ToastService.Show("Reset", "Settings restored to defaults.", InfoBarSeverity.Success);
    }
}
