#include "register_types.h"

#include "core/object/class_db.h"
#include "module_source/acid_rain.h"

void initialize_acid_rain_module(ModuleInitializationLevel p_level) {
	if (p_level != MODULE_INITIALIZATION_LEVEL_SCENE) {
		return;
	}
	ClassDB::register_class<AcidRainManager>();
}

void uninitialize_acid_rain_module(ModuleInitializationLevel p_level) {
	if (p_level != MODULE_INITIALIZATION_LEVEL_SCENE) {
		return;
	}
}
