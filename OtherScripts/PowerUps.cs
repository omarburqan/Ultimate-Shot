using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
// a class to manager the player ineraction with power ups and for driver it also manager shooting (rocket,plazma,block ball)

public class PowerUps : NetworkBehaviour
{
    public ParticleSystem Rocket;
    bool hasRocket;
    bool hasPlasma;
    bool hasBall;
    bool isShooter;
    bool isDriver;
    public ParticleSystem Plasma;
    public GameObject pickupEffect;
    public GameObject FireBall;
    GameObject powerUp;
    public GameObject frontBumper;
    public GameObject rearBumper;
    Camera playerCamera;
    public AudioClip pickedPower;
    public AudioClip explosionSound;
    public GameObject explosionEffect;
    private void Start()
    {
        if (name.IndexOf("Driver") == -1)
        { // in case of shooting player(Shooter)
            isShooter = true;
            isDriver = false;
        }
        else if (name.IndexOf("Driver") != -1)
        {
            isDriver = true;
            isShooter = false;
        }
        playerCamera = Camera.main;
        print(playerCamera.transform.name);

    }
    private void Update()
    {
        if (!isLocalPlayer)
            return;
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            hasRocket = true;
            hasPlasma = true;
            hasBall = true;
        }
        if (isDriver) // shooting from car by player input
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && hasRocket)
            {
                shotFromCar(Rocket);
            }
            if (Input.GetKeyDown(KeyCode.Mouse1) && hasPlasma)
            {
                shotEffect("plasma");
                if (isServer)
                {
                    RpcshotEffect("plasma");
                }
                else
                {
                    CmdshotEffect("plasma");
                }
            }
            if (Input.GetKeyDown(KeyCode.Mouse2) && hasBall)
            {
                shotEffect("ball");
                if (isServer)
                {
                    RpcshotEffect("ball");
                }
                else
                {
                    CmdshotEffect("ball");
                }
            }
        }  
    }
    [Command]
    void CmdshotEffect(string shotType)
    {
        if (isServer)
        {
            shotEffect(shotType);
        }
        RpcshotEffect(shotType);
    }
    [ClientRpc]
    void RpcshotEffect(string shotType)
    {
        if (!isLocalPlayer && !isServer)
        {
            shotEffect(shotType);
        }
    }
    void shotEffect(string shotType)
    {
        if(shotType == "plasma")
        {
            ParticleSystem instantShot = Instantiate(Plasma, rearBumper.transform.position, rearBumper.transform.rotation);
            instantShot.Play();
        }
        else if(shotType == "ball")
        {
            GameObject instantShot = Instantiate(FireBall, rearBumper.transform.position, rearBumper.transform.rotation);
        }
    }
    private void rocketShot(Quaternion vector,Vector3 hit)
    {
        rocketEffect(vector,hit);
        if (isServer)
        {
            RpcrocketEffect(vector,hit);
        }
        else
        {
            CmdrocketEffect(vector,hit);
        }
    }
    [Command]
    void CmdrocketEffect(Quaternion vector, Vector3 hit)
    {
        if (isServer)
        {
            rocketEffect(vector,hit);
        }
        RpcrocketEffect(vector,hit);
    }
    [ClientRpc]
    void RpcrocketEffect(Quaternion vector, Vector3 hit)
    {
        if (!isLocalPlayer && !isServer)
        {
            rocketEffect(vector,hit);
        }
    }
    void rocketEffect(Quaternion vector,Vector3 hitpoint)
    {
        AudioSource.PlayClipAtPoint(explosionSound, transform.position, 100f);
        GameObject Effect = Instantiate(explosionEffect);
        Effect.transform.position = hitpoint;
        Effect.SetActive(true);
        Destroy(Effect, 3);
        ParticleSystem instantShot = Instantiate(Rocket, frontBumper.transform.position, vector);
        instantShot.Play();
    }
    private void shotFromCar(ParticleSystem powerToShot)
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit = default(RaycastHit);
        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.collider.transform != this.transform)
            {
                // Handle shot effects on target.
                //ParticleSystem instantShot = Instantiate(powerToShot, frontBumper.transform.position, Quaternion.LookRotation(hit.point - this.transform.position));
                //instantShot.Play();
                rocketShot(Quaternion.LookRotation(hit.point - this.transform.position),hit.point);
                // Call the damage behaviour of target if exists.
                if (hit.collider.gameObject.GetComponent<HealthManager>() || hit.collider.gameObject.GetComponent<CarHealthManager>())
                {
                    CmdTelltheServer(hit.collider.name, 100 , this.name, hit.point);
                }
            }
            else
            {
                Vector3 destination = (ray.direction * 100f) - ray.origin;
                // Handle shot effects without a specific target.
                //ParticleSystem instantShot = Instantiate(powerToShot, frontBumper.transform.position, Quaternion.LookRotation(destination - this.transform.position));
                //instantShot.Play();
                rocketShot(Quaternion.LookRotation(hit.point - this.transform.position),destination);
            }
        }
    }
    [Command]
    void CmdTelltheServer(string playerID, float damage, string Name, Vector3 direction)
    {
        if (playerID.IndexOf("Driver") == -1)
        { // in case of shooting player(Shooter)
            HealthManager _player = GameManager.instance.getPlayer(playerID);
            _player.TakeDamage(damage, Name);
        }
        else if (playerID.IndexOf("Driver") != -1)
        {
            CarHealthManager _driver = GameManager.instance.getDriver(playerID);
            _driver.TakeDamage(damage, Name, direction);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag=="PowerUpEuro" && isShooter)
        {
            Destroy(other.transform.parent.gameObject);
            if (!isLocalPlayer)
                return;
            Pickup("PowerUpEuro");
            AudioSource.PlayClipAtPoint(pickedPower, transform.position, 3f);
        }
        if (other.gameObject.tag == "PowerUpDollar" && isDriver)
        {
            Destroy(other.transform.parent.gameObject);
            if (!isLocalPlayer)
                return;
            Pickup("PowerUpDollar");
            AudioSource.PlayClipAtPoint(pickedPower, transform.position, 3f);
        }
        if (other.gameObject.tag == "PowerUpElectric" && isDriver)
        {
            Destroy(other.transform.parent.gameObject);
            if (!isLocalPlayer)
                return;
            Pickup("PowerUpElectric");
            AudioSource.PlayClipAtPoint(pickedPower, transform.position, 3f);
        }
        else if (other.gameObject.tag == "PowerUpHealthBlue")
        {
            Destroy(other.transform.parent.gameObject);
            if (!isLocalPlayer)
                return;
            Pickup("PowerUpHealthBlue");
            AudioSource.PlayClipAtPoint(pickedPower, transform.position, 3f);
            if (!isServer && isDriver)
            {
                CmdPickup(1);
            }
        }
        else if (other.gameObject.tag == "PowerUpHealthRed")
        {
            Destroy(other.transform.parent.gameObject);
            if (!isLocalPlayer)
                return;
            Pickup("PowerUpHealthRed");
            AudioSource.PlayClipAtPoint(pickedPower, transform.position, 3f);

            if (!isServer)
            {
                CmdPickup(2);
            }
        }
        else if (other.gameObject.tag == "PowerUpRocket" && isDriver)
        {
            Destroy(other.transform.parent.gameObject);
            if (!isLocalPlayer)
                return;
            Pickup("PowerUpRocket");
            AudioSource.PlayClipAtPoint(pickedPower, transform.position, 3f);
        }


    }
    [Command]
    void CmdPickup(int add)
    {
        if (isServer)
        {
            if (add == 2)
            {
                if (this.isShooter)
                {
                    HealthManager healthManager = this.GetComponent<HealthManager>();
                    healthManager.SetHealthPoints();
                }
                else if (this.isDriver)
                {
                    CarHealthManager carHealthManager = this.GetComponent<CarHealthManager>();
                    carHealthManager.SetHealthPoints();
                    try
                    {
                        Destroy(transform.Find("SmokeEffect(Clone)").gameObject);
                    }
                    catch (Exception e)
                    {
                        print("no effect");
                    }
                }
            }
            else
            {
                CarHealthManager carHealthManager = this.GetComponent<CarHealthManager>();
                carHealthManager.SetDefendPoints();
            }
        }
    }
    private void Pickup(string powerUpType)
    {
        if(powerUpType == "PowerUpElectric")
        {
            this.hasPlasma = true;
        }
        else if (powerUpType == "PowerUpRocket")
        {
             this.hasRocket = true;
        }
        else if (powerUpType == "PowerUpHealthBlue") // defend for car  , fly power for shooter
        {
            if (isShooter)
            {
                FlyBehaviour flyBehaviour = this.GetComponent<FlyBehaviour>();
                flyBehaviour.setFlyPoints();
            }
            else if (isDriver)
            {
                CarHealthManager carHealthManager = this.GetComponent<CarHealthManager>();
                carHealthManager.SetDefendPoints();
            }
        }
        else if (powerUpType == "PowerUpHealthRed") 
        {
            if (isShooter)
            {
                HealthManager healthManager = this.GetComponent<HealthManager>();
                healthManager.SetHealthPoints();
            }
            else if (isDriver)
            {
                CarHealthManager carHealthManager = this.GetComponent<CarHealthManager>();
                carHealthManager.SetHealthPoints();
                try
                {
                    Destroy((transform.Find("SmokeEffect(Clone)")).gameObject);
                }
                catch (Exception e)
                {
                    print("no effect");
                }
            }
        }
        else if(powerUpType == "PowerUpEuro") // defend for ran over for shooter
        {
            HealthManager healthManager = this.GetComponent<HealthManager>();
            healthManager.hasDefened = true;
        }
        else if (powerUpType == "PowerUpDollar") // fireball for cars
        {
            this.hasBall = true;
        }
    }
}
