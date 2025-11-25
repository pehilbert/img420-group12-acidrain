#include "scene/2d/node_2d.h"

class AcidRainManager : public Node2D {
	GDCLASS(AcidRainManager, Node2D);

public:
	AcidRainManager();
	~AcidRainManager();

protected:
	static void _bind_methods();
};
