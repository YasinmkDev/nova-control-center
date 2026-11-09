using System.Collections.ObjectModel;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using NovaControlCenter.Models;
using NovaControlCenter.Services;

namespace NovaControlCenter.Views;

/// <summary>Data page: 60 rows, sortable DataGrid (Toolkit, virtualized), live search filter, skeleton shimmer, insert/delete with entrance animation.</summary>
public sealed partial class DataPage : Page
{
    private ObservableCollection<ServerRow> _all = new();
    private ObservableCollection<ServerRow> _view = new();
    private string _sortProp = "Id"; private bool _sortAsc = true;
    public DataPage() { InitializeComponent(); }
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _all = SampleData.Servers(60);
        _view = new ObservableCollection<ServerRow>(_all);
        Grid.ItemsSource = _view; UpdateCount();
        // Skeleton shimmer: pulse opacity via Storyboard, then swap in grid with fade
        var sb = new Storyboard();
        var anim = new DoubleAnimation { From = 0.3, To = 1, Duration = TimeSpan.FromMilliseconds(600), AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
        Storyboard.SetTarget(anim, Skeleton); Storyboard.SetTargetProperty(anim, "Opacity");
        sb.Children.Add(anim); sb.Begin();
        await System.Threading.Tasks.Task.Delay(900);
        sb.Stop(); Skeleton.Visibility = Visibility.Collapsed;
        Grid.Visibility = Visibility.Visible;
        Grid.Opacity = 0;
        var fade = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(350) };
        var sb2 = new Storyboard(); Storyboard.SetTarget(fade, Grid); Storyboard.SetTargetProperty(fade, "Opacity"); sb2.Children.Add(fade); sb2.Begin();
    }
    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var q = SearchBox.Text.Trim().ToLowerInvariant();
        _view.Clear();
        foreach (var r in _all.Where(r => string.IsNullOrEmpty(q) || r.Name.ToLowerInvariant().Contains(q) || r.Region.ToLowerInvariant().Contains(q) || r.Status.ToLowerInvariant().Contains(q)))
            _view.Add(r);
        ApplySort(); UpdateCount();
    }
    private void Grid_Sorting(object sender, DataGridColumnEventArgs e)
    {
        _sortProp = e.Column.SortMemberPath ?? "Id";
        _sortAsc = e.Column.SortDirection != DataGridSortDirection.Ascending;
        e.Column.SortDirection = _sortAsc ? DataGridSortDirection.Ascending : DataGridSortDirection.Descending;
        foreach (var c in Grid.Columns.Where(c => c != e.Column)) c.SortDirection = null;
        ApplySort();
    }
    private void ApplySort()
    {
        var sorted = _sortAsc
            ? _view.OrderBy(r => Prop(r)).ToList()
            : _view.OrderByDescending(r => Prop(r)).ToList();
        _view.Clear(); foreach (var r in sorted) _view.Add(r);
        object? Prop(ServerRow r) => _sortProp switch { "Name" => r.Name, "Region" => r.Region, "Cpu" => r.Cpu, "Memory" => r.Memory, "UptimeDays" => r.UptimeDays, "Status" => r.Status, _ => r.Id };
    }
    private void AddRow_Click(object sender, RoutedEventArgs e)
    {
        var row = new ServerRow { Id = _all.Max(r => r.Id) + 1, Name = $"nova-{2000 + _all.Count}", Region = "EU-West", Cpu = 12.4, Memory = 31, UptimeDays = 0, Status = "Online" };
        _all.Insert(0, row); _view.Insert(0, row); UpdateCount();
        ToastService.Show("Row added", $"{row.Name} inserted at the top.", InfoBarSeverity.Success);
    }
    private void DeleteRow_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is ServerRow r) { _all.Remove(r); _view.Remove(r); UpdateCount(); ToastService.Show("Row deleted", r.Name, InfoBarSeverity.Warning); }
        else ToastService.Show("Nothing selected", "Select a row first.", InfoBarSeverity.Warning);
    }
    private void UpdateCount() => CountText.Text = $"{_view.Count} shown / {_all.Count} total";
}
