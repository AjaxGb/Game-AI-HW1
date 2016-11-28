using UnityEngine;
using UnityEngine.UI;

public class SteeringEvade : ISteering {
	
	public float maxPrediction;

	public SteeringEvade(float maxPrediction = 2f) {
		this.maxPrediction = maxPrediction;
	}

	private Text nameDisplay;
	private Image targetDisplay;

	public void ShowDebug(DynamicBase source, Canvas canvas) {
		HideDebug(source);
		nameDisplay = canvas.MakeText("Evade Name", source.font, "Dynamic Evade");
		targetDisplay = canvas.MakeDot("Evade Target", source.knob);
		nameDisplay.rectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, source.transform.position);
	}

	public void UpdateDebug(DynamicBase source) {
		if (nameDisplay != null) {
			nameDisplay.rectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, source.transform.position);
		}
	}

	public void HideDebug(DynamicBase source) {
		if (nameDisplay != null) Object.Destroy(nameDisplay.gameObject);
		if (targetDisplay != null) Object.Destroy(targetDisplay.gameObject);
	}

	public void GetSteering(DynamicBase source, Kinematic target, ref Vector2 accel, ref float torque) {
		if (target == null) return;

		// Get prediction length
		Vector2 direction = target.position_u - (Vector2)source.transform.position;
		float distance = direction.magnitude;
		float speed = target.velocity_u_s.magnitude;
		float prediction = maxPrediction;
		//if (speed <= distance / maxPrediction) {
		//	prediction = maxPrediction;
		//} else {
		//	prediction = distance / speed;
		//}

		// Predict and face away from location
		Vector2 predictedPos = target.position_u + target.velocity_u_s * maxPrediction;
		if (targetDisplay != null) {
			targetDisplay.rectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, predictedPos);
		}
		Debug.DrawLine(source.transform.position, predictedPos, Color.yellow);
		target.position_u = predictedPos;
		direction = (Vector2)source.transform.position - target.position_u;
		source.FaceHeading(direction, ref torque);

		// Find target speed + velocity
		float targetSpeed = source.maxSpeed_u_s;
		Vector2 targetVelocity = source.forward * targetSpeed;

		// Find target acceleration
		accel = targetVelocity - source.rb.velocity;
		accel = Vector2.ClampMagnitude(accel, source.maxAccel_u_s2);
	}
}
