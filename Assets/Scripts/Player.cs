using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	// ---------------------------------------------------------------------------
	// Public variables
	// ---------------------------------------------------------------------------

	// Attributes for determining launch strength
	public float strength = 75f, chargeTime = 0f, jumpStrength = 35;

	public float minTheta = 0f, maxTheta = 2f	 * Mathf.PI;

	/// Attributes for the launch curve
	// The amount of time that takes for the initial charge
	public float ascTime = 1f;

	// The amount of time that it remains at full charge
	public float fullTime = 0.2f;

	// The amount of time that it takes to drop off to the remaining power after
	// over charging.
	public float overChargeTime = 0.4f;

	// The percentile power that it will drop to over the overChargeTime
	public float overChargePower = 60.0f;

	// The speed at which the character gains momentum
	public float walkSpeed = 2f;

	public float airWalkSpeed = 0.7f;

	public float EPSILON = 0.001f;

	public float AIR_DRAG = 0.025f;
	public float MOVEMENT_DRAG = 0.05f;
	public float STILL_DRAG = 0.125f;

	public float NORMAL_GRAV = 1.0f;
	public float LAUNCH_GRAV = 0.333f;

	public float MAX_JUMP_TICK_TIMEOUT = 0.25f;

	public float FORCE_JUMP_FACTOR = 1.5f;

    // Whether the player is in a bouncing state or not
    public bool isBouncing = false;

  // Ref to child Grounder instance
  public Grounder grounder;

	// ---------------------------------------------------------------------------
	// Public component variables
	// ---------------------------------------------------------------------------

	// ---------------------------------------------------------------------------
	// Private variables
	// ---------------------------------------------------------------------------

	private Rigidbody2D m_rigidBody;

    private Animator anim;

	private bool charging = false;

	private float walking = 0f;

	private bool readyForJump = false;

	private bool lowGravity = false;

	// ---------------------------------------------------------------------------
	// Overloaded system functions
	// ---------------------------------------------------------------------------

	// Use this for initialization
	void Start () {
		m_rigidBody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        grounder.isGrounded = true;
	}

    // Update is called once per frame
    void Update()
    {
        if(isBouncing)
        {
            updateBouncing();
        }
        else
        {
            updateWalking();
        }
    }

	
	// Updates the walking of the player while not bouncing
	void updateWalking () {

		if(Input.GetAxisRaw("Jump") > 0)
        {
			charging = true;
		} else if(charging && chargeTime < MAX_JUMP_TICK_TIMEOUT){
			charging = false;
			jump();
		} else if(charging){
			charging = false;
			fire();
		} else { // Should be unneeded, but lets just check for an escape.
			chargeTime = 0f;
		}

		// Check for a forced jump
		// Debug.Log(chargeTime+" > "+FORCE_JUMP_FACTOR+ " * "+ maxChargePeriod());
		if(chargeTime > FORCE_JUMP_FACTOR * maxChargePeriod()){
			charging = false;
			jump();
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

		updatePhysics(Time.deltaTime);
	}

	// This updates the physics system.
	void updatePhysics(float dt) {
		if(charging){
			chargeTime += grounder.isGrounded ? dt : dt / 2f;
		}

		if(grounder.isGrounded){
            // On the ground and not bouncing, either idle or walking
            anim.SetBool("isWalking", Mathf.Abs(walking) > EPSILON);
            anim.SetBool("isFalling", false);
            m_rigidBody.gravityScale = NORMAL_GRAV;
			if(Mathf.Abs(walking) > EPSILON){
				m_rigidBody.drag = MOVEMENT_DRAG;
				m_rigidBody.AddForce(new Vector2(walkSpeed * walking * getWalkingSpeedModifier(), 0f));
			} else {
				m_rigidBody.drag = STILL_DRAG;
			}
		} else {
            anim.SetBool("isWalking", false);
            anim.SetBool("isFalling", true);
			m_rigidBody.drag = AIR_DRAG;
			if(lowGravity){
				lowGravity = false;
				m_rigidBody.gravityScale = LAUNCH_GRAV;
			}
			if(Mathf.Abs(walking) > EPSILON){
				m_rigidBody.AddForce(new Vector2(airWalkSpeed * walking, 0f));
			}
		}
	}

    // Updates the bouncing while not walking
    void updateBouncing()
    {
        if(Input.GetAxisRaw("Jump") > 0)
        {
            isBouncing = false;
        }
        anim.SetBool("isBouncing", isBouncing);
    }

	// ---------------------------------------------------------------------------
	// High level action functions
	// ---------------------------------------------------------------------------

	// This function serves to fire the object at the mouse.
	private void fire(){
		if(!grounder.isGrounded) return;
        isBouncing = true;
        anim.SetBool("isWalking", false);
        anim.SetBool("isFalling", false);
        anim.SetBool("isBouncing", isBouncing);
		float vx, vy;
		float angle = getAngle(getMousePosition());
		float speed = getLaunchStrength(chargeTime);
		chargeTime = 0f;
		vx = Mathf.Cos(angle) * speed;
		vy = Mathf.Sin(angle) * speed;

		lowGravity = true;

		m_rigidBody.AddForce(new Vector2(vx, vy));
	}

	private void faceLeft(){
		transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
	}

	private void faceRight(){
		transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
	}

	// This does a small `normal` jump.
	private void jump(){
		if(!grounder.isGrounded) return;

		m_rigidBody.AddForce(new Vector2(0f, jumpStrength));
        anim.SetBool("isWalking", false);
        anim.SetBool("isFalling", true);
        anim.SetBool("isBouncing", false);
		chargeTime = 0f;
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

	private bool isCharging(){
		return chargeTime > MAX_JUMP_TICK_TIMEOUT;
	}

	// ---------------------------------------------------------------------------
	// Internal functions for some sort of paramaterized computation.
	// ---------------------------------------------------------------------------

	// Returns a float in the [0, 100] range that represents the percentile
	// launch power that will be used given a _time.
	private float getLaunchPower(float _time){
		float __fullTime = ascTime + fullTime;
		float __overChargeTime = maxChargePeriod();
		float __powerTime = _time *= 100f;
		if (_time <= 0f){
			return 0f;
		}
		else if(_time < ascTime){
			return Mathf.Pow(__powerTime, 4f) / Mathf.Pow(ascTime, 3f);
		}
		else if(_time < __fullTime){
			return 100f;
		}
		else if(_time < __overChargeTime){
			float __maxTime = __overChargeTime - __fullTime;
			float __tempTime = __powerTime - __fullTime;
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

	private float getWalkingSpeedModifier(){
		return isCharging() ? chargeTime > (maxChargePeriod() * 1.5f + MAX_JUMP_TICK_TIMEOUT) / 2f ? 0.25f : 0.5f : 1f;
	}

	private float maxChargePeriod(){
		// Debug.Log(ascTime + " + " + fullTime + " + " + overChargeTime);
		return ascTime + fullTime + overChargeTime;
	}
}
