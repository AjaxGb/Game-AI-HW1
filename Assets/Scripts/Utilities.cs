using UnityEngine;

public static class Utilities {

	public static Vector2 SetMagnitude(this Vector2 vec, float magnitude) {
		vec.Normalize();
		vec *= magnitude;
		return vec;
	}

	public static Rect OrthographicBounds(this Camera camera) {
		if (!camera.orthographic) {
			throw new System.InvalidOperationException("Camera is not orthographic");
		}
		float screenAspect = (float)Screen.width / (float)Screen.height;
		float cameraHeight = camera.orthographicSize * 2;
		Vector2 extent = new Vector2(cameraHeight * screenAspect, cameraHeight);
		Vector2 minPos = (Vector2)camera.transform.position - extent / 2;
		return new Rect(minPos, extent);
	}

	public static Vector2[] GetPoints(this Rect rect) {
		return new Vector2[] {
			new Vector2(rect.xMin, rect.yMin),
			new Vector2(rect.xMax, rect.yMin),
			new Vector2(rect.xMax, rect.yMax),
			new Vector2(rect.xMin, rect.yMax),
		};
	}

	public static void DrawRect(Rect rect, Color? color = null, float duration = 0.0f) {
		Color realColor = color ?? Color.white;
		Debug.DrawLine(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax), realColor, duration, false);
		Debug.DrawLine(new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMax, rect.yMax), realColor, duration, false);

		Debug.DrawLine(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMin), realColor, duration, false);
		Debug.DrawLine(new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMax, rect.yMax), realColor, duration, false);
	}

}
