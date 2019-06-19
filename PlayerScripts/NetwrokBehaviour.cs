﻿
using UnityEngine;
using UnityEngine.Networking;
[RequireComponent(typeof(HealthManager))]
public class NetwrokBehaviour : NetworkBehaviour
{
    [SerializeField]
    public Behaviour[] ComToDisable;
    
    public Canvas PlayerUI;
    void Start()
    {
        if (!isLocalPlayer)
        {
            for (int i = 0; i < ComToDisable.Length; i++)
            {
                ComToDisable[i].enabled = false;
            }
            PlayerUI.enabled = false;
        }
        string _playerID = "Player " + GetComponent<NetworkIdentity>().netId;
        this.transform.name = _playerID;
        GameManager.instance.RegisterPlayer(this.transform.name, GetComponent<HealthManager>(),GetComponent<StatusManager>());
    }
    public void FreezePlayer()
    {
        if (!isLocalPlayer)
            return;
        for (int i = 0; i < ComToDisable.Length; i++)
        {
            if(i!=4 && i!=5)
                ComToDisable[i].enabled = false;
        }
    }
    public void unFreezePlayer()
    {
        if (!isLocalPlayer)
            return;
        for (int i = 0; i < ComToDisable.Length; i++)
        {
            if (i != 4 && i != 5)
                ComToDisable[i].enabled = true;
        }
    }
}