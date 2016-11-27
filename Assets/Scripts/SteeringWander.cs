using UnityEngine;

public class SteeringWander : ISteering {

	public float circleDisplacement;
	public float circleRadius;

	public SteeringWander(float circleDisplacement = 11f, float circleRadius = 10f) {
		this.circleDisplacement = circleDisplacement;
		this.circleRadius = circleRadius;
	}

	public void getSteering(DynamicBase source, Kinematic target, ref Vector2 accel, ref float torque) {
		Vector2 position = (Vector2)source.transform.position;
		float angle = Random.Range(0, 2 * Mathf.PI);
		Vector2 circleCenter = position + source.forward * circleDisplacement;
		Vector2 wanderTarget = circleCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * circleRadius;
		Debug.DrawLine(position, circleCenter);
		Debug.DrawLine(circleCenter, wanderTarget);
		source.Seek(wanderTarget, ref accel);
		Debug.DrawRay(position, accel * 5, Color.black);
		source.FaceHeading(accel, ref torque);

		float cRot = source.transform.rotation.eulerAngles.z * Mathf.Deg2Rad - Mathf.PI / 2;
		Debug.DrawRay(position, new Vector2(Mathf.Cos(torque + cRot), Mathf.Sin(torque + cRot)), Color.yellow);
	}

}
