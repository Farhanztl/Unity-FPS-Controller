using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{

    public float moveSpeed, lifeTime;
    public Rigidbody theRB;
    
    public GameObject impactEffect;

    public int damage = 1;
    
    void Update()
    {
        //chon az velocity estefade mikonim va velocity sorat ro mohasebe mikone na toole harekat ro Time.delaTime nmikonim
        theRB.velocity = transform.forward * moveSpeed;
        
        
        lifeTime -= Time.deltaTime;

        if(lifeTime <= 0) Destroy(gameObject);
        
    }

    private void OnTriggerEnter(Collider other)
    {

        if(other.gameObject.tag == "Enemy")
        {
           other.gameObject.GetComponent<EnemyHealthController>().DamageEnemy(damage);
        }

        Destroy(gameObject);                            //
        Instantiate(impactEffect, transform.position + (transform.forward * (-moveSpeed * Time.deltaTime)), transform.rotation);
    }
}
