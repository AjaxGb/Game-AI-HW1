using UnityEngine;

public class Tester : MonoBehaviour {

	public bool forceAdded;
	Rigidbody2D rb;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!forceAdded) {
			rb.AddTorque(10f * rb.inertia);
			forceAdded = true;
		}
		print("inertia: " + rb.inertia);
		print(""+rb.angularVelocity);
		print(Time.time + ": " + transform.rotation.eulerAngles.z);
	}
}
