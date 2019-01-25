using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class CarHealthManager : NetworkBehaviour
{
    public GameObject SmokeEffect;
    public GameObject FireEffect;
    public GameObject Explosion;
    public GameObject Front;
    public AudioClip[] ExplosionSound;
    [SyncVar]
    private float Healthpoints = 100;
    [SerializeField]
    private float maxHealth = 100;
    [SerializeField]
    public RectTransform HealthBar;
    private bool SyncTeams = false;
    private bool HasSmoke;
    private bool HasFire;
    private bool Exploded;
    public GameObject BurntCar;
    [SerializeField]
    public int Team;
    private void Start()
    {
        if (!isLocalPlayer)
            return;
        this.Healthpoints = maxHealth;
        this.HasSmoke = false;
        this.HasFire = false;
        this.Exploded = false;
        StartCoroutine(DelaySeconds());
        GameManager.instance.startLaps();
    }
    [ClientRpc]
    private void RpcSyncPlayerTeam(int team)
    {
        if (!isServer && !isLocalPlayer)
        {
            this.Team = team;
        }
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
    private void Update()
    {
        if (!isLocalPlayer)
            return;
        if (!Exploded)
        {
            SetHealthAmout(this.Healthpoints / maxHealth);
            CheckHealth();
        }
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
    }

    private void SetHealthAmout(float Amount)
    {
        if (Amount < 0)
            Amount = 0;
        HealthBar.localScale = new Vector3(1f, Amount, 1f);
    }
    public void TakeDamage(float damage, string Killer,Vector3 direction)
    {
        if(this.Healthpoints <= 0)
        {
            return;
        }
        this.Healthpoints -= damage/2;
        RpcAddForce(direction);
        print(this.transform.name + " " + Healthpoints);
        if (Healthpoints < 1)
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
        player.KillScore.text = player.Kills.ToString();
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
            this.Exploded = true;
            MakeBigExplosion();
            if (isServer)
            {
                RpcMakeBigExplosion();
            }
            else
            {
                CmdMakeBigExplosion();
            }
        }
     }
    /******************************************************/
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
        GameObject instantShot = Instantiate(SmokeEffect, Front.transform.position, Front.transform.rotation);
        instantShot.SetActive(true);
        instantShot.transform.SetParent(this.transform);
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
        GameObject instantShot = Instantiate(FireEffect, Front.transform.position, Front.transform.rotation);
        instantShot.SetActive(true);
        instantShot.transform.SetParent(this.transform);
        Destroy(instantShot, 1.5f);
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
        AudioSource.PlayClipAtPoint(ExplosionSound[1], transform.position, 50f);
        GameObject instantShot = Instantiate(Explosion, Front.transform.position, Front.transform.rotation);
        instantShot.SetActive(true);
        instantShot.transform.SetParent(this.transform);
        Destroy(instantShot, 3);
    }
    /*************************************/
    IEnumerator DelaySeconds()
    {
        yield return new WaitForSeconds(10);
        this.SyncTeams = true;
    }
}

