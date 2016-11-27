using UnityEngine;

public class SteeringEvade : ISteering {

	public float slowRadius;

	public SteeringEvade(float slowRadius = 5f) {
		this.slowRadius = slowRadius;
	}

	public void getSteering(DynamicBase source, Kinematic target, ref Vector2 accel, ref float torque) {
		if (target == null) return;

		Vector2 direction = (Vector2)source.transform.position - target.position_u;
		source.FaceHeading(direction, ref torque);

		// Find target speed + velocity
		float targetSpeed = source.maxSpeed_u_s;
		Vector2 targetDist = target.position_u - (Vector2)source.transform.position;
		if (targetDist.sqrMagnitude < slowRadius * slowRadius) {
			targetSpeed *= targetDist.magnitude / slowRadius;
		}
		Vector2 targetVelocity = source.forward * targetSpeed;

		// Find target acceleration
		accel = targetVelocity - source.rb.velocity;
		accel = Vector2.ClampMagnitude(accel, source.maxAccel_u_s2);
	}
}
