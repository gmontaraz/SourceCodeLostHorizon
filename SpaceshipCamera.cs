using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = spaceship.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 spaceshipPos = spaceship.position+(spaceship.rotation*distance);
        transform.position = Vector3.Lerp(transform.position,spaceshipPos, Time.deltaTime * smooth);
        Quaternion toRot = Quaternion.LookRotation(spaceship.position - transform.position, spaceship.up);
        Quaternion curRot = Quaternion.Slerp(transform.rotation, toRot, rotSmooth * Time.deltaTime);
        transform.rotation = curRot;

    }
    public Transform spaceship;

    [SerializeField, Range(0, 100)]
    float smooth,rotSmooth;

    [SerializeField]
    Vector3 distance = new Vector3(0f,2f,-10);



}
