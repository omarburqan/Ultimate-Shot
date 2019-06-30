using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// this class manage the players score-board in order to activate or deactivate it ....
public class PlayerUI : MonoBehaviour {
    [SerializeField]
    public GameObject ScoreBoard;
	
	
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
