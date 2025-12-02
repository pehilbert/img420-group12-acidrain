using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 200f;
	[Export] public float Gravity = 1200f;
	[Export] public float JumpVelocity = -400f;
	[Export] public float AttackDuration = 0.4f;

	private AnimatedSprite2D _anim;
	private bool _isAttacking = false;
	private float _attackTimeLeft = 0f;

	public override void _Ready()
	{
		_anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		// Update attack timer
		if (_isAttacking)
		{
			_attackTimeLeft -= dt;
			if (_attackTimeLeft <= 0f)
				_isAttacking = false;
		}

		ApplyGravity(dt);
		HandleMovement();
		HandleAttack();
		UpdateAnimation();
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
	}

	private void HandleAttack()
	{
		// "attack" is an input action bound to left mouse button
		if (Input.IsActionJustPressed("attack") && !_isAttacking)
		{
			_isAttacking = true;
			_attackTimeLeft = AttackDuration;
			_anim.Play("attack");
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
}
