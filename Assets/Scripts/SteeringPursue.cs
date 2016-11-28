using UnityEngine;
using UnityEngine.UI;

public class SteeringPursue : ISteering {

	public float slowRadius;
	public float maxPrediction;


	public SteeringPursue(float slowRadius = 5f, float maxPrediction = 2f) {
		this.slowRadius = slowRadius;
		this.maxPrediction = maxPrediction;
	}

	private Text nameDisplay;
	private Image targetDisplay;
	private SpriteRenderer circleDisplay;

	public void ShowDebug(DynamicBase source, Canvas canvas) {
		HideDebug(source);
		nameDisplay = canvas.MakeText("Pursue Name", source.font, "Dynamic Pursue");
		targetDisplay = canvas.MakeDot("Pursue Target", source.knob);
		circleDisplay = source.transform.MakeCircle("Pursue Radius", source.radius);
		circleDisplay.color = new Color(1, 0, 0, 0.5f);
		circleDisplay.transform.localScale = new Vector2(slowRadius, slowRadius);
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
		if (circleDisplay != null) Object.Destroy(circleDisplay.gameObject);
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
		if (targetDisplay != null) {
			targetDisplay.rectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, predictedPos);
		}
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
}
