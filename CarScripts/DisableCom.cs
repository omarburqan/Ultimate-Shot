using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
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
        this.GetComponent<RCC_CarControllerV3>().enabled = false;
    }
}
