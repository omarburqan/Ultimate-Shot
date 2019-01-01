using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControllerHealth : MonoBehaviour {
    public GameObject SmokeEffect;
    public GameObject FireEffect;
    public GameObject Explosion;
    public GameObject Front;
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.J))
        {
            GameObject instantShot = Instantiate(SmokeEffect, Front.transform.position, Front.transform.rotation);
            instantShot.SetActive(true);
            instantShot.transform.SetParent(this.transform);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            GameObject instantShot = Instantiate(FireEffect, Front.transform.position, Front.transform.rotation);
            instantShot.SetActive(true);
            instantShot.transform.SetParent(this.transform);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            GameObject instantShot = Instantiate(Explosion, Front.transform.position, Front.transform.rotation);
            instantShot.SetActive(true);
            instantShot.transform.SetParent(this.transform);
        }
    }
}

