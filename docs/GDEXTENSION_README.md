# Acid Rain GDExtension for Godot 4.4

This is a GDExtension that provides the `AcidRainManager` node for Godot 4.4.

## Building the Extension

### Prerequisites

1. **Python 3.6+** and **SCons** build system
   ```powershell
   pip install scons
   ```

2. **Visual Studio 2019 or newer** (for Windows builds) with C++ development tools

3. **godot-cpp** - The C++ bindings for Godot 4.4

### Setup Instructions

1. **Clone godot-cpp into the project directory:**
   ```powershell
   cd "c:\Users\Peter\Downloads\Godot_v4.4.1-stable_win64.exe\img420-group12-acidrain"
   git clone https://github.com/godotengine/godot-cpp.git
   cd godot-cpp
   git checkout 4.4
   ```

2. **Build godot-cpp:**
   ```powershell
   # For debug build (recommended during development)
   scons platform=windows target=template_debug
   
   # For release build (for production)
   scons platform=windows target=template_release
   ```
   
   This will take several minutes the first time.

3. **Return to the project root and build the extension:**
   ```powershell
   cd ..
   
   # For debug build
   scons platform=windows target=template_debug
   
   # For release build
   scons platform=windows target=template_release
   ```

### Build Output

The compiled library will be placed in `game_prototype/bin/`:
- Debug: `libacidrain.windows.template_debug.x86_64.dll`
- Release: `libacidrain.windows.template_release.x86_64.dll`

## Using in Godot

1. Open your Godot project (`game_prototype/`)
2. The extension should be automatically loaded via the `acid_rain.gdextension` file
3. You can now use the `AcidRainManager` node in your scenes:
   - Add Node → Node2D → AcidRainManager

## Troubleshooting

### If the extension doesn't load:
- Check that the `.dll` file exists in `game_prototype/bin/`
- Verify the `acid_rain.gdextension` file is in the `game_prototype/` directory
- Check Godot's output console for error messages
- Make sure you built for the correct architecture (x86_64 is standard)

### Build errors:
- Ensure you have Visual Studio C++ tools installed
- Make sure you're in the correct directory when running scons
- Verify godot-cpp was built successfully first

## Platform-Specific Builds

For Linux:
```bash
scons platform=linux target=template_debug
scons platform=linux target=template_release
```

For macOS:
```bash
scons platform=macos target=template_debug
scons platform=macos target=template_release
```

## Development Notes

- Source code is in `gdextension_source/`
- Main class: `AcidRainManager` (inherits from `Node2D`)
- Registration code: `register_types.cpp`
- Build configuration: `SConstruct`

## Next Steps

Add your game logic to `acid_rain.cpp`:
- Override `_ready()`, `_process()`, `_physics_process()` as needed
- Add custom methods and properties
- Bind them in `_bind_methods()` to make them available in GDScript
