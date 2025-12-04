using Godot;
using System;
using System.Collections.Generic;

public partial class Shelter : Area2D
{
	[Export] public PackedScene EnemyScene;
	[Export(PropertyHint.Range, "1,10,1")] public int EnemiesPerShelter = 2;
	[Export] public float DefaultEnemySpacing = 48f;
	[Export] public NodePath[] EnemySpawnPointPaths = Array.Empty<NodePath>();

	private readonly List<Node2D> _spawnMarkers = new();
	private bool _enemiesSpawned = false;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;

		EnsureEnemySceneLoaded();
		CacheSpawnMarkers();
		CallDeferred(nameof(SpawnEnemiesDeferred));
	}

	private void EnsureEnemySceneLoaded()
	{
		if (EnemyScene != null)
		{
			GD.Print($"Shelter '{Name}' using preassigned enemy scene '{EnemyScene.ResourcePath}'.");
			return;
		}

		const string defaultEnemyPath = "res://scenes/Enemy.tscn";
		if (ResourceLoader.Exists(defaultEnemyPath))
		{
			EnemyScene = ResourceLoader.Load<PackedScene>(defaultEnemyPath);
			GD.Print($"Shelter '{Name}' auto-loaded enemy scene '{defaultEnemyPath}'.");
		}
		else
		{
			GD.PushWarning($"Shelter at {GlobalPosition} could not load default enemy scene '{defaultEnemyPath}'. No enemies will spawn.");
		}
	}

	private void CacheSpawnMarkers()
	{
		if (EnemySpawnPointPaths != null && EnemySpawnPointPaths.Length > 0)
		{
			foreach (NodePath path in EnemySpawnPointPaths)
			{
				if (path.IsEmpty)
					continue;

				Node2D marker = GetNodeOrNull<Node2D>(path);
				if (marker != null)
				{
					_spawnMarkers.Add(marker);
					GD.Print($"Shelter '{Name}' registered spawn marker '{marker.Name}'.");
				}
				else
				{
					GD.PushWarning($"Shelter '{Name}' could not find spawn marker at path '{path}'.");
				}
			}
		}

		if (_spawnMarkers.Count > 0)
			return;

		// Fallback: look for Marker2D/Node2D children with "EnemySpawn" prefix.
		foreach (Node child in GetChildren())
		{
			if (child is Node2D node2D && node2D.Name.ToString().StartsWith("EnemySpawn", StringComparison.OrdinalIgnoreCase))
			{
				_spawnMarkers.Add(node2D);
				GD.Print($"Shelter '{Name}' auto-detected spawn marker '{node2D.Name}'.");
			}
		}
	}

	private void SpawnEnemiesDeferred()
	{
		SpawnEnemiesInternal();
	}

	private void SpawnEnemiesInternal()
	{
		if (_enemiesSpawned || EnemyScene == null || EnemiesPerShelter <= 0)
			return;

		Node parent = GetTree().CurrentScene ?? GetParent();
		if (parent == null)
		{
			GD.PushWarning($"Shelter '{Name}' failed to find a parent scene to spawn enemies.");
			return;
		}

		for (int i = 0; i < EnemiesPerShelter; i++)
		{
			Node enemyInstance = EnemyScene.Instantiate();
			parent.AddChild(enemyInstance);

			if (enemyInstance is Node2D node2D)
			{
				node2D.GlobalPosition = GetSpawnPositionForIndex(i);
			}
			GD.Print($"Shelter '{Name}' spawned enemy #{i + 1} at {GetSpawnPositionForIndex(i)}");
		}

		_enemiesSpawned = true;
	}

	private Vector2 GetSpawnPositionForIndex(int index)
	{
		if (_spawnMarkers.Count > 0)
		{
			Node2D marker = _spawnMarkers[index % _spawnMarkers.Count];
			return marker.GlobalPosition;
		}

		float centerOffset = (EnemiesPerShelter - 1) * 0.5f;
		float xOffset = (index - centerOffset) * DefaultEnemySpacing;
		return GlobalPosition + new Vector2(xOffset, 0);
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player p)
			p.IsInsideShelter = true;
	}

	private void OnBodyExited(Node2D body)
	{
		if (body is Player p)
			p.IsInsideShelter = false;
	}
}
