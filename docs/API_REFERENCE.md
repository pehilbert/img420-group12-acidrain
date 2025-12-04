# AcidRainManager API Reference

## Overview

The `AcidRainManager` is a custom GDExtension class that manages an acid rain cycle system in Godot 4.4. It handles state transitions between clear weather and acid rain, tracks entities exposed to the sky, and emits signals for game events.

**Inherits:** `Node2D`  
**Location:** Add as a child node to your scene

---

## State Cycle

The AcidRainManager operates through three distinct states that cycle automatically:

Clear → Warning → Raining → Clear → ...

### States

1. **Clear** - Safe period with no acid rain
   - Duration: `clear_seconds` - `warning_time`
   - No damage to exposed entities
   - Players can move freely outdoors

2. **Warning** - Pre-rain alert period
   - Duration: `warning_time`
   - Triggers `pre_rain_warning` signal
   - Gives players time to seek shelter
   - No damage yet

3. **Raining** - Active acid rain period
   - Duration: `rain_seconds`
   - Emits `exposure_tick` signals for exposed entities
   - Entities under cover are protected

---

## Properties

### `rain_seconds`
**Type:** `float`  

Duration (in seconds) of the active rain phase where exposed entities take damage.

```gdscript
# GDScript
rain_manager.rain_seconds = 45.0
```

```csharp
// C#
rainManager.RainSeconds = 45.0f;
```

---

### `clear_seconds`
**Type:** `float`

Duration (in seconds) of the clear weather phase. This includes both the safe period and the warning period. Actual safe time = `clear_seconds - warning_time`.

```gdscript
# GDScript
rain_manager.clear_seconds = 90.0
```

```csharp
// C#
rainManager.ClearSeconds = 90.0f;
```

---

### `warning_time`
**Type:** `float`  
**Note:** Cannot exceed `clear_seconds`

Duration (in seconds) of the warning phase before rain begins. This time is subtracted from the end of the clear phase.

```gdscript
# GDScript
rain_manager.warning_time = 10.0
```

```csharp
// C#
rainManager.WarningTime = 10.0f;
```

---

## Methods

### `start_cycle()`
**Returns:** `void`

Starts the acid rain cycle. Initializes the system in the Clear state and begins processing.

```gdscript
# GDScript
func _ready():
    rain_manager.start_cycle()
```

```csharp
// C#
public override void _Ready()
{
    rainManager.StartCycle();
}
```

---

### `is_raining()`
**Returns:** `bool`

Returns `true` if currently in the Raining state, `false` otherwise.

```gdscript
# GDScript
if rain_manager.is_raining():
    show_rain_effects()
```

```csharp
// C#
if (rainManager.IsRaining())
{
    ShowRainEffects();
}
```

---

### `get_time_until_rain()`
**Returns:** `float`

Returns the number of seconds until rain begins. Returns `0.0` if already raining.

Useful for displaying countdown timers to players.

```gdscript
# GDScript
var countdown = rain_manager.get_time_until_rain()
label.text = "Rain in: %.1f seconds" % countdown
```

```csharp
// C#
float countdown = rainManager.GetTimeUntilRain();
label.Text = $"Rain in: {countdown:F1} seconds";
```

---

### `get_time_remaining_in_phase()`
**Returns:** `float`

Returns the number of seconds remaining in the current phase (Clear, Warning, or Raining).

```gdscript
# GDScript
var remaining = rain_manager.get_time_remaining_in_phase()
```

```csharp
// C#
float remaining = rainManager.GetTimeRemainingInPhase();
```

---

### `set_cycle_durations(clear_seconds: float, rain_seconds: float)`
**Returns:** `void`

Convenience method to set both `clear_seconds` and `rain_seconds` at once.

```gdscript
# GDScript
rain_manager.set_cycle_durations(120.0, 60.0)
```

```csharp
// C#
rainManager.SetCycleDurations(120.0f, 60.0f);
```

---

### `register_entity(entity: Node2D)`
**Returns:** `void`

Registers an entity (typically the player or NPCs) for exposure tracking. Registered entities will be checked for sky exposure during the rain phase.

**Important:** Call this method for each entity that should take acid rain damage.

```gdscript
# GDScript
func _ready():
    var player = get_node("Player")
    rain_manager.register_entity(player)
```

```csharp
// C#
public override void _Ready()
{
    var player = GetNode<Node2D>("Player");
    rainManager.RegisterEntity(player);
}
```

---

## Signals

### `rain_started`
**Parameters:** None

Emitted when transitioning from Warning to Raining state.

Use this to:
- Start rain visual effects
- Play rain sound effects
- Update UI to show active rain

