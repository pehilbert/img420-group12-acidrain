using Godot;
using System;

public partial class Main : Node2D
{
	[Export]
	public NodePath AcidRainManagerPath { get; set; }
	[Export]
	public int DamagePerSecond { get; set; } = 10;

	[Export]
	public bool EnableDebugPrints { get; set; } = true;
	[Export]
	public AudioStream RainSound { get; set; }

    private GpuParticles2D _rainParticles;

	private Node2D _acidRainManager;
	private Camera2D _camera;
	private double _debugTimer = 0.0;
	private const double DEBUG_INTERVAL = 0.5;
	private AudioStreamPlayer2D _rainSoundPlayer;

	public override void _Ready()
	{
		GD.Print("\n========================================");
		GD.Print("Main._Ready() - Initializing AcidRainManager");
		GD.Print("========================================");

		if (AcidRainManagerPath == null || AcidRainManagerPath.IsEmpty)
		{
			GD.PushError("AcidRainManagerPath is not set in the editor.");
			return;
		}

		GD.Print($"Looking for AcidRainManager at path: {AcidRainManagerPath}");

		_acidRainManager = GetNode<Node2D>(AcidRainManagerPath);

		if (_acidRainManager == null)
		{
			GD.PushError("AcidRainManager node not found at path: " + AcidRainManagerPath);
			return;
		}

		GD.Print($"✓ AcidRainManager found: {_acidRainManager.Name}");
		GD.Print($"✓ Type: {_acidRainManager.GetType().Name}");

		// Connect to all the AcidRainManager signals
		GD.Print("\nConnecting to signals...");
		_acidRainManager.Connect("rain_started", new Callable(this, nameof(OnRainStarted)));
		GD.Print("  ✓ Connected to 'rain_started'");
		
		_acidRainManager.Connect("rain_stopped", new Callable(this, nameof(OnRainStopped)));
		GD.Print("  ✓ Connected to 'rain_stopped'");
		
		_acidRainManager.Connect("pre_rain_warning", new Callable(this, nameof(OnRainWarning)));
		GD.Print("  ✓ Connected to 'pre_rain_warning'");
		
		_acidRainManager.Connect("exposure_tick", new Callable(this, nameof(OnExposureTick)));
		GD.Print("  ✓ Connected to 'exposure_tick'");

		// Register entities that should be tracked for exposure
		GD.Print("\nRegistering tracked entities...");
		RegisterTrackedEntities();

		// Create and setup rain sound player
		_rainSoundPlayer = new AudioStreamPlayer2D();
		_rainSoundPlayer.Stream = RainSound;
		_rainSoundPlayer.Bus = "Master";
		_rainSoundPlayer.Autoplay = false;
		AddChild(_rainSoundPlayer);

		// Start the rain cycle
		GD.Print("\nStarting rain cycle...");
		_acidRainManager.Call("start_cycle");
		GD.Print("✓ AcidRainManager cycle started!");
		GD.Print("========================================\n");

		CreateRainParticles();
	}

