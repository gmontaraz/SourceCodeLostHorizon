using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
public class Stats : MonoBehaviour
{
    float cooldown;
    public bool pause;
    public bool carry, nearSpaceship, createPortal;
    public Text gravityText;
    LineRenderer lineRenderer;
    public Volume volume;
    public Animator playeraAnimator;
    public Transform trailPos;
    public float oxygen = 100;
    public float hp = 100;
    public float player_fuel = 100;
    public float ship_fuel = 100;
    public Slider player_fuel_slider;
    public Slider oxygen_slider;
    public Slider ship_fuel_slider, ship_fuel_slider_world;
    public Slider player_hp;
    public float vulnerable;
    public bool onShip;
    public bool dead;
    public Transform seat, exit;
    public GameObject spaceshipCamera, orbitCamera;
    public int power;
    public GameObject walkingParticles;
    public GameObject[] playerObjects;
    public GameObject explosion, item;
    public Canvas gameCanvas, deadCanvas;
    public Transform lastCheckpoint, lastCheckpointSpaceShip, spaceShip;
    public AudioSource breathingSound;
    public Text enterSpaceship;
    public Image[] inventory;
    PlayerController controller;
    public AudioSource walkSound;
    public GameObject model;
    public Canvas pauseMenu;

    // Start is called before the first frame update
    void Start()
    {
        breathingSound.volume = 0f;
        power = 0;
        cooldown = 0;
        onShip = false;
        createPortal = false;
        vulnerable = 0;
        dead = false;
        pause = false;
        lineRenderer = GetComponent<LineRenderer>();
        controller = GetComponent<PlayerController>();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (controller.onToxic)
        {
            hp -= Time.deltaTime * 10;
            volume.weight = 1;
        }
        else
        {
            volume.weight = 0;
        }
        if (onShip)
        {
            walkSound.volume = 0f;
            model.SetActive(false);
        }
        else
        {
            model.SetActive(true);
            walkSound.volume = 0.1f;
        }
        string gravity = Mathf.Abs(CustomGravity.GetGravity(transform.position).magnitude).ToString("F1");
        lineRenderer.SetPosition(0, spaceShip.position);
        lineRenderer.SetPosition(1, trailPos.position);

        gravityText.text ="Gravity \n"+ gravity + " G";

        player_fuel_slider.value = player_fuel;
        ship_fuel_slider.value = ship_fuel;
        ship_fuel_slider_world.value = ship_fuel;
        oxygen_slider.value = oxygen;
        player_hp.value = hp;

        if (Vector3.Distance(transform.position, spaceShip.transform.position)< 15)
        {
            nearSpaceship = true;
            lineRenderer.enabled = true;
        }
        else
        {
            nearSpaceship = false;
            lineRenderer.enabled = false;
        }

        if (!nearSpaceship && oxygen>=0)
        {
            oxygen -= Time.deltaTime;
            
        }
        else if(oxygen<100 && nearSpaceship)
        {
            oxygen += Time.deltaTime*20;
        }
       
        if (oxygen < 20)
        {
            volume.weight =1-( oxygen / 20);
            breathingSound.volume = 0.6f * (1 - (oxygen / 20));
            breathingSound.pitch = 1.5f + (0.5f * (1 - (oxygen / 20)));
        }
        else
        {
            breathingSound.volume = 0f;
        }
        if (oxygen <= 0 && !dead)
        {
            hp -= Time.deltaTime * 5;
        }
        if (hp <= 0 && !dead)
        {
            Die();
        }
        if (cooldown >= 0)
        {
            cooldown -= Time.deltaTime;
        }
        if(dead && Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
        if (ship_fuel <= 0)
        {
            dead = true;
            spaceShip.GetComponent<SpaceShipController>().velocityParticles.emissionRate = 0;
            gameCanvas.enabled = false;
            deadCanvas.enabled = true;
        }
        if (vulnerable >= 0)
        {
            volume.weight = vulnerable;
            vulnerable -= Time.deltaTime;
        }
        if (onShip)
        {
            ship_fuel_slider_world.gameObject.SetActive(false);
            GetComponent<SphereCollider>().enabled = false;
            enterSpaceship.enabled = false;
        }
        else
        {
            ship_fuel_slider_world.gameObject.SetActive(true);
            GetComponent<SphereCollider>().enabled = true;
        }
        if (Input.GetKeyDown(KeyCode.Escape) && pause == false)
        { 
            PauseMenuOn();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && pause == true)
        {
            PauseMenuOff();
        }


    }
    public void PauseMenuOn()
    {
        FindObjectOfType<OrbitCamera>().enabled = false;
        Debug.Log(pause);
        pause = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        Time.timeScale = 0f;
        pauseMenu.enabled = true;
        gameCanvas.enabled = false;
    }
    public void PauseMenuOff()
    {
        FindObjectOfType<OrbitCamera>().enabled = true;
        pause = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
        pauseMenu.enabled = false;
        gameCanvas.enabled = true;
    }
    void Die()
    {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        volume.weight = 1;
        breathingSound.volume = 0;
        hp = -10;
        Invoke("DeadCanvas", 2f);
        dead = true;
        playeraAnimator.SetTrigger("Die");
    }
    void Restart()
    {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        hp = 100;
        oxygen = 100;
        volume.weight = 0;
        playeraAnimator.SetTrigger("Respawn");
        dead = false;
        onShip = false;
        FindObjectOfType<PlayerController>().enabled = true;
        spaceShip.GetComponent<SpaceShipController>().velocity = Vector3.zero;
        spaceShip.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        ship_fuel = 100f;
        spaceShip.GetComponent<SpaceShipController>().trailLeft.emitting = false;
        spaceShip.GetComponent<SpaceShipController>().trailRight.emitting = false;
        orbitCamera.SetActive(true);
        spaceshipCamera.SetActive(false);

        transform.position = lastCheckpoint.position;
        transform.rotation = lastCheckpoint.rotation;
        spaceShip.position = lastCheckpointSpaceShip.position;
        spaceShip.rotation = lastCheckpointSpaceShip.rotation;
        gameCanvas.enabled = true;
        deadCanvas.enabled = false;
        foreach (GameObject playerObject in playerObjects)
        {
            playerObject.SetActive(true);
        }
        

    }
    private void FixedUpdate()
    {
        if (onShip)
        {
            transform.position = seat.position;
            walkingParticles.SetActive(false);

        }
        else
        {
            walkingParticles.SetActive(true);
        }
    }
    private void LateUpdate()
    {
        if (Input.GetKeyUp(KeyCode.F) && FindObjectOfType<SpaceShipController>().OnGround && onShip && cooldown <= 0 && !dead)
        {
            onShip = false;
            FindObjectOfType<PlayerController>().enabled = true;
            orbitCamera.SetActive(true);
            spaceshipCamera.SetActive(false);
            transform.position = exit.position;
        }
    }
    void changeState()
    {
        onShip = !onShip;
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.tag == "Enemy")
        {
            ReceiveDamage(collision.gameObject.GetComponent<EnemyStats>().damage);
            
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectable") && other.GetComponent<Collect>().collected == false)
        {
            Collect(other);
        }
        if (other.CompareTag("Spaceship") && item!=null)
        {
            item.GetComponent<LineRenderer>().enabled = false;
            item.GetComponent<SphereCollider>().enabled = false;
            item.GetComponent<BoxCollider>().enabled = false;
            var SpringJoint = item.GetComponent<SpringJoint>();
            Destroy(SpringJoint);
            var Rigidbody = item.GetComponent<Rigidbody>();
            Destroy(Rigidbody);
            item.GetComponent<Collect>().collected=false;
            item.GetComponent<Collect>().moveObject();
            item.transform.parent = other.transform;
            inventory[item.GetComponent<Collect>().index].enabled =true;
            bool check = true;
            foreach(Image image in inventory)
            {
                if (image.enabled == false)
                {
                     check = false;
                }
            }
            if (check)
            {
                createPortal = true;
            }
            
        }
    }
    void Collect(Collider other)
    {
        item = other.gameObject;
        item.GetComponent<Collect>().collected = true;
        item.GetComponent<Collect>().CreateJoint();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Spaceship"))
        {
            enterSpaceship.enabled = true;
        }
        if (other.CompareTag("Spaceship") && Input.GetKey(KeyCode.F)&&cooldown<=0 && !dead)
        {
            
            if (!onShip)
            {
                cooldown = 1f;
                onShip = true;
                FindObjectOfType<PlayerController>().enabled = false;
                orbitCamera.SetActive(false);
                spaceshipCamera.SetActive(true);
            }       
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Spaceship"))
        {
            enterSpaceship.enabled = false;
        }
    }
    void ReceiveDamage(int amount)
    {
        if(vulnerable <= 0)
        {
            FindObjectOfType<CameraEffects>().Shake();
            
            if (hp - amount > 0)
            {
                hp -= amount;
                vulnerable = 1f;
            }
            else if(!dead)
            {
                Die();
            }
        }
        
    }
    public void Explosion()
    {
        spaceShip.GetComponent<SpaceShipController>().velocityParticles.emissionRate = 0;
        Debug.Log("Explosion");
        Instantiate(explosion, transform.position, Quaternion.identity);
        foreach (GameObject playerObject in playerObjects){
            playerObject.SetActive(false);
        }
        gameCanvas.enabled = false;
        Invoke("DeadCanvas", 2f);
    }
    void DeadCanvas()
    {
        dead = true;
        deadCanvas.enabled = true;
    }
   

}
