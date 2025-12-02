using Godot;

public partial class AcidRainManager : Node
{
    [Signal] public delegate void RainStartedEventHandler();
    [Signal] public delegate void RainStoppedEventHandler();

    [Export] public float RainDuration = 5.0f;
    [Export] public float DryDuration = 8.0f;

    private Timer _rainTimer;
    private Timer _dryTimer;
    private bool _isRaining = false;

    public override void _Ready()
    {
        _rainTimer = new Timer();
        _rainTimer.WaitTime = RainDuration;
        _rainTimer.OneShot = true;
        _rainTimer.Timeout += OnRainTimerTimeout;
        AddChild(_rainTimer);

        _dryTimer = new Timer();
        _dryTimer.WaitTime = DryDuration;
        _dryTimer.OneShot = true;
        _dryTimer.Timeout += OnDryTimerTimeout;
        AddChild(_dryTimer);

        // Start with dry period
        _dryTimer.Start();
    }

    private void OnDryTimerTimeout()
    {
        _isRaining = true;
        EmitSignal(SignalName.RainStarted);
        _rainTimer.Start();
    }

    private void OnRainTimerTimeout()
    {
        _isRaining = false;
        EmitSignal(SignalName.RainStopped);
        _dryTimer.Start();
    }

    public bool IsRaining => _isRaining;
}

