using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
// a class to initialize the score board when enabling it
public class ScoreBoardScript : NetworkBehaviour {
    [SerializeField]
    GameObject playerScoreBoardItem;
    [SerializeField]
    Transform playerList;
    private void OnEnable()
    {
        CarHealthManager[] Drivers = GameManager.instance.getallDrivers();
        HealthManager[] Shooters= GameManager.instance.getallPlayers();
        foreach (HealthManager shooter in Shooters)
        {
           GameObject itemGo= (GameObject)Instantiate(playerScoreBoardItem, playerList);
            ScoreboardItem item = itemGo.GetComponent<ScoreboardItem>();
            item.setup(shooter.nickName, shooter.Kills, shooter.Team,shooter.Color,shooter.getLost());
        }
        foreach (CarHealthManager driver in Drivers)
        {
            GameObject itemGo = (GameObject)Instantiate(playerScoreBoardItem, playerList);
            ScoreboardItem item = itemGo.GetComponent<ScoreboardItem>();
            item.setup(driver.nickName, driver.Score, driver.Team,driver.Color,driver.Exploded);
        }
    }
    private void OnDisable()
    {
        foreach (Transform child in playerList)
        {
            Destroy(child.gameObject);
        }
    }
}