```gdscript
# GDScript
func _ready():
    rain_manager.rain_started.connect(_on_rain_started)

func _on_rain_started():
    rain_particles.emitting = true
    rain_sound.play()
```

```csharp
// C#
public override void _Ready()
{
    rainManager.RainStarted += OnRainStarted;
}

private void OnRainStarted()
{
    rainParticles.Emitting = true;
    rainSound.Play();
}
```

---

### `rain_stopped`
**Parameters:** None

Emitted when transitioning from Raining to Clear state.

Use this to:
- Stop rain visual effects
- Stop rain sound effects
- Update UI to show clear weather

```gdscript
# GDScript
func _on_rain_stopped():
    rain_particles.emitting = false
    rain_sound.stop()
```

```csharp
// C#
private void OnRainStopped()
{
    rainParticles.Emitting = false;
    rainSound.Stop();
}
```

---

### `pre_rain_warning`
**Parameters:** None

Emitted when transitioning from Clear to Warning state.

Use this to:
- Display warning UI elements
- Play warning sound
- Alert the player to seek shelter

```gdscript
# GDScript
func _on_pre_rain_warning():
    warning_label.visible = true
    warning_sound.play()
```

```csharp
// C#
private void OnPreRainWarning()
{
    warningLabel.Visible = true;
    warningSound.Play();
}
```

---

### `exposure_tick(entity: Node2D, exposure_time: float)`
**Parameters:**
- `entity` - The Node2D that is currently exposed to acid rain
- `exposure_time` - The delta time (in seconds) since last tick

Emitted every frame during the Raining state for each registered entity that is exposed to the sky.

**Important:** Only entities that have been registered via `register_entity()` will trigger this signal.

Use this to:
- Apply damage to exposed entities
- Show damage indicators
- Track cumulative exposure

```gdscript
# GDScript
func _on_exposure_tick(entity: Node2D, exposure_time: float):
    if entity == player:
        var damage = ACID_DAMAGE_PER_SECOND * exposure_time
        player.take_damage(damage)
```

```csharp
// C#
private void OnExposureTick(Node2D entity, float exposureTime)
{
    if (entity == player)
    {
        float damage = AcidDamagePerSecond * exposureTime;
        player.TakeDamage(damage);
    }
}
```

---

## Exposure Tracking System

The AcidRainManager uses a raycast-based system to determine if entities are exposed to the sky.

### How It Works

1. **Registration**: Entities must be registered using `register_entity()`
2. **Raycasting**: During the Raining state, the system performs upward raycasts from each entity
3. **Collision Detection**: If the raycast hits a collider, the entity is considered "under cover"
4. **Signal Emission**: If no collider is hit (exposed to sky), an `exposure_tick` signal is emitted

### Raycast Details

- **Direction**: Straight up (`Vector2(0, -1)`)
- **Length**: 1000 pixels (configurable in code)
- **Collisions**: 
  - Ignores the entity's own collider (self-exclusion)
  - Ignores `CharacterBody2D` nodes (allows player/NPC colliders to not block rain)
  - Detects `StaticBody2D`, `TileMap`, and other physics bodies

### Creating Shelter

To create areas that protect entities from acid rain:

1. **Use physics colliders**: Add `StaticBody2D` or `TileMap` with collision shapes above entities
2. **Roof tiles**: In TileMaps, ensure roof tiles have collision layers enabled
3. **Coverage area**: Colliders must be positioned above (lower Y value) the entity

## Troubleshooting

### Entities always take damage (even under shelter)
- **Cause**: Shelter colliders may not be positioned correctly
- **Solution**: Ensure collision shapes are above (negative Y) the entities
- **Check**: Enable "Visible Collision Shapes" in Godot editor (Debug menu)

### `exposure_tick` signal not firing
- **Cause**: Entity not registered
- **Solution**: Call `register_entity(entity)` before `start_cycle()`

### Cycle not starting
- **Cause**: `start_cycle()` not called
- **Solution**: Call `start_cycle()` in `_ready()` or after scene initialization

---

## Technical Notes

- The AcidRainManager uses Godot's `_process()` method for frame-by-frame updates
- Raycasts use `PhysicsDirectSpaceState2D` for efficient collision detection
- Entity self-collision is automatically excluded using RID filtering
- CharacterBody2D nodes are filtered out to allow player movement under other characters
- All timing uses delta time for frame-rate independent behavior

---

## Version Information

**GDExtension Version:** 1.0  
**Godot Version:** 4.4.1  
**Platform:** Windows, Linux, macOS

---

## License

This GDExtension is part of the Acid Rain game project. See project LICENSE for details.
