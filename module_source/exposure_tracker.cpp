#include "exposure_tracker.h"

#include "scene/resources/world_2d.h"
#include "servers/physics_server_2d.h"

ExposureTracker::ExposureTracker(Node2D *p_owner) {
	owner = p_owner;
}

void ExposureTracker::register_entity(Node2D *entity) {
	if (!entity) {
		return;
	}

	for (int i = 0; i < tracked.size(); ++i) {
		if (tracked[i] == entity) {
			return; // prevent duplicates
		}
	}

	tracked.push_back(entity);
}

bool ExposureTracker::is_entity_exposed(Node2D *entity) const {
	if (!owner || !entity) {
		return false;
	}
	if (!entity->is_inside_tree()) {
		return false;
	}

	Ref<World2D> world = owner->get_world_2d();
	if (world.is_null()) {
		return false;
	}

	PhysicsDirectSpaceState2D *space_state =
			world->get_direct_space_state();
	if (!space_state) {
		return false;
	}

	Vector2 from = entity->get_global_position();
	Vector2 to = from + Vector2(0, -10000.0);

	PhysicsDirectSpaceState2D::RayParameters params;
	params.from = from;
	params.to = to;
	params.collide_with_areas = true;
	params.collide_with_bodies = true;

	PhysicsDirectSpaceState2D::RayResult result;
	bool hit = space_state->intersect_ray(params, result);

	// No hit = sky visible = exposed to rain
	return !hit;
}

const Vector<Node2D *> &ExposureTracker::get_tracked() const {
	return tracked;
}
