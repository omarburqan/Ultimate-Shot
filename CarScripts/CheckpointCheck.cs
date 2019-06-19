using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    using UnityEngine.Networking;
/// <summary>
/// a class to control or manage the car when entering a checkpoint , which will increase the score and disappear the checkpoint 
/// syncing the driver score to other players in order to get a correct scoreboard when ever enabled.
/// </summary>
    public class CheckpointCheck : NetworkBehaviour {

        private bool[] CheckPointbool=new bool[10];
        public int levelNumber;
        private Transform mesh;
        private void Start()
        {
            if (!isLocalPlayer)
                return;
            levelNumber = 1;
            for (int i = 0; i < CheckPointbool.Length; i++)
            {
                CheckPointbool[i] = false;
            }
            GameObject.FindGameObjectWithTag("Checkpoint1").GetComponent<MeshRenderer>().materials[0].color = Color.cyan;
        }
    //private int mapnumber = 0 ; // map number
    private void Update()
    {
        if (!isLocalPlayer)
            return;

        /*GameObject []Mapname; // all the map names
        if (mapnumber >= Mapname.Length())
            mapnumber = 0;
       int map = Random.Range(mapnumber, Mapname.Length());
        swap(Mapname[mapnumber], Mapname[map]);
        mapnumber += 1;*/
        
    }
      
        void OnTriggerEnter(Collider other)
        {
            if (isLocalPlayer) { 
                switch (other.tag)
                {
                    case "Checkpoint1":
                        CheckPointbool[0] = true;
                        CalculateScore(other, "Checkpoint2");
                        break;
                    case "Checkpoint2":
                        if (CheckPointbool[0] == true)
                        {
                            CheckPointbool[1] = true;
                            CalculateScore(other, "Checkpoint3");
                        }
                        break;
                    case "Checkpoint3":
                        if (CheckPointbool[1] == true)
                        {
                            CheckPointbool[2] = true;
                            CalculateScore(other, "Checkpoint4");
                        }
                        break;
                    case "Checkpoint4":
                        if (CheckPointbool[2] == true)
                        {
                            CheckPointbool[3] = true;
                            CalculateScore(other, "Checkpoint5");
                        }
                        break;
                    case "Checkpoint5":
                        if (CheckPointbool[3] == true)
                        {
                            CheckPointbool[4] = true;
                            CalculateScore(other, "Checkpoint6");
                        }
                        break;
                    case "Checkpoint6":
                        if (CheckPointbool[4] == true)
                        {
                            CheckPointbool[5] = true;
                            CalculateScore(other, "Checkpoint7");
                        }
                        break;
                    case "Checkpoint7":
                        if (CheckPointbool[5] == true)
                        {
                            CheckPointbool[6] = true;
                            CalculateScore(other, "Checkpoint8");
                        }
                        break;
                    case "Checkpoint8":
                        if (CheckPointbool[6] == true)
                        {
                            CheckPointbool[7] = true;
                            CalculateScore(other, "Checkpoint9");
                        }
                        break;
                    case "Checkpoint9":
                        if (CheckPointbool[7] == true)
                        {
                            CheckPointbool[8] = true;
                            CalculateScore(other, "finish");
                        }
                        break;
                    case "finish":
                       //if(CheckPointbool[8] == true)
                       //{
                            CheckPointbool[9] = true;
                            string Place = GameManager.instance.DriverWinner();
                            SetPlace(Place);
                            if (isServer)
                            {
                                RpcSetPlace(Place);
                            }
                            else
                            {
                                CmdSetPlace(Place);
                            }
                            if(Place == "1stplace")
                            {
                                CalculateScore(other," " ,15); // 1ST PLACE
                            }
                            else if (Place == "2ndplace")
                            {
                                CalculateScore(other," ",10); // 2ND PLACE
                            }
                            else if(Place == "3rdplace")
                            {
                                CalculateScore(other," ",5); // 3RD PLACE
                            }
                            GameManager.instance.changeLap();
                            if (levelNumber == 3)
                            {
                                this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                                this.GetComponent<DisableCom>().FreezePlayer();
                                this.GetComponent<PlayerUI>().ScoreBoard.SetActive(true);
                                CmdEndGame();
                            }
                            else
                            {
                                if (GetComponent<StatusManager>().maptoLoad != "")
                                {
                                    return;
                                }
                                LoadNextLevel();
                            }
                       //}
                        break;
                }
            }
        }
    /****************************************/
    [Command]
    void CmdSetPlace(string Place)
    {
        if (isServer)
        {
            SetPlace(Place);
        }
        RpcSetPlace(Place);
    }
    [ClientRpc]
    void RpcSetPlace(string Place)
    {
        if(!isLocalPlayer && !isServer)
        {
            SetPlace(Place);
        }
    }
    void SetPlace(string Place)
    {
        if (Place == "1stplace")
        {
            GameManager.instance.stPlace = true;
        }
        else if (Place == "2ndplace")
        {
            GameManager.instance.ndPlace = true;
        }
        else if (Place == "3rdplace")
        {
            GameManager.instance.rdPlace = true;
        }
    }
    void OnScoreChanged(int score)
    {
        if (isServer)
        {
            RpcSyncScore(score);
        }
        else
        {
            CmdSyncScore(score);
        }
    }
    [Command]
    void CmdSyncScore(int score)
    {
        if (isServer)
        {
            this.GetComponent<CarHealthManager>().Score = score;
        }
        RpcSyncScore(score);
    }
    [ClientRpc]
    void RpcSyncScore(int score)
    {
        if (!isServer && !isLocalPlayer)
        {
            this.GetComponent<CarHealthManager>().Score = score;
        }
    }
    private void CalculateScore(Collider other, string nextCheck, int score = 0)
    {
        print(this.GetComponent<CarHealthManager>().teamMate);
        if (!isLocalPlayer)
            return;
        other.gameObject.SetActive(false);
        CmdPartnerFollow(other.gameObject.tag,this.GetComponent<CarHealthManager>().teamMate,this.name);
        this.GetComponent<CarHealthManager>().Score += 5;
        if (score != 0)
            this.GetComponent<CarHealthManager>().Score += score;
        OnScoreChanged(this.GetComponent<CarHealthManager>().Score);
        if(nextCheck!=" ")
            GameObject.FindGameObjectWithTag(nextCheck).GetComponent<MeshRenderer>().materials[0].color = Color.cyan;

    }
    [Command]
    void CmdPartnerFollow(string CheckpointTag,string teamMate,string myName)
    {
        if (teamMate != "")
        {
            print(teamMate);
            if (GameManager.instance.getStatus(teamMate)!=null)
            {
                GameManager.instance.getStatus(teamMate).RpcteamMatePosition(CheckpointTag,myName);
            }
            
        }
    }
    [Command]
    void CmdEndGame()
    {
        StatusManager[] playersStatus = GameManager.instance.getAll();
        foreach (StatusManager player in playersStatus)
        {
            if (player.transform.name != this.transform.name)
            {
                player.FinishPermission();
            }
        }
    }
    [Command]
    void CmdChangeMap(string mapName,string myName)
    {
        StatusManager[] playersStatus = GameManager.instance.getAll();
        int shootersCounter = 0;
        int carCounter = 1;
        foreach (StatusManager player in playersStatus)
        {
            if (player.transform.name != this.transform.name)
            {
                if (player.transform.name.IndexOf("Driver") == -1)
                {
                    player.ChangeMap(mapName, shootersCounter);
                    shootersCounter++;
                }
                else
                {
                    player.ChangeMap(mapName, carCounter);
                    carCounter++;
                }
            }
        }
    }
   
    void LoadNextLevel()
    {
        int mapNumber = Random.Range(4, 6);
        string[] semesterArray = { "A", "S", "W" };
        int mapSeason = Random.Range(0, 2);

        string[] timeArray = { "D", "N" };
        int mapTime = Random.Range(0, 1);
        
        this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        List<GameObject> spawnPoints = new List<GameObject>(GameObject.FindGameObjectsWithTag("SpawnPoints3"));
        spawnPoints.Sort(CompareListByName);
        this.transform.position = spawnPoints[1].transform.position;
        /*string mapName = "Map" + mapNumber.ToString() +
                semesterArray[mapSeason].ToString() +
                timeArray[mapTime].ToString();*/

        GameObject map = Instantiate(Resources.Load("Map4SD") as GameObject);
        mesh = map.transform.Find("Collision").gameObject.transform;
        /*NetworkServer.Spawn(map);*/
        StartCoroutine(DestroyOldMap());
        StartCoroutine(FreezePlayer());
        CmdChangeMap("Map4SD", this.name);
        this.GetComponent<StatusManager>().PlayerUpgrade();
        StartCoroutine(SpawnPowers());
        levelNumber += 1;

    }

    public Transform getCol()
    {
        return mesh;
    }
    IEnumerator FreezePlayer()
    {
        yield return new WaitForSeconds(.2f);
        this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

    }
    IEnumerator SpawnPowers()
    {
        yield return new WaitForSeconds(5f);
        GetComponent<placePowerUps>().placePowerUp();
        GetComponent<placePowerUps>().placeWeapons();
        GameObject.FindGameObjectWithTag("Checkpoint1").GetComponent<MeshRenderer>().materials[0].color = Color.cyan;


    }
    IEnumerator DestroyOldMap()
    {
        yield return new WaitForEndOfFrame();
        if(GameObject.FindGameObjectWithTag("MAP1"))
            GameObject.FindGameObjectWithTag("MAP1").SetActive(false);
        else if(GameObject.FindGameObjectWithTag("MAP2"))
            GameObject.FindGameObjectWithTag("MAP2").SetActive(false);
        else if (GameObject.FindGameObjectWithTag("Map#"))
        {
            GameObject.FindGameObjectWithTag("Map#").SetActive(false);
        }
    }
    private static int CompareListByName(GameObject i1, GameObject i2)
    {
        return i1.name.CompareTo(i2.name);
    }
}
