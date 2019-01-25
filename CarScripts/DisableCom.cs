using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DisableCom : NetworkBehaviour {
    [SerializeField]
    public Behaviour[] ComToDisable;
    public Canvas CarScore;
    public Canvas PlayerUI;

    void Start () {
        if (!isLocalPlayer)
        {
            for (int i = 0; i < ComToDisable.Length; i++)
            {
                ComToDisable[i].enabled = false;
            }
            CarScore.enabled = false;
            PlayerUI.enabled = false;
        }
        string _playerID = "Driver " + GetComponent<NetworkIdentity>().netId;
        this.transform.name = _playerID;
        GameManager.instance.RegisterDriver(this.transform.name, GetComponent<CarHealthManager>());
    }

    private void OnDisable()
    {
        GameManager.instance.UnRegisterDriver(transform.name);
    }
}
