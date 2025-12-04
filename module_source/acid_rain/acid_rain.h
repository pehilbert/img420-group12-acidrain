#ifndef ACID_RAIN_H
#define ACID_RAIN_H

#include <godot_cpp/classes/node2d.hpp>
#include "exposure_tracker.h"

using namespace godot;

enum AcidRainState {
	Raining, Clear, Warning
};

class AcidRainManager : public Node2D {
	GDCLASS(AcidRainManager, Node2D)

public:
	AcidRainManager();
	~AcidRainManager();
	
	// Property setters/getters
	void set_rain_seconds(float p_rainSeconds);
	float get_rain_seconds() const;
	void set_clear_seconds(float p_clearSeconds);
	float get_clear_seconds() const;
	void set_warning_time(float p_warningTime);
	float get_warning_time() const;

	// Cycle control methods
	void start_cycle();
	bool is_raining() const;
	float get_time_until_rain() const;
	float get_time_remaining_in_phase() const;
	void set_cycle_durations(float p_clear_seconds, float p_rain_seconds);

	// Entity tracking methods
	void register_entity(Node2D *entity);

	void _process(double delta) override;

protected:
	static void _bind_methods();

private:
	float rainSeconds;
	float clearSeconds;
	float warningTime;
	AcidRainState currentState;
	bool cycleActive;
	float phaseTimer;
	bool warningEmitted;
	ExposureTracker *exposureTracker;

	void _transition_to_warning();
	void _transition_to_raining();
	void _transition_to_clear();
	void _emit_exposure_ticks(double delta);
};

#endif // ACID_RAIN_H
