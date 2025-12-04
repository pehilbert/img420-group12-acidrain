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
	private GpuParticles2D _rainParticles;
	private Camera2D _camera;

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

		// Create rain particle effect
		CreateRainParticles();

		// Start with dry period
		_dryTimer.Start();
	}

	private void CreateRainParticles()
	{
		_rainParticles = new GpuParticles2D();
		_rainParticles.Amount = 500;
		_rainParticles.Lifetime = 0.8;
		_rainParticles.Emitting = false;
		_rainParticles.ZIndex = 10; // Render on top so rain is visible

		// Create a simple raindrop texture (white rectangle that will be tinted purple)
		var texture = new GradientTexture2D();
		texture.Width = 2;
		texture.Height = 8;
		texture.Fill = GradientTexture2D.FillEnum.Linear;
		texture.FillFrom = new Vector2(0.5f, 0);
		texture.FillTo = new Vector2(0.5f, 1);
		var texGradient = new Gradient();
		texGradient.SetColor(0, new Color(1, 1, 1, 1));
		texGradient.SetColor(1, new Color(1, 1, 1, 0.3f));
		texture.Gradient = texGradient;
		_rainParticles.Texture = texture;

		// Create the particle material
		var material = new ParticleProcessMaterial();
		
		// Direction: falling down with slight angle
		material.Direction = new Vector3(0.2f, 1, 0);
		material.Spread = 5f;
		
		// Speed - fast rain
		material.InitialVelocityMin = 500f;
		material.InitialVelocityMax = 700f;
		
		// Gravity to make rain fall faster
		material.Gravity = new Vector3(50, 600, 0);
		
		// Emission shape - wide rectangle above the screen
		material.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box;
		material.EmissionBoxExtents = new Vector3(400, 5, 0);
		
		// Purple color for acid rain
		material.Color = new Color(0.7f, 0.3f, 1.0f, 0.85f); // Bright purple
		
		// Scale for rain drops
		material.ScaleMin = 1.0f;
		material.ScaleMax = 2.0f;

		_rainParticles.ProcessMaterial = material;
		
		// Set visibility rect so particles render even when emitter is off-screen
		_rainParticles.VisibilityRect = new Rect2(-500, -100, 1000, 600);

		AddChild(_rainParticles);
	}

	public override void _Process(double delta)
	{
		// Find camera and follow it so rain is always visible on screen
		if (_camera == null)
		{
			var player = GetTree().GetFirstNodeInGroup("player") as Node2D;
			if (player != null)
			{
				_camera = player.GetNodeOrNull<Camera2D>("Camera2D");
			}
		}

		if (_camera != null && _rainParticles != null)
		{
			// Position rain above the camera view
			_rainParticles.GlobalPosition = _camera.GlobalPosition + new Vector2(0, -300);
		}
	}

	private void OnDryTimerTimeout()
	{
		_isRaining = true;
		if (_rainParticles != null)
			_rainParticles.Emitting = true;
		EmitSignal(SignalName.RainStarted);
		_rainTimer.Start();
	}

	private void OnRainTimerTimeout()
	{
		_isRaining = false;
		if (_rainParticles != null)
			_rainParticles.Emitting = false;
		EmitSignal(SignalName.RainStopped);
		_dryTimer.Start();
	}

	public bool IsRaining => _isRaining;
}
