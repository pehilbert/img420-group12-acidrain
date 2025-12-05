using Godot;
using System;

public partial class HealthPack : Node2D
{
	[Export]
	public int HealAmount { get; set; } = 25;

	private Area2D _pickupArea;
	private AnimatedSprite2D _anim;

	public override void _Ready()
	{
		// Play animated sprite
		_anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		
		if (_anim == null)
		{
			GD.PushError("HealthPack: AnimatedSprite2D child node not found!");
			return;
		}

		_anim.Play();

		// Get the Area2D child node
		_pickupArea = GetNode<Area2D>("Area2D");
		
		if (_pickupArea == null)
		{
			GD.PushError("HealthPack: Area2D child node not found!");
			return;
		}

		// Connect to the body_entered signal
		_pickupArea.BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player && !player.IsDead)
		{
			if (player.Health < 100)
			{
				player.Heal(HealAmount);
				QueueFree();
			}
		}
	}

	public override void _ExitTree()
	{
		// Clean up signal connection
		if (_pickupArea != null)
		{
			_pickupArea.BodyEntered -= OnBodyEntered;
		}
	}
}
