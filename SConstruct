#!/usr/bin/env python
import os
import sys

env = SConscript("godot-cpp/SConstruct")

# For reference:
# - CCFLAGS are compilation flags shared between C and C++
# - CFLAGS are for C-specific compilation flags
# - CXXFLAGS are for C++-specific compilation flags
# - CPPFLAGS are for pre-processor flags
# - CPPDEFINES are for pre-processor defines
# - LINKFLAGS are for linking flags

# tweak this if you want to use different folders, or more folders, to store your source code in.
source_dir = "module_source/acid_rain"
env.Append(CPPPATH=[source_dir])
sources = Glob(f"{source_dir}/*.cpp")

output_dir = "game_prototype/exports/bin"

if env["platform"] == "macos":
    library = env.SharedLibrary(
        "{}/libacidrain.{}.{}.framework/libacidrain.{}.{}".format(
            output_dir, env["platform"], env["target"], env["platform"], env["target"]
        ),
        source=sources,
    )
else:
    library = env.SharedLibrary(
        "{}/libacidrain{}{}".format(output_dir, env["suffix"], env["SHLIBSUFFIX"]),
        source=sources,
    )

Default(library)