	public override void _Process(double delta)
	{
		// You can query the rain manager state at any time
		if (_acidRainManager != null && EnableDebugPrints)
		{
			_debugTimer += delta;
			
			// Print debug info periodically
			if (_debugTimer >= DEBUG_INTERVAL)
			{
				_debugTimer = 0.0;
				
				bool isRaining = (bool)_acidRainManager.Call("is_raining");
				float timeUntilRain = (float)_acidRainManager.Call("get_time_until_rain");
				float timeRemaining = (float)_acidRainManager.Call("get_time_remaining_in_phase");

				GD.Print($"[DEBUG] Status: {(isRaining ? "RAINING" : "CLEAR")} | " +
						 $"Time until rain: {timeUntilRain:F1}s | " +
						 $"Time remaining in phase: {timeRemaining:F1}s");
			}
		}

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

	private void RegisterTrackedEntities()
	{
		int registeredCount = 0;
		
		// Example: Register the player node if it exists
		var player = GetNodeOrNull<Node2D>("Player");
		if (player != null)
		{
			_acidRainManager.Call("register_entity", player);
			GD.Print($"  ✓ Player registered for acid rain exposure tracking");
			registeredCount++;
		}
		else
		{
			GD.Print("  ℹ No 'Player' node found to register");
		}

		// Example: Register all entities in a group
		var entities = GetTree().GetNodesInGroup("acid_rain_vulnerable");
		GD.Print($"  Found {entities.Count} entities in 'acid_rain_vulnerable' group");
		
		foreach (Node entity in entities)
		{
			if (entity is Node2D node2D)
			{
				_acidRainManager.Call("register_entity", node2D);
				GD.Print($"  ✓ Registered entity: {entity.Name}");
				registeredCount++;
			}
		}
		
		GD.Print($"  Total entities registered: {registeredCount}");
	}

	// Called when rain starts
	private void OnRainStarted()
	{
		GD.Print("\n┌────────────────────────────┐");
		GD.Print("│  🌧️  RAIN STARTED  🌧️     │");
		GD.Print("└────────────────────────────┘");
		
		if (_rainParticles != null) _rainParticles.Emitting = true;
		if (_rainSoundPlayer != null && RainSound != null)
		{
			_rainSoundPlayer.Play();
		}
	}

	// Called when rain stops
	private void OnRainStopped()
	{
		GD.Print("\n┌────────────────────────────┐");
		GD.Print("│  ☀️  RAIN STOPPED  ☀️      │");
		GD.Print("└────────────────────────────┘");
		// Add your logic here: stop rain sound, hide rain particles, etc.

		if (_rainParticles != null) _rainParticles.Emitting = false;
		if (_rainSoundPlayer != null && RainSound != null)
		{
			_rainSoundPlayer.Stop();
		}
	}

	// Called before rain starts (warning period)
	private void OnRainWarning()
	{
		GD.Print("\n┌────────────────────────────┐");
		GD.Print("│ ⚠️  PRE-RAIN WARNING ⚠️    │");
		GD.Print("│   Take cover NOW!          │");
		GD.Print("└────────────────────────────┘");
		// Add your logic here: show warning UI, play warning sound, etc.
	}

	// Called every frame for each exposed entity during rain
	private void OnExposureTick(Node2D entity, float exposureTime)
	{
		// This signal is emitted for each exposed entity every frame during rain
		// exposureTime is the delta time (time since last frame)
		
		if (EnableDebugPrints)
		{
			GD.Print($"[EXPOSURE] {entity.Name} exposed for {exposureTime:F3}s (delta time)");
		}
		
		if (entity is Player player)
		{
			// Apply damage based on exposure time and DamagePerSecond
			float damage = DamagePerSecond * exposureTime;
			if (damage > 0)
			{
				player.TakeDamage(damage);
				if (EnableDebugPrints)
				{
					GD.Print($"[DAMAGE] {entity.Name} took {damage} damage from acid rain.");
				}
			}
		}
	}

	// Helper method you can call to manually register new entities at runtime
	public void RegisterEntity(Node2D entity)
	{
		if (_acidRainManager != null && entity != null)
		{
			_acidRainManager.Call("register_entity", entity);
			GD.Print($"[Runtime Registration] Entity registered: {entity.Name}");
		}
	}

	// Helper methods to query rain state from other scripts
	public bool IsRaining()
	{
		return _acidRainManager != null && (bool)_acidRainManager.Call("is_raining");
	}

	public float GetTimeUntilRain()
	{
		return _acidRainManager != null ? (float)_acidRainManager.Call("get_time_until_rain") : 0.0f;
	}

	public float GetTimeRemainingInPhase()
	{
		return _acidRainManager != null ? (float)_acidRainManager.Call("get_time_remaining_in_phase") : 0.0f;
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
}
