using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RealisticOrbit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        velocity = init_velocity;
       
        solar_system = FindObjectOfType<SolarSystem>();  
    }
   

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(Vector3.up, rot_speed/100);

        Gravity();
    }
    void Gravity()
    {
        foreach (AstronomicalObject a_object in solar_system.a_objects)
        {
            if (a_object != this)
            {
                Rigidbody rb2 = a_object.GetComponent<Rigidbody>();
                float gravity_force = Universe.gravitationalConstant * (rb.mass * rb2.mass)
                    / ((Vector3.Distance(rb.position, rb2.position) * (Vector3.Distance(rb.position, rb2.position))));
                Vector3 acceleration = -gravity_force * (rb.position - rb2.position).normalized;
                velocity += acceleration * Universe.physicsTimeStep;
            }

        }
        transform.Translate(velocity * Universe.physicsTimeStep);
    }
    public Vector3 velocity;
    Rigidbody rb;
    public Vector3 init_velocity;
    SolarSystem solar_system;
    public float rot_speed;
    public int radius;

}
