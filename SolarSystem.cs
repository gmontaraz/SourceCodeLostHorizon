using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarSystem : MonoBehaviour
{
    // Start is called before the first frame update
    public List<AstronomicalObject> a_objects;
    private void Start()
    {
        player = FindObjectOfType<Stats>().transform;
    }
    void Update()
    {
        playerPlanet = false;

        foreach(AstronomicalObject ao in a_objects)
        {
            if (Vector3.Distance(player.position, ao.transform.position) < 200)
            {
                ao.isPlayer = true;
                playerPlanet = true;
                foreach (AstronomicalObject other in a_objects)
                {
                    if (other != ao)
                    {
                        other.center = ao.transform;
                    }
                }
            }
            else
            {
                ao.isPlayer = false;
            }
           
        }
        foreach (AstronomicalObject ao in a_objects)
        {
            if (!ao.isPlayer && playerPlanet)
            {
                ao.Orbit();
            }
            else if (!playerPlanet)
            {
                ao.OrbitOut();
            }
        }
        if (playerPlanet)
        {
            RenderSettings.skybox.SetFloat("_Rotation", Time.time * 1);
        }
        
    }
    Transform player;
    public Transform sun;
    public bool playerPlanet;
}
