using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class CarHealthManager : NetworkBehaviour
{
    public GameObject SmokeEffect;
    public GameObject FireEffect;
    public GameObject Explosion;
    public GameObject Front;
    public AudioClip[] ExplosionSound;
    public string teamMate;
    [SyncVar]
    private float Healthpoints = 200;
    [SerializeField]
    private float maxHealth = 200;
    [SerializeField]
    public RectTransform HealthBar;
    [SyncVar]
    private float Defendpoints = 100;
    [SerializeField]
    private float maxDefence = 100;
    [SerializeField]
    public RectTransform DefenceBar;

    public int Score = 0;
    private bool HasSmoke;
    private bool HasFire;
    [SerializeField]
    public bool Exploded;
    public GameObject canvas;
    public GameObject BurntCar;
    [SyncVar]
    public int Team;
    [SyncVar]
    public String nickName;
    [SyncVar]
    public Color Color;
    [SyncVar]
    public int Numofplayers;
    private bool disabledCanvas;
    private void Start()
    {
        if (!isLocalPlayer)
            return;
        this.disabledCanvas = false;
        canvas.SetActive(false);
        this.Healthpoints = maxHealth;
        this.Defendpoints = maxDefence;
        SetHealthAmout(this.Healthpoints / maxHealth);
        SetDefendAmout(this.Defendpoints / maxDefence);
        this.HasSmoke = false;
        this.HasFire = false;
        this.Exploded = false;
    }
    
    private void Update()
    {
        if (!isLocalPlayer)
            return;
        if (!Exploded)
        {
            SetHealthAmout(this.Healthpoints / maxHealth);
            SetDefendAmout(this.Defendpoints / maxDefence);
            CheckHealth();
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            this.Defendpoints -= 10;
            this.Healthpoints -= 90;
        }
        if (!this.disabledCanvas)
        {
            CarHealthManager[] drivers = GameManager.instance.getallDrivers();
            HealthManager[] shooters = GameManager.instance.getallPlayers();
            if (drivers.Length + shooters.Length == Numofplayers)
            {
                DisableOtherCanvas(drivers, shooters);
                this.disabledCanvas = true;
            }
        }
    }
    private void DisableOtherCanvas(CarHealthManager[] drivers, HealthManager[] shooters)
    {
        if (drivers != null)
        {
            foreach (CarHealthManager driver in drivers)
            {
                driver.canvas.SetActive(false);
            }
        }
        if (shooters != null)
        {
            foreach (HealthManager shooter in shooters)
            {
                if (shooter.Team != this.Team)
                {
                    shooter.canvas.SetActive(false);
                }
                else
                {
                    this.teamMate = shooter.name;
                    
                }
            }
        }
    }
    public void TakeDamage(float damage, string Killer,Vector3 direction)
    {
        if(this.Healthpoints <= 0 || GameManager.instance.getPlayer(Killer).Team == this.Team)
        {
            return;
        }
        if (Defendpoints > 0)
        {
            this.Healthpoints -= damage / 3;
            this.Defendpoints -= damage;
        }
        else
        {
            this.Healthpoints -= damage / 2;
        }
        RpcAddForce(direction);
        print(this.transform.name + " " + Healthpoints);
        if (Healthpoints <= 0)
        {
            RpcEditScore(Killer);
        }
    }
    [ClientRpc]
    void RpcAddForce(Vector3 direction)
    {
        this.GetComponent<Rigidbody>().AddForce(direction * 3f, ForceMode.Impulse);
    }
    [ClientRpc]
    void RpcEditScore(string Killer)
    {
        HealthManager player= GameManager.instance.getPlayer(Killer);
        player.Kills+=3;
    }
    /******************/
    private void SetHealthAmout(float Amount)
    {
        if (Amount < 0)
            Amount = 0;
        HealthBar.localScale = new Vector3(1f, Amount, 1f);
    }
    private void SetDefendAmout(float Amount)
    {
        if (Amount < 0)
            Amount = 0;
        DefenceBar.localScale = new Vector3(1f, Amount, 1f);
    }
    /****************************************/
    private void CheckHealth()
     {
        if (Healthpoints <= 60 && !HasSmoke)
        {
            this.HasSmoke = true;
            MakeEffects();
            if (isServer)
            {
                RpcMakeEffects();
            }
            else
            {
                CmdMakeEffects();
            }
        }
        if (Healthpoints <= 25 && !HasFire) 
        {
            this.HasFire = true;
            MakeSmallExplosion();
            if (isServer)
            {
            RpcMakeSmallExplosion();
            }
            else
            {
                CmdMakeSmallExplosion();
            }
        }
        if(Healthpoints <= 0 && !Exploded)
        {
            GameManager.instance.Lose();
            MakeBigExplosion();
            if (isServer)
            {
                RpcMakeBigExplosion();
            }
            else
            {
                CmdMakeBigExplosion();
            }
            this.GetComponent<DisableCom>().FreezePlayer();
            this.GetComponent<PlayerUI>().ScoreBoard.SetActive(true);
        }
     }
    /*********************************************************/
    [Command]
    void CmdMakeEffects()
    {
        if (isServer)
        {
            MakeEffects();
        }
        RpcMakeEffects();
    }
    [ClientRpc]
    void RpcMakeEffects()
    {
        if (!isLocalPlayer && !isServer)
        {
            MakeEffects();
        }
    }
    void MakeEffects()
    {
        GameObject Effect = Instantiate(SmokeEffect, Front.transform.position, Front.transform.rotation);
        Effect.SetActive(true);
        Effect.transform.SetParent(this.transform);
    }
    /***************************************/
    [Command]
    void CmdMakeSmallExplosion()
    {
        if (isServer)
        {
            MakeSmallExplosion();
        }
        RpcMakeSmallExplosion();
    }
    [ClientRpc]
    void RpcMakeSmallExplosion()
    {
        if (!isLocalPlayer && !isServer)
        {
            MakeSmallExplosion();
        }
    }
    void MakeSmallExplosion()
    {
        AudioSource.PlayClipAtPoint(ExplosionSound[0], transform.position, 50f);
        GameObject Effect = Instantiate(FireEffect, Front.transform.position, Front.transform.rotation);
        Effect.SetActive(true);
        Effect.transform.SetParent(this.transform);
        Destroy(Effect, 1.5f);
    }
    /*****************************/
    [Command]
    void CmdMakeBigExplosion()
    {
        if (isServer)
        {
            MakeBigExplosion();
        }
        RpcMakeBigExplosion();
    }
    [ClientRpc]
    void RpcMakeBigExplosion()
    {
        if (!isLocalPlayer && !isServer)
        {
            MakeBigExplosion();
        }
    }
    void MakeBigExplosion()
    {
        this.Exploded = true;
        AudioSource.PlayClipAtPoint(ExplosionSound[1], transform.position, 100f);
        GameObject Effect = Instantiate(Explosion, Front.transform.position, Front.transform.rotation);
        Effect.SetActive(true);
        Effect.transform.SetParent(this.transform);
        Destroy(Effect, 3);
        Destroy(this.gameObject, 3);
    }
    /*************************************/
    /*IEnumerator DelaySeconds()
    {
        yield return new WaitForSeconds(4);
        
    }*/
    public void SetHealthPoints()
    {
        Healthpoints = maxHealth;
    }
    public void SetDefendPoints()
    {
        Defendpoints = maxDefence;
    }
}

