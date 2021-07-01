using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collect : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("TrailPos").transform;
        lineRenderer = GetComponent<LineRenderer>();
        spaceship = FindObjectOfType<SpaceShipController>().transform;
    }

    // Update is called once per frame
    void Update()
    {
       
        if (collected)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, player.position);
        }
        
    }
    public void CreateJoint()
    {
        springJoint = gameObject.AddComponent<SpringJoint>();
        springJoint.connectedBody = player.parent.GetComponent<Rigidbody>();
        springJoint.connectedMassScale = 0f;
        springJoint.minDistance = 2;
        springJoint.maxDistance = 3;
        springJoint.autoConfigureConnectedAnchor = false;
        springJoint.anchor = Vector3.zero;
        springJoint.connectedAnchor = Vector3.zero;
        springJoint.massScale = 10f;
        springJoint.connectedBody = player.parent.GetComponent<Rigidbody>();
    }
    public IEnumerator moveObjectCO()
    {
        float totalMovementTime = 2f; 
        float currentMovementTime = 0f;
        Vector3 Origin = transform.position;
        Vector3 Destination = spaceship.transform.position;
        while (Vector3.Distance(transform.position, spaceship.position) > 0)
        {
            currentMovementTime += Time.deltaTime;
            transform.position = Vector3.Lerp(Origin, Destination, currentMovementTime / totalMovementTime);
            yield return null;
        }
        

    }
    public void moveObject()
    {
        StartCoroutine(moveObjectCO());
        Invoke("DestroyGameObject", 2f);
    }
    void DestroyGameObject()
    {
        Destroy(gameObject);
    }
    Transform player, spaceship;
    [SerializeField]
    float speed = 0.1f;
    SpringJoint springJoint;
    LineRenderer lineRenderer;
    public int index;
    public bool collected, moveToSpaceship;
    
}
