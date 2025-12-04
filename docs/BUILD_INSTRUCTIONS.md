# Build Instructions

## Prerequisites
- Godot 4.4 or newer with the .NET workload installed (for C# scripts).
- A compiler toolchain supported by Godot's GDExtension workflow (MSVC on Windows, clang on macOS, gcc/clang on Linux).
- Python 3 and SCons (already vendored via `godot-cpp/SConstruct`).

## Building the native library
1. Initialize the Godot C++ bindings in `godot-cpp` (follow the upstream README to run `scons platform=<platform> target=release` as needed).
2. From the repository root run:
   ```bash
   scons platform=<platform> target=<debug|release>
   ```
   - Sources are loaded from `module_source/acid_rain`.
   - Outputs land in `game_prototype/exports/bin` so the Godot project can load them through `res://exports/bin/...`.
3. Copy the generated binary bundle to teammates if they cannot build locally.

## Running the Godot prototype
1. Open `game_prototype/project.godot` in the Godot editor.
2. Let Godot import the moved assets (`game_prototype/assets`).
3. Ensure the `.NET` solution restores successfully (Godot auto-invokes `dotnet restore`).
4. Press **Play** to load `res://scenes/main.tscn`.
5. To export playable builds, configure export presets in Godot and point the output directory to `game_prototype/exports`.

## Tests
- Automated tests live under `tests/`.
- Place standalone Godot test scenes inside `tests/scenes/` and .NET unit tests inside `tests/unit/`.
- Run `dotnet test` in any future test project that you add under `tests/unit`.
