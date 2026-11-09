using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.UI;

namespace NovaControlCenter.Controls;

/// <summary>
/// Self-drawing line chart: no chart library. Points are drawn as a Polyline
/// clipped by DrawProgress (0..1, animated 900ms on load). Live update every
/// 2s appends a point and shifts the window — see DashboardPage timer.
/// LIMITATION: WinUI has no native chart control; this custom Canvas chart is
/// the idiomatic native alternative (same approach as Toolkit's charts).
/// </summary>
public sealed partial class LineChart : UserControl
{
    public List<double> Values { get; } = new();
    private double _progress = 1;
    public LineChart() { InitializeComponent(); SizeChanged += (_, _) => Redraw(); }
    public void SetValues(IEnumerable<double> v) { Values.Clear(); Values.AddRange(v); AnimateDraw(); }
    public void Push(double v) { Values.Add(v); if (Values.Count > 24) Values.RemoveAt(0); Redraw(); }
    public void AnimateDraw()
    {
        _progress = 0; var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        var start = DateTime.Now;
        t.Tick += (_, _) =>
        {
            _progress = Math.Min(1, (DateTime.Now - start).TotalMilliseconds / 900);
            Redraw();
            if (_progress >= 1) t.Stop();
        };
        t.Start();
    }
    private void Redraw()
    {
        ChartCanvas.Children.Clear();
        if (Values.Count < 2 || ActualWidth < 10) return;
        EmptyText.Visibility = Visibility.Collapsed;
        double max = Values.Max() * 1.1, min = Values.Min() * 0.9, w = ActualWidth, h = ActualHeight;
        int visible = Math.Max(2, (int)Math.Ceiling(Values.Count * _progress));
        var pts = new PointCollection();
        for (int i = 0; i < visible; i++)
            pts.Add(new Point(i * w / (Values.Count - 1), h - 12 - (Values[i] - min) / (max - min) * (h - 24)));
        // Gradient fill under line
        var fillPts = new PointCollection();
        foreach (var pt in pts) fillPts.Add(pt);
        fillPts.Add(new Point(pts[^1].X, h));
        fillPts.Add(new Point(pts[0].X, h));
        ChartCanvas.Children.Add(new Polygon
        {
            Points = fillPts,
            Fill = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0), EndPoint = new Point(0, 1),
                GradientStops = { new GradientStop { Color = Color.FromArgb(60, 108, 92, 231), Offset = 0 }, new GradientStop { Color = Colors.Transparent, Offset = 1 } }
            }
        });
        ChartCanvas.Children.Add(new Polyline
        {
            Points = pts, Stroke = new SolidColorBrush(Color.FromArgb(255, 108, 92, 231)),
            StrokeThickness = 2.5, StrokeLineJoin = PenLineJoin.Round
        });
        var last = pts[^1];
        ChartCanvas.Children.Add(new Ellipse { Width = 10, Height = 10, Fill = new SolidColorBrush(Colors.White), Stroke = new SolidColorBrush(Color.FromArgb(255, 108, 92, 231)), StrokeThickness = 2, Margin = new Thickness(last.X - 5, last.Y - 5, 0, 0) });
    }
}
