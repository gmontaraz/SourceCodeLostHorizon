using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        playerStats = FindObjectOfType<Stats>();
        cooldown = 0f;
        controller = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerStats.onShip && !playerStats.dead)
        {
            selectedEnemy = null;
            Collider[] enemies = Physics.OverlapSphere(transform.position, distance, enemyLayer);
            float min_distance = Mathf.Infinity;
            foreach (Collider enemy in enemies)
            {
                if (Vector3.Distance(enemy.transform.position, transform.position) < min_distance)
                {
                    selectedEnemy = enemy.gameObject;
                    min_distance = Vector3.Distance(enemy.transform.position, transform.position);
                }
            }
            if (Input.GetButtonDown("Fire1")&&cooldown<=0)
            {
                cooldown = timer;
                animator.SetTrigger("Shoot");
                Invoke("Shoot", 0.2f);
            }
            
        }
        if (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
        }
        if (cooldown > 0 )
        {
            controller.groundSpeed = 0.1f;
            controller.waterSpeed = 0.1f;
        }
        else
        {
            controller.groundSpeed = 10f;
            controller.waterSpeed = 5f;
        }

        if (Input.GetButton("Fire2"))
        {
            astronaut.localRotation = Quaternion.Euler(astronaut.localRotation.x, astronaut.localRotation.y+correction, model.localRotation.z);
            animator.SetBool("Aim", true);
            controller.aim = true;
        }
        else if(Input.GetButtonUp("Fire2"))
        {
            astronaut.localRotation = Quaternion.Euler(astronaut.localRotation.x, astronaut.localRotation.y, model.localRotation.z);
            animator.SetBool("Aim", false);
            controller.aim = false;
        }
        
    }
    void Shoot()
    {
        FindObjectOfType<CameraEffects>().Shake();
        if (selectedEnemy != null && Vector3.Angle(model.forward, selectedEnemy.transform.position - transform.position) < 60)
        {
            Quaternion bulletRotation = Quaternion.LookRotation(selectedEnemy.transform.position - transform.position);
            GameObject bulletClone = Instantiate(bullet, gun.position, bulletRotation);
            Rigidbody body = bulletClone.GetComponent<Rigidbody>();
            body.velocity = body.transform.forward * speed;
        }
        else
        {
            GameObject bulletClone = Instantiate(bullet, gun.position, model.rotation);
            Rigidbody body = bulletClone.GetComponent<Rigidbody>();
            body.velocity = body.transform.forward * speed;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
    }

    public GameObject bullet;
    public float speed;
    public GameObject selectedEnemy;
    public Transform model, astronaut;
    public float distance;
    public Transform gun;
    public LayerMask enemyLayer;
    public Animator animator;
    PlayerController controller;
    Stats playerStats;
    public float timer;
    float cooldown;
    public float correction;
   
}
