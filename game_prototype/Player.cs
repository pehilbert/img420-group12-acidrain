using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 200f;
	[Export] public float Gravity = 1200f;
	[Export] public float JumpVelocity = -400f;
	[Export] public float AttackDuration = 0.4f;

	[Export] public int AttackDamage = 100;
	[Export] public NodePath AttackAreaPath;
	public float Health = 100;
	public bool IsInsideShelter = false;
	public bool IsDead = false;
	private AnimatedSprite2D _anim;
	private bool _isAttacking = false;
	private float _attackTimeLeft = 0f;
	private Area2D _attackArea;
	private CollisionShape2D _attackAreaShape;
	private float _attackAreaBaseOffsetX = 0f;
	private readonly HashSet<Enemy> _damagedEnemiesThisSwing = new();
	private TileMapLayer _collisionTileLayer;
	private CollisionShape2D _collisionShape;
	private Vector2 _collisionHalfExtents = Vector2.Zero;


	public override void _Ready()
	{
		_anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		if (_collisionShape?.Shape is RectangleShape2D rectShape)
		{
			_collisionHalfExtents = rectShape.Size * 0.5f;
		}

		if (AttackAreaPath != null && !AttackAreaPath.IsEmpty)
		{
			_attackArea = GetNodeOrNull<Area2D>(AttackAreaPath);
		}
		else
		{
			_attackArea = GetNodeOrNull<Area2D>("AttackArea");
		}

		if (_attackArea != null)
		{
			_attackAreaBaseOffsetX = _attackArea.Position.X;
			_attackAreaShape = _attackArea.GetNodeOrNull<CollisionShape2D>("AttackCollision") ?? _attackArea.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
			_attackArea.BodyEntered += OnAttackAreaBodyEntered;
			SetAttackAreaActive(false);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		if (IsDead)
			return;

		// Update attack timer
		if (_isAttacking)
		{
			_attackTimeLeft -= dt;
			DamageEnemiesInRange();
			if (_attackTimeLeft <= 0f)
			{
				_isAttacking = false;
				SetAttackAreaActive(false);
				_damagedEnemiesThisSwing.Clear();
			}
		}

		ApplyGravity(dt);
		HandleMovement();
		HandleAttack();
		UpdateAnimation();
	}
	public async void PlayHurtFX()
	{
		_anim.Modulate = new Color(1, 0.5f, 0.5f); // Tint red
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		_anim.Modulate = new Color(1, 1, 1); // Reset color
	}


	private void ApplyGravity(float dt)
	{
		// Apply gravity when not on the floor
		if (!IsOnFloor())
		{
			Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * dt);
		}
		else if (Velocity.Y > 0f)
		{
			// Clear any downward leftover velocity when touching floor
			Velocity = new Vector2(Velocity.X, 0f);
		}
	}

	private void HandleMovement()
	{
		Vector2 velocity = Velocity;

		// Horizontal movement (disabled while attacking)
		float inputX = 0f;
		if (!_isAttacking)
		{
			inputX = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
		}

		velocity.X = inputX * Speed;

		// Jump (only when on floor and not attacking)
		if (!_isAttacking && IsOnFloor() && Input.IsActionJustPressed("jump"))
		{
			velocity.Y = JumpVelocity;
		}

		Velocity = velocity;
		MoveAndSlide();

		// Flip sprite left/right
		if (inputX != 0)
		{
			_anim.FlipH = inputX < 0;
		}

		UpdateAttackAreaFacing();
	}

	private void HandleAttack()
	{
		// "attack" is an input action bound to left mouse button
		if (Input.IsActionJustPressed("attack") && !_isAttacking)
		{
			_isAttacking = true;
			_attackTimeLeft = AttackDuration;
			_anim.Play("attack");
			_damagedEnemiesThisSwing.Clear();
			SetAttackAreaActive(true);
			DamageEnemiesInRange();
		}
	}
	public async void TakeDamage(float dmg)
	{
		if (IsDead)
			return;

		Health -= dmg;

		// Optional hurt flash
		PlayHurtFX();

		if (Health <= 0)
		{
			IsDead = true;
			Velocity = Vector2.Zero;   // stop movement
			_anim.Play("dead");       // play death animation

			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
			GetTree().ChangeSceneToFile("res://GameOver.tscn");
		}
	}


	private void UpdateAnimation()
	{
		// During attack, let the attack animation play
		if (_isAttacking)
			return;

		// On ground: run or idle
		if (IsOnFloor())
		{
			if (Mathf.Abs(Velocity.X) > 0.1f)
			{
				if (_anim.Animation != "run")
					_anim.Play("run");
			}
			else
			{
				if (_anim.Animation != "idle")
					_anim.Play("idle");
			}
		}
		else
		{
			// Optional: if you have a "jump" animation, you can play it here
			// _anim.Play("jump");
		}
	}

	private void UpdateAttackAreaFacing()
	{
		if (_attackArea == null)
			return;

		float facingDir = (_anim != null && _anim.FlipH) ? -1f : 1f;
		Vector2 pos = _attackArea.Position;
		pos.X = _attackAreaBaseOffsetX * facingDir;
		_attackArea.Position = pos;
	}

	private void DamageEnemiesInRange()
	{
		if (!_isAttacking || _attackArea == null)
			return;

		Godot.Collections.Array<Node2D> bodies = _attackArea.GetOverlappingBodies();
		foreach (Node2D body in bodies)
		{
			if (body is Enemy enemy)
				DamageEnemy(enemy);
		}
	}

	private void DamageEnemy(Enemy enemy)
	{
		if (!_isAttacking || enemy == null)
			return;

		if (_damagedEnemiesThisSwing.Add(enemy))
		{
			enemy.Kill();
		}
	}

	private void OnAttackAreaBodyEntered(Node2D body)
	{
		if (body is Enemy enemy)
			DamageEnemy(enemy);
	}

	private void SetAttackAreaActive(bool active)
	{
		if (_attackArea == null)
			return;

		_attackArea.Monitoring = active;
		if (_attackAreaShape != null)
			_attackAreaShape.Disabled = !active;
	}
}
