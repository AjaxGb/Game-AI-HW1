using UnityEngine;

public class SteeringPath : ISteering {

	public float totalLength { get; private set; }
	public float[] lengths { get; private set; }
	private Vector2[] _points;
	public Vector2[] points {
		get { return _points; }
		set {
			_points = value;
			lengths = new float[_points.Length - 1];
			totalLength = 0f;
			for (int i = 1; i < _points.Length; i++) {
				float length = (_points[i] - _points[i - 1]).magnitude;
				lengths[i - 1] = length;
				totalLength += length;
			}
		}
	}

	public float targetDist;

	public SteeringPath(Vector2[] points, float targetDist = 1f) {
		this.points = points;
		this.targetDist = targetDist;
	}

	public void GetSteering(DynamicBase source, Kinematic target, ref Vector2 accel_u_s2, ref float torque_r_s2) {
		if (target == null) return;

		// Predict and face location
		float nearestDistFromStart;
		Vector2 nearestPoint = NearestPoint(source.transform.position, out nearestDistFromStart);

		target.position_u = PointAtDist(nearestDistFromStart + this.targetDist);
		source.FacePosition(target.position_u, ref torque_r_s2);

		// Find target speed + velocity
		float targetSpeed = source.maxSpeed_u_s;
		//Vector2 targetDist = target.position_u - (Vector2)source.transform.position;
		//if (targetDist.sqrMagnitude < slowRadius * slowRadius) {
		//	targetSpeed *= targetDist.magnitude / slowRadius;
		//}
		Vector2 targetVelocity = source.forward * targetSpeed;

		// Find target acceleration
		accel_u_s2 = targetVelocity - source.rb.velocity;
		accel_u_s2 = Vector2.ClampMagnitude(accel_u_s2, source.maxAccel_u_s2);
	}

	float d = 0f;
	public void UpdateDebug(DynamicBase source) {
		Debug.DrawLine(source.transform.position, PointAtDist(d));
		d += 0.1f;
		if (d > totalLength) d = 0f;
	}

	public static Vector2 NearestPointOnSegment(Vector2 p, Vector2 s1, Vector2 s2, out float sqrDist, out float t) {
		Vector2 seg = s2 - s1;
		float sqrSegLength = seg.sqrMagnitude;
		if (sqrSegLength == 0f) {
			sqrDist = sqrSegLength;
			t = 0f;
			return s1;
		}
		t = Mathf.Clamp01(Vector2.Dot(p - s1, seg) / sqrSegLength);
		Vector2 projected = s1 + t * seg;
		sqrDist = (projected - p).sqrMagnitude;
		return projected;
	}

	public Vector2 NearestPoint(Vector2 p, out float distFromStart) {
		if (points.Length == 0) throw new System.InvalidOperationException("There are no points");
		float sqrMinDist = Mathf.Infinity;
		Vector2 target = points[0];
		float totalDistFromStart = 0f;
		distFromStart = 0f;
		for (int i = 1; i < points.Length; i++) {
			float sqrCurrDist, t;
			Vector2 curr = NearestPointOnSegment(p, points[i - 1], points[i], out sqrCurrDist, out t);
			if (sqrCurrDist < sqrMinDist) {
				sqrMinDist = sqrCurrDist;
				distFromStart = totalDistFromStart + t * lengths[i - 1];
				target = curr;
			}
			totalDistFromStart += lengths[i - 1];
		}
		return target;
	}

	public Vector2 PointAtDist(float dist) {
		if (points.Length == 0) throw new System.InvalidOperationException("There are no points");
		dist = Mathf.Clamp(dist, 0, totalLength);
		int p;
		for (p = 0; p < lengths.Length; p++) {
			if (dist > lengths[p]) {
				dist -= lengths[p];
			} else {
				p++;
				break;
			}
		}
		return Vector2.MoveTowards(points[p - 1], points[p], dist);
	}
}
