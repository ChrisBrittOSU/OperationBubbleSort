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

    // event triggered upon a 2D collider entering
    private void OnTriggerStay2D(Collider2D other)
    {
        bGrounded = other.tag.Equals("Ground");
    }

}
