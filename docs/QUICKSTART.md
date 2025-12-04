# Quick Start Guide

## One-Time Setup (Required First Time Only)

1. **Install SCons** (if not already installed):
   ```powershell
   pip install scons
   ```

2. **Clone and build godot-cpp** (from project root directory):
   ```powershell
   git clone https://github.com/godotengine/godot-cpp.git
   cd godot-cpp
   git checkout 4.4
   scons platform=windows target=template_debug
   cd ..
   ```

## Build the Extension

From the project root, run:
```powershell
scons platform=windows target=template_debug
```

That's it! The extension will be compiled to `game_prototype/bin/` and Godot will automatically load it.

## Using in Godot

Open your project in Godot 4.4 and the `AcidRainManager` node will be available under Node2D in the scene tree.
