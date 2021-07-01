using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AiEnemy : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ProjectWaypoints();
        gravity = CustomGravity.GetUpAxis(transform.position);
        counter = 0;
        body = GetComponent<Rigidbody>();
        enemyState = EnemyStates.Patrol;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 gravity = CustomGravity.GetGravity(transform.position, out upAxis);
        StateMachine();
        // Grounded check
        Ray ray = new Ray(ground.position, -transform.up);
        Debug.DrawRay(ground.position, -transform.up);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1f, groundedMask))
        {
            grounded = true;
            normal = hit.normal;
        }
        else
        {
            grounded = false;
        }
        if(enemyState == EnemyStates.Patrol)
        {
            objective = waypoint[counter];
            if (Vector3.Distance(objective.position, transform.position) < 0.5)
            {
                counter++;
                if (counter > 3)
                {
                    counter = 0;
                }
            }
        }
        else if(enemyState == EnemyStates.Chase)
        {
            objective = player;
        }
        Vector3 objectiveModified= transform.InverseTransformPoint(objective.position);
        objectiveModified.y = transform.InverseTransformPoint(transform.position).y;
        objectiveModified = transform.TransformPoint(objectiveModified);
        Vector3 direction = objectiveModified-transform.position;
        
        
        Quaternion rotation = Quaternion.LookRotation(direction, CustomGravity.GetUpAxis(transform.position));
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotSpeed * Time.deltaTime);
        body.AddForce(gravity,ForceMode.Acceleration);
        if (body.velocity.magnitude < maxSpeed && grounded)
        {
            
            body.AddForce(direction * acceleration,ForceMode.Acceleration);
        }
        for (int i = 0; i < 4; i++)
        {

            Debug.DrawRay(sensors[i].position, -transform.up);
        }
    }
    void StateMachine()
    {
        if (enemyState == EnemyStates.Patrol && Vector3.Distance(transform.position, player.position) < patrolArea)
        {
            enemyState = EnemyStates.Chase;
        }
        if(enemyState == EnemyStates.Chase && Vector3.Distance(transform.position, player.position) > patrolArea)
        {
            enemyState = EnemyStates.Patrol;
        }
    }
    void ProjectWaypoints()
    {
        for(int i = 0; i<4; i++)
        {
            
            RaycastHit hit;
            Vector3 randomPos = new Vector3(Random.Range(-patrolArea, patrolArea), 
            Random.Range(-patrolArea, patrolArea), Random.Range(-patrolArea, patrolArea)) + transform.position;
            if (Physics.Raycast(sensors[i].position, -transform.up, out hit, Mathf.Infinity, groundedMask))
            {
                waypoint[i].position = hit.point;
                waypoint[i].parent = hit.transform;
            }

        }
    }
    public float patrolArea;
    Vector3 normal;
    Vector3 gravity;
    int counter;
    public LayerMask groundedMask;
    public float rotSpeed;
    public bool grounded;
    Transform objective;
    public float acceleration;
    public float maxSpeed;
    public float damping;
    public Transform player;
    public Transform ground;
    public Transform[] sensors;
    Rigidbody body;
    public Transform[] waypoint;
    Vector3 velocity, upAxis;
    EnemyStates enemyState;
    enum EnemyStates {Patrol, Chase, Return}
}
