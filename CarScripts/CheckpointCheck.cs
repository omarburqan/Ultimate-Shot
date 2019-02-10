    using UnityEngine;
    using UnityEngine.Networking;
/// <summary>
/// a class to control or manage the car when entering a checkpoint , which will increase the score and disappear the checkpoint 
/// syncing the driver score to other players in order to get a correct scoreboard when ever enabled.
/// start the  laps for each car.
/// </summary>
    public class CheckpointCheck : NetworkBehaviour {

        private bool[] CheckPointbool=new bool[10];
        private int LapNumber;
        private void Start()
        {
            if (!isLocalPlayer)
                return;
            for (int i = 0; i < CheckPointbool.Length; i++)
            {
                CheckPointbool[i] = false;
            }
            if (GameObject.FindGameObjectWithTag("MAP1"))
            {
                GameManager.instance.Laps[0].SetActive(true);
                GameManager.instance.Laps[1].SetActive(false);
                LapNumber = 0;
                GameManager.instance.Laps[LapNumber].transform.Find("Checkpoint1").
                    GetComponent<MeshRenderer>().materials[0].color = Color.cyan;
            }
            else if (GameObject.FindGameObjectWithTag("MAP2"))
            {
                GameManager.instance.Laps[2].SetActive(true);
                GameManager.instance.Laps[3].SetActive(false);
                LapNumber = 2;
                GameManager.instance.Laps[LapNumber].transform.Find("Checkpoint1").
                    GetComponent<MeshRenderer>().materials[0].color = Color.cyan;
            }
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
                            CalculateScore(other, "Finish");
                        }
                        break;
                    case "finish":
                       if(CheckPointbool[8] == true)
                       {
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
                            bool FINISHED = GameManager.instance.changeLap();
                            if (FINISHED)
                            {
                                this.GetComponent<DisableCom>().FreezePlayer();
                                this.GetComponent<PlayerUI>().ScoreBoard.SetActive(true);
                            }
                            else
                            {
                                LapNumber++;
                                GameManager.instance.Laps[LapNumber].transform.Find("Checkpoint1").
                                    GetComponent<MeshRenderer>().materials[0].color = Color.cyan;
                            }
                       }
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
        if (!isLocalPlayer)
            return;
        other.gameObject.SetActive(false);
        this.GetComponent<CarHealthManager>().Score += 5;
        if (score != 0)
            this.GetComponent<CarHealthManager>().Score += score;
        OnScoreChanged(this.GetComponent<CarHealthManager>().Score);
        if(nextCheck!=" ")
            GameManager.instance.Laps[LapNumber].transform.Find(nextCheck).GetComponent<MeshRenderer>().materials[0].color = Color.cyan;

    }
}
