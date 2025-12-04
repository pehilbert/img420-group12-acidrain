using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 200f;
	[Export] public float Gravity = 1200f;
	[Export] public float JumpVelocity = -400f;
	[Export] public float AttackDuration = 0.4f;

	public float Health = 100;
	public bool IsInsideShelter = false;
	public bool IsDead = false;
	[Export] public AnimatedSprite2D HurtFlash;
	private AnimatedSprite2D _anim;
	private bool _isAttacking = false;
	private float _attackTimeLeft = 0f;
	private bool _isUnderRoof = false;

	// Returns true if player is protected from rain (under shelter OR under any collidable object)
	public bool IsSheltered => IsInsideShelter || _isUnderRoof;

	public override void _Ready()
	{
		_anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		if (IsDead)
			return;

		// Check for roof/shelter above player using direct physics query
		_isUnderRoof = CheckForRoofAbove();

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
	public async void PlayHurtFX()
	{
		if (HurtFlash == null) return;

		HurtFlash.Visible = true;
		HurtFlash.Play("hurt_fx");
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		HurtFlash.Visible = false;
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
	public async void TakeDamage(float dmg)
	{
		if (IsDead)
			return;

		Health -= dmg;

		// Optional hurt flash
		if (HurtFlash != null)
		{
			HurtFlash.Visible = true;
			HurtFlash.Play("hurt_fx");
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
			HurtFlash.Visible = false;
		}

		if (Health <= 0)
		{
			IsDead = true;
			Velocity = Vector2.Zero;   // stop movement
			_anim.Play("death");       // play death animation

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

	private bool CheckForRoofAbove()
	{
		// Use direct physics query to detect objects above the player
		var spaceState = GetWorld2D().DirectSpaceState;
		
		// Cast ray from player's head upward
		var from = GlobalPosition + new Vector2(0, -10); // Start slightly above player center
		var to = GlobalPosition + new Vector2(0, -200);   // Check 200 pixels above
		
		var query = PhysicsRayQueryParameters2D.Create(from, to);
		query.Exclude = new Godot.Collections.Array<Rid> { GetRid() }; // Exclude player
		query.CollideWithBodies = true;
		query.CollideWithAreas = true;
		
		var result = spaceState.IntersectRay(query);
		
		if (result.Count > 0)
		{
			// Something is above the player - they're sheltered!
			// GD.Print("Sheltered by: " + result["collider"]); // Debug
			return true;
		}
		
		return false;
	}
}
