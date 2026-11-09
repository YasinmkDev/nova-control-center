using Microsoft.UI;
using Path = Microsoft.UI.Xaml.Shapes.Path;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.UI;

namespace NovaControlCenter.Controls;

/// <summary>Donut with hover-expanding segments: each segment is a Path; PointerEntered scales it outward via a translate along its mid-angle (Composition). Segments animate in with a sweep on load.</summary>
public sealed partial class DonutChart : UserControl
{
    // Plain class (not a record): XAML type-info code assigns Segment properties,
    // which fails on a record's init-only properties.
    public sealed class Segment
    {
        public string Label { get; set; } = "";
        public double Value { get; set; }
        public Color Color { get; set; }
        public Segment(string label, double value, Color color) { Label = label; Value = value; Color = color; }
    }
    public List<Segment> Segments { get; } = new()
    {
        new("Compute", 42, Color.FromArgb(255,108,92,231)),
        new("Storage", 28, Color.FromArgb(255,0,210,255)),
        new("Network", 18, Color.FromArgb(255,0,230,118)),
        new("Other", 12, Color.FromArgb(255,255,176,32)),
    };
    public DonutChart() { InitializeComponent(); Loaded += (_, _) => Draw(1); }
    private void Draw(double sweepProgress)
    {
        DonutCanvas.Children.Clear();
        double total = Segments.Sum(s => s.Value), start = -90, cx = 110, cy = 110, rOuter = 100, rInner = 68;
        foreach (var seg in Segments)
        {
            double sweep = 360 * seg.Value / total * sweepProgress;
            if (sweep <= 0) continue;
            var path = new Path
            {
                Fill = new SolidColorBrush(seg.Color),
                Data = ArcGeometry(cx, cy, rOuter, rInner, start, start + sweep),
                Tag = seg
            };
            path.PointerEntered += OnSegmentHover; path.PointerExited += OnSegmentLeave;
            ToolTipService.SetToolTip(path, $"{seg.Label}: {seg.Value}%");
            DonutCanvas.Children.Add(path);
            start += 360 * seg.Value / total;
        }
    }
    private static PathGeometry ArcGeometry(double cx, double cy, double ro, double ri, double a0, double a1)
    {
        double R(double a) => a * Math.PI / 180;
        Point P(double r, double a) => new(cx + r * Math.Cos(R(a)), cy + r * Math.Sin(R(a)));
        bool large = (a1 - a0) > 180;
        var g = new PathGeometry();
        var f = new PathFigure { StartPoint = P(ro, a0), IsClosed = true };
        f.Segments.Add(new ArcSegment { Point = P(ro, a1), Size = new Size(ro, ro), IsLargeArc = large, SweepDirection = SweepDirection.Clockwise });
        f.Segments.Add(new LineSegment { Point = P(ri, a1) });
        f.Segments.Add(new ArcSegment { Point = P(ri, a0), Size = new Size(ri, ri), IsLargeArc = large, SweepDirection = SweepDirection.Counterclockwise });
        g.Figures.Add(f); return g;
    }
    private void OnSegmentHover(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Path p || p.Tag is not Segment seg) return;
        CenterValue.Text = $"{seg.Value}%"; // centre shows hovered segment
        var v = Microsoft.UI.Xaml.Hosting.ElementCompositionPreview.GetElementVisual(p);
        var mid = Segments.TakeWhile(s => s != seg).Sum(s => s.Value) + seg.Value / 2;
        double ang = (-90 + 360 * mid / Segments.Sum(s => s.Value)) * Math.PI / 180;
        var anim = v.Compositor.CreateVector3KeyFrameAnimation();
        anim.InsertKeyFrame(1f, new System.Numerics.Vector3((float)(Math.Cos(ang) * 8), (float)(Math.Sin(ang) * 8), 0));
        anim.Duration = TimeSpan.FromMilliseconds(200);
        v.StartAnimation("Offset", anim);
    }
    private void OnSegmentLeave(object sender, PointerRoutedEventArgs e)
    {
        CenterValue.Text = "100%";
        if (sender is not Path p) return;
        var v = Microsoft.UI.Xaml.Hosting.ElementCompositionPreview.GetElementVisual(p);
        var anim = v.Compositor.CreateVector3KeyFrameAnimation();
        anim.InsertKeyFrame(1f, new System.Numerics.Vector3(0, 0, 0));
        anim.Duration = TimeSpan.FromMilliseconds(200);
        v.StartAnimation("Offset", anim);
    }
}
