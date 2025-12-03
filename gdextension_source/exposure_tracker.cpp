#include "exposure_tracker.h"
#include <godot_cpp/classes/world2d.hpp>
#include <godot_cpp/classes/physics_direct_space_state2d.hpp>
#include <godot_cpp/classes/physics_ray_query_parameters2d.hpp>

using namespace godot;

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

	PhysicsDirectSpaceState2D *space_state = world->get_direct_space_state();
	if (!space_state) {
		return false;
	}

	Vector2 from = entity->get_global_position();
	Vector2 to = from + Vector2(0, -10000.0);

	Ref<PhysicsRayQueryParameters2D> params = PhysicsRayQueryParameters2D::create(from, to);
	params->set_collide_with_areas(true);
	params->set_collide_with_bodies(true);

	Dictionary result = space_state->intersect_ray(params);

	// No hit = sky visible = exposed to rain
	return result.is_empty();
}

const Vector<Node2D *> &ExposureTracker::get_tracked() const {
	return tracked;
}
