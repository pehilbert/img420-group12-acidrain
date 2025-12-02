#include "scene/2d/node_2d.h"

enum AcidRainState {
	Raining, Clear
};

class AcidRainManager : public Node2D {
	GDCLASS(AcidRainManager, Node2D);

public:
	AcidRainManager();
	~AcidRainManager();
	void setRainSeconds(float p_rainSeconds);
	float getRainSeconds();
	void setClearSeconds(float p_clearSeconds);
	float getClearSeconds();
	void setWarningTime(float p_warningTime);
	float getWarningTime();

	float rainSeconds;
	float clearSeconds;
	float warningTime;
	AcidRainState currentState;

protected:
	static void _bind_methods();
};
