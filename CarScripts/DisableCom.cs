using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// a class to control or manage the interrputing for each player scripting which will disable component for non local players
/// also register for game manager to save this player attributes in special dictionary will the server will use for increasing score and decrease healthpoints
/// or even to sync the status of the game for each player (Levels)
/// </summary>

[RequireComponent(typeof(CarHealthManager))]
public class DisableCom : NetworkBehaviour {
    [SerializeField]
    public Behaviour[] ComToDisable;
    public Canvas PlayerUI;

    void Start () {
        if (!isLocalPlayer)
        {
            for (int i = 0; i < ComToDisable.Length; i++)
            {
                ComToDisable[i].enabled = false;
            }
            PlayerUI.enabled = false;
        }
        string _playerID = "Driver " + GetComponent<NetworkIdentity>().netId;
        this.transform.name = _playerID;
        GameManager.instance.RegisterDriver(this.transform.name, GetComponent<CarHealthManager>(),GetComponent<StatusManager>());
    }
    // disable the player component when needed
    public void FreezePlayer()
    {
        if (!isLocalPlayer)
            return;
        for (int i = 0; i < ComToDisable.Length; i++)
        {
                ComToDisable[i].enabled = false;
        }
        this.GetComponent<RCC_CarControllerV3>().enabled = false;
    }
    // enalbe the player component when needed
    public void unFreezePlayer()
    {
        if (!isLocalPlayer)
            return;
        for (int i = 0; i < ComToDisable.Length; i++)
        {
            ComToDisable[i].enabled = true;
        }
        this.GetComponent<RCC_CarControllerV3>().enabled = true;
    }
}
