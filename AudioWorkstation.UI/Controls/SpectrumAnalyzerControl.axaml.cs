using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using AudioWorkstation.Core.Services;

namespace AudioWorkstation.UI.Controls;

public class SpectrumAnalyzerControl : Control
{
    public static readonly StyledProperty<float[]> SpectrumDataProperty =
        AvaloniaProperty.Register<SpectrumAnalyzerControl, float[]>(nameof(SpectrumData));

    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<SpectrumAnalyzerControl, bool>(nameof(IsActive), true);

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
        AffectsRender<SpectrumAnalyzerControl>(SpectrumDataProperty, IsActiveProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (!IsActive || SpectrumData == null || SpectrumData.Length == 0)
            return;

        var bounds = Bounds;
        var width = bounds.Width;
        var height = bounds.Height;

        if (width <= 0 || height <= 0)
            return;

        using var geometryGroup = new GeometryGroup();
        var barWidth = width / SpectrumData.Length;
        
        for (int i = 0; i < SpectrumData.Length; i++)
        {
            var magnitude = Math.Max(0, (SpectrumData[i] + 60) / 60); // Normalize -60dB to 0dB range
            var barHeight = magnitude * height;
            
            var rect = new Rect(i * barWidth, height - barHeight, barWidth - 1, barHeight);
            var rectGeometry = new RectangleGeometry(rect);
            geometryGroup.Children.Add(rectGeometry);
        }

        // Create gradient brush
        var gradientBrush = new LinearGradientBrush
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

        context.DrawGeometry(gradientBrush, null, geometryGroup);
    }
}