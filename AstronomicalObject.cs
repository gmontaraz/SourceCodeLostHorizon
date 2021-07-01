using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AstronomicalObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        isPlayer = false;
        rb = GetComponent<Rigidbody>();
        velocity = init_velocity;
        player = FindObjectOfType<Stats>().transform;
        solar_system = FindObjectOfType<SolarSystem>();
    }
    public void Orbit()
    {
        transform.Rotate(Vector3.up, rot_speed * Time.deltaTime);
        
        transform.RotateAround(center.position, Vector3.up, orbit_speed * Time.deltaTime);
    }
    public void OrbitOut()
    {
        transform.Rotate(Vector3.up, rot_speed * Time.deltaTime);

        transform.RotateAround(center.position, Vector3.up, (orbit_speed/2) * Time.deltaTime);
    }
    public Vector3 velocity;
    Rigidbody rb;
    public Vector3 init_velocity;
    SolarSystem solar_system;
    public float rot_speed;
    Transform player;
    public Transform center;
    public float orbit_speed;
    public int radius;
    public bool isPlayer;

}
