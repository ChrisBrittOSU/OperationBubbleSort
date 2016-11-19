using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	// ---------------------------------------------------------------------------
	// Public variables
	// ---------------------------------------------------------------------------

	// Attributes for determining launch strength
	public float strength = 500f, chargeTime = 0f, chargeTimeIncrement = 1f;

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

	// ---------------------------------------------------------------------------
	// Private variables
	// ---------------------------------------------------------------------------

	private Rigidbody2D m_rigidBody;

	private bool charging = false;

	// ---------------------------------------------------------------------------
	// Overloaded system functions
	// ---------------------------------------------------------------------------

	// Use this for initialization
	void Start () {
		m_rigidBody = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	// IO
	void Update () {
		if(Input.GetKeyDown(KeyCode.Space)){
			charging = true;
		} else if(Input.GetKeyUp(KeyCode.Space)) {
			charging = false;
			fire();
		}
	}

	// Internal
	void FixedUpdate() {
		if(charging){
			chargeTime += chargeTimeIncrement;
		}
	}

	// ---------------------------------------------------------------------------
	// High level action functions
	// ---------------------------------------------------------------------------

	// This function serves to fire the object at the mouse.
	private void fire(){
		float vx, vy;
		float angle = getAngle(getMousePosition());
		float speed = getLaunchStrength(chargeTime);
		chargeTime = 0;
		vx = Mathf.Cos(angle) * speed;
		vy = Mathf.Sin(angle) * speed;

		m_rigidBody.AddForce(new Vector2(vx, vy));
	}

	// ---------------------------------------------------------------------------
	// Functions to wrap around the Unity API to give us a more intuitive interface
	// ---------------------------------------------------------------------------

	// This function returns as a Vector2 the current mouse position.
	private Vector2 getMousePosition(){
		Vector3 v = Input.mousePosition;
		v = new Vector3(v.x, v.y, 10f);
		v = Camera.main.ScreenToWorldPoint(v);
		return new Vector2(v.x, v.y);
	}

	// This function returns the current player's position as a Vector2.
	private Vector2 position(){
		return new Vector2(transform.position.x, transform.position.y);
	}


	// ---------------------------------------------------------------------------
	// Internal functions for some sort of paramaterized computation.
	// ---------------------------------------------------------------------------

	// Returns a float in the [0, 100] range that represents the percentile
	// launch power that will be used given a _time.
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

	// This function returns the strength adjusted getLaunchPower(_time).
	private float getLaunchStrength(float _time){
		return strength * getLaunchPower(_time) / 100f;
	}

	// Return the angle between this and another object in radians
	private float getAngle(Vector2 other){
		return Vector2.Angle(position(), other);
	}
}
