using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
// This is a template script for in-game object health manager.
// Any in-game entity that reacts to a shot must have this script with the public function TakeDamage().

public class HealthManager : NetworkBehaviour
{
    [SyncVar]
    private float Healthpoints = 100;
    [SerializeField]
    private float maxHealth=100;
    [HideInInspector]
    public int Kills = 0;
    [SerializeField]
    public RectTransform HealthBar;
    public Text KillScore;
    private NetworkAnimator anim;
    private bool Lost = false;
    private bool Win = false;
    private bool canAccess = false;
    private bool SyncTeams = false;
    public GameObject ragdoll;
    public GameObject Spectate;
    public Camera PlayerCam;
    [SerializeField]
    public int Team ;
    public bool getWin()
    {
        return this.Win;
    }
    public void setWin(bool win)
    {
        this.Win = win;
    }
    public bool getLost()
    {
        return this.Lost;
    }
    public void setLost(bool lose)
    {
        this.Lost = lose;
    }
    private void Start()
    {
        if (!isLocalPlayer)
            return;
        this.Healthpoints = maxHealth;
        anim = GetComponent<NetworkAnimator>();
        StartCoroutine(DelaySeconds());
        GameManager.instance.DisableCarCamera();
        GameManager.instance.startLaps();
    }
    
    [Command]
    private void CmdSyncPlayerTeam(int team)
    {
        if (isServer)
        {
            this.Team = team;
        }
        RpcSyncPlayerTeam(team);
    }
    [ClientRpc]
    private void RpcSyncPlayerTeam(int team)
    {
        if(!isServer && !isLocalPlayer)
        {
            this.Team = team;
        }
    }

    // This is the mandatory function that receives damage from shots.
    // You may remove the 'virtual' keyword before coding the content.
    private void Update()
    {
        if (!isLocalPlayer)
            return;
        if (SyncTeams)
        {
            if (isServer)
            {
                RpcSyncPlayerTeam(this.Team);
            }
            else
            {
                CmdSyncPlayerTeam(this.Team);
            }
            this.SyncTeams = false;
        }
        if (canAccess) // checking match status 
        {
            GameObject[] Shooters = GameObject.FindGameObjectsWithTag("Player");
            GameObject[] Drivers = GameObject.FindGameObjectsWithTag("Driver");
            if (Shooters.Length == 10 && /*check if this driver in the same team */  this.Lost == false)
            {
                GameManager.instance.Win();
            }
        }
        if (GameManager.instance.stPlace && GameManager.instance.ndPlace)
        {
            GameManager.instance.changeLap();
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            anim.SetTrigger("Death");
            this.setLost(true);
            GameManager.instance.Lose();
            this.GetComponent<CapsuleCollider>().direction = 0;
            GameObject Driver = GameObject.FindGameObjectWithTag("DriverPlayer");
            if (Driver != null)
            {
                PlayerCam.GetComponent<ThirdPersonOrbitCam>().player = Driver.transform;
                GameObject obj = Instantiate(Spectate);
            }
            Destroy(this.gameObject, 6);
        }
        if (this.Lost == false)
        {
            SetHealthAmout(this.Healthpoints / maxHealth);
            CheckHealth();
        }
    }

    private void SetHealthAmout(float Amount)
    {
        if (Amount < 0)
            Amount = 0;
        HealthBar.localScale = new Vector3(1f, Amount, 1f);
    }

    private void CheckHealth()
    {
        if(this.Healthpoints <= 0) {
            anim.SetTrigger("Death");
            this.setLost(true);
            GameManager.instance.Lose();
            Die();
            if (isServer)
            {
                RpcDie();
            }
            else
            {
                CmdDie();
            }
        }
    }
    private void OnCollisionEnter(Collision collision) // Die by car run over 
    {
        float minimumCollisionForce = 15f;
        if(this.gameObject.tag=="Player" && collision.gameObject.tag == "DriverPlayer" && collision.relativeVelocity.magnitude > minimumCollisionForce )
        {
            this.gameObject.GetComponent<CapsuleCollider>().isTrigger = true;
            if (!isLocalPlayer)
                return;
            this.Healthpoints = 0;
            this.setLost(true);
            GameManager.instance.Lose();
            Dies();
            if (isServer)
            {
                RpcDies();
            }
            else
            {
                CmdDies();
            }

        }
    }
    [Command]
    void CmdDies()
    {
        if (isServer)
        {
            Dies();
        }
        RpcDies();
    }
    [ClientRpc]
    void RpcDies()
    {
        if (!isLocalPlayer && !isServer)
        {
            Dies();
        }
    }
    void Dies()
    {
        GameObject Ragdoll = Instantiate(ragdoll, this.transform.position, this.transform.rotation);
        Ragdoll.SetActive(true);
        Destroy(this.gameObject,3);
    }
    public void TakeDamage(/*Vector3 location,Vector3 direction,*/float damage,string Killer)
    {
        if (this.Healthpoints <= 0)
        {
            return;
        }
        this.Healthpoints -= damage;
        print(this.transform.name+ " " + Healthpoints);
        if (Healthpoints < 1)
        {
            RpcEditScore(Killer);
        }
    }
    [ClientRpc]
    void RpcEditScore(string Killer)
    {
        HealthManager player = GameManager.instance.getPlayer(Killer);
        player.Kills++;
        player.KillScore.text = player.Kills.ToString();
    }
    /******************************************************/
    [Command]
    void CmdDie()
    {
        if (isServer)
        {
            Die();
        }
        RpcDie();
    }
    [ClientRpc]
    void RpcDie()
    {
        if (!isLocalPlayer && !isServer)
        {
            Die();
        }
    }
    void Die()
    {
        this.GetComponent<CapsuleCollider>().direction = 0;
        Destroy(this.gameObject, 6);
    }
   
    IEnumerator DelaySeconds()
    {
        yield return new WaitForSeconds(10);
        this.canAccess = true;
        this.SyncTeams = true;
    }

}
