using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpaceShipController : MonoBehaviour
{
    float maxAcceleration = 0f;
    [SerializeField]
    Transform playerInputSpace = default;

    [SerializeField, Range(0f, 1000f)]
    public float maxSpeed = 10f;

    [SerializeField, Range(0f, 1000f)]
    float maxAirAcceleration = 1f, propelForce;

    [SerializeField, Range(0f, 50f)]
    int torque, Rotate;

    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;

    [SerializeField, Range(0, 90)]
    float maxGroundAngle = 25f, maxStairsAngle = 50f;

    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = 100f;

    [SerializeField, Min(0f)]
    float probeDistance = 1f;

    [SerializeField]
    LayerMask probeMask = -1, stairsMask = -1;

    public Rigidbody body;

    public Vector3 velocity, desiredVelocity, instantVelocity;

    public Vector3 upAxis, rightAxis, forwardAxis;

    public float currentX, currentZ, newX, newZ;

    public Vector3 xAxis, zAxis;

    bool desiredJump;

    Vector3 contactNormal, steepNormal;

    int groundContactCount, steepContactCount;

    public bool OnGround => groundContactCount > 0;

    public bool onWater;

    bool sounds;

    AudioSource propelSound;
    bool OnSteep => steepContactCount > 0;

    int jumpPhase;

    float minGroundDotProduct, minStairsDotProduct;

    int stepsSinceLastGrounded, stepsSinceLastJump;

    public Vector2 playerInput;

    public Animator animator;

    public ParticleSystem walkingParticles;

    [SerializeField]
    float waterSpeed, waterAcceleration, groundSpeed, groundAcceleration;

    public ParticleSystem[] particles;

    public ParticleSystem[] propel;

    public ParticleSystem[] propel_left;

    public ParticleSystem[] propel_right;

    public Light light, lightPropel;

    public ParticleSystem velocityParticles;

    public Text textSpeed;

    public int consumeSpeed;

    [SerializeField]
    float groundCooldown;

    Stats playerStats;

    public TrailRenderer trailLeft, trailRight;


    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    void Awake()
    {
        propelSound = GetComponent<AudioSource>();
        groundCooldown = 2;
        playerStats = FindObjectOfType<Stats>();
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        OnValidate();
        onWater = false;
    }
    void Steer()
    {
 
        Vector3 mc = new Vector3(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"), 0);
        body.AddRelativeTorque(mc * torque, ForceMode.Acceleration);
    }
    void VFX()
    {
        if (playerStats.ship_fuel <= 0)
        {
            propelSound.Stop();
            trailLeft.emitting = false;
            trailRight.emitting = false;

            light.enabled = false;
            foreach (ParticleSystem particle in particles)
            {
                particle.Stop();
            }
            lightPropel.enabled = false;
            foreach (ParticleSystem particle in propel_right)
            {
                particle.Stop();
            }
            lightPropel.enabled = false;
            foreach (ParticleSystem particle in propel_left)
            {
                particle.Stop();
            }
            lightPropel.enabled = false;
            foreach (ParticleSystem particle in propel)
            {
                particle.Stop();
            }
        }
        Vector3 velocityLocal = transform.InverseTransformVector(velocity);
        if (velocityLocal.z >= 40 && !playerStats.dead)
        {
            velocityParticles.emissionRate = velocityLocal.z;
        }
        else
        {
            velocityParticles.emissionRate = 0;
        }
        if(!(Input.GetKey(KeyCode.W)|| Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.LeftShift)))
        {
            propelSound.Stop();
        }
        if (Input.GetKeyDown(KeyCode.W) )
        {
            propelSound.Play();
            propelSound.panStereo = 0f;
            trailLeft.emitting = true;
            trailRight.emitting = true;

            light.enabled = true;
            foreach (ParticleSystem particle in particles)
            {
                particle.Play();
            }

        }

        else if (Input.GetKeyUp(KeyCode.W))
        {
            
            trailLeft.emitting = false;
            trailRight.emitting = false;

            light.enabled = false;
            foreach (ParticleSystem particle in particles)
            {
                particle.Stop();
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            propelSound.Play();
            propelSound.panStereo = 0.2f;
            lightPropel.enabled = true;
            foreach (ParticleSystem particle in propel_right)
            {
                particle.Play();
            }
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            lightPropel.enabled = false;
            foreach (ParticleSystem particle in propel_right)
            {
                particle.Stop();
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            propelSound.Play();
            propelSound.panStereo = -0.2f;
            lightPropel.enabled = true;
            foreach (ParticleSystem particle in propel_left)
            {
                particle.Play();
            }
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            lightPropel.enabled = false;
            foreach (ParticleSystem particle in propel_left)
            {
                particle.Stop();
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            propelSound.Play();
            propelSound.panStereo = 0f;
            lightPropel.enabled = true;
            foreach (ParticleSystem particle in propel)
            {
                particle.Play();
            }
        }
        else if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            lightPropel.enabled = false;
            foreach (ParticleSystem particle in propel)
            {
                particle.Stop();
            }
        }
    }
    void Update()
    {
        if (FindObjectOfType<Stats>().onShip){
            VFX();
        }
        if (groundCooldown > 0)
        {
            groundCooldown -= Time.deltaTime;
            body.constraints = RigidbodyConstraints.None;
        }
        else
        {
            body.constraints = RigidbodyConstraints.FreezeRotation;
        }
        if (FindObjectOfType<Stats>().onShip && playerStats.ship_fuel > 0)
        {
            
            
            
            textSpeed.text = Mathf.Round(body.velocity.magnitude) + " m/s";
            

            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");
            playerInput = Vector2.ClampMagnitude(playerInput, 1f);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            if (playerInputSpace)
            {
                rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
                forwardAxis =
                    ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
            }
            else
            {
                rightAxis = ProjectDirectionOnPlane(transform.right, upAxis);
                forwardAxis = ProjectDirectionOnPlane(transform.forward, upAxis);
            }
            desiredVelocity =
                new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        }
    }
    void Propel(float propelForce)
    {
        body.AddForce(transform.up * propelForce, ForceMode.Acceleration);
    }

    void FixedUpdate()
    {
        if (FindObjectOfType<Stats>().onShip && playerStats.ship_fuel>0)
        {
            groundCooldown = 2;
            if (!OnGround)
            {
                Steer();
            }
           
            if (Input.GetKey(KeyCode.Q))
            {
                playerStats.ship_fuel -= Time.deltaTime * consumeSpeed;
               
                body.AddRelativeTorque(Vector3.forward * Rotate, ForceMode.Acceleration);
    
            }

            if (Input.GetKey(KeyCode.E))
            {
                playerStats.ship_fuel -= Time.deltaTime * consumeSpeed;
                body.AddRelativeTorque(-Vector3.forward * Rotate, ForceMode.Acceleration);
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                playerStats.ship_fuel -= Time.deltaTime * consumeSpeed;
               
                Propel(propelForce);
            }
            if (Input.GetKey(KeyCode.LeftControl))
            {
                playerStats.ship_fuel -= Time.deltaTime * consumeSpeed;
                Propel(-propelForce);
            }
            UpdateState();
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            {
                playerStats.ship_fuel -= Time.deltaTime * consumeSpeed;
                AdjustVelocity();
            }     
        }
        Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);
        velocity += gravity * Time.deltaTime;
        instantVelocity += gravity * Time.deltaTime;

        body.velocity = velocity;
        ClearState();
    }

    void ClearState()
    {
        groundContactCount = steepContactCount = 0;
        contactNormal = steepNormal = Vector3.zero;
    }

    void UpdateState()
    {
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
        velocity = body.velocity;
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;
            if (stepsSinceLastJump > 1)
            {
                jumpPhase = 0;
            }
            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = upAxis;
        }
    }

    bool SnapToGround()
    {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
        {
            return false;
        }
        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }
        if (!Physics.Raycast(
            body.position, -upAxis, out RaycastHit hit,
            probeDistance, probeMask
        ))
        {
            return false;
        }

        float upDot = Vector3.Dot(upAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }

        groundContactCount = 1;
        contactNormal = hit.normal;
        float dot = Vector3.Dot(velocity, hit.normal);
        if (dot > 0f)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }
        return true;
    }

    bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();
            float upDot = Vector3.Dot(upAxis, steepNormal);
            if (upDot >= minGroundDotProduct)
            {
                steepContactCount = 0;
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }

    void AdjustVelocity()
    {
        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;
        
        xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
        zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);

        currentX = Vector3.Dot(velocity, xAxis);
        currentZ = Vector3.Dot(velocity, zAxis);

        Vector3 velocityLocal = transform.InverseTransformVector(velocity);
        velocityLocal.x*=0.98f;
        velocityLocal.y*=0.98f;
        velocity = transform.TransformVector(velocityLocal);
        
        newX =
            Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        newZ =
            Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        velocity += (xAxis * (newX -currentX) + zAxis * (newZ-currentZ));
        

        instantVelocity = xAxis * (desiredVelocity.x) + zAxis * (desiredVelocity.z);
    }

    void Propel2(float force)
    {
        
        float jumpSpeed = force;
        Vector3 jumpDirection = transform.up;
        velocity += jumpDirection * jumpSpeed;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            onWater = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            onWater = false;
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
        if (body.velocity.magnitude > 40)
        {
            playerStats.Explosion();
        }
    }
   
    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(upAxis, normal);
            if (upDot >= minDot)
            {
                groundContactCount += 1;
                contactNormal += normal;
            }
            else if (upDot > -0.01f)
            {
                steepContactCount += 1;
                steepNormal += normal;
            }
        }
    }

    Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    float GetMinDot(int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ?
            minGroundDotProduct : minStairsDotProduct;
    }


}
