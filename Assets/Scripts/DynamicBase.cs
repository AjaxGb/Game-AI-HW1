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
	void GetSteering(DynamicBase source, Kinematic target, ref Vector2 accel_u_s2, ref float torque_r_s2);
	void UpdateDebug(DynamicBase source);
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
			if (_targetChar == null) {
				_targetChar = null;
				_targetRB = null;
			} else {
				_targetRB = _targetChar.GetComponent<Rigidbody2D>();
				if (_targetRB == null) _targetRB = null;
			}
		}
	}
	public Rigidbody2D targetRB {
		get { return _targetRB; }
		set {
			_targetRB = value;
			// Because Unity is crazy, and x == null doesn't always mean what it says.
			if (_targetRB == null) {
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
	void Awake() {
		rb = GetComponent<Rigidbody2D>();

		if (defaultLayer == 0) defaultLayer = LayerMask.NameToLayer("Default");
		if (stayOnCamLayer == 0) stayOnCamLayer = LayerMask.NameToLayer("StayOnCamera");
		keepOnCamera = _keepOnCamera;
		//targetChar = GameObject.FindObjectOfType<Transform>();
	}

	void Update() {
		if (steering != null) steering.UpdateDebug(this);
	}

	void FixedUpdate() {
		rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed_u_s);
		rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -maxRotSpeed_r_s, maxRotSpeed_r_s);
		
		if (steering == null) return;

		Kinematic target = this.target;
		Vector2 accel_u_s2 = Vector2.zero;
		float torque_r_s2 = 0f;

		steering.GetSteering(this, target, ref accel_u_s2, ref torque_r_s2);

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
		Debug.DrawLine(transform.position, position, Color.blue);
		FaceHeading(position - (Vector2)this.transform.position, ref torque);
	}

	public void FaceHeading(Vector2 targetFacing, ref float torque) {
		FaceHeading(Mathf.Atan2(targetFacing.y, targetFacing.x), ref torque);
	}

	public void FaceHeading(float targetRot, ref float torque) {

		// Find target speed
		float orientation = this.orientation;
		Debug.DrawRay(transform.position, Utilities.VectorFromAngle(orientation));
		Debug.DrawRay(transform.position, Utilities.VectorFromAngle(targetRot), Color.black);
		float targetRotDist = MapSteeringRadians(targetRot - orientation);
		float rotDistMag = Mathf.Abs(targetRotDist);
		float targetSpeed_r_s = this.maxRotSpeed_r_s * Mathf.Sign(targetRotDist);
		if (Mathf.Abs(targetSpeed_r_s) > rotDistMag) {
			targetSpeed_r_s = targetRotDist;
		}
		//if (log) Debug.Log("(" + targetRot + "," + orientation + "," + (targetRot - orientation) + ") --> " + targetRotDist);
		if (rotDistMag < this.slowRotRadius_r) {
			targetSpeed_r_s *= rotDistMag / this.slowRotRadius_r;
		}
		//if (log) Debug.Log(targetSpeed_r_s);
		//Debug.DrawRay(transform.position, Utilities.VectorFromAngle(orientation + targetSpeed_r_s), Color.yellow);

		// Find target rotAccel
		torque = targetSpeed_r_s - (this.rb.angularVelocity * Mathf.Deg2Rad);
		//if (log) Debug.Log(torque);
		torque = Mathf.Clamp(MapSteeringRadians(torque), -this.maxAccel_u_s2, this.maxAccel_u_s2);
		//if (log) Debug.Log(torque);
	}

	public static float MapSteeringRadians(float rot) {
		float rot2 = rot;
		rot += Mathf.PI;
		rot = ((rot % TAU) + TAU) % TAU;
		rot -= Mathf.PI;
		while (rot2 >= Mathf.PI) rot2 -= TAU;
		while (rot2 < -Mathf.PI) rot2 += TAU;
		if (!Utilities.Approximately(rot, rot2)) Debug.Log(rot + "!=" + rot2);
		if (rot2 > Mathf.PI || rot2 < -Mathf.PI) Debug.Log(rot2);
		return rot2;
	}

	//public static Vector2 Project(Vector2 a, Vector2 b) {
	//	return (Vector2.Dot(a, b) / Vector2.Dot(b, b)) * b;
	//}

	//public static float ProjectScalar(Vector2 a, Vector2 b) {
	//	return Vector2.Dot(a, b) / b.magnitude;
	//}

}
