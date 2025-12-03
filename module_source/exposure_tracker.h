#pragma once

#include "core/templates/vector.h"
#include "scene/2d/node_2d.h"

// Tracks entities and determines whether they are exposed to the sky via raycast.
class ExposureTracker {
	Node2D *owner = nullptr;
	Vector<Node2D *> tracked;

public:
	explicit ExposureTracker(Node2D *p_owner);

	void register_entity(Node2D *entity);

	// Returns true if the entity has no collider above it (exposed to "sky").
	bool is_entity_exposed(Node2D *entity) const;

	const Vector<Node2D *> &get_tracked() const;
};
