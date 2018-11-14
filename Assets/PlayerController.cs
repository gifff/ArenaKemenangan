using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private Rigidbody rb;
    private float speed = 10;
	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        
    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        rb.MovePosition(rb.position + movement);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
