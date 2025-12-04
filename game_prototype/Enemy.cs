using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public float Speed = 120f;
	[Export] public float DetectionRange = 250f;
	[Export] public int Damage = 10;
	[Export] public float AttackCooldown = 1.0f;

	private Player _player;
	private Timer _attackTimer;
	private AnimatedSprite2D _anim;

	public override void _Ready()
	{
		_player = GetTree().GetFirstNodeInGroup("player") as Player;

		_attackTimer = new Timer();
		_attackTimer.WaitTime = AttackCooldown;
		_attackTimer.OneShot = true;
		AddChild(_attackTimer);

		_anim = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_player == null || _player.IsDead)
			return;

		float distance = GlobalPosition.DistanceTo(_player.GlobalPosition);

		if (distance < DetectionRange)
		{
			Vector2 dir = (_player.GlobalPosition - GlobalPosition).Normalized();
			Velocity = dir * Speed;
			MoveAndSlide();
		}
		else
		{
			Velocity = Vector2.Zero;
		}
	}

	private void _on_Hitbox_body_entered(Node2D body)
	{
		if (body is Player player)
		{
			if (_attackTimer.IsStopped())
			{
				player.TakeDamage(Damage);
				_attackTimer.Start();
			}
		}
	}
}
