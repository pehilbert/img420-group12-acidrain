using Godot;

public partial class Main : Node2D
{
    private Player _player;
    private AcidRainManager _rainManager;
    private bool _raining = false;
    private float _damageTimer = 0f;
    private const float DamageInterval = 0.5f; // Damage every 0.5 seconds

    public override void _Ready()
    {
        _player = GetTree().GetFirstNodeInGroup("player") as Player;
        _rainManager = GetNode<AcidRainManager>("AcidRainManager");
        
        _rainManager.RainStarted += OnRainStarted;
        _rainManager.RainStopped += OnRainStopped;
    }

    private void OnRainStarted()
    {
        _raining = true;
        GD.Print("Acid rain started!");
    }

    private void OnRainStopped()
    {
        _raining = false;
        GD.Print("Acid rain stopped.");
    }

    public override void _Process(double delta)
    {
        if (_player == null || _player.IsDead)
            return;

        if (_raining && !_player.IsInsideShelter)
        {
            _damageTimer += (float)delta;
            if (_damageTimer >= DamageInterval)
            {
                _player.TakeDamage(5);
                _damageTimer = 0f;
            }
        }
        else
        {
            _damageTimer = 0f;
        }
    }
}
