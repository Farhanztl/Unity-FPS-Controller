using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private bool chasing;
    public float distanceToChase = 10f, distaceToLose = 15f, distanceToStop = 2f;

    private Vector3 targetPoint, startPoint;

    public NavMeshAgent agent;

    //yeki baraye zakhire etelaat va yeki baraye har bar estefade ke bad az bare aval be bad betavanad taghib konad. 
    public float keepChasingTime = 5f; //constant-like
    private float chaseCounter; //runtime state 

    public GameObject bullet;
    public Transform firePoint;
    private float fireCount;
    public float fireRate;
    
    void Start()
    {
        startPoint = transform.position;
    }

    void Update()
    {   
        targetPoint = PlayerController.instance.transform.position;
        targetPoint.y = transform.position.y;

        if (!chasing)
        {
            if (Vector3.Distance(transform.position, targetPoint) < distanceToChase) // Vector3.Distance fasele beine do object man ro barresi mikone.
            {
                chasing = true;
                fireCount = 1f;
            }

            if (chaseCounter > 0)
            {
                chaseCounter -= Time.deltaTime;

                if(chaseCounter <= 0)
                {
                    agent.destination = startPoint;
                }
            }
        }
            else
            {
                //transform.LookAt(targetPoint);
                //theRB.velocity = transform.forward * moveSpeed;
                if (Vector3.Distance(transform.position, targetPoint) > distanceToStop)
               {
                agent.destination = targetPoint;
               }else
               {
                agent.destination = transform.position;
               }
            
                if(Vector3.Distance(transform.position, targetPoint) > distaceToLose)
                {
                    chasing = false;

                    chaseCounter = keepChasingTime;
                }

                fireCount -= Time.deltaTime;

                if(fireCount <= 0)
                {
                    fireCount = fireRate;

                    Instantiate(bullet, firePoint.position, firePoint.rotation);
                }
            }

              
    }
}
