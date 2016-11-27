using UnityEngine;

public class CameraBounds : MonoBehaviour {

	public string layerName;
	EdgeCollider2D bounds;
	
	void Start () {
		bounds = new GameObject("Camera Bounds").AddComponent<EdgeCollider2D>();
		bounds.transform.SetParent(this.transform);
		bounds.gameObject.layer = LayerMask.NameToLayer(layerName);
		Vector2[] points = new Vector2[5];
		Camera.main.OrthographicBounds().GetPoints().CopyTo(points, 0);
		points[4] = points[0];
		bounds.points = points;
	}
}
