using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
// a script to disable all the character scripts when not localplayer.
public class NetwrokBehaviour : NetworkBehaviour {
    [SerializeField]
    Behaviour[] ComToDisable;
    void Start () {
        if (!isLocalPlayer)
        {
            for(int i = 0; i < ComToDisable.Length; i++)
            {
                ComToDisable[i].enabled = false;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
