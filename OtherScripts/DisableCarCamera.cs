using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// this class disable the RCCCamera which stay in game scene in order to not make any wrong interruption
public class DisableCarCamera : MonoBehaviour {
    public static DisableCarCamera instance = null;
    public GameObject Cam;
    public GameObject driverCanvas;
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
    public void DisableCamera() // in order to prevent the camera of shooter to join the camera of the driver
    {
        this.Cam.SetActive(false);
        this.driverCanvas.SetActive(false);
    }
}
