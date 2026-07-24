using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{

    public float moveSpeed, lifeTime;
    public Rigidbody theRB;
    
    public GameObject impactEffect;

    public int damage = 1;

    public bool damageEnemy, damagePlayer;
    
    void Update()
    {
        //chon az velocity estefade mikonim va velocity sorat ro mohasebe mikone na toole harekat ro Time.delaTime nmikonim
        theRB.velocity = transform.forward * moveSpeed;
        
        
        lifeTime -= Time.deltaTime;

        if(lifeTime <= 0) Destroy(gameObject);
        
    }

    private void OnTriggerEnter(Collider other)
    {

        if(other.gameObject.tag == "Enemy" && damageEnemy)
        {
           other.gameObject.GetComponent<EnemyHealthController>().DamageEnemy(damage);
        }

        if (other.gameObject.tag == "Player" && damagePlayer)
        {
            PlayerHealthController phc = other.gameObject.GetComponent<PlayerHealthController>();
            if (phc != null)
                phc.TakeDamage(damage);
            else
                Debug.LogWarning("[BulletController] Player is missing PlayerHealthController component!");
        }

        Destroy(gameObject);                            //
        Instantiate(impactEffect, transform.position + (transform.forward * (-moveSpeed * Time.deltaTime)), transform.rotation);
    }
}
