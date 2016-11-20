using UnityEngine;
using System.Collections;

public class PlayerFollow : MonoBehaviour {

    public Transform playerTransform;
	
	// Update is called once per frame
	void Update () {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform; ;
            }
        }
        else
        {
            Vector3 position = playerTransform.position;
            position = new Vector3(position.x, position.y, transform.position.z);
            transform.position = position;
        }
	}
}
