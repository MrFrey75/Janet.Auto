using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Controls.Shapes;

namespace AudioWorkstation.UI.Controls;

public partial class LevelMeterControl : UserControl
{
    public static readonly StyledProperty<float> LevelProperty =
        AvaloniaProperty.Register<LevelMeterControl, float>(nameof(Level));

    public static readonly StyledProperty<float> PeakLevelProperty =
        AvaloniaProperty.Register<LevelMeterControl, float>(nameof(PeakLevel));

    public static readonly StyledProperty<bool> IsVerticalProperty =
        AvaloniaProperty.Register<LevelMeterControl, bool>(nameof(IsVertical), true);

    public static readonly StyledProperty<bool> ShowScaleProperty =
        AvaloniaProperty.Register<LevelMeterControl, bool>(nameof(ShowScale), true);

    private Canvas? _meterCanvas;
    private Rectangle? _levelBar;
    private Rectangle? _peakHoldBar;
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

    public bool ShowScale
    {
        get => GetValue(ShowScaleProperty);
        set => SetValue(ShowScaleProperty, value);
    }

    static LevelMeterControl()
    {
        LevelProperty.Changed.AddClassHandler<LevelMeterControl>((x, e) => x.UpdateLevel());
        PeakLevelProperty.Changed.AddClassHandler<LevelMeterControl>((x, e) => x.UpdatePeakLevel());
    }

    public LevelMeterControl()
    {
        InitializeComponent();
        
        _peakTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _peakTimer.Tick += PeakTimer_Tick;
        _peakTimer.Start();
        
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _meterCanvas = this.FindControl<Canvas>("PART_MeterCanvas");
        InitializeMeterElements();
    }

    private void InitializeMeterElements()
    {
        if (_meterCanvas == null) return;

        _meterCanvas.Children.Clear();

        // Create background gradient
        var background = new Rectangle
        {
            Width = _meterCanvas.Bounds.Width,
            Height = _meterCanvas.Bounds.Height,
            Fill = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Color.FromRgb(0, 17, 0), 0.0),
                    new GradientStop(Color.FromRgb(0, 34, 0), 0.7),
                    new GradientStop(Color.FromRgb(51, 34, 0), 0.85),
                    new GradientStop(Color.FromRgb(51, 0, 0), 1.0)
                }
            }
        };

        // Create level bar
        _levelBar = new Rectangle
        {
            Width = _meterCanvas.Bounds.Width,
            Height = 0,
            Fill = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Colors.Green, 0.0),
                    new GradientStop(Colors.Yellow, 0.7),
                    new GradientStop(Colors.Orange, 0.85),
                    new GradientStop(Colors.Red, 1.0)
                }
            }
        };

        // Create peak hold bar
        _peakHoldBar = new Rectangle
        {
            Width = _meterCanvas.Bounds.Width,
            Height = 2,
            Fill = Brushes.White,
            IsVisible = false
        };

        Canvas.SetBottom(_levelBar, 0);
        Canvas.SetTop(_peakHoldBar, 0);

        _meterCanvas.Children.Add(background);
        _meterCanvas.Children.Add(_levelBar);
        _meterCanvas.Children.Add(_peakHoldBar);
    }

    private void UpdateLevel()
    {
        if (_levelBar == null || _meterCanvas == null) return;

        var canvasHeight = _meterCanvas.Bounds.Height;
        if (canvasHeight <= 0) return;

        var levelHeight = canvasHeight * Level;
        _levelBar.Height = levelHeight;
        Canvas.SetBottom(_levelBar, 0);
    }

    private void UpdatePeakLevel()
    {
        if (_peakHoldBar == null || _meterCanvas == null) return;

        var canvasHeight = _meterCanvas.Bounds.Height;
        if (canvasHeight <= 0) return;

        var peakY = canvasHeight * (1 - PeakLevel);
        Canvas.SetTop(_peakHoldBar, peakY);
        _peakHoldBar.IsVisible = PeakLevel > 0;
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
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        
        if (_meterCanvas != null && e.NewSize.Width > 0 && e.NewSize.Height > 0)
        {
            Dispatcher.UIThread.Post(() => 
            {
                InitializeMeterElements();
                UpdateLevel();
                UpdatePeakLevel();
            }, DispatcherPriority.Background);
        }
    }
}