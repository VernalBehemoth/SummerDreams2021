using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToPosition : MonoBehaviour
{
    public Transform target;
    public float speed;
    bool inTransit = true;
    void Update()
    {
        
        if(inTransit)
        {
            inTransit = Vector3.Distance(target.transform.position, this.transform.position) > 1;
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target.position, step);
            speed += 0.006F;
        }
        
    }
}
