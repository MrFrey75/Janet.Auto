using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace AudioWorkstation.UI.Controls;

public class LevelMeterControl : Control
{
    public static readonly StyledProperty<float> LevelProperty =
        AvaloniaProperty.Register<LevelMeterControl, float>(nameof(Level));

    public static readonly StyledProperty<float> PeakLevelProperty =
        AvaloniaProperty.Register<LevelMeterControl, float>(nameof(PeakLevel));

    public static readonly StyledProperty<bool> IsVerticalProperty =
        AvaloniaProperty.Register<LevelMeterControl, bool>(nameof(IsVertical), true);

    private float _peakHoldLevel;
    private DateTime _peakHoldTime;
    private readonly DispatcherTimer _peakTimer;

    public float Level
    {
        get => GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    public float PeakLevel
    {
        get => GetValue(PeakLevelProperty);
        set => SetValue(PeakLevelProperty, value);
    }

    public bool IsVertical
    {
        get => GetValue(IsVerticalProperty);
        set => SetValue(IsVerticalProperty, value);
    }

    static LevelMeterControl()
    {
        AffectsRender<LevelMeterControl>(LevelProperty, PeakLevelProperty, IsVerticalProperty);
    }

    public LevelMeterControl()
    {
        _peakTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _peakTimer.Tick += PeakTimer_Tick;
        _peakTimer.Start();
    }

    private void PeakTimer_Tick(object sender, EventArgs e)
    {
        if (Level > _peakHoldLevel)
        {
            _peakHoldLevel = Level;
            _peakHoldTime = DateTime.Now;
            PeakLevel = _peakHoldLevel;
        }
        else if ((DateTime.Now - _peakHoldTime).TotalSeconds > 1.0)
        {
            _peakHoldLevel *= 0.95f; // Decay peak hold
            PeakLevel = _peakHoldLevel;
        }

        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        // Background
        context.FillRectangle(Brushes.Black, bounds);

        // Level bar
        var levelBrush = CreateLevelBrush();
        Rect levelRect;

        if (IsVertical)
        {
            var levelHeight = bounds.Height * Level;
            levelRect = new Rect(0, bounds.Height - levelHeight, bounds.Width, levelHeight);
        }
        else
        {
            var levelWidth = bounds.Width * Level;
            levelRect = new Rect(0, 0, levelWidth, bounds.Height);
        }

        context.FillRectangle(levelBrush, levelRect);

        // Peak hold line
        var peakBrush = Brushes.White;
        if (IsVertical)
        {
            var peakY = bounds.Height - (bounds.Height * PeakLevel);
            context.FillRectangle(peakBrush, new Rect(0, peakY - 1, bounds.Width, 2));
        }
        else
        {
            var peakX = bounds.Width * PeakLevel;
            context.FillRectangle(peakBrush, new Rect(peakX - 1, 0, 2, bounds.Height));
        }

        // Scale markings
        DrawScale(context, bounds);
    }

    private IBrush CreateLevelBrush()
    {
        return new LinearGradientBrush
        {
            StartPoint = IsVertical ? new RelativePoint(0, 1, RelativeUnit.Relative) : new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = IsVertical ? new RelativePoint(0, 0, RelativeUnit.Relative) : new RelativePoint(1, 0, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Colors.Green, 0.0),
                new GradientStop(Colors.Yellow, 0.75),
                new GradientStop(Colors.Orange, 0.9),
                new GradientStop(Colors.Red, 1.0)
            }
        };
    }

    private void DrawScale(DrawingContext context, Rect bounds)
    {
        var textBrush = Brushes.LightGray;
        var lineBrush = Brushes.Gray;
        var typeface = new Typeface("Arial");
        var fontSize = 8;

        // Draw scale marks at -60, -40, -20, -10, -6, -3, 0 dB
        var scalePoints = new[] { 0.0f, 0.1f, 0.25f, 0.5f, 0.75f, 0.9f, 1.0f };
        var scaleLabels = new[] { "-60", "-40", "-20", "-10", "-6", "-3", "0" };

        for (int i = 0; i < scalePoints.Length; i++)
        {
            if (IsVertical)
            {
                var y = bounds.Height - (bounds.Height * scalePoints[i]);
                context.DrawLine(new Pen(lineBrush, 1), new Point(bounds.Width - 5, y), new Point(bounds.Width, y));
                
                var text = new FormattedText(scaleLabels[i], CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, textBrush);
                context.DrawText(text, new Point(bounds.Width - 20, y - text.Height / 2));
            }
            else
            {
                var x = bounds.Width * scalePoints[i];
                context.DrawLine(new Pen(lineBrush, 1), new Point(x, bounds.Height - 5), new Point(x, bounds.Height));
                
                var text = new FormattedText(scaleLabels[i], CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, textBrush);
                context.DrawText(text, new Point(x - text.Width / 2, bounds.Height - 15));
            }
        }
    }
}