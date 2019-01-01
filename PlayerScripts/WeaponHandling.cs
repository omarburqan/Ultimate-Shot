using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WeaponHandling : NetworkBehaviour
{
    private InteractiveWeapon Weapon;
    private ShootBehaviour Player;

    

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
        Weapon = other.GetComponent<InteractiveWeapon>();
        Player = this.GetComponent<ShootBehaviour>();
        if (isLocalPlayer)
        {
            if (other.tag == "Gun")
            {
                Weapon.pickable = true;
                Weapon.TooglePickupHUD(true);
            }
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (isLocalPlayer)
        {
            if (other.tag == "Gun")
            {
                Weapon.pickable = false;
                Weapon.TooglePickupHUD(false);
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
        Weapon.TooglePickupHUD(false);
        Weapon.Toggle(true);

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
    public void playsound1()
    {
        Player.getWeapons()[Player.getActiveWeapon()].OnShooting();
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
    [Command]
    void CmdsetFlash(Vector3 destination)
    {
        if (isServer)
        {
            setFlash(Player.getMuzzle(),destination);
        }
        RpcsetFlash(destination);
    }
    [ClientRpc]
    void RpcsetFlash(Vector3 destination)
    {
        if (!isLocalPlayer && !isServer)
        {
            setFlash(Player.getMuzzle(), destination);
        }
    }
    public void setFlash1(Vector3 destination)
    {
        if (isServer)
        {
            RpcsetFlash(destination);
        }
        else
        {
            CmdsetFlash(destination);
        }
    }
    /******************************/
    [Command]
    void CmdRemoveFlash()
    {
        if (isServer)
        {
            Player.getobj().SetActive(false);
        }
        RpcRemoveFlash();
    }

    [ClientRpc]
    void RpcRemoveFlash()
    {
        if (!isLocalPlayer && !isServer)
        {
            Player.getobj().SetActive(false);
        }
    }
    public void RemoveFlash()
    {
        if (isServer)
        {
            RpcRemoveFlash();
        }
        else
        {
            CmdRemoveFlash();
        }
    }
}