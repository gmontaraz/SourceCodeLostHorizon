using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    Transform playerInputSpace = default;

    [SerializeField, Range(0f, 100f)]
    public float maxSpeed = 10f;

    [SerializeField, Range(0f, 100f)]
    public float maxAcceleration = 10f, maxAirAcceleration = 1f;

    [SerializeField, Range(0f, 100f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;

    [SerializeField, Range(0, 90)]
    float maxGroundAngle = 25f, maxStairsAngle = 50f;

    [SerializeField, Range(0f, 100000f)]
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

    Stats playerStats;

    public BoxCollider jetpackTrigger;


    Vector3 contactNormal, steepNormal;

    int groundContactCount, steepContactCount;

    public bool OnGround => groundContactCount > 0;

    public bool onWater, onToxic, aim;
    

    bool OnSteep => steepContactCount > 0;

    int jumpPhase;
    [SerializeField]
    float jumpTimer;

    [SerializeField]
    float jumpTime;

    float minGroundDotProduct, minStairsDotProduct;

    int stepsSinceLastGrounded, stepsSinceLastJump;

    bool activateJetpack;

    public Vector2 playerInput;

    public Animator animator;

    public ParticleSystem walkingParticles;
    [SerializeField] 
    public float groundSpeed, waterSpeed, waterAcceleration, groundAcceleration, jetpackForce, jetpackForceMin, jetpackConsume;

    public AudioSource audioSourceJetpack, audioSourceWalk;
    

    public ParticleSystem fireParticles, smokeParticles;

    bool walkSound;

    public Canvas playerFuel;
    
    void OnValidate()
    {
        jumpTimer = jumpTime;
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    void Awake()
    {
    
        playerStats = GetComponent<Stats>();
        walkingParticles.enableEmission = false;
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        OnValidate();
        onWater = false;
    }

    void Update()
    {
        transform.up = CustomGravity.GetUpAxis(transform.position);

        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        if (playerStats.onShip)
        {
            audioSourceWalk.volume = 0f;
        }
        else if (playerInput.magnitude > 0.1 && !walkSound && OnGround && !playerStats.dead)
        {
            audioSourceWalk.Play();
            walkSound = true;
        }
        else if(playerInput.magnitude<0.1 || !OnGround)
        {
            walkSound = false;
            audioSourceWalk.Stop();
        }
        else
        {
            audioSourceWalk.Stop();
        }

        if (onWater)
        {
            maxSpeed = waterSpeed;
            maxAcceleration = waterAcceleration;
        }
        else
        {
            maxSpeed = groundSpeed;
            maxAcceleration = groundAcceleration;
        }
        if (playerInputSpace)
        {
            rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
            forwardAxis =
                ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
        }
        else
        {
            rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
        }
        

        
        animator.SetBool("Grounded",OnGround);
        if (playerStats.onShip)
        {
            walkingParticles.Stop();
        }
        else
        {
            if (OnGround)
            {
                walkingParticles.enableEmission = true;
            }
            else
            {
                walkingParticles.enableEmission = false;
            }

        }

        desiredJump |= Input.GetButtonDown("Jump");
        if (playerStats.dead)
        {
            desiredVelocity = Vector3.zero;
        }
        else
        {
            desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        }
        if (OnGround && playerStats.player_fuel < 100)
        {
            playerStats.player_fuel += Time.fixedDeltaTime * (jetpackConsume * 2);
        }
        if (Input.GetButtonDown("Jump") && activateJetpack)
        {
            playerFuel.enabled = true;
            audioSourceJetpack.Play();
        }
        else if (Input.GetButtonUp("Jump"))
        {
            playerFuel.enabled = false;
            audioSourceJetpack.Stop();
        }
    }


    void FixedUpdate()
    {
        Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);
        UpdateState();
        ManageMovement();
        ManageJump();
        velocity += gravity * Time.fixedDeltaTime;
        instantVelocity += gravity * Time.fixedDeltaTime;

        body.velocity = velocity;
        ClearState();

    }
    void ManageJump()
    {
        
        Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);
        if (desiredJump && !onWater && OnGround)
        {
            desiredJump = false;
            animator.SetTrigger("Jump");
            Jump(gravity);
        }
        else if (desiredJump == false && !OnGround)
        {
            activateJetpack = true;
        }
        else if (Input.GetButton("Jump") && playerStats.player_fuel > 0 && activateJetpack)
        {
            fireParticles.Play();
            smokeParticles.emissionRate = 100;
            jetpackTrigger.enabled = true;

            playerStats.player_fuel -= Time.deltaTime * jetpackConsume;

            body.AddForce(upAxis * Mathf.Clamp(jetpackForce * (playerStats.player_fuel / 100), jetpackForceMin, jetpackForce), ForceMode.Acceleration);
        }
        else 
        {
            desiredJump = false;
            fireParticles.Stop();
            smokeParticles.emissionRate = 0;
            jetpackTrigger.enabled = false;
        }

       

        if (OnGround)
        {
            activateJetpack = false;
        }
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

    void ManageMovement()
    {
        xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
        zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);

        currentX = Vector3.Dot(velocity, xAxis);
        currentZ = Vector3.Dot(velocity, zAxis);

        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.fixedDeltaTime;

        if (!aim)
        {
            newX =
            Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
            newZ =
                Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);
        }
        else
        {
            newX =
            Mathf.MoveTowards(currentX, 0, maxSpeedChange);
            newZ =
                Mathf.MoveTowards(currentZ, 0, maxSpeedChange);
        }
        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        instantVelocity = xAxis * (desiredVelocity.x ) + zAxis * (desiredVelocity.z);
    }

    void Jump(Vector3 gravity)
    {
        Vector3 jumpDirection;
        if (OnGround)
        {
            jumpDirection = contactNormal;
        }
        else if (OnSteep)
        {
            jumpDirection = steepNormal;
            jumpPhase = 0;
        }
        else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
        {
            if (jumpPhase == 0)
            {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
        }
        else
        {
            return;
        }

        stepsSinceLastJump = 0;
        jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
        jumpDirection = (jumpDirection + upAxis).normalized;
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        { 
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        velocity += jumpDirection * jumpSpeed;
        

    }
  

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            onWater = true;
        }
        else if (other.CompareTag("Toxic"))
        {
            onWater = true;
            onToxic = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            onWater = false;
           
        }
        else if (other.CompareTag("Toxic"))
        {
            onWater = false;
            onToxic = false;
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
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
