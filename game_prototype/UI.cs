using Godot;

public partial class UI : CanvasLayer
{
	private ProgressBar _bar;
	private Label _percentLabel;
	private Player _player;

	public override void _Ready()
	{
		_bar = GetNode<ProgressBar>("HealthContainer/HealthBar");
		_percentLabel = GetNode<Label>("HealthContainer/PercentLabel");
		_player = GetTree().GetFirstNodeInGroup("player") as Player;
		_bar.MaxValue = 100;
	}

	public override void _Process(double delta)
	{
		if (_player != null)
		{
			int health = _player.Health;
			_bar.Value = health;
			_percentLabel.Text = $" {health}%";
		}
	}
}
