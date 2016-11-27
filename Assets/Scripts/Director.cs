using UnityEngine;

public class Director : MonoBehaviour {

	public DynamicBase wolf;
	public DynamicBase woodcutter;
	public DynamicBase ridingHood;
	public Transform house;

	// Use this for initialization
	void Start () {
		wolf.targetRB = woodcutter.rb;
		woodcutter.targetRB = wolf.rb;
		wolf.keepOnCamera = false;
		woodcutter.keepOnCamera = false;
		wolf.steering = new SteeringEvade();
		woodcutter.steering = new SteeringPursue();

		ridingHood.targetChar = house;
		ridingHood.steering = new SteeringPursue();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
