﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// a class which manage each player Name plate in order to draw a canvas for each player which will be used only for team mates;
public class PlayerNamePlate : MonoBehaviour {

    [SerializeField]
    Text userName;
    [SerializeField]
    private HealthManager player;
    [SerializeField]
    private CarHealthManager driver;

    private Camera m_Camera;

    // Use this for initialization
    void Start () {
        if (player)
        {
            userName.text = player.nickName;
            userName.color = player.Color;
        }
        if (driver)
        {
            userName.text = driver.nickName;
            userName.color = driver.Color;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (!enabled)
            return;

        if (Camera.main)
        {
            m_Camera = Camera.main;
            transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward,
            m_Camera.transform.rotation * Vector3.up);
        }
            
        
           
        
	}
}
