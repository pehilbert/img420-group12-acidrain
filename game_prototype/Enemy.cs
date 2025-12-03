using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public float Speed = 120f;
	[Export] public float DetectionRange = 250f;
	[Export] public int Damage = 33;
	[Export] public float AttackCooldown = 1.0f;
	[Export] public int MaxHealth = 100;

	private Player _player;
	private Timer _attackTimer;
	private AnimatedSprite2D _anim;
	private Sprite2D _sprite;
	private Area2D _hitbox;
	private int _currentHealth;
	private bool _isDead = false;

	public override void _Ready()
	{
		_player = GetTree().GetFirstNodeInGroup("player") as Player;
		_currentHealth = MaxHealth;

		_attackTimer = new Timer();
		_attackTimer.WaitTime = AttackCooldown;
		_attackTimer.OneShot = true;
		AddChild(_attackTimer);

		_anim = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		_hitbox = GetNodeOrNull<Area2D>("Hitbox");
		_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_player == null || _player.IsDead || _isDead)
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

	public void TakeDamage(int amount)
	{
		if (_isDead)
			return;

		_currentHealth -= amount;
		if (_currentHealth <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		_isDead = true;
		Velocity = Vector2.Zero;
		_hitbox?.SetDeferred("monitoring", false);

		bool hasDeathAnim = _anim?.SpriteFrames?.HasAnimation("death") == true;
		if (_anim != null && hasDeathAnim)
		{
			_anim.Play("death");
			_anim.AnimationFinished += OnEnemyAnimationFinished;
		}
		else
		{
			_sprite?.Hide();
			QueueFree();
		}
	}

	private void OnEnemyAnimationFinished()
	{
		_anim.AnimationFinished -= OnEnemyAnimationFinished;
		QueueFree();
	}

	public void Kill()
	{
		if (_isDead)
			return;

		_currentHealth = 0;
		Die();
	}
}
