using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Director : MonoBehaviour {

	public Text text;
	public DynamicBase wolf;
	public DynamicBase woodcutter;
	public DynamicBase ridingHood;
	public Transform house;
	public Transform idleSpot;
	public LineRenderer path;

	public float hunterWolfSpotDistance = 5f;
	public float redWolfSpotDistance = 1.2f;
	public float redWolfSpotDelay = 3f;

	public Vector2[] pathPoints;

	/// <summary>
	/// <para>State 0: Hunter + Wolf: Wander, Red: Not appeared</para>
	/// <para>--> Hunter + Wolf get close</para>
	/// <para>State 1: Hunter + Wolf: Pursue, evade</para>
	/// <para>--> Hunter + Wolf offscreen</para>
	/// <para>State 2: Red walks to Granny's, Wolf pursues</para>
	/// <para>--> Meet on path</para>
	/// <para>State 3: Talking</para>
	/// <para>--> Delay</para>
	/// <para>State 4: Wolf goes to house</para>
	/// <para>--> Arrives</para>
	/// <para>State 5: Red goes to house</para>
	/// <para>--> Arrives</para>
	/// <para>State 6: Hunter appears, goes to house</para>
	/// <para>--> Arrives</para>
	/// <para>State 7: End</para>
	/// </summary>
	private int state = 0;

	private float timer;
	private ISteering pausedPath;

	// Use this for initialization
	void Start () {
		Vector3[] points = (from p in pathPoints select new Vector3(p.x, p.y, 1f)).ToArray();
		path.SetVertexCount(points.Length);
		path.SetPositions(points);
		wolf.keepOnCamera = true;
		woodcutter.keepOnCamera = true;
		wolf.steering = new SteeringWander();
		woodcutter.steering = new SteeringWander();
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 dist;
		switch(state) {
			case 0:
				dist = wolf.transform.position - woodcutter.transform.position;
				if (dist.sqrMagnitude <= hunterWolfSpotDistance * hunterWolfSpotDistance || Input.GetKeyDown(KeyCode.E)) {
					Debug.Log(state = 1);
					// State 1: Hunter + Wolf: Pursue, evade
					text.gameObject.SetActive(false);
					woodcutter.targetRB = wolf.rb;
					wolf.targetRB = woodcutter.rb;
					woodcutter.steering = new SteeringPursue(0f);
					wolf.steering = new SteeringEvade(0f);
					woodcutter.keepOnCamera = wolf.keepOnCamera = false;
				}
				break;
			case 1:
				Rect bounds = Camera.main.OrthographicBounds();
				// Add 1 unit margin
				bounds.width += 2;
				bounds.height += 2;
				bounds.x -= 1;
				bounds.y -= 1;

				if (!bounds.Contains(wolf.transform.position) && !bounds.Contains(woodcutter.transform.position)) {
					Debug.Log(state = 2);
					// State 2: Red walks to Granny's, Wolf pursues
					woodcutter.gameObject.SetActive(false);
					ridingHood.gameObject.SetActive(true);
					ridingHood.keepOnCamera = false;
					ridingHood.transform.position = pathPoints[0];
					ridingHood.steering = new SteeringPath(pathPoints);
					ridingHood.targetChar = house;
					wolf.transform.position = bounds.max;
					wolf.steering = new SteeringPursue(0f);
					wolf.targetRB = ridingHood.rb;
				}
				break;
			case 2:
				dist = wolf.transform.position - ridingHood.transform.position;
				if (dist.sqrMagnitude <= redWolfSpotDistance * redWolfSpotDistance) {
					Debug.Log(state = 3);
					// State 3: Talking
					ridingHood.rb.drag = wolf.rb.drag = 5f;
					ridingHood.rb.angularDrag = wolf.rb.angularDrag = 5f;
					ridingHood.enabled = wolf.enabled = false;
					timer = redWolfSpotDelay;
				}
				break;
			case 3:
				timer -= Time.deltaTime;
				if (timer <= 0f) {
					Debug.Log(state = 4);
					// State 4: Wolf goes to house
					wolf.rb.drag = wolf.rb.angularDrag = 0f;
					//ridingHood.targetChar = idleSpot;
					//pausedPath = ridingHood.steering;
					//ridingHood.steering = new SteeringPursue();
					wolf.targetChar = house;
					wolf.steering = new SteeringPursue();
					wolf.enabled = true;
				}
				break;
			case 4:
				dist = wolf.transform.position - house.position;
				if (dist.sqrMagnitude <= 0.1) {
					Debug.Log(state = 5);
					// State 5: Red goes to house
					wolf.gameObject.SetActive(false);
					ridingHood.enabled = true;
				}
				break;
			case 5:
				dist = ridingHood.transform.position - house.position;
				if (dist.sqrMagnitude <= 0.1) {
					Debug.Log(state = 6);
					// State 6: Hunter goes to house
					ridingHood.gameObject.SetActive(false);
					Vector2 start = Camera.main.OrthographicBounds().min;
					start.x -= 1;
					start.y -= 1;
					woodcutter.transform.position = start;
					woodcutter.targetChar = house;
					woodcutter.steering = new SteeringPursue();
					woodcutter.gameObject.SetActive(true);
				}
				break;
			case 6:
				dist = woodcutter.transform.position - house.position;
				if (dist.sqrMagnitude <= 0.1) {
					Debug.Log(state = 7);
					// State 7: End
					woodcutter.gameObject.SetActive(false);
					text.text = "The End";
					text.gameObject.SetActive(true);
				}
				break;
		}
	}
}
