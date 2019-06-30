using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// class which handle player input and weapon interactiong (for example when player hits a weapon in ground if he choose to 
//pickup he will pickup this weapon and sync to other players
// also these scripts handle player shooting from weapon by using shootBehaviour script
public class WeaponHandling : NetworkBehaviour
{
    public InteractiveWeapon Weapon;

    [SerializeField]
    ShootBehaviour Player;
    
    public Camera playerCamera;

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer)
        {
            // drop down weapon
            if (Input.GetKey(KeyCode.Q) && Player && Player.getActiveWeapon() > 0) 
            {
                Player.getWeapons()[Player.getActiveWeapon()].Toggle(false);
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
            // hide weapon 
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
            // a special time between each time (shooting Rate)
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
    //player interaction with weapons
    void OnTriggerStay(Collider other)
    {
        if (!isLocalPlayer)
            return;
        if (other.tag == "Gun")
        {
            InteractiveWeapon tempWeapon = other.GetComponent<InteractiveWeapon>();
            tempWeapon.pickable = true;
            tempWeapon.TooglePickupHUD(true, this.playerCamera);
            if (Input.GetButtonDown(Player.pickButton) && tempWeapon && tempWeapon.pickable == true)//pick up weapon
            {
                this.Weapon = tempWeapon;
                if (Weapon.label == "AWM")
                {
                    Player.shotRateFactor = 0.1f;
                }
                else
                {
                    Player.shotRateFactor = 1f;
                }
                Player.AddWeapon(Weapon);
                tempWeapon.TooglePickupHUD(false, this.playerCamera);
                Weapon.Toggle(true);
                if (isServer)
                {
                    RpcpickWeapon(this.Weapon.id);
                }
                else
                {
                    CmdpickWeapon(this.Weapon.id);
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!isLocalPlayer)
            return;
        if (other.tag == "Gun" )
        {
            InteractiveWeapon tempWeapon = other.GetComponent<InteractiveWeapon>();
            tempWeapon.pickable = false;
            tempWeapon.TooglePickupHUD(false, this.playerCamera);
        }
    }
    //************************//
    [Command]
    void CmdpickWeapon(string weaponId)
    {
        if (isServer)
        {
            pickWeapon(weaponId);
        }
        RpcpickWeapon(weaponId);
    }
    [ClientRpc]
    void RpcpickWeapon(string weaponId)
    {
        if (!isLocalPlayer && !isServer)
        {
            pickWeapon(weaponId);
        }
    }
    void pickWeapon(string weaponId)
    {
        InteractiveWeapon[] pickedWeapon = GameObject.FindObjectsOfType<InteractiveWeapon>();
        foreach (InteractiveWeapon w in pickedWeapon)
        {
            if (w.id == weaponId)
            {
                w.TooglePickupHUD(false, playerCamera);
                this.Weapon = w;
                this.Player.AddWeapon(this.Weapon);

                return;
            }
        }
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
    public void dropWeapon()
    {
        if (!Player || Player.getActiveWeapon() == 0)
            return;
        Player.EndReloadWeapon();
        int weaponToDrop = Player.getActiveWeapon();
        Player.ChangeWeapon(Player.getActiveWeapon(), 0);
        Player.getWeapons()[weaponToDrop].Drop();
        Player.getWeapons()[weaponToDrop] = null;
        this.Weapon = null;
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
    void CmdplaySound()
    {
        if (isServer)
        {
            RaycastShooting();
        }
        RpcplaySound();
    }
    [ClientRpc]
    void RpcplaySound()
    {
        if (!isLocalPlayer && !isServer)
        {
            RaycastShooting();
        }
    }
    //shooting from weapon
    public void RaycastShooting()
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
            this.setFlash(Player.getMuzzle(), destination);
        }
        Player.getWeapons()[Player.getActiveWeapon()].OnShooting();

    }
    // tell the server which object has been shot in order to take speical actions
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

    public void playSound()
    {
        RaycastShooting();
        if (isServer)
        {
            RpcplaySound();
        }
        else
        {
            CmdplaySound();
        }
    }
    /*************** stop shooting from weapon     ************/
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
    /*************  show or hide weapon  *****************/
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
    /****** spawning bullet tracer      ******/
    public void setFlash(Transform gunMuzzle2,Vector3 destination)
    {
        GameObject instantShot = Instantiate(Player.shot, gunMuzzle2.position, Quaternion.LookRotation(destination - gunMuzzle2.position));
        instantShot.SetActive(true);
    }
    

}