using UnityEngine;
using UnityEngine.UI;

public class SteeringWander : ISteering {

	public float circleDisplacement;
	public float circleRadius;

	public SteeringWander(float circleDisplacement = 11f, float circleRadius = 10f) {
		this.circleDisplacement = circleDisplacement;
		this.circleRadius = circleRadius;
	}

	public void GetSteering(DynamicBase source, Kinematic target, ref Vector2 accel, ref float torque) {
		Vector2 position = source.transform.position;
		float angle = Random.Range(0, 2 * Mathf.PI);
		Vector2 circleCenter = position + source.forward * circleDisplacement;
		Vector2 wanderTarget = circleCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * circleRadius;
		Debug.DrawLine(position, circleCenter);
		Debug.DrawLine(circleCenter, wanderTarget);
		source.Seek(wanderTarget, ref accel);
		Debug.DrawRay(position, accel * 5, Color.black);
		source.FaceHeading(accel, ref torque);
	}

	private Text nameDisplay;

	public void ShowDebug(DynamicBase source, Canvas canvas) {
		HideDebug(source);
		nameDisplay = canvas.MakeText("Wander Name", source.font, "Dynamic Wander");
		nameDisplay.rectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, source.transform.position);
	}

	public void UpdateDebug(DynamicBase source) {
		if (nameDisplay != null) {
			nameDisplay.rectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, source.transform.position);
		}
	}

	public void HideDebug(DynamicBase source) {
		if (nameDisplay != null) Object.Destroy(nameDisplay.gameObject);
	}
}
