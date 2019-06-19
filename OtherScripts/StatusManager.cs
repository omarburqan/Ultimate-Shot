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
    [SyncVar]
    bool FinishFinal = false;
    [SyncVar]
    int position;
    bool isShooter;
    bool isDriver;
    // Use this for initialization
    void Start() {
        
        team = this.GetComponent<HealthManager>() ? GetComponent<HealthManager>().Team : GetComponent<CarHealthManager>().Team;
        timerInt = maxTime;
        maptoLoad = "";
        position = 0;
        timeCounter.text = " ";
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
    }

    // Update is called once per frame
    void Update () {
        if (!isLocalPlayer)
            return;
        if (countDown && maptoLoad != "")
        {
            timeCounter.text = timerInt.ToString();
            countDown = false;
            StartCoroutine(Timer());
        }
        if (FinishFinal)
        {
            FinishOfTheGame();
            FinishFinal = false;
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            ChangeMap("Map4WD",1);
        }

    }
    [ClientRpc]
    public void RpcteamMatePosition(string CheckpointTag,string partnerName)
    {
        if (this.GetComponent<HealthManager>())
        {
            if (GameManager.instance.getDriver(partnerName) )
            {
                if (GameManager.instance.getDriver(partnerName).Team == this.team)
                {
                    if (GameObject.FindGameObjectWithTag(CheckpointTag))
                        GameObject.FindGameObjectWithTag(CheckpointTag).SetActive(false);
                }
            }
        } 
    }
    
    public void ChangeMap(string mapName,int counter)
    {
        this.position = counter;
        this.maptoLoad = mapName;
        this.countDown = true;
    }
    public void FinishPermission()
    {
        this.FinishFinal = true;
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
            if (isShooter)
            {
                List<GameObject> spawnPoints = new List<GameObject>(GameObject.FindGameObjectsWithTag("SpawnPints3S"));
                spawnPoints.Sort(CompareListByName);
                this.transform.position = spawnPoints[position].transform.position;
            }
            else if (isDriver)
            {
                List<GameObject> spawnPoints = new List<GameObject>(GameObject.FindGameObjectsWithTag("SpawnPoints3"));
                spawnPoints.Sort(CompareListByName);
                this.transform.position = spawnPoints[position].transform.position;
                this.GetComponent<CheckpointCheck>().levelNumber += 1;
                GameManager.instance.changeLap();
            }
            StartCoroutine(DestroyOldMap());
            timeCounter.text = " ";
            CmdonValueChange();
            timerInt = maxTime;
            PlayerUpgrade();
        }
    }
    IEnumerator DestroyOldMap()
    {
        yield return new WaitForEndOfFrame();
        if (GameObject.FindGameObjectWithTag("MAP1")) {
            GameObject.FindGameObjectWithTag("MAP1").SetActive(false); }
        else if (GameObject.FindGameObjectWithTag("MAP2")) { 
            GameObject.FindGameObjectWithTag("MAP2").SetActive(false); }
        else if (GameObject.FindGameObjectWithTag("Map#")){
            GameObject.FindGameObjectWithTag("Map#").SetActive(false); }
        if(isDriver)
            GameObject.FindGameObjectWithTag("Checkpoint1").GetComponent<MeshRenderer>().materials[0].color = Color.cyan;


    }
    [Command]
    void CmdonValueChange()
    {
        this.countDown = false;
        this.maptoLoad = "";
        this.position = 0;
    }
    public void PlayerUpgrade()
    {
        if (isShooter) // in case of player(Shooter)
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
        else if (isDriver) // in case of driver(Shooter)
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
    private void FinishOfTheGame()
    {
        this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        if(isDriver)
            this.GetComponent<DisableCom>().FreezePlayer();
        if(isShooter)
            this.GetComponent<NetwrokBehaviour>().FreezePlayer();

        this.GetComponent<PlayerUI>().ScoreBoard.SetActive(true);
        FinishFinal = false;
    }
    private static int CompareListByName(GameObject i1, GameObject i2)
    {
        return i1.name.CompareTo(i2.name);
    }
}
