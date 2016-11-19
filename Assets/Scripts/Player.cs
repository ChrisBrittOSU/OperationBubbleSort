using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	// ---------------------------------------------------------------------------
	// Public variables
	// ---------------------------------------------------------------------------

	// Attributes for determining launch strength
	public float strength = 200f, chargeTime = 0f, chargeTimeIncrement = 3f;

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

	// The speed at which the character gains momentum
	public float walkSpeed = 5f;

	public float EPSILON = 0.001f;

	public float MOVEMENT_DRAG = 0.05f;
	public float STILL_DRAG = 0.125f;

  // Ref to child Grounder instance
  public Grounder grounder;

	// ---------------------------------------------------------------------------
	// Public component variables
	// ---------------------------------------------------------------------------

	// ---------------------------------------------------------------------------
	// Private variables
	// ---------------------------------------------------------------------------

	private Rigidbody2D m_rigidBody;

	private bool charging = false;

	private float walking = 0f;

	// ---------------------------------------------------------------------------
	// Overloaded system functions
	// ---------------------------------------------------------------------------

	// Use this for initialization
	void Start () {
		m_rigidBody = GetComponent<Rigidbody2D>();
        grounder.isGrounded = true;
	}

	// Update is called once per frame
	// IO
	void Update () {
		if(Input.GetKey(KeyCode.Space) && grounder.isGrounded){
			charging = true;
		} else if(charging){
			charging = false;
			fire();
		}

		walking = Input.GetAxis("Horizontal");
		if(walking < 0f){
			faceLeft();
		} else if(walking > 0f) {
			faceRight();
		}

		if(grounder.isGrounded){

		}
		else {
			if(m_rigidBody.velocity.x > 0f){
				faceRight();
			} else if(m_rigidBody.velocity.x < 0f){
				faceLeft();
			}
		}
	}

	// Internal
	void FixedUpdate() {
		if(charging){
			chargeTime += chargeTimeIncrement;
		}

		if(grounder.isGrounded){
			if(Mathf.Abs(walking) > EPSILON){
				m_rigidBody.drag = MOVEMENT_DRAG;
				m_rigidBody.AddForce(new Vector2(walkSpeed * walking, 0));
			} else {
				m_rigidBody.drag = STILL_DRAG;
			}
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

	private void faceLeft(){
		transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
	}

	private void faceRight(){
		transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
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
		Vector3 relative = transform.InverseTransformPoint(other);

		float sign = transform.localScale.x >= 0f ? -1f : 1f;

		return Mathf.Atan2(sign*relative.x, relative.y) + 1f * Mathf.PI / 2f;;
	}
}
