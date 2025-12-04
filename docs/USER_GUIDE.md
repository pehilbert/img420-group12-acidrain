# User Guide

## Overview
The prototype demonstrates an acid rain survival loop: the player explores a ruined landscape, collects health packs, and seeks shelter whenever the AcidRainManager announces an incoming storm.

## Controls
- **Move**: `A/D`, Left/Right arrow keys, or a gamepad's left stick.
- **Jump**: `Space`.
- **Attack**: Left mouse button.

## Gameplay Flow
1. Watch the HUD for rain warnings emitted a few seconds before each storm.
2. Reach a shelter (wood-and-metal structures) to avoid the exposure ticks dealt during rain phases.
3. Pick up `HealthPack` pickups to regain health after taking damage.
4. Defeat enemies that spawn near shelters and block your path.
5. Survive as many cycles as possible without letting health reach zero.

## Tips
- The warning timer is shorter than the clear window; move early to avoid being caught in the open.
- Multiple shelters can be activated, but each will only spawn enemies the first time you arrive.
- When playing exported builds, copy the `exports/bin` folder next to the executable so the native module can load.
