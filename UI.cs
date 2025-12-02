using Godot;

public partial class UI : CanvasLayer
{
    private TextureProgressBar _bar;
    private Player _player;

    public override void _Ready()
    {
        _bar = GetNode<TextureProgressBar>("HealthBar");
        _player = GetTree().GetFirstNodeInGroup("player") as Player;
        _bar.MaxValue = 100;
    }

    public override void _Process(double delta)
    {
        if (_player != null)
            _bar.Value = _player.Health;
    }
}
