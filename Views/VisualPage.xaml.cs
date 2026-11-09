using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NovaControlCenter.Services;

namespace NovaControlCenter.Views;
public sealed partial class VisualPage : Page
{
    public VisualPage() { InitializeComponent(); }
    private void Toast_Click(object sender, RoutedEventArgs e)
        => ToastService.Show("Visual ready", "Particle field is running on the GPU canvas.", InfoBarSeverity.Informational);
}
