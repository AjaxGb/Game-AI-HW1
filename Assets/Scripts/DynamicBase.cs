using System.Collections.Generic;
using UnityEngine;

public class Kinematic {
	public Vector2 position_u;    // Units
	public float orientation_r;   // Radians
	public Vector2 velocity_u_s;  // Units/sec
	public float rotVelocity_r_s; // Radians/sec

	public Kinematic(Vector2 position_u, float orientation_r, Vector2 velocity_u_s, float rotVelocity_r_s) {
		this.position_u = position_u;
		this.orientation_r = orientation_r;
		this.velocity_u_s = velocity_u_s;
		this.rotVelocity_r_s = rotVelocity_r_s;
	}
}

public interface ISteering {
	void getSteering(DynamicBase source, Kinematic target, ref Vector2 accel_u_s2, ref float torque_r_s2);
}

[RequireComponent(typeof(Rigidbody2D))]
public class DynamicBase : MonoBehaviour {

	#region Properties

	public const float TAU = 2 * Mathf.PI;

	public float maxAccel_u_s2 = 0.5f;   // Units/sec^2
	public float maxSpeed_u_s = 1.0f;    // Units/sec

	public float maxTorque_r_s2 = 5.0f;  // Radians/sec^2
	public float maxRotSpeed_r_s = 6.0f; // Radians/sec
	public float slowRotRadius_r = 1.0f;     // Radians
	
	private bool _keepOnCamera = true;
	private static int defaultLayer = 0;
	private static int stayOnCamLayer = 0;
	public bool keepOnCamera {
		get { return _keepOnCamera; }
		set {
			_keepOnCamera = value;
			if (_keepOnCamera) {
				gameObject.layer = stayOnCamLayer;
			} else {
				gameObject.layer = defaultLayer;
			}
		}
	}

	public ISteering steering;
	
	private Transform _targetChar;
	private Rigidbody2D _targetRB;
	public Transform targetChar {
		get { return _targetChar; }
		set {
			_targetChar = value;
			// Because Unity is crazy, and x == null doesn't always mean what it says.
			if (!_targetChar) {
				_targetChar = null;
				_targetRB = null;
			} else {
				_targetRB = _targetChar.GetComponent<Rigidbody2D>();
				if (!_targetRB) _targetRB = null;
			}
		}
	}
	public Rigidbody2D targetRB {
		get { return _targetRB; }
		set {
			_targetRB = value;

			if (!_targetRB) {
				_targetRB = null;
				_targetChar = null;
			} else {
				_targetChar = _targetRB.transform;
			}
		}
	}

	public Kinematic target {
		get {
			if (targetChar == null) return null;
			Vector2 forward = -targetChar.up;
			return new Kinematic(
				targetChar.position,                              // Units
				Mathf.Atan2(forward.y, forward.x),                // Radians
				targetRB?.velocity ?? Vector2.zero,               // Units/sec
				(targetRB?.angularVelocity ?? 0f) * Mathf.Deg2Rad // Radians/sec
			);
		}
	}

	public Vector2 forward { get { return -this.transform.up; } }
	public float orientation {
		get {
			Vector2 forward = this.forward;
			return Mathf.Atan2(forward.y, forward.x);
		}
	}

	public Rigidbody2D rb { get; private set; }

	#endregion

	#region Control Code

	// Use this for initialization
	void Start() {
		rb = GetComponent<Rigidbody2D>();
		steering = new SteeringEvade();

		if (defaultLayer == 0) defaultLayer = LayerMask.NameToLayer("Default");
		if (stayOnCamLayer == 0) stayOnCamLayer = LayerMask.NameToLayer("StayOnCamera");
		keepOnCamera = _keepOnCamera;
		//targetChar = GameObject.FindObjectOfType<Transform>();
	}

	void FixedUpdate() {
		rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed_u_s);
		rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -maxRotSpeed_r_s, maxRotSpeed_r_s);

		if (steering == null) return;

		Kinematic target = this.target;
		Vector2 accel_u_s2 = Vector2.zero;
		float torque_r_s2 = 0f;

		steering.getSteering(this, target, ref accel_u_s2, ref torque_r_s2);

		accel_u_s2 = Vector2.ClampMagnitude(accel_u_s2, maxAccel_u_s2);
		Debug.DrawRay(transform.position, rb.velocity, Color.blue);
		Debug.DrawRay(transform.position, accel_u_s2, Color.green);
		torque_r_s2 = Mathf.Clamp(torque_r_s2, -maxTorque_r_s2, maxTorque_r_s2);

		rb.velocity += accel_u_s2; // Because ForceMode2D.Acceleration isn't a thing
		rb.angularVelocity += torque_r_s2 * Mathf.Rad2Deg; // I dunno what's up with AddTorque()
	}

	#endregion

	public void Seek(Vector2 target, ref Vector2 accel) {
		// Find target speed + velocity
		float targetSpeed = this.maxSpeed_u_s;
		Vector2 targetVel = target - (Vector2)this.transform.position;
		targetVel.Normalize();
		targetVel *= targetSpeed;

		// Find target acceleration
		accel = targetVel - this.rb.velocity;
		accel = Vector2.ClampMagnitude(accel, this.maxAccel_u_s2);
	}

	public void FacePosition(Vector2 position, ref float torque) {
		FaceHeading(position - (Vector2)this.transform.position, ref torque);
	}

	public void FaceHeading(Vector2 targetFacing, ref float torque) {
		FaceHeading(Mathf.Atan2(targetFacing.y, targetFacing.x), ref torque);
	}

	public void FaceHeading(float targetRot, ref float torque) {

		// Find target speed
		float orientation = this.orientation;
		float targetSpeed_r_s = this.maxRotSpeed_r_s;
		float targetRotDist = targetRot - orientation;
		if (targetRotDist < this.slowRotRadius_r) {
			targetSpeed_r_s *= targetRotDist / this.slowRotRadius_r;
		}

		// Find target rotAccel
		torque = targetSpeed_r_s - this.rb.angularVelocity * Mathf.Deg2Rad;
		torque = Mathf.Clamp(torque, -this.maxAccel_u_s2, this.maxAccel_u_s2);
	}
	
	//public static float MapSteeringRadians(float rot) {
	//	float oldRot = rot;
	//	rot += Mathf.PI;
	//	rot = ((rot % TAU) + TAU) % TAU;
	//	rot -= Mathf.PI;
	//	return rot;
	//}

	//public static Vector2 Project(Vector2 a, Vector2 b) {
	//	return (Vector2.Dot(a, b) / Vector2.Dot(b, b)) * b;
	//}

	//public static float ProjectScalar(Vector2 a, Vector2 b) {
	//	return Vector2.Dot(a, b) / b.magnitude;
	//}

}
