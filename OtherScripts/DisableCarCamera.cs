using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableCarCamera : MonoBehaviour {
    public static DisableCarCamera instance = null;
    public GameObject Cam;

    // Use this for initialization
    void Start () {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public void DisableCamera() // in order to prevent the camera of shooter to join the camera of the driver
    {
        this.Cam.SetActive(false);
    }
}
