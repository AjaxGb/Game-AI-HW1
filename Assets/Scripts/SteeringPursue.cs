using UnityEngine;

public class SteeringPursue : ISteering {

	public float slowRadius;
	public float maxPrediction;

	public SteeringPursue(float slowRadius = 5f, float maxPrediction = 2f) {
		this.slowRadius = slowRadius;
		this.maxPrediction = maxPrediction;
	}

	public void GetSteering(DynamicBase source, Kinematic target, ref Vector2 accel, ref float torque) {
		if (target == null) return;

		// Get prediction length
		Vector2 direction = target.position_u - (Vector2)source.transform.position;
		float distance = direction.magnitude;
		float speed = source.rb.velocity.magnitude;
		float prediction;
		if (speed <= distance / maxPrediction) {
			prediction = maxPrediction;
		} else {
			prediction = distance / speed;
		}

		// Predict and face location
		Vector2 predictedPos = target.position_u + target.velocity_u_s * prediction;
		//Debug.DrawLine(source.transform.position, predictedPos, Color.yellow);
		target.position_u = predictedPos;
		source.FacePosition(target.position_u, ref torque);

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

	public void UpdateDebug(DynamicBase source) { }
}
