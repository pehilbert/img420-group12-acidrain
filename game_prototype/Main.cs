using Godot;
using System;

public partial class Main : Node2D
{
	[Export]
	public NodePath AcidRainManagerPath { get; set; }

	[Export]
	public float RainDuration { get; set; } = 30.0f;

	[Export]
	public float ClearDuration { get; set; } = 60.0f;

	[Export]
	public float WarningTime { get; set; } = 5.0f;

	[Export]
	public bool EnableDebugPrints { get; set; } = true;

	private Node2D _acidRainManager;
	private double _debugTimer = 0.0;
	private const double DEBUG_INTERVAL = 2.0; // Print debug info every 2 seconds

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

		// Configure the cycle durations
		GD.Print("\nConfiguring cycle durations...");
		GD.Print($"  Clear Duration: {ClearDuration}s");
		GD.Print($"  Rain Duration: {RainDuration}s");
		GD.Print($"  Warning Time: {WarningTime}s");
		
		_acidRainManager.Call("set_cycle_durations", ClearDuration, RainDuration);
		_acidRainManager.Call("set_warning_time", WarningTime);
		GD.Print("  ✓ Cycle durations configured");

		// Register entities that should be tracked for exposure
		GD.Print("\nRegistering tracked entities...");
		RegisterTrackedEntities();

		// Start the rain cycle
		GD.Print("\nStarting rain cycle...");
		_acidRainManager.Call("start_cycle");
		GD.Print("✓ AcidRainManager cycle started!");
		GD.Print("========================================\n");
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
		// Add your logic here: play rain sound, show rain particles, etc.
	}

	// Called when rain stops
	private void OnRainStopped()
	{
		GD.Print("\n┌────────────────────────────┐");
		GD.Print("│  ☀️  RAIN STOPPED  ☀️      │");
		GD.Print("└────────────────────────────┘");
		// Add your logic here: stop rain sound, hide rain particles, etc.
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
		
		// Example: Apply damage or track exposure
		if (entity.HasMethod("TakeDamage"))
		{
			// Apply damage based on exposure time
			float damagePerSecond = 10.0f;
			float damage = damagePerSecond * exposureTime;
			entity.Call("TakeDamage", damage);
			
			if (EnableDebugPrints)
			{
				GD.Print($"  → Applied {damage:F2} damage to {entity.Name}");
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
}
