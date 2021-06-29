using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    float turnSpeed = 10.0F;
    float moveSpeed = 10.0F;
    float mouseTurnMultiplier = 1;
    float x;
    float z;

    // Update is called once per frame
    void Update()
    {
        // x is used for the x axis.  set it to zero so it doesn't automatically rotate
        x = 0;

        // check to see if the W or S key is being pressed.  
        z = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;

        // Move the character forwards or backwards
        transform.Translate(0, 0, z);

        // Check to see if the A or S key are being pressed
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S))
        {
            // Get the A or S key (-1 or 1)
            x = Input.GetAxis("Horizontal");
        }

        // Check to see if the right mouse button is pressed
        if (Input.GetMouseButton(1))
        {
            // Get the difference in horizontal mouse movement
            x = Input.GetAxis("Mouse X") * turnSpeed * mouseTurnMultiplier;
        }

        // rotate the character based on the x value
        transform.Rotate(0, x, 0);
    }
}