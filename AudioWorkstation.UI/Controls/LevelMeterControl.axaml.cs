using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace AudioWorkstation.UI.Controls;

public partial class LevelMeterControl : Control
{
    public static readonly StyledProperty<float> LevelProperty =
        AvaloniaProperty.Register<LevelMeterControl, float>(nameof(Level));

    public static readonly StyledProperty<float> PeakLevelProperty =
        AvaloniaProperty.Register<LevelMeterControl, float>(nameof(PeakLevel));

    public static readonly StyledProperty<bool> IsVerticalProperty =
        AvaloniaProperty.Register<LevelMeterControl, bool>(nameof(IsVertical), true);

    // Add missing styled properties that the XAML is trying to set
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        Border.BackgroundProperty.AddOwner<LevelMeterControl>();

    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        Border.BorderBrushProperty.AddOwner<LevelMeterControl>();

    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        Border.BorderThicknessProperty.AddOwner<LevelMeterControl>();

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        Border.CornerRadiusProperty.AddOwner<LevelMeterControl>();

    public static readonly StyledProperty<bool> ShowScaleProperty =
        AvaloniaProperty.Register<LevelMeterControl, bool>(nameof(ShowScale), true);

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

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public bool ShowScale
    {
        get => GetValue(ShowScaleProperty);
        set => SetValue(ShowScaleProperty, value);
    }

    static LevelMeterControl()
    {
        AffectsRender<LevelMeterControl>(LevelProperty, PeakLevelProperty, IsVerticalProperty, 
            BackgroundProperty, BorderBrushProperty, BorderThicknessProperty, CornerRadiusProperty);
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

    private void PeakTimer_Tick(object? sender, EventArgs e)
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
        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        // Draw border
        if (BorderThickness.Top > 0 && BorderBrush != null)
        {
            var borderRect = new Rect(bounds.Size);
            var pen = new Pen(BorderBrush, BorderThickness.Top);
            context.DrawRectangle(null, pen, borderRect, CornerRadius.TopLeft, CornerRadius.TopRight, 
                CornerRadius.BottomRight, CornerRadius.BottomLeft);
        }

        // Background
        if (Background != null)
        {
            var backgroundRect = new Rect(bounds.Size);
            context.FillRectangle(Background, backgroundRect);
        }

        var contentBounds = bounds.Deflate(BorderThickness);

        // Level bar
        var levelBrush = CreateLevelBrush();
        Rect levelRect;

        if (IsVertical)
        {
            var levelHeight = contentBounds.Height * Level;
            levelRect = new Rect(contentBounds.X, contentBounds.Bottom - levelHeight, contentBounds.Width, levelHeight);
        }
        else
        {
            var levelWidth = contentBounds.Width * Level;
            levelRect = new Rect(contentBounds.X, contentBounds.Y, levelWidth, contentBounds.Height);
        }

        context.FillRectangle(levelBrush, levelRect);

        // Peak hold line
        var peakBrush = Brushes.White;
        if (IsVertical)
        {
            var peakY = contentBounds.Bottom - (contentBounds.Height * PeakLevel);
            context.FillRectangle(peakBrush, new Rect(contentBounds.X, peakY - 1, contentBounds.Width, 2));
        }
        else
        {
            var peakX = contentBounds.X + (contentBounds.Width * PeakLevel);
            context.FillRectangle(peakBrush, new Rect(peakX - 1, contentBounds.Y, 2, contentBounds.Height));
        }

        // Scale markings
        if (ShowScale)
        {
            DrawScale(context, contentBounds);
        }
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
        if (!ShowScale || bounds.Width < 30) return; // Don't draw scale if too small

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
                var y = bounds.Bottom - (bounds.Height * scalePoints[i]);
                var lineStart = new Point(bounds.Right - 5, y);
                var lineEnd = new Point(bounds.Right, y);
                context.DrawLine(new Pen(lineBrush, 1), lineStart, lineEnd);
                
                var text = new FormattedText(scaleLabels[i], CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, textBrush);
                context.DrawText(text, new Point(bounds.Right - 25, y - text.Height / 2));
            }
            else
            {
                var x = bounds.X + (bounds.Width * scalePoints[i]);
                var lineStart = new Point(x, bounds.Bottom - 5);
                var lineEnd = new Point(x, bounds.Bottom);
                context.DrawLine(new Pen(lineBrush, 1), lineStart, lineEnd);
                
                var text = new FormattedText(scaleLabels[i], CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, textBrush);
                context.DrawText(text, new Point(x - text.Width / 2, bounds.Bottom - 15));
            }
        }
    }