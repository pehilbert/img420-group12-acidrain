using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 200f;
	[Export] public float Gravity = 1200f;
	[Export] public float JumpVelocity = -400f;
	[Export] public float AttackDuration = 0.4f;
	[Export] public NodePath CollisionTileLayerPath;

	[Export] public int AttackDamage = 100;
	[Export] public NodePath AttackAreaPath;
	public float Health = 100;
	public bool IsInsideShelter = false;
	public bool IsDead = false;
	[Export] public AnimatedSprite2D HurtFlash;
	private AnimatedSprite2D _anim;
	private bool _isAttacking = false;
	private float _attackTimeLeft = 0f;
	private Area2D _attackArea;
	private CollisionShape2D _attackAreaShape;
	private float _attackAreaBaseOffsetX = 0f;
	private readonly HashSet<Enemy> _damagedEnemiesThisSwing = new();
	private bool _isUnderRoof = false;
	private TileMapLayer _collisionTileLayer;
	private CollisionShape2D _collisionShape;
	private Vector2 _collisionHalfExtents = Vector2.Zero;

	// Returns true if player is protected from rain (under shelter OR under any collidable object)
	public bool IsSheltered => IsInsideShelter || _isUnderRoof;

	public override void _Ready()
	{
		_anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		if (_collisionShape?.Shape is RectangleShape2D rectShape)
		{
			_collisionHalfExtents = rectShape.Size * 0.5f;
		}

		if (!CollisionTileLayerPath.IsEmpty)
		{
			_collisionTileLayer = GetNodeOrNull<TileMapLayer>(CollisionTileLayerPath);
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

		// Check for roof/shelter above player using direct physics query
		_isUnderRoof = CheckForRoofAbove();

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

	private bool CheckForRoofAbove()
	{
		if (_collisionTileLayer != null && IsTileCoveringPlayer())
			return true;

		return CheckForRoofAboveWithPhysics();
	}

	private bool IsTileCoveringPlayer()
	{
		if (_collisionTileLayer?.TileSet == null)
			return false;

		int physicsLayerCount = _collisionTileLayer.TileSet.GetPhysicsLayersCount();
		if (physicsLayerCount <= 0)
			return false;

		if (_collisionHalfExtents == Vector2.Zero)
		{
			_collisionHalfExtents = new Vector2(16, 16);
		}

		// Sample a few cells above the player's head to catch roofs even when off-screen
		float inset = Mathf.Min(4f, _collisionHalfExtents.X * 0.9f);
		Vector2[] offsets = new Vector2[]
		{
			new Vector2(-_collisionHalfExtents.X + inset, 0),
			Vector2.Zero,
			new Vector2(_collisionHalfExtents.X - inset, 0)
		};

		foreach (Vector2 offset in offsets)
		{
			Vector2 sampleGlobal = GlobalPosition + new Vector2(offset.X, -_collisionHalfExtents.Y - 4f);
			Vector2 sampleLocal = _collisionTileLayer.ToLocal(sampleGlobal);
			Vector2I cell = _collisionTileLayer.LocalToMap(sampleLocal);

			var tileData = _collisionTileLayer.GetCellTileData(cell);
			if (tileData == null)
				continue;

			for (int layerIdx = 0; layerIdx < physicsLayerCount; layerIdx++)
			{
				if (tileData.GetCollisionPolygonsCount(layerIdx) > 0)
					return true;
			}
		}

		return false;
	}

	private bool CheckForRoofAboveWithPhysics()
	{
		// Use direct physics query to detect objects above the player (fallback)
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
