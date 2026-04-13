using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    public bool shouldMove, shouldRotate;

    public float moveSpeed, rotateSpeed;
    void Start()
    {
        
    }

    
    void Update()
    {
        if(shouldRotate)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, rotateSpeed * Time.deltaTime, 0f));
        }
    }
}
