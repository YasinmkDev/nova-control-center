using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;
using Windows.UI;

namespace NovaControlCenter.Controls;

/// <summary>
/// Win2D particle field: 120 particles drift, connect when close (&lt;90px), and
/// are repelled by the mouse. Drawn every frame via CanvasControl.Invalidate()
/// on a 60fps DispatcherTimer — GPU canvas, not XAML shapes (which would jank).
/// LIMITATION: WinUI 3 has no native 3D scene graph or HLSL shader material
/// for XAML. True 3D needs Win2D 3D / DirectX interop or a WebView2 shader.
/// This is the closest native GPU visual: Win2D + Composition gradient mesh.
/// </summary>
public sealed partial class ParticleCanvas : UserControl
{
    private sealed class P { public float X, Y, VX, VY, R; public Color C; }
    private readonly List<P> _ps = new();
    private readonly Random _rnd = new(3);
    private Point _mouse = new(-1000, -1000);
    private DispatcherTimer? _timer;
    public ParticleCanvas()
    {
        InitializeComponent();
        Loaded += (_, _) => { _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; _timer.Tick += (_, _) => Canvas.Invalidate(); _timer.Start(); };
        Unloaded += (_, _) => _timer?.Stop();
    }
    private void Canvas_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
    {
        _ps.Clear();
        var cols = new[] { Color.FromArgb(255, 108, 92, 231), Color.FromArgb(255, 0, 210, 255), Color.FromArgb(255, 0, 230, 118) };
        for (int i = 0; i < 120; i++)
            _ps.Add(new P { X = _rnd.Next(0, 800), Y = _rnd.Next(0, 500), VX = (float)(_rnd.NextDouble() * 0.8 - 0.4), VY = (float)(_rnd.NextDouble() * 0.8 - 0.4), R = _rnd.Next(2, 5), C = cols[i % 3] });
    }
    private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e) => _mouse = e.GetCurrentPoint(Canvas).Position;
    private void Canvas_PointerExited(object sender, PointerRoutedEventArgs e) => _mouse = new Point(-1000, -1000);
    private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        var ds = args.DrawingSession;
        float w = (float)sender.ActualWidth, h = (float)sender.ActualHeight;
        // Gradient mesh background: two large radial gradients
        ds.FillCircle(w * 0.25f, h * 0.3f, 220, Color.FromArgb(28, 108, 92, 231));
        ds.FillCircle(w * 0.8f, h * 0.7f, 240, Color.FromArgb(22, 0, 210, 255));
        foreach (var p in _ps)
        {
            p.X += p.VX; p.Y += p.VY;
            if (p.X < 0 || p.X > w) p.VX *= -1;
            if (p.Y < 0 || p.Y > h) p.VY *= -1;
            double dx = p.X - _mouse.X, dy = p.Y - _mouse.Y, d = Math.Sqrt(dx * dx + dy * dy);
            if (d < 110 && d > 1) { p.X += (float)(dx / d * 2.2); p.Y += (float)(dy / d * 2.2); }
            ds.FillCircle(p.X, p.Y, p.R, p.C);
        }
        // Connect near neighbours
        for (int i = 0; i < _ps.Count; i++)
            for (int j = i + 1; j < _ps.Count; j++)
            {
                var a = _ps[i]; var b = _ps[j];
                double d = Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
                if (d < 90) ds.DrawLine(a.X, a.Y, b.X, b.Y, Color.FromArgb((byte)(70 * (1 - d / 90)), 255, 255, 255), 1);
            }
    }
}
