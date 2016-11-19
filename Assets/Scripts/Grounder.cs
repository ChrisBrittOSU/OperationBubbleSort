using UnityEngine;
using System.Collections;

public class Grounder : MonoBehaviour {
    // bool to check whether the gameObject is touching the ground or not
    private bool bGrounded = true;
    public bool isGrounded
    {
        get { return bGrounded; }
        set { bGrounded = value; }
    }

    private bool isGroundTag(Collider2D collider){
      return collider.tag.Equals("Ground");
    }

    // event triggered upon a 2D collider entering
    private void OnTriggerStay2D(Collider2D other)
    {
        bGrounded = isGroundTag(other) ? true : bGrounded;
    }

    // Event trigged upon leaving the ground
    private void OnTriggerExit2D(Collider2D other){
      bGrounded = isGroundTag(other) ? false : bGrounded;
    }

}
