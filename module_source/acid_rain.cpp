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

	// Bind cycle control methods
	ClassDB::bind_method(D_METHOD("start_cycle"), &AcidRainManager::start_cycle);
	ClassDB::bind_method(D_METHOD("is_raining"), &AcidRainManager::is_raining);
	ClassDB::bind_method(D_METHOD("get_time_until_rain"), &AcidRainManager::get_time_until_rain);
	ClassDB::bind_method(D_METHOD("get_time_remaining_in_phase"), &AcidRainManager::get_time_remaining_in_phase);
	ClassDB::bind_method(D_METHOD("set_cycle_durations", "clear_seconds", "rain_seconds"), &AcidRainManager::set_cycle_durations);

	// Bind entity tracking methods
	ClassDB::bind_method(D_METHOD("register_entity", "entity"), &AcidRainManager::register_entity);

	// Bind signals
	ADD_SIGNAL(MethodInfo("rain_started"));
	ADD_SIGNAL(MethodInfo("rain_stopped"));
	ADD_SIGNAL(MethodInfo("pre_rain_warning"));
	ADD_SIGNAL(MethodInfo("exposure_tick", PropertyInfo(Variant::OBJECT, "entity", PROPERTY_HINT_NODE_TYPE, "Node2D"), PropertyInfo(Variant::FLOAT, "exposure_time")));
}

// Constructor
AcidRainManager::AcidRainManager() {
	// sensible defaults so new nodes have usable values in the editor
	rainSeconds = 30.0f;
	clearSeconds = 60.0f;
	warningTime = 5.0f;
	currentState = AcidRainState::Clear;
	cycleActive = false;
	phaseTimer = 0.0f;
	warningEmitted = false;
	exposureTracker = memnew(ExposureTracker(this));
}

// Destructor
AcidRainManager::~AcidRainManager() {
	if (exposureTracker) {
		memdelete(exposureTracker);
		exposureTracker = nullptr;
	}
}

void AcidRainManager::_notification(int p_what) {
	switch (p_what) {
		case NOTIFICATION_PROCESS: {
			if (cycleActive) {
				_process(get_process_delta_time());
			}
		} break;
	}
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

// Cycle control methods
void AcidRainManager::start_cycle() {
	cycleActive = true;
	currentState = AcidRainState::Clear;
	phaseTimer = 0.0f;
	warningEmitted = false;
	set_process(true);
}

bool AcidRainManager::is_raining() const {
	return currentState == AcidRainState::Raining;
}

float AcidRainManager::get_time_until_rain() const {
	if (currentState == AcidRainState::Raining) {
		return 0.0f;
	}
	
	// In Clear or Warning state, return time until rain starts
	if (currentState == AcidRainState::Clear) {
		float timeUntilWarning = (clearSeconds - warningTime) - phaseTimer;
		if (timeUntilWarning > 0.0f) {
			// Still in clear phase before warning
			return timeUntilWarning + warningTime;
		} else {
			// In warning phase
			return warningTime - (phaseTimer - (clearSeconds - warningTime));
		}
	}
	
	// In Warning state
	if (currentState == AcidRainState::Warning) {
		return warningTime - phaseTimer;
	}
	
	return 0.0f;
}

float AcidRainManager::get_time_remaining_in_phase() const {
	if (currentState == AcidRainState::Raining) {
		return rainSeconds - phaseTimer;
	} else if (currentState == AcidRainState::Clear) {
		return (clearSeconds - warningTime) - phaseTimer;
	} else if (currentState == AcidRainState::Warning) {
		return warningTime - phaseTimer;
	}
	return 0.0f;
}

void AcidRainManager::set_cycle_durations(float p_clear_seconds, float p_rain_seconds) {
	setClearSeconds(p_clear_seconds);
	setRainSeconds(p_rain_seconds);
}

void AcidRainManager::register_entity(Node2D *entity) {
	if (exposureTracker) {
		exposureTracker->register_entity(entity);
	}
}

// Private methods
void AcidRainManager::_process(double delta) {
	phaseTimer += delta;

	// State transitions
	if (currentState == AcidRainState::Clear) {
		// Check if we need to emit warning
		if (!warningEmitted && warningTime > 0.0f && phaseTimer >= (clearSeconds - warningTime)) {
			_transition_to_warning();
		}
		// Check if clear phase is over (no warning time)
		else if (warningTime <= 0.0f && phaseTimer >= clearSeconds) {
			_transition_to_raining();
		}
	} else if (currentState == AcidRainState::Warning) {
		// Check if warning phase is over
		if (phaseTimer >= warningTime) {
			_transition_to_raining();
		}
	} else if (currentState == AcidRainState::Raining) {
		// Emit exposure ticks for exposed entities
		_emit_exposure_ticks(delta);

		// Check if rain phase is over
		if (phaseTimer >= rainSeconds) {
			_transition_to_clear();
		}
	}
}

void AcidRainManager::_transition_to_warning() {
	currentState = AcidRainState::Warning;
	phaseTimer = 0.0f;
	warningEmitted = true;
	emit_signal("pre_rain_warning");
}

void AcidRainManager::_transition_to_raining() {
	currentState = AcidRainState::Raining;
	phaseTimer = 0.0f;
	emit_signal("rain_started");
}

void AcidRainManager::_transition_to_clear() {
	currentState = AcidRainState::Clear;
	phaseTimer = 0.0f;
	warningEmitted = false;
	emit_signal("rain_stopped");
}

void AcidRainManager::_emit_exposure_ticks(double delta) {
	if (!exposureTracker) {
		return;
	}

	const Vector<Node2D *> &tracked = exposureTracker->get_tracked();
	for (int i = 0; i < tracked.size(); ++i) {
		Node2D *entity = tracked[i];
		if (entity && exposureTracker->is_entity_exposed(entity)) {
			emit_signal("exposure_tick", entity, (float)delta);
		}
	}
}
