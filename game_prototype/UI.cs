using Godot;

public partial class UI : CanvasLayer
{
	[Export]
	public NodePath AcidRainManagerPath { get; set; }
	private ProgressBar _bar;
	private Label _percentLabel;
	private Label _warningLabel;
	private Player _player;
	private Node2D _acidRainManager;

	public override void _Ready()
	{
		_bar = GetNode<ProgressBar>("HealthContainer/HealthBar");
		_percentLabel = GetNode<Label>("HealthContainer/PercentLabel");
		_player = GetTree().GetFirstNodeInGroup("player") as Player;
		_bar.MaxValue = 100;
		_warningLabel = GetNode<Label>("WarningText");
		_warningLabel.Visible = false;

		// Set AcidRainManager
		_acidRainManager = GetNode<Node2D>(AcidRainManagerPath);

		if (_acidRainManager == null)
		{
			GD.PushError("UI: AcidRainManager node not found at path: " + AcidRainManagerPath);
			return;
		}

		_acidRainManager.Connect("pre_rain_warning", new Callable(this, nameof(OnWarning)));
		_acidRainManager.Connect("rain_started", new Callable(this, nameof(OnRainStart)));
	}

	public override void _Process(double delta)
	{
		if (_player != null)
		{
			int health = (int)_player.Health;
			_bar.Value = health;
			_percentLabel.Text = $" {health}%";
		}
	}

	private void OnWarning()
	{
		_warningLabel.Visible = true;
	}

	private void OnRainStart()
	{
		_warningLabel.Visible = false;
	}
}
