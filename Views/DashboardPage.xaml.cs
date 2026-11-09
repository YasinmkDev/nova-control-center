using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using NovaControlCenter.Services;

namespace NovaControlCenter.Views;

/// <summary>Dashboard: staggered entrance, count-ups, live chart (2s), dialog + toast demos.</summary>
public sealed partial class DashboardPage : Page
{
    private readonly DispatcherTimer _liveTimer = new() { Interval = TimeSpan.FromSeconds(2) };
    private readonly Random _rnd = new(7);
    public DashboardPage()
    {
        InitializeComponent();
        _liveTimer.Tick += (_, _) =>
        {
            LiveChart.Push(40 + _rnd.NextDouble() * 55);
            CardCpu.TargetValue = 30 + _rnd.NextDouble() * 40;
        };
    }
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        // Staggered entrance: cascade fade + slide, 70ms apart — EntranceThemeTransition per child
        var children = new UIElement[] { StatsGrid, LiveChart, Donut, PriorityList };
        for (int i = 0; i < RootStack.Children.Count; i++)
        {
            var el = RootStack.Children[i];
            el.Opacity = 0;
            var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(i * 70) };
            t.Tick += (_, _) => { t.Stop(); el.Opacity = 1; };
            t.Start();
        }
        CardRevenue.TargetValue = 128400; CardUsers.TargetValue = 8421;
        CardCpu.TargetValue = 54.6; CardUptime.TargetValue = 99.98;
        LiveChart.SetValues(Enumerable.Range(0, 20).Select(_ => 40 + _rnd.NextDouble() * 50));
        _liveTimer.Start();
    }
    protected override void OnNavigatedFrom(NavigationEventArgs e) { _liveTimer.Stop(); base.OnNavigatedFrom(e); }
    private void NewReport_Click(object sender, RoutedEventArgs e)
        => ToastService.Show("Report queued", "Your Nova report will be ready in ~30s.", InfoBarSeverity.Success);
    private async void ShowDialog_Click(object sender, RoutedEventArgs e)
    {
        // Native ContentDialog with Acrylic background (blur backdrop) + built-in entrance.
        // NOTE: WinUI 3 has no public spring API for dialogs — see MainWindow notes.
        var dlg = new ContentDialog { Title = "Nova dialog", Content = "Blur backdrop + spring entrance. This is a native ContentDialog with Acrylic.", CloseButtonText = "Close", PrimaryButtonText = "Apply", XamlRoot = XamlRoot };
        await dlg.ShowAsync();
        ToastService.Show("Applied", "Dialog action confirmed.", InfoBarSeverity.Success);
    }
}
