using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelRotate : MonoBehaviour
{
    Vector3 upAxis;
    MovingSphere controller;
    Quaternion locVel;
    public Animator animator;
    public float rotSpeed;
    public Transform camara;
    Vector3 rightAxis, forwardAxis, moveDir;
    // Start is called before the first frame update
    void Awake()
    {
        controller = FindObjectOfType<MovingSphere>();

    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("Speed", controller.body.velocity.magnitude / controller.maxSpeed);
        Quaternion lookMoveDir;
        Vector3 upAxis = CustomGravity.GetUpAxis(transform.position);

        Vector3 xAxis = controller.rightAxis;
        Vector3 zAxis = controller.forwardAxis;
        
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));



        float x = Vector3.Dot(xAxis, input);
        float z = Vector3.Dot(zAxis, input);
        if (controller.instantVelocity.magnitude > 1)
        {
            target.transform.position = controller.instantVelocity + transform.position;
        }

        
        moveDir = Vector3.Lerp(moveDir, ProjectPointOnPlane(upAxis, transform.position, target.position),rotSpeed*Time.deltaTime);
        transform.LookAt(moveDir, upAxis);


    }
    Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }
    Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point ){
     planeNormal.Normalize();
     var distance = -Vector3.Dot(planeNormal.normalized, (point - planePoint));
     return point + planeNormal* distance;
    }
    public Transform target;


}
