using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
// This is a template script for in-game object health manager.
// Any in-game entity that reacts to a shot must have this script with the public function TakeDamage().

public class HealthManager : NetworkBehaviour
{
    private CarHealthManager teamMate;
    [SyncVar]
    private float Healthpoints = 100;
    [SerializeField]
    private float maxHealth=100;
    [SyncVar(hook = "OnKillsChanged")]
    public int Kills = 0;
    [SerializeField]
    public RectTransform HealthBar;
    public GameObject canvas; // player nameplate
    private NetworkAnimator anim;
    private bool Lost = false;
    private bool Win = false;
    public GameObject ragdoll;
    public GameObject Spectate;
    public Camera PlayerCam;
    private bool lapChanged;
    [SyncVar]
    public int Team;
    [SyncVar]
    public String nickName;
    [SyncVar]
    public Color Color;
    [SyncVar]
    public int Numofplayers;
    private bool disabledCanvas;
    
  
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
        this.lapChanged = false;
        this.disabledCanvas = false;
        canvas.SetActive(false);
        this.Healthpoints = maxHealth;
        anim = GetComponent<NetworkAnimator>();
        DisableCarCamera.instance.DisableCamera();
        if (GameObject.FindGameObjectWithTag("MAP1"))
        {
            GameManager.instance.Laps[0].SetActive(true);
            GameManager.instance.Laps[1].SetActive(false);
        } else if (GameObject.FindGameObjectWithTag("MAP2"))
        {
            GameManager.instance.Laps[2].SetActive(true);
            GameManager.instance.Laps[3].SetActive(false);
        }
        
    }
    // This is the mandatory function that receives damage from shots.
    // You may remove the 'virtual' keyword before coding the content.
    private void Update()
    {
        if (!isLocalPlayer)
            return;
        if (Input.GetKeyDown(KeyCode.J))
        {
            this.Healthpoints -= 10;
        }
        if (this.Lost == false) // check match status according to his team-mate
        {
            SetHealthAmout(this.Healthpoints / maxHealth);
            CheckHealth();
           /* if (teamMate && teamMate.Team == this.Team)
            {
                if (teamMate.Exploded) // if team mate lost as the shooter lose
                {
                    GameManager.instance.Lose();
                    this.Healthpoints = 0f;
                }
                if(teamMate.Score >= 50 ) // if my team mate has finished first lap 
                {
                    // changing the lap according to his team mate so he can protect him
                    bool FINISHED = GameManager.instance.changeLap();
                    if (FINISHED)
                    {
                        this.GetComponent<NetwrokBehaviour>().FreezePlayer();
                        this.GetComponent<PlayerUI>().ScoreBoard.SetActive(true);
                    }
                }
            }*/
        }
        if (!this.disabledCanvas) // keep checking untill all players are ready in order to disable Nameplates for non team member.
        {
            CarHealthManager[] drivers = GameManager.instance.getallDrivers();
            HealthManager[] shooters = GameManager.instance.getallPlayers();
            if(drivers.Length+shooters.Length == Numofplayers)
            {
                DisableOtherCanvas(drivers,shooters);
                this.disabledCanvas = true;
            }
        }
    }
   private void DisableOtherCanvas(CarHealthManager[] drivers,HealthManager[] shooters)
    {
        if (drivers != null)
        {
            foreach (CarHealthManager driver in drivers)
            {
                if (driver.Team != this.Team)
                {
                    driver.canvas.SetActive(false);
                }
                else
                {
                    this.teamMate = GameManager.instance.getDriver(driver.name);
                }
            }
        }
        if (shooters != null) 
        {
            foreach (HealthManager shooter in shooters)
            {
                shooter.canvas.SetActive(false);
            }
        }
    }
    void OnKillsChanged(int kills)
    {
        this.Kills = kills;
        if (isServer)
        {
            RpcSyncScore(kills);
        }
        else
        {
            CmdSyncScore(kills);
        }
    }
    [Command]
    void CmdSyncScore(int kills)
    {
        if (isServer)
        {
            this.Kills = kills;
        }
        RpcSyncScore(kills);
    }
    [ClientRpc]
    void RpcSyncScore(int kills)
    {
        if (!isServer && !isLocalPlayer)
        {
            this.Kills = kills;
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
            if (this.GetComponent<FlyBehaviour>().fly == true)
            {
                this.GetComponent<NetwrokBehaviour>().FreezePlayer();
                this.GetComponent<PlayerUI>().ScoreBoard.SetActive(true);
                GameManager.instance.Lose();
                WeaponHandling player = this.GetComponent<WeaponHandling>();
                player.dropWeapon();
                this.setLost(true);
                GameObject Ragdoll = Instantiate(ragdoll, this.transform.position, this.transform.rotation);
                Ragdoll.SetActive(true);
                SpectateTeamMate();
                Destroy(this.gameObject,1);
                if (isServer)
                {
                    RpcDies();
                }
                else
                {
                    CmdDies();
                }
            }
            else
            {
                anim.SetTrigger("Death");
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
                this.GetComponent<NetwrokBehaviour>().FreezePlayer();
                this.GetComponent<PlayerUI>().ScoreBoard.SetActive(true);
                StartCoroutine(DelaySeconds());
            }
            
        }
    }
    
    private void OnCollisionStay(Collision collision) // Die by car run over 
    {
        float minimumCollisionForce = 10f;
        if (this.gameObject.tag=="Player" && collision.gameObject.tag == "DriverPlayer" && collision.relativeVelocity.magnitude > minimumCollisionForce)
        {
            this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
            this.gameObject.GetComponent<CapsuleCollider>().isTrigger = true;
            if (!isLocalPlayer)
                return;
            this.GetComponent<NetwrokBehaviour>().FreezePlayer();
            this.GetComponent<PlayerUI>().ScoreBoard.SetActive(true);
            GameManager.instance.Lose();
            WeaponHandling player = this.GetComponent<WeaponHandling>();
            player.dropWeapon();
            this.setLost(true);
            GameObject Ragdoll = Instantiate(ragdoll, this.transform.position, this.transform.rotation);
            Ragdoll.SetActive(true);
            SpectateTeamMate();
            Destroy(this.gameObject, 1);
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
        WeaponHandling player = this.GetComponent<WeaponHandling>();
        player.dropWeapon();
        this.setLost(true);
        GameObject Ragdoll = Instantiate(ragdoll, this.transform.position, this.transform.rotation);
        Ragdoll.SetActive(true);
        this.gameObject.SetActive(false);
        Destroy(this.gameObject,6);
    }
    void SpectateTeamMate()
    {
        if (teamMate && teamMate.Exploded==false)
        {
            PlayerCam.GetComponent<ThirdPersonOrbitCam>().player = teamMate.transform;
            GameObject obj = Instantiate(Spectate);
            teamMate.canvas.SetActive(false);
            return;
        }
    }
    public void TakeDamage(/*Vector3 location,Vector3 direction,*/float damage,string Killer)
    {
        if (this.Healthpoints <= 0 || GameManager.instance.getPlayer(Killer).Team == this.Team)
        {
            return;
        }
        this.Healthpoints -= damage/2;
        if (Healthpoints <= 0)
        {
            RpcEditScore(Killer);
        }
    }
    [ClientRpc]
    void RpcEditScore(string Killer)
    {
        HealthManager player = GameManager.instance.getPlayer(Killer);
        player.Kills++;
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
        WeaponHandling player = this.GetComponent<WeaponHandling>();
        player.dropWeapon();
        this.setLost(true);
        this.GetComponent<CapsuleCollider>().direction = 0;
        Destroy(this.gameObject, 6);
    }

    IEnumerator DelaySeconds()
    {
        yield return new WaitForSeconds(3);
        SpectateTeamMate();

    }
    public void SetHealthPoints()
    {
        Healthpoints = maxHealth;
    }
}
