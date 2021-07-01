using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("Destroy", 3f);
        
        body = GetComponent<Rigidbody>();
        Transform gunEnd = GameObject.Find("GunEnd").transform;
        Instantiate(explosion, gunEnd.position, Quaternion.identity);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        body.AddForce(CustomGravity.GetGravity(transform.position)*gravityMult,ForceMode.Acceleration);
    }
    void Destroy()
    {
        Destroy(this.gameObject);
    }
    private void OnCollisionEnter(Collision collision)
    {
        Instantiate(impact, transform.position, Quaternion.Euler(collision.GetContact(0).normal));
        if (collision.collider.CompareTag("Enemy"))
        {
            collision.collider.GetComponent<EnemyStats>().health -= 1;
            collision.collider.GetComponent<Rigidbody>().AddRelativeForce(-Vector3.forward * 8, ForceMode.VelocityChange);
            if (collision.collider.GetComponent<EnemyStats>().health <= 0)
            {
                Destroy(collision.gameObject);
            }
        }
        Destroy();
    }
    Rigidbody body;
    public int gravityMult;
    public GameObject explosion;
    public GameObject impact;
}
