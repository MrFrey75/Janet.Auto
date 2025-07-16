using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System.Collections.Generic;
using Avalonia.Controls.Shapes;

namespace AudioWorkstation.UI.Controls;

public partial class SpectrumAnalyzerControl : UserControl
{
    public static readonly StyledProperty<float[]> SpectrumDataProperty =
        AvaloniaProperty.Register<SpectrumAnalyzerControl, float[]>(nameof(SpectrumData));

    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<SpectrumAnalyzerControl, bool>(nameof(IsActive), true);

    private Canvas? _spectrumCanvas;
    private readonly List<Rectangle> _spectrumBars = new();
    private readonly DispatcherTimer _updateTimer;

    public float[] SpectrumData
    {
        get => GetValue(SpectrumDataProperty);
        set => SetValue(SpectrumDataProperty, value);
    }

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    static SpectrumAnalyzerControl()
    {
        SpectrumDataProperty.Changed.AddClassHandler<SpectrumAnalyzerControl>((x, e) => x.OnSpectrumDataChanged());
    }

    public SpectrumAnalyzerControl()
    {
        InitializeComponent();
        
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS
        };
        _updateTimer.Tick += UpdateTimer_Tick;
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _spectrumCanvas = this.FindControl<Canvas>("SpectrumCanvas");
        InitializeSpectrumBars();
        
        if (IsActive)
            _updateTimer.Start();
    }

    private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _updateTimer.Stop();
    }

    private void InitializeSpectrumBars()
    {
        if (_spectrumCanvas == null) return;

        _spectrumBars.Clear();
        _spectrumCanvas.Children.Clear();

        const int barCount = 32; // Number of frequency bars
        var canvasWidth = _spectrumCanvas.Bounds.Width;
        var canvasHeight = _spectrumCanvas.Bounds.Height;

        if (canvasWidth <= 0 || canvasHeight <= 0) return;

        var barWidth = canvasWidth / barCount;

        for (int i = 0; i < barCount; i++)
        {
            var bar = new Rectangle
            {
                Width = barWidth - 1,
                Height = 0,
                Fill = CreateSpectrumBrush(),
                Stroke = null
            };

            Canvas.SetLeft(bar, i * barWidth);
            Canvas.SetBottom(bar, 0);

            _spectrumBars.Add(bar);
            _spectrumCanvas.Children.Add(bar);
        }
    }

    private IBrush CreateSpectrumBrush()
    {
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Colors.Green, 0.0),
                new GradientStop(Colors.Yellow, 0.7),
                new GradientStop(Colors.Red, 1.0)
            }
        };
    }

    private void OnSpectrumDataChanged()
    {
        if (!IsActive || _spectrumCanvas == null || SpectrumData == null)
            return;

        UpdateSpectrumDisplay();
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        if (IsActive && SpectrumData != null)
        {
            UpdateSpectrumDisplay();
        }
    }

    private void UpdateSpectrumDisplay()
    {
        if (_spectrumCanvas == null || SpectrumData == null || _spectrumBars.Count == 0)
            return;

        var canvasHeight = _spectrumCanvas.Bounds.Height;
        if (canvasHeight <= 0) return;

        var dataLength = SpectrumData.Length;
        var barCount = _spectrumBars.Count;

        for (int i = 0; i < barCount && i < dataLength; i++)
        {
            var magnitude = SpectrumData[i];
            
            // Convert dB to 0-1 range (assuming -60dB to 0dB range)
            var normalizedMagnitude = Math.Max(0, (magnitude + 60) / 60);
            normalizedMagnitude = Math.Min(1, normalizedMagnitude);

            var barHeight = normalizedMagnitude * canvasHeight;
            _spectrumBars[i].Height = barHeight;

            // Update position from bottom
            Canvas.SetBottom(_spectrumBars[i], 0);
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        
        // Reinitialize bars when size changes
        if (_spectrumCanvas != null && e.NewSize.Width > 0 && e.NewSize.Height > 0)
        {
            Dispatcher.UIThread.Post(() => InitializeSpectrumBars(), DispatcherPriority.Background);
        }
    }
}