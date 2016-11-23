using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MoveToTarget : MonoBehaviour {

	public float maxAccel = 1.0f;
	public float maxSpeed = 3.0f;
	public float slowRadius = 2.0f;

	public float maxTorque = 1.0f;
	public float maxRotSpeed = 3.0f;
	public float slowRotRadius = 10.0f;

	//[HideInInspector]
	public Transform target;

	private Rigidbody2D rb;

	// Use this for initialization
	void Start() {
		rb = GetComponent<Rigidbody2D>();
	}

	void FixedUpdate() {
		if (target == null) return;

		// Find target speed + velocity
		float targetSpeed = maxSpeed;
		Vector2 targetVel = target.position - this.transform.position;
		if (targetVel.sqrMagnitude < slowRadius * slowRadius) {
			targetSpeed *= targetVel.magnitude / slowRadius;
		}
		targetVel.Normalize();
		targetVel *= targetSpeed;

		// Find target acceleration
		Vector2 targetAccel = targetVel - rb.velocity;
		if (targetAccel.sqrMagnitude > maxAccel * maxAccel) {
			targetAccel.Normalize();
			targetAccel *= maxAccel;
		}
		
		rb.AddForce(targetAccel);
	}
	
}
