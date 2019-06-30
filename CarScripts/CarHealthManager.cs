using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// this class  will control or manage the player health and defense when being shot and spawn effects(explosion,smoke) when needed according to player health 
/// </summary>

public class CarHealthManager : NetworkBehaviour
{
    // efects for car to spawn when needed
    public GameObject SmokeEffect;
    public GameObject FireEffect;
    public GameObject Explosion;
    public GameObject Front;
    // sound for car effects 
    public AudioClip[] ExplosionSound;
    public string teamMate;
    [SyncVar]
    private float Healthpoints = 200; // player health
    [SerializeField]
    private float maxHealth = 200;
    [SerializeField]
    // health bar 
    public RectTransform HealthBar;
    [SyncVar]
    private float Defendpoints = 100; // // player defense
    [SerializeField]
    private float maxDefence = 100;
    [SerializeField]
    // defence bar 
    public RectTransform DefenceBar;

    public int Score = 0;
    // boolean to not spawn effect more than 1 time
    private bool HasSmoke; 
    private bool HasFire;
    [SerializeField]
    public bool Exploded;
    public GameObject canvas;
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
        // disabling other player canvas (name-plates)
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
    // handling player health when being hit
    public void TakeDamage(float damage, string Killer,Vector3 direction)
    {
        float forcePower = 0f;
        if (Killer.IndexOf("Driver") == -1)
        { 
            if(GameManager.instance.getPlayer(Killer).Team == this.Team)
            {
                return;
            }
            forcePower = 3f;
        }
        else if (Killer.IndexOf("Driver") != -1)
        {
            if (GameManager.instance.getDriver(Killer).Team == this.Team)
            {
                return;
            }
            forcePower = 10f;
        }
        if (this.Healthpoints <= 0)
        {
            return;
        }
        if (Defendpoints > 0)
        {
            this.Healthpoints -= (damage / 3);
            this.Defendpoints -= damage;
        }
        else
        {
            this.Healthpoints -= (damage / 2);
        }
        RpcAddForce(direction, forcePower);
        if (Healthpoints <= 0)
        {
            RpcEditScore(Killer);
        }
    }
    // adding force for car when being shot by a bullet
    [ClientRpc]
    void RpcAddForce(Vector3 direction,float forcePower)
    {
        this.GetComponent<Rigidbody>().AddForce(direction * forcePower, ForceMode.Impulse);
    }
    [ClientRpc]
    void RpcEditScore(string Killer)
    {
        try
        {
            HealthManager player = GameManager.instance.getPlayer(Killer);
            player.Kills += 3;
        }catch
        {
            CarHealthManager player = GameManager.instance.getDriver(Killer);
            player.Score += 3;
        }
    }
    /******** Handling player bars (health,defense) **********/
    
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
    
    public void SetHealthPoints()
    {
        Healthpoints = maxHealth;
    }
    public void SetDefendPoints()
    {
        Defendpoints = maxDefence;
    }
    private void OnParticleCollision(GameObject other)
    {
        if (!isLocalPlayer)
            return;
        if (other.gameObject.tag == "PlasmaShock")
        {
            int random = Random.Range(0,2);
            print(random);
            StartCoroutine(DelaySeconds(random));
        }
    }
    IEnumerator DelaySeconds(int random)
    {
        if (random == 0)
        {
            this.GetComponent<DisableCom>().FreezePlayer();
        }
        else if (random == 1)
        {
            this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
        yield return new WaitForSeconds(2);
        if (random == 0)
        {
            this.GetComponent<DisableCom>().unFreezePlayer();
        }
        else if (random == 1)
        {
            this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        }

    }
}

