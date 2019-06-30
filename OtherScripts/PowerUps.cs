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
    bool isShooter;
    bool isDriver;
    public ParticleSystem Plasma;
    public GameObject pickupEffect;
    GameObject powerUp;
    public GameObject frontBumper;
    Camera playerCamera;
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
        print(playerCamera.name);
    }
    private void Update()
    {
        if (!isDriver)
            return;
        if (Input.GetKeyDown(KeyCode.Mouse0) && hasRocket)
        {
            shotFromCar(Rocket);
        }
        if (Input.GetKeyDown(KeyCode.Mouse1) && hasPlasma)
        {
            shotFromCar(Plasma);
        }
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
                ParticleSystem instantShot = Instantiate(powerToShot, frontBumper.transform.position, Quaternion.LookRotation(hit.point - this.transform.position));
                instantShot.Play();
                // Call the damage behaviour of target if exists.
                if (hit.collider.gameObject.GetComponent<HealthManager>() || hit.collider.gameObject.GetComponent<CarHealthManager>())
                {
                    print("hit from car " + this.name);
                }
            }
            else
            {
                print("s1s1s");
                Vector3 destination = (ray.direction * 100f) - ray.origin;
                // Handle shot effects without a specific target.
                ParticleSystem instantShot = Instantiate(powerToShot, frontBumper.transform.position, Quaternion.LookRotation(destination - this.transform.position));
                instantShot.Play();
            }
        }


    }

    void OnTriggerEnter(Collider other)
    {
        print(other.gameObject.name + "        dddddddd ");
        
        if (other.gameObject.tag == "PowerUpElectric" && isDriver)
        {
            Pickup("PowerUpElectric");
            Destroy(other.transform.parent.gameObject);
        }
        else if (other.gameObject.tag == "PowerUpHealthBlue")
        {
            Pickup("PowerUpHealthBlue");
            Destroy(other.transform.parent.gameObject);
        }
        else if (other.gameObject.tag == "PowerUpHealthRed")
        {
            Pickup("PowerUpHealthRed");
            Destroy(other.transform.parent.gameObject);
        }
        else if (other.gameObject.tag == "PowerUpRocket" && isDriver)
        {
            Pickup("PowerUpRocket");
            Destroy(other.transform.parent.gameObject);

        }
    }
    // sync players attributes when taking power ups if needed like (healthpoints,defendpoints)
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
        else if (powerUpType == "PowerUpHealthBlue")
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

            }
        }
    }
}
