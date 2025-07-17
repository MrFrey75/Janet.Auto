using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace AudioWorkstation.UI.Controls;

public partial class TransportControl : UserControl
{
    public event EventHandler? PlayClicked;
    public event EventHandler? PauseClicked;
    public event EventHandler? StopClicked;
    public event EventHandler? RecordClicked;
    public event EventHandler<bool>? LoopChanged;
    public event EventHandler<bool>? MetronomeChanged;

    public TransportControl()
    {
        InitializeComponent();
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        var playButton = this.FindControl<Button>("PlayButton");
        var pauseButton = this.FindControl<Button>("PauseButton");
        var stopButton = this.FindControl<Button>("StopButton");
        var recordButton = this.FindControl<Button>("RecordButton");
        var loopButton = this.FindControl<ToggleButton>("LoopButton");
        var metronomeButton = this.FindControl<ToggleButton>("MetronomeButton");

        playButton!.Click += (s, e) => PlayClicked?.Invoke(this, EventArgs.Empty);
        pauseButton!.Click += (s, e) => PauseClicked?.Invoke(this, EventArgs.Empty);
        stopButton!.Click += (s, e) => StopClicked?.Invoke(this, EventArgs.Empty);
        recordButton!.Click += (s, e) => RecordClicked?.Invoke(this, EventArgs.Empty);
        
        // Use IsCheckedChanged instead of Checked/Unchecked
        loopButton!.IsCheckedChanged += (s, e) => 
        {
            if (loopButton.IsChecked.HasValue)
                LoopChanged?.Invoke(this, loopButton.IsChecked.Value);
        };
        
        metronomeButton!.IsCheckedChanged += (s, e) => 
        {
            if (metronomeButton.IsChecked.HasValue)
                MetronomeChanged?.Invoke(this, metronomeButton.IsChecked.Value);
        };
    }
}