using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
/// <summary>
///  this class corresponds to disable not local players to interact with other players scripts.
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
        GameManager.instance.RegisterDriver(this.transform.name, GetComponent<CarHealthManager>());
    }

    /*private void OnDisable()
    {
        GameManager.instance.UnRegisterDriver(transform.name);
    }*/
    public void FreezePlayer()
    {
        if (!isLocalPlayer)
            return;
        for (int i = 0; i < ComToDisable.Length; i++)
        {
                ComToDisable[i].enabled = false;
        }
    }
}
