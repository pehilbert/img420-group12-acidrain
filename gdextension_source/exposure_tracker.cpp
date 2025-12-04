#include "exposure_tracker.h"
#include <godot_cpp/classes/world2d.hpp>
#include <godot_cpp/classes/physics_direct_space_state2d.hpp>
#include <godot_cpp/classes/physics_ray_query_parameters2d.hpp>
#include <godot_cpp/variant/typed_array.hpp>
#include <godot_cpp/classes/collision_object2d.hpp>
#include <godot_cpp/classes/character_body2d.hpp>
#include <godot_cpp/variant/utility_functions.hpp>

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
		UtilityFunctions::print("[ExposureTracker] No owner or entity");
		return false;
	}
	if (!entity->is_inside_tree()) {
		UtilityFunctions::print("[ExposureTracker] Entity not in tree: ", entity->get_name());
		return false;
	}

	Ref<World2D> world = owner->get_world_2d();
	if (world.is_null()) {
		UtilityFunctions::print("[ExposureTracker] No world2d");
		return false;
	}

	PhysicsDirectSpaceState2D *space_state = world->get_direct_space_state();
	if (!space_state) {
		UtilityFunctions::print("[ExposureTracker] No space state");
		return false;
	}

	Vector2 from = entity->get_global_position();
	Vector2 to = from + Vector2(0, -10000.0);
	Ref<PhysicsRayQueryParameters2D> params = PhysicsRayQueryParameters2D::create(from, to);
	params->set_collide_with_areas(true);
	params->set_collide_with_bodies(true);
	
	// Exclude the entity itself from the raycast (if it's a CollisionObject2D)
	CollisionObject2D *collision_obj = Object::cast_to<CollisionObject2D>(entity);
	if (collision_obj) {
		TypedArray<RID> exclude;
		exclude.append(collision_obj->get_rid());
		params->set_exclude(exclude);
		UtilityFunctions::print("[ExposureTracker] Excluding collision for: ", entity->get_name());
	} else {
		UtilityFunctions::print("[ExposureTracker] Entity is not CollisionObject2D: ", entity->get_name());
	}

	Dictionary result = space_state->intersect_ray(params);

	bool exposed = result.is_empty();
	
	// If we hit something, check if it's a CharacterBody2D - if so, ignore it
	if (!exposed && result.has("collider")) {
		Object *collider = Object::cast_to<Object>(result["collider"]);
		CharacterBody2D *char_body = Object::cast_to<CharacterBody2D>(collider);
		if (char_body) {
			UtilityFunctions::print("[ExposureTracker] Ignoring CharacterBody2D: ", char_body->get_name());
			exposed = true; // Treat as exposed since we ignore character bodies
		}
	}
	
	UtilityFunctions::print("[ExposureTracker] ", entity->get_name(), " at ", from, " exposed: ", exposed, " (hit: ", !exposed, ")");
	
	if (!exposed && result.has("collider")) {
		Object *collider = Object::cast_to<Object>(result["collider"]);
		if (collider) {
			Node *collider_node = Object::cast_to<Node>(collider);
			if (collider_node) {
				UtilityFunctions::print("  Hit collider: ", collider_node->get_name());
			}
		}
	}

	// No hit = sky visible = exposed to rain
	return exposed;
}

const Vector<Node2D *> &ExposureTracker::get_tracked() const {
	return tracked;
}
