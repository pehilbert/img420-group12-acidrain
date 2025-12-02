#include "acid_rain.h"
#include "core/object/class_db.h"

void AcidRainManager::_bind_methods() {
	// Bind setters/getters so they appear in the editor and can be used from scripts
	ClassDB::bind_method(D_METHOD("set_rain_seconds", "seconds"), &AcidRainManager::setRainSeconds);
	ClassDB::bind_method(D_METHOD("get_rain_seconds"), &AcidRainManager::getRainSeconds);
	ADD_PROPERTY(PropertyInfo(Variant::FLOAT, "rain_seconds", PROPERTY_HINT_RANGE, "0.1,3600,0.1"), "set_rain_seconds", "get_rain_seconds");

	ClassDB::bind_method(D_METHOD("set_clear_seconds", "seconds"), &AcidRainManager::setClearSeconds);
	ClassDB::bind_method(D_METHOD("get_clear_seconds"), &AcidRainManager::getClearSeconds);
	ADD_PROPERTY(PropertyInfo(Variant::FLOAT, "clear_seconds", PROPERTY_HINT_RANGE, "0.1,3600,0.1"), "set_clear_seconds", "get_clear_seconds");

	ClassDB::bind_method(D_METHOD("set_warning_time", "seconds"), &AcidRainManager::setWarningTime);
	ClassDB::bind_method(D_METHOD("get_warning_time"), &AcidRainManager::getWarningTime);
	ADD_PROPERTY(PropertyInfo(Variant::FLOAT, "warning_time", PROPERTY_HINT_RANGE, "0,600,0.1"), "set_warning_time", "get_warning_time");
}

// Constructor
AcidRainManager::AcidRainManager() {
	// sensible defaults so new nodes have usable values in the editor
	rainSeconds = 30.0f;
	clearSeconds = 60.0f;
	warningTime = 5.0f;
	currentState = AcidRainState::Clear;
}

// Destructor
AcidRainManager::~AcidRainManager() {
}

// Setters / getters
void AcidRainManager::setRainSeconds(float p_rainSeconds) {
	if (p_rainSeconds < 0.0f) {
		p_rainSeconds = 0.0f;
	}
	rainSeconds = p_rainSeconds;
}

float AcidRainManager::getRainSeconds() {
	return rainSeconds;
}

void AcidRainManager::setClearSeconds(float p_clearSeconds) {
	if (p_clearSeconds < 0.0f) {
		p_clearSeconds = 0.0f;
	}
	clearSeconds = p_clearSeconds;
}

float AcidRainManager::getClearSeconds() {
	return clearSeconds;
}

void AcidRainManager::setWarningTime(float p_warningTime) {
	if (p_warningTime < 0.0f) {
		p_warningTime = 0.0f;
	}

	// only allow warning times less than the amount of clear time
	if (p_warningTime < clearSeconds) {
		warningTime = p_warningTime;
	} else {
		warningTime = clearSeconds;
	}
}

float AcidRainManager::getWarningTime() {
	return warningTime;
}
