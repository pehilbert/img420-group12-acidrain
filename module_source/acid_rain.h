#include "scene/2d/node_2d.h"
#include "exposure_tracker.h"

enum AcidRainState {
	Raining, Clear, Warning
};

class AcidRainManager : public Node2D {
	GDCLASS(AcidRainManager, Node2D);

public:
	AcidRainManager();
	~AcidRainManager();
	
	// Property setters/getters
	void setRainSeconds(float p_rainSeconds);
	float getRainSeconds();
	void setClearSeconds(float p_clearSeconds);
	float getClearSeconds();
	void setWarningTime(float p_warningTime);
	float getWarningTime();

	// Cycle control methods
	void start_cycle();
	bool is_raining() const;
	float get_time_until_rain() const;
	float get_time_remaining_in_phase() const;
	void set_cycle_durations(float p_clear_seconds, float p_rain_seconds);

	// Entity tracking methods
	void register_entity(Node2D *entity);

	float rainSeconds;
	float clearSeconds;
	float warningTime;
	AcidRainState currentState;

protected:
	static void _bind_methods();
	void _notification(int p_what);

private:
	bool cycleActive;
	float phaseTimer;
	bool warningEmitted;
	ExposureTracker *exposureTracker;

	void _process(double delta);
	void _transition_to_warning();
	void _transition_to_raining();
	void _transition_to_clear();
	void _emit_exposure_ticks(double delta);
};
