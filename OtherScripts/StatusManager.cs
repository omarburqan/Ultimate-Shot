using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class StatusManager : NetworkBehaviour {
    private int team;
    [SerializeField]
    private Text timeCounter;
    [SyncVar]
    private int timerInt;
    [SerializeField]
    private int maxTime = 3;
    [SyncVar]
    public string maptoLoad;
    [SyncVar]
    bool countDown = false;
    // Use this for initialization
    void Start() {
        if (!isLocalPlayer)
            return;
        team = this.GetComponent<HealthManager>() ? GetComponent<HealthManager>().Team : GetComponent<CarHealthManager>().Team;
        timerInt = maxTime;
        timeCounter.text = " ";
    }
	
	// Update is called once per frame
	void Update () {

        if (countDown && maptoLoad != "")
        {
            timeCounter.text = timerInt.ToString();
            countDown = false;
            StartCoroutine(Timer());
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            PlayerUpgrade();
        }
    }
    [ClientRpc]
    public void RpcteamMatePosition(string CheckpointTag,string partnerName)
    {
        if (this.GetComponent<HealthManager>())
        {
            print("ss");
            if (GameManager.instance.getDriver(partnerName) )
            {
                print("xx");
                if (GameManager.instance.getDriver(partnerName).Team == this.team)
                {
                    if (GameObject.FindGameObjectWithTag(CheckpointTag))
                        GameObject.FindGameObjectWithTag(CheckpointTag).SetActive(false);
                }
            }
        } 
    }
    
    public void ChangeMap(string mapName)
    {
        this.countDown = true;
        this.maptoLoad = mapName;
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(1); 
        if (timerInt > 0)
        {
            this.timerInt -= 1;
            countDown = true;
        }
        if (timerInt == 0 && maptoLoad != "")
        {
            this.countDown = false;
            Instantiate(Resources.Load(maptoLoad) as GameObject);
            maptoLoad = "";
            this.transform.position = GameObject.FindGameObjectWithTag("SpawnPoints3").transform.position;
            if (GameObject.FindGameObjectWithTag("MAP1"))
                GameObject.FindGameObjectWithTag("MAP1").SetActive(false);
            else if (GameObject.FindGameObjectWithTag("MAP2"))
                GameObject.FindGameObjectWithTag("MAP2").SetActive(false);
            timeCounter.text = " ";
            CmdonValueChange();
            timerInt = maxTime;
            PlayerUpgrade();
        }
    }
    [Command]
    void CmdonValueChange()
    {
        this.countDown = false;
        this.maptoLoad = "";
    }
    public void PlayerUpgrade()
    {
        if (this.name.IndexOf("Driver") == -1) // in case of player(Shooter)
        {
            try
            {
                InteractiveWeapon activeWeapon = this.GetComponent<WeaponHandling>().Weapon;
                activeWeapon.recoilAngle = 0f;
                activeWeapon.bulletDamage = 50;
                activeWeapon.upgradeWeapon();
            }
            catch (Exception e)
            {
                print("No Weapon");
            }
        }
        else if (this.name.IndexOf("Driver") != -1) // in case of driver(Shooter)
        {
            try
            {
                RCC_CarControllerV3 carController = this.GetComponent<RCC_CarControllerV3>();
                carController.useNOS = true;
                carController.useTurbo = true;
                carController.useExhaustFlame = true;
            }
            catch (Exception e)
            {
                print("No shooter");
            }
        }
    }
}
