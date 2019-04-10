using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour {
    [SerializeField]
    public GameObject ScoreBoard;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.X))
        {
            ScoreBoard.SetActive(true);
        }
        else if(Input.GetKeyUp(KeyCode.X))
        {
            ScoreBoard.SetActive(false);
        }
    }
}
