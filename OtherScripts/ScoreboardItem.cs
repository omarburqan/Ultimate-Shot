using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// a class to initialize each score board item according to player properties
public class ScoreboardItem : MonoBehaviour {

    [SerializeField]
    Text userName;

    [SerializeField]
    Text Score;
    [SerializeField]
    Text Team;
    [SerializeField]
    Image isDead;

    public void setup(string username, int score, int Team,Color color,bool Status)
    {
        userName.text = username;
        userName.color = color;
        Score.text = "Score: " + score.ToString();
        Score.color = color;
        this.Team.text = "Team: " +Team.ToString();
        this.Team.color = color;
        if (Status)
        {
            isDead.enabled = true;
        }
        else
        {
            isDead.enabled = false;
        }

    }
}
