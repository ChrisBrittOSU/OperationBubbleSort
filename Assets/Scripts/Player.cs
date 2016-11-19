using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	private RigidBody2D m_rigidBody;

	// Attributes for determining launch strength
	public float strength = 10f, chargeTime = 0f;

	public float minTheta = 0f, maxTheta = 2f	 * Mathf.PI;

	/// Attributes for the launch curve
	// The amount of time that takes for the initial charge
	public float ascTime = 100f;

	// The amount of time that it remains at full charge
	public float fullTime = 20f;

	// The amount of time that it takes to drop off to the remaining power after
	// over charging.
	public float overChargeTime = 40f;

	// The percentile power that it will drop to over the overChargeTime
	public float overChargePower = 60.0f;

	// Use this for initialization
	void Start () {
		m_rigidBody = GetComponent<RigidBody2D>();
	}

	// Update is called once per frame
	// IO
	void Update () {

	}

	// Internal
	void FixedUpdate() {

	}

	void fire(){
		float vx, vy;
		float angle = getAngle(getMousePosition());
		float speed = getLaunchStrength(chargeTime);
		vx = Math.Cos(angle) * speed;
		vy = Math.Sin(angle) * speed;

		m_rigidBody.AddVelocity(new Vector2(vx, vy));
	}

	private Vector2 getMousePosition(){
		Vector3 v = Input.mousePosition;
		v = new Vector3(v.x, v.y, 10f);
		v = Camera.main.ScreenToWorldPosition(v);
		return new Vector2(v.x, v.y);
	}

	private Vector2 position(){
		return new Vector2(transform.position.x, transform.position.y);
	}

	// Return the angle between this and another object in radians
	private float getAngle(Vector2 other){
		return Vector2.Angle(position(), other);
	}

	// Returns a float in the [0, 100] range that represents the percentile
	// launch power that will be used.
	private float getLaunchPower(float _time){
		float __fullTime = ascTime + fullTime;
		float __overChargeTime = ascTime + fullTime + overChargeTime;
		if (_time <= 0f){
			return 0f;
		}
		else if(_time < ascTime){
			return Mathf.Pow(_time, 4f) / Mathf.Pow(ascTime, 3f);
		}
		else if(_time < __fullTime){
			return 100f;
		}
		else if(_time < __overChargeTime){
			float __maxTime = __overChargeTime - __fullTime;
			float __tempTime = _time - __fullTime;
			float __perThrough = (__maxTime - __tempTime) / __maxTime;
			return overChargePower + (100f - overChargePower) * __perThrough;
		}
		else{
			return overChargePower;
		}
	}

	private float getLaunchStrength(float _time){
		return strength * getLaunchPower(_time);
	}
}
