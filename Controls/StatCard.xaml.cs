using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;

namespace NovaControlCenter.Controls;

/// <summary>Count-up + Composition hover lift. Target is animated with a DispatcherTimer lerp (easeOutCubic), 60fps, no Storyboard layout cost.</summary>
public sealed partial class StatCard : UserControl
{
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(StatCard), new PropertyMetadata("", (d, e) => ((StatCard)d).LabelText.Text = (string)e.NewValue));
    public static readonly DependencyProperty TargetValueProperty = DependencyProperty.Register(nameof(TargetValue), typeof(double), typeof(StatCard), new PropertyMetadata(0.0, (d, e) => ((StatCard)d).AnimateTo((double)e.NewValue)));
    public static readonly DependencyProperty DeltaProperty = DependencyProperty.Register(nameof(Delta), typeof(string), typeof(StatCard), new PropertyMetadata("", (d, e) => ((StatCard)d).DeltaText.Text = (string)e.NewValue));
    public static readonly DependencyProperty HintProperty = DependencyProperty.Register(nameof(Hint), typeof(string), typeof(StatCard), new PropertyMetadata("", (d, e) => ((StatCard)d).HintText.Text = (string)e.NewValue));
    public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(nameof(Format), typeof(string), typeof(StatCard), new PropertyMetadata("N0"));

    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public double TargetValue { get => (double)GetValue(TargetValueProperty); set => SetValue(TargetValueProperty, value); }
    public string Delta { get => (string)GetValue(DeltaProperty); set => SetValue(DeltaProperty, value); }
    public string Hint { get => (string)GetValue(HintProperty); set => SetValue(HintProperty, value); }
    public string Format { get => (string)GetValue(FormatProperty); set => SetValue(FormatProperty, value); }

    private DispatcherTimer? _timer; private double _from, _to; private DateTime _start;
    public StatCard()
    {
        InitializeComponent();
        PointerEntered += (_, _) => Lift(1.02f);
        PointerExited += (_, _) => Lift(1f);
    }
    private void Lift(float scale)
    {
        var v = ElementCompositionPreview.GetElementVisual(Card);
        var a = v.Compositor.CreateScalarKeyFrameAnimation();
        a.InsertKeyFrame(1f, scale); a.Duration = TimeSpan.FromMilliseconds(180);
        v.CenterPoint = new System.Numerics.Vector3((float)ActualWidth / 2, (float)ActualHeight / 2, 0);
        v.StartAnimation("Scale.X", a); v.StartAnimation("Scale.Y", a);
    }
    public void AnimateTo(double to)
    {
        _from = double.TryParse(ValueText.Text.Replace(",", "").Replace("%", ""), out var cur) ? cur : 0;
        _to = to; _start = DateTime.Now;
        _timer ??= new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick -= OnTick; _timer.Tick += OnTick; _timer.Start();
    }
    private void OnTick(object? s, object e)
    {
        double t = Math.Min(1, (DateTime.Now - _start).TotalMilliseconds / 700);
        double eased = 1 - Math.Pow(1 - t, 3);
        ValueText.Text = (_from + (_to - _from) * eased).ToString(Format);
        if (t >= 1) _timer?.Stop();
    }
}
