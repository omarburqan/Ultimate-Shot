using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WeaponHandling : NetworkBehaviour
{
    public InteractiveWeapon Weapon;
    public ShootBehaviour Player;
    public Camera playerCamera;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer)
        {
            if (Player && Input.GetButtonDown(Player.pickButton) && Weapon && Weapon.pickable == true)//pick up weapon
            {
                pickWeapon();
                if (isServer)
                {
                    RpcpickWeapon();
                }
                else
                {
                    CmdpickWeapon();
                }
            }
            if (Input.GetKey(KeyCode.Q) && Player && Player.getActiveWeapon() > 0) // drop down weapon
            {
                dropWeapon();
                if (isServer)
                {
                    RpcdropWeapon();
                }
                else
                {
                    CmddropWeapon();
                }
            }
            /// shooting from weapon 
            if (Player && Input.GetKey(KeyCode.Mouse0) && !Player.getIsShooting() && Player.getActiveWeapon() > 0 && Player.getBurst() == 0)
            {
                ShotWeapon();
                if (isServer)
                {
                    RpcShotWeapon();
                }
                else
                {
                    CmdShotWeapon();
                }
            }// stopped shooting
            if (Player && Player.getIsShooting() && !Input.GetKey(KeyCode.Mouse0))
            {
                StopWeapon();
                if (isServer)
                {
                    RpcStopWeapon();
                }
                else
                {
                    CmdStopWeapon();
                }
            }
            if (Player && Input.GetKey(KeyCode.Tab) && !Player.getIsChanging())
            {
                Player.setIsChanging(true);
                CHideWeapon();
                if (isServer)
                {
                    RpcCHideWeapon();
                }
                else
                {
                    CmdCHideWeapon();
                }
            }
            if (Player && Input.GetKey(KeyCode.Tab) == false)
            {
                Player.setIsChanging(false);
            }
            if (Player && Player.getShotalive())
            {
                ShotDecai();
                if (isServer)
                {
                    RpcShotDecai();
                }
                else
                {
                    CmdShotDecai();
                }
            }

        }
    }
    void OnTriggerStay(Collider other)
    {
        Player = this.GetComponent<ShootBehaviour>();
        Weapon = other.GetComponent<InteractiveWeapon>();
        if (other.tag == "Gun")
        {
            //Weapon = other.GetComponent<InteractiveWeapon>();
            Weapon.pickable = true;
            //Weapon.TooglePickupHUD(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (isLocalPlayer)
        {
            if (other.tag == "Gun" && Weapon)
            {
                Weapon.pickable = false;
                //Weapon.TooglePickupHUD(false);
            }
        }
    }
    //************************//
    [Command]
    void CmdpickWeapon()
    {
        if (isServer)
        {
            pickWeapon();
        }
        RpcpickWeapon();
    }
    [ClientRpc]
    void RpcpickWeapon()
    {
        if (!isLocalPlayer && !isServer)
        {
            pickWeapon();
        }
    }
    void pickWeapon()
    {
        Player.AddWeapon(Weapon);
        if (!isLocalPlayer)
            return;
        //Weapon.TooglePickupHUD(false);
        //Weapon.Toggle(true);

    }
    /*****************************************/
    [Command]
    void CmddropWeapon()
    {
        if (isServer)
        {
            dropWeapon();
        }
        RpcdropWeapon();
    }
    [ClientRpc]
    void RpcdropWeapon()
    {
        if (!isLocalPlayer && !isServer)
        {
            dropWeapon();
        }
    }
    void dropWeapon()
    {
        Player.EndReloadWeapon();
        int weaponToDrop = Player.getActiveWeapon();
        Player.ChangeWeapon(Player.getActiveWeapon(), 0);
        Player.getWeapons()[weaponToDrop].Drop();
        Player.getWeapons()[weaponToDrop] = null;
    }
    /********************************************/
    [Command]
    void CmdShotWeapon()
    {
        if (isServer)
        {
            ShotWeapon();
        }
        RpcShotWeapon();
    }
    [ClientRpc]
    void RpcShotWeapon()
    {
        if (!isLocalPlayer && !isServer)
        {
            ShotWeapon();
        }
    }
    void ShotWeapon()
    {
        Player.setIsShooting(true);
        Player.ShootWeapon(Player.getActiveWeapon());
        Player.setShotalive(true);
    }
    [Command]
    void Cmdplaysound()
    {
        if (isServer)
        {
            playsound1();
        }
        Rpcplaysound();
    }
    [ClientRpc]
    void Rpcplaysound()
    {
        if (!isLocalPlayer && !isServer)
        {
            playsound1();
        }
    }
    [Client]
    public void playsound1()
    {
        float shotErrorRate = 0.02f;
        int shotMask = shotMask = ~((1 << LayerMask.NameToLayer("Ignore Shot")) | 1 << LayerMask.NameToLayer("Ignore Raycast"));
        // Cast the shot to find a target.
        Vector3 imprecision = Random.Range(-shotErrorRate, shotErrorRate) * playerCamera.transform.right;
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward + imprecision);
        RaycastHit hit = default(RaycastHit);
        // Target was hit.
        if (Physics.Raycast(ray, out hit, 1000f, shotMask))
        {

            if (hit.collider.transform != this.transform)
            {
                // Handle shot effects on target.
                //Player.DrawShoot(Player.getWeapons()[Player.getActiveWeapon()].gameObject, hit.point, hit.normal, hit.collider.transform);
                this.setFlash(Player.getMuzzle(),hit.point);
                // Call the damage behaviour of target if exists.
                if (hit.collider.gameObject.GetComponent<HealthManager>() ||
                    hit.collider.gameObject.GetComponent<CarHealthManager>()
                    )
                {
                    //hit.collider.gameObject.GetComponent<HealthManager>().TakeDamage(hit.point, ray.direction, Player.getWeapons()[Player.getActiveWeapon()].bulletDamage);
                    CmdTelltheServer(hit.collider.name, this.Player.getWeapons()[Player.getActiveWeapon()].bulletDamage,this.name, hit.point);
                }
            }
        }
        // No target was hit.
        else
        {
            Vector3 destination = (ray.direction * 500f) - ray.origin;
            // Handle shot effects without a specific target.
            //Player.DrawShoot(Player.getWeapons()[Player.getActiveWeapon()].gameObject, destination, Vector3.up, null, false, false);
            this.setFlash(playerCamera.transform, hit.point);
        }
        Player.getWeapons()[Player.getActiveWeapon()].OnShooting();

    }
    [Command]
    void CmdTelltheServer(string playerID,float damage,string Name,Vector3 direction)
    {
        if (playerID.IndexOf("Driver") == -1) { // in case of shooting player(Shooter)
            HealthManager _player =GameManager.instance.getPlayer(playerID);
            _player.TakeDamage(damage,Name);
        }
        else if (playerID.IndexOf("Driver") != -1)
        {
            CarHealthManager _driver = GameManager.instance.getDriver(playerID);
            _driver.TakeDamage(damage, Name,direction);
        }
    }

    public void playsound()
    {
        playsound1();
        if (isServer)
        {
            Rpcplaysound();
        }
        else
        {
            Cmdplaysound();
        }
    }
    /***************************/
    [Command]
    void CmdStopWeapon()
    {
        if (isServer)
        {
            StopWeapon();
        }
        RpcStopWeapon();
    }
    [ClientRpc]
    void RpcStopWeapon()
    {
        if (!isLocalPlayer && !isServer)
        {
            StopWeapon();
        }
    }
    void StopWeapon()
    {
        Player.setIsShooting(false);
    }
    /*********************************************/
    [Command]
    void CmdCHideWeapon()
    {
        if (isServer)
        {
            CHideWeapon();
        }
        RpcCHideWeapon();
    }
    [ClientRpc]
    void RpcCHideWeapon()
    {
        if (!isLocalPlayer && !isServer)
        {
            CHideWeapon();
        }
    }
    void CHideWeapon()
    {
        int nextWeapon = Player.getActiveWeapon() + 1;
        Player.ChangeWeapon(Player.getActiveWeapon(), (nextWeapon) % Player.getWeapons().Count);
    }
    /**********************************/
    [Command]
    void CmdShotDecai()
    {
        if (isServer)
        {
            ShotDecai();
        }
        RpcShotDecai();
    }
    [ClientRpc]
    void RpcShotDecai()
    {
        if (!isLocalPlayer && !isServer)
        {
            ShotDecai();
        }
    }
    void ShotDecai()
    {
        Player.ShotDecay();
    }
    /*************/
    public void setFlash(Transform gunMuzzle2,Vector3 destination)
    {
        GameObject instantShot = Instantiate(Player.shot, gunMuzzle2.position, Quaternion.LookRotation(destination - gunMuzzle2.position));
        instantShot.SetActive(true);
    }
    

}