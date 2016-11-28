using UnityEngine;

public class CameraBounds : MonoBehaviour {

	public string layerName;
	EdgeCollider2D bounds;
	
	void Start () {
		bounds = new GameObject("Camera Bounds").AddComponent<EdgeCollider2D>();
		bounds.transform.SetParent(this.transform);
		bounds.gameObject.layer = LayerMask.NameToLayer(layerName);
		Vector2[] points = new Vector2[5];
		Rect camBounds = Camera.main.OrthographicBounds();
		camBounds.width -= 3f;
		camBounds.height -= 3f;
		camBounds.x += 1.5f;
		camBounds.y += 1.5f;
		camBounds.GetPoints().CopyTo(points, 0);
		points[4] = points[0];
		bounds.points = points;
	}
}
