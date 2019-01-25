﻿
using UnityEngine;
using UnityEngine.Networking;
[RequireComponent(typeof(HealthManager))]
public class NetwrokBehaviour : NetworkBehaviour
{
    [SerializeField]
    public Behaviour[] ComToDisable;
    public Canvas ShooterScore;
    public Canvas PlayerUI;
    void Start()
    {
        if (!isLocalPlayer)
        {
            for (int i = 0; i < ComToDisable.Length; i++)
            {
                ComToDisable[i].enabled = false;
            }
            ShooterScore.enabled = false;
            PlayerUI.enabled = false;
        }
        string _playerID = "Player " + GetComponent<NetworkIdentity>().netId;
        this.transform.name = _playerID;
        GameManager.instance.RegisterPlayer(this.transform.name, GetComponent<HealthManager>());
    }
  
    private void OnDisable()
    {
        GameManager.instance.UnRegisterPlayer(transform.name);
    }
}