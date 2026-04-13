using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed;
    public Rigidbody theRB;
    private bool chasing;
    public float distanceToChase = 10f, distaceToLose = 15f;

    private Vector3 targetPoint;
    

    void Update()
    {
        targetPoint = PlayerController.instance.transform.position;
        targetPoint.y = transform.position.y;

        if (!chasing)
        {
            if (Vector3.Distance(transform.position, targetPoint) < distanceToChase)
            {
                chasing = true;
            }
        }
            else
            {
                transform.LookAt(targetPoint);

                theRB.velocity = transform.forward * moveSpeed;

                if(Vector3.Distance(transform.position, targetPoint) > distaceToLose)
                {
                    chasing = false;
                }
            }
        
    }
}
