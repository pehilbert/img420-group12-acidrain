# API Reference

This document summarizes the public API surface that ships with the Acid Rain prototype.

## Native GDExtension (`module_source/acid_rain`)

### `AcidRainManager` (C++)
- **Signals**: `rain_started`, `rain_stopped`, `pre_rain_warning`, `exposure_tick(entity, exposure_time)`.
- **Properties**:
  - `rain_seconds` (`float`): Duration of the damaging phase.
  - `clear_seconds` (`float`): Duration of the safe phase.
  - `warning_time` (`float`): Lead time before rain starts, clamped to `clear_seconds`.
- **Methods**:
  - `start_cycle()`: Starts the clear → warning → rain loop and enables `_process`.
  - `is_raining() -> bool`: True when the manager is inside the rain phase.
  - `get_time_until_rain() -> float`: Seconds remaining until rain starts; returns 0 when already raining.
  - `get_time_remaining_in_phase() -> float`: Remaining seconds inside the current phase.
  - `set_cycle_durations(clear_seconds, rain_seconds)`: Convenience wrapper that clamps via the setters.
  - `register_entity(Node2D *entity)`: Adds a Node2D to the exposure tracker so it can emit `exposure_tick` signals.

### `ExposureTracker` (C++)
- Keeps a list of tracked `Node2D` instances and exposes:
  - `register_entity(Node2D *entity)`.
  - `is_entity_exposed(Node2D *entity) -> bool`: Returns true when no collider blocks the sky above the entity.
  - `get_tracked() -> Vector<Node2D *>`: Read-only access to the tracked entities.

## Gameplay scripts (`game_prototype/scripts`)

### `Player`
Handles movement, attacking, and health tracking. Emits a game over via `GetTree().ChangeSceneToFile("res://scenes/GameOver.tscn")` when health hits 0.

### `Enemy`
Chases the nearest player, deals contact damage, and can be defeated after taking sufficient hits.

### `UI`
Displays the player health percentage and surfaces Acid Rain warnings based on the manager signals.

### `Shelter`
Determines safe areas, optionally instantiates `Enemy` scenes when the player enters, and relays shelter-specific signals to the UI.

### `AcidRainManager` (`scripts`)
C# helper that communicates with the native node instance to display countdown timers and orchestrate scene-level state.

Refer to the inline XML comments inside each script for specific signals and exported properties.
