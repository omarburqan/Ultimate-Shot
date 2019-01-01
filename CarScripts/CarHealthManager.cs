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

    private float Healthpoints;
    private bool HasSmoke;
    private bool HasExtraSmoke;
    private bool HasFire;
    private bool Exploded;
    private void Start()
    {
        if (!isLocalPlayer)
            return;
        this.Healthpoints = 100;
        this.HasSmoke = false;
        this.HasExtraSmoke = false;
        this.HasFire = false;
        this.Exploded = false;
        
    }
    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return;
        if (Input.GetKeyDown(KeyCode.J))
        {
            MakeBigExplosion();
        }
    }
   
    private void OnParticleCollision(GameObject other)
    {
        if (!isLocalPlayer)
            return;
        print("hit");
        if (other.gameObject.tag == "M4A1")
        {
            print("hitss");
            this.Healthpoints -= 15;
            if (Healthpoints <= 75 && !HasSmoke)
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
            if (Healthpoints <= 50 && !HasExtraSmoke)
            {
                this.HasExtraSmoke = true;
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
            /*************************************/
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
            /***********************************/
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

        GameObject instantShot = Instantiate(FireEffect, Front.transform.position, Front.transform.rotation);
        instantShot.SetActive(true);
        instantShot.transform.SetParent(this.transform);
        StartCoroutine(Example(instantShot));
        Destroy(instantShot);
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
        GameObject instantShot = Instantiate(Explosion, Front.transform.position, Front.transform.rotation);
        instantShot.SetActive(true);
        instantShot.transform.SetParent(this.transform);
        StartCoroutine(Example(instantShot));
    }
    /*************************************/
    IEnumerator Example(GameObject obj)
    {
        yield return new WaitForSeconds(2.5f);
        Destroy(obj);
       
    }
}

